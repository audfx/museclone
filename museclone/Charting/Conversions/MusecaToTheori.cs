using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using theori;
using theori.Charting;

namespace Museclone.Charting.Conversions
{
    public static class MusecaToTheori
    {
        class TimingInfo
        {
            public int MusecaWhen;
            public double BeatsPerMinute;
            public int Numerator;
            public int Denominator;
        }

        class EventInfo
        {
            public long StartTimeMillis;
            public long EndTimeMillis;
            public int Type;
            public int Kind;
        }

        public static Chart CreateChartFromXml(Stream inStream)
        {
            using var reader = XmlReader.Create(inStream);

            var chart = MusecloneChartFactory.Instance.CreateNew();

            var timingInfo = new Dictionary<int, TimingInfo>();
            var eventInfos = new List<EventInfo>();
            EventInfo? curEvent = null;

            reader.MoveToContent();

            #region Read Timing Information and Event Creation

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.Name == "event")
                        {
                            Logger.Log($"End event block: {curEvent!.StartTimeMillis}, {curEvent!.EndTimeMillis}, {curEvent!.Type}, {curEvent!.Kind}");

                            eventInfos.Add(curEvent);
                            curEvent = null;
                        }
                        break;

                    case XmlNodeType.Element:
                    {
                        if (reader.Name == "tempo")
                        {
                            int time = 0, deltaTime = 0, value = 500_000;
                            long bpm = 120_00;
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "time":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <time ...>
                                            time = reader.ReadContentAsInt();
                                        } break;
                                        
                                        case "delta_time":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            deltaTime = reader.ReadContentAsInt();
                                        } break;
                                        
                                        case "val":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            value = reader.ReadContentAsInt();
                                        } break;
                                        
                                        case "bpm":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            bpm = reader.ReadContentAsLong();
                                        } break;
                                    }
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tempo")
                                {
                                    Logger.Log($"End tempo block: {time}, {deltaTime}, {value}, {bpm}");

                                    if (!timingInfo.TryGetValue(time, out var info))
                                        timingInfo[time] = info = new TimingInfo();
                                    info.MusecaWhen = time;
                                    info.BeatsPerMinute = bpm / 100.0;
                                    break;
                                }
                            }
                        }
                        else if (reader.Name == "sig_info")
                        {
                            int time = 0, deltaTime = 0, num = 4, denomi = 4;
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "time":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <time ...>
                                            time = reader.ReadContentAsInt();
                                        }
                                        break;

                                        case "delta_time":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            deltaTime = reader.ReadContentAsInt();
                                        }
                                        break;

                                        case "num":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            num = reader.ReadContentAsInt();
                                        }
                                        break;

                                        case "denomi":
                                        {
                                            //string type = reader["__type"];
                                            reader.Read(); // <delta_time ...>
                                            denomi = reader.ReadContentAsInt();
                                        }
                                        break;
                                    }
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "sig_info")
                                {
                                    Logger.Log($"End sig_info block: {time}, {deltaTime}, {num}, {denomi}");

                                    if (!timingInfo.TryGetValue(time, out var info))
                                        timingInfo[time] = info = new TimingInfo();
                                    info.Numerator = num;
                                    info.Denominator = denomi;
                                    break;
                                }
                            }
                        }

                        switch (reader.Name)
                        {
                            case "event": curEvent = new EventInfo(); break;

                            case "stime_ms":
                            {
                                reader.Read();
                                curEvent!.StartTimeMillis = reader.ReadContentAsLong();
                            }
                            break;

                            case "etime_ms":
                            {
                                reader.Read();
                                curEvent!.EndTimeMillis = reader.ReadContentAsLong();
                            }
                            break;

                            case "type":
                            {
                                reader.Read();
                                curEvent!.Type = reader.ReadContentAsInt();
                            }
                            break;

                            case "kind":
                            {
                                reader.Read();
                                curEvent!.Kind = reader.ReadContentAsInt();
                            }
                            break;
                        }
                    } break;
                }
            }

            #endregion

            #region Construct :theori Timing Data

            foreach (var info in from pair in timingInfo
                                 orderby pair.Key select pair.Value)
            {
                tick_t position = chart.CalcTickFromTime(info.MusecaWhen / 1000.0);

                var cp = chart.ControlPoints.GetOrCreate(position, true);
                if (info.BeatsPerMinute > 0)
                    cp.BeatsPerMinute = info.BeatsPerMinute;

                if (info.Numerator != 0 && info.Denominator != 0 && (cp.BeatCount != info.Numerator || cp.BeatKind != info.Denominator))
                {
                    cp = chart.ControlPoints.GetOrCreate(MathL.Ceil((double)position), true);
                    cp.BeatCount = info.Numerator;
                    cp.BeatKind = info.Denominator;
                }
            }

            #endregion

            #region Determine timing info from beat events

            if (timingInfo.Count == 0)
            {
                var barEvents = from e in eventInfos where e.Kind == 11 && e.Kind == 12 select e;

                // bpm related
                long lastBeatDurationMillis = 60_000_00 / 120_00, lastBeatStartMillis = 0;
                int bpmTotalBeats = 0;

                // sig related
                int n = 4;
                int measure = 0, totalMeasures = 0, beatCount = 0, totalBeatsInMeasure = 0;
                long sigMeasureStartMillis = 0, sigCurrentMeasureStartMillis = 0, sigOffsetMillis = 0;
                tick_t offset = 0;

                var bpmChanges = new Dictionary<int, (long Millis, double BeatsPerMinute)>();
                var timeSigChanges = new Dictionary<int, (int Numerator, int Denominator)>();

                foreach (var e in barEvents)
                {
                    long millis = e.StartTimeMillis;

                    if (totalBeatsInMeasure > 0)
                    {
                        long beatDuration = millis - lastBeatStartMillis;
                        // TODO(local): if this is within like a millisecond then maybe it's fine
                        if (beatDuration != lastBeatDurationMillis)
                            bpmChanges[bpmTotalBeats] = (lastBeatStartMillis, 60_000.0 / beatDuration);

                        lastBeatDurationMillis = beatDuration;
                    }

                    bpmTotalBeats++;
                    lastBeatStartMillis = millis;

                    if (e.Kind == 11) // measure marker
                    {
                        totalMeasures++;

                        if (totalBeatsInMeasure == 0) // first one in the chart
                        {
                            sigOffsetMillis = millis;
                            sigMeasureStartMillis = millis;

                            beatCount++;
                            totalBeatsInMeasure++;
                        }
                        else // check that the previous beat count matches `n`
                        {
                            if (beatCount != n)
                            {
                                timeSigChanges[totalMeasures - 1] = (beatCount, 4);

                                totalBeatsInMeasure = 0;
                                measure = 0;
                            }

                            // continue as normal
                            totalBeatsInMeasure++;
                            measure++;

                            beatCount = 1;
                            sigCurrentMeasureStartMillis = millis;
                        }
                    }
                    else // beat marker
                    {
                        beatCount++;
                        totalBeatsInMeasure++;
                    }
                }

                chart.Offset = sigOffsetMillis / 1_000.0;
                foreach (var (measureIndex, (num, denom)) in timeSigChanges)
                {
                    tick_t position = measureIndex;
                    var cp = chart.ControlPoints.GetOrCreate(position, true);
                    cp.BeatCount = num;
                    cp.BeatKind = denom;
                }

                foreach (var (beatIndex, (timeMillis, bpm)) in bpmChanges)
                {
                    int beatsLeft = beatIndex;

                    tick_t? where = null;
                    foreach (var cp in chart.ControlPoints)
                    {
                        if (!cp.HasNext)
                        {
                            where = cp.Position + (double)beatsLeft / cp.BeatCount;
                            break;
                        }
                        else
                        {
                            int numBeatsInCp = (int)(cp.BeatCount * (double)(cp.Next.Position - cp.Position));
                            if (beatsLeft > numBeatsInCp)
                                beatsLeft -= numBeatsInCp;
                            else
                            {
                                where = cp.Position + (double)beatsLeft / cp.BeatCount;
                                break;
                            }
                        }
                    }

                    if (where.HasValue)
                    {
                        var cp = chart.ControlPoints.GetOrCreate(where.Value, true);
                        cp.BeatsPerMinute = bpm;
                    }
                    else Logger.Log($"Bpm change at beat {beatIndex} (timeMillis) could not be created for bpm {bpm}");
                }
            }

            #endregion

            var noteInfos = from e in eventInfos where e.Kind != 11 && e.Kind != 12 && e.Kind != 14 && e.Kind != 15 select e;

            foreach (var entity in noteInfos)
            {
                tick_t startTicks = chart.CalcTickFromTime(entity.StartTimeMillis / 1000.0);
                tick_t endTicks = chart.CalcTickFromTime(entity.EndTimeMillis / 1000.0);

                const int q = 192;
                startTicks = MathL.Round((double)(startTicks * q)) / q;
                endTicks = MathL.Round((double)(endTicks * q)) / q;

                tick_t durTicks = endTicks - startTicks;

                if (entity.Kind == 1 && entity.Type == 5)
                    chart[5].Add<ButtonEntity>(startTicks, durTicks);
                else
                {
                    switch (entity.Kind)
                    {
                        // "chip" tap note
                        case 0: chart[entity.Type].Add<ButtonEntity>(startTicks); break;
                        // hold tap note, ignore foot pedal bc handled above
                        case 1: chart[entity.Type - 6].Add<ButtonEntity>(startTicks, durTicks); break;
                        // large spinner
                        case 2:
                        {
                            var e = chart[entity.Type % 6].Add<SpinnerEntity>(startTicks);
                            e.Large = true;
                        } break;
                        // large spinner left
                        case 3:
                        {
                            var e = chart[entity.Type % 6].Add<SpinnerEntity>(startTicks);
                            e.Direction = LinearDirection.Left;
                            e.Large = true;
                        } break;
                        // large spinner right
                        case 4:
                        {
                            var e = chart[entity.Type % 6].Add<SpinnerEntity>(startTicks);
                            e.Direction = LinearDirection.Right;
                            e.Large = true;
                        } break;
                        // small spinner
                        case 5:
                        {
                            var e = chart[entity.Type].Add<SpinnerEntity>(startTicks);
                        } break;
                        // small spinner left
                        case 6:
                        {
                            var e = chart[entity.Type].Add<SpinnerEntity>(startTicks);
                            e.Direction = LinearDirection.Left;
                        } break;
                        // small spinner right
                        case 7:
                        {
                            var e = chart[entity.Type].Add<SpinnerEntity>(startTicks);
                            e.Direction = LinearDirection.Right;
                        } break;
                    }
                }
            }

            return chart;
        }
    }
}
