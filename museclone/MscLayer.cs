using System;
using System.IO;

using MoonSharp.Interpreter;

using theori;
using theori.Charting;
using theori.Resources;

using Museclone.Charting.Conversions;

namespace Museclone
{
    public class MscLayer : Layer
    {
        public readonly Table tblMsc;

        public readonly Table tblMscCharts;

        public MscLayer(ClientResourceLocator locator, string layerPath, params DynValue[] args)
            : base(locator, layerPath, args)
        {
            m_script["msc"] = tblMsc = m_script.NewTable();

            tblMsc["charts"] = tblMscCharts = m_script.NewTable();

            tblMscCharts["loadXmlFile"] = (Func<string, ChartHandle>)(path => new ChartHandle(m_resources, m_script, Client.DatabaseWorker, MusecaToTheori.CreateChartFromXml(File.OpenRead(path))));
        }

        protected override Layer CreateNewLuaLayer(string layerPath, DynValue[] args) =>
            new MscLayer(ResourceLocator, layerPath, args);
    }
}
