using System.IO;
using System.Text;
using Museclone.Charting.Conversions;
using theori;
using theori.Database;
using theori.Platform;
using theori.Resources;

namespace Museclone
{
    public class MscClient : Client
    {
        public static ClientResourceLocator DefaultResourceLocator = new ClientResourceLocator("skins/user-custom", "materials/basic");

        static MscClient()
        {
            DefaultResourceLocator.AddManifestResourceLoader(ManifestResourceLoader.GetResourceLoader(typeof(ClientResourceLocator).Assembly, "theori.Resources"));
            DefaultResourceLocator.AddManifestResourceLoader(ManifestResourceLoader.GetResourceLoader(typeof(MscClient).Assembly, "Museclone.Resources"));
        }

        public MscClient()
        {
            ChartDatabaseService.Initialize();
        }

        protected override Layer CreateInitialLayer() => new Layer(DefaultResourceLocator, "driver");

        protected override UnhandledExceptionAction OnUnhandledException()
        {
            return UnhandledExceptionAction.GiveUpRethrow;
        }

        public override void SetHost(ClientHost host)
        {
            base.SetHost(host);

            var test = MusecaToTheori.CreateChartFromXml(new MemoryStream(Encoding.UTF8.GetBytes(TESTDATA)));

            host.Exited += OnExited;
        }

        private void OnExited()
        {
        }

        const string TESTDATA = @"<data>
<smf_info>
<ticks __type=""s32"">480</ticks>
<tempo_info>
<tempo>
<time __type=""s32"">0</time>
<delta_time __type=""s32"">0</delta_time>
<val __type=""s32"">1000000</val>
<bpm __type=""s64"">6000</bpm>
</tempo>
</tempo_info>
<sig_info>
<signature>
<time __type=""s32"">0</time>
<delta_time __type=""s32"">0</delta_time>
<num __type=""s32"">4</num>
<denomi __type=""s32"">4</denomi>
</signature>
</sig_info>
</smf_info>
<event>
<stime_ms __type=""s64"">0</stime_ms>
<etime_ms __type=""s64"">0</etime_ms>
<type __type=""s32"">11</type>
<!--  measure marker  -->
<kind __type=""s32"">11</kind>
<!--  measure marker  -->
</event>
<event>
<stime_ms __type=""s64"">1000</stime_ms>
<etime_ms __type=""s64"">1000</etime_ms>
<type __type=""s32"">12</type>
<!--  beat marker  -->
<kind __type=""s32"">12</kind>
<!--  beat marker  -->
</event>
<event>
<stime_ms __type=""s64"">2000</stime_ms>
<etime_ms __type=""s64"">2000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">3000</stime_ms>
<etime_ms __type=""s64"">3000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">4000</stime_ms>
<etime_ms __type=""s64"">4000</etime_ms>
<type __type=""s32"">14</type>
<!--  Grafica start (should be 3 of these total)  -->
<kind __type=""s32"">14</kind>
<!--  Grafica start (should be 3 of these total)  -->
</event>
<event>
<stime_ms __type=""s64"">4000</stime_ms>
<etime_ms __type=""s64"">4000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">4000</stime_ms>
<!--  start of foot pedal hold  -->
<etime_ms __type=""s64"">8000</etime_ms>
<!--  end of hold  -->
<type __type=""s32"">5</type>
<!--  type 5 is always the foot pedal,  -->
<kind __type=""s32"">1</kind>
<!--  which is always kind 1 (hold note)  -->
</event>
<event>
<stime_ms __type=""s64"">5000</stime_ms>
<etime_ms __type=""s64"">5000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">6000</stime_ms>
<etime_ms __type=""s64"">6000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">7000</stime_ms>
<etime_ms __type=""s64"">7000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">8000</stime_ms>
<etime_ms __type=""s64"">8000</etime_ms>
<type __type=""s32"">15</type>
<!--  grafica end (should be 3 of these total)  -->
<kind __type=""s32"">15</kind>
<!--  grafica end (should be 3 of these total)  -->
</event>
<event>
<stime_ms __type=""s64"">8000</stime_ms>
<etime_ms __type=""s64"">8000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">9000</stime_ms>
<etime_ms __type=""s64"">9000</etime_ms>
<type __type=""s32"">14</type>
<kind __type=""s32"">14</kind>
</event>
<event>
<stime_ms __type=""s64"">9000</stime_ms>
<etime_ms __type=""s64"">9000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">10000</stime_ms>
<etime_ms __type=""s64"">10000</etime_ms>
<type __type=""s32"">15</type>
<kind __type=""s32"">15</kind>
</event>
<event>
<stime_ms __type=""s64"">10000</stime_ms>
<etime_ms __type=""s64"">10000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">11000</stime_ms>
<etime_ms __type=""s64"">11000</etime_ms>
<type __type=""s32"">14</type>
<kind __type=""s32"">14</kind>
</event>
<event>
<stime_ms __type=""s64"">11000</stime_ms>
<etime_ms __type=""s64"">11000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">15</type>
<kind __type=""s32"">15</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">0</type>
<!--
 0,1,2,3,4 correspond to the tap notes (spinner press) 
-->
<kind __type=""s32"">0</kind>
<!--
 single (tap) note, stime and etime are always the same
-->
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">1</type>
<kind __type=""s32"">0</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">2</type>
<kind __type=""s32"">0</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">3</type>
<kind __type=""s32"">0</kind>
</event>
<event>
<stime_ms __type=""s64"">12000</stime_ms>
<etime_ms __type=""s64"">12000</etime_ms>
<type __type=""s32"">4</type>
<kind __type=""s32"">0</kind>
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">0</type>
<!--  indicates spinner 1  -->
<kind __type=""s32"">6</kind>
<!--  small spin left  -->
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">1</type>
<kind __type=""s32"">6</kind>
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">2</type>
<kind __type=""s32"">6</kind>
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">3</type>
<kind __type=""s32"">6</kind>
</event>
<event>
<stime_ms __type=""s64"">13000</stime_ms>
<etime_ms __type=""s64"">13000</etime_ms>
<type __type=""s32"">4</type>
<kind __type=""s32"">6</kind>
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">0</type>
<!--  indicates spinner 1  -->
<kind __type=""s32"">7</kind>
<!--  small spin right  -->
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">1</type>
<kind __type=""s32"">7</kind>
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">2</type>
<kind __type=""s32"">7</kind>
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">3</type>
<kind __type=""s32"">7</kind>
</event>
<event>
<stime_ms __type=""s64"">14000</stime_ms>
<etime_ms __type=""s64"">14000</etime_ms>
<type __type=""s32"">4</type>
<kind __type=""s32"">7</kind>
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">0</type>
<kind __type=""s32"">5</kind>
<!--  small spin any direction  -->
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">1</type>
<kind __type=""s32"">5</kind>
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">2</type>
<kind __type=""s32"">5</kind>
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">3</type>
<kind __type=""s32"">5</kind>
</event>
<event>
<stime_ms __type=""s64"">15000</stime_ms>
<etime_ms __type=""s64"">15000</etime_ms>
<type __type=""s32"">4</type>
<kind __type=""s32"">5</kind>
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">16000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">6</type>
<!--  spinner numbers change to 6,7,8,9,10   -->
<kind __type=""s32"">2</kind>
<!--
 large spin/tornado. Will always have different s/etime 
-->
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">7</type>
<kind __type=""s32"">2</kind>
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">8</type>
<kind __type=""s32"">2</kind>
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">9</type>
<kind __type=""s32"">2</kind>
</event>
<event>
<stime_ms __type=""s64"">16000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">10</type>
<kind __type=""s32"">2</kind>
</event>
<event>
<stime_ms __type=""s64"">17000</stime_ms>
<etime_ms __type=""s64"">17000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">18000</stime_ms>
<etime_ms __type=""s64"">18000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">19000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">6</type>
<kind __type=""s32"">3</kind>
<!--  large spin/tornado left  -->
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">7</type>
<kind __type=""s32"">3</kind>
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">8</type>
<kind __type=""s32"">3</kind>
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">9</type>
<kind __type=""s32"">3</kind>
</event>
<event>
<stime_ms __type=""s64"">19000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">10</type>
<kind __type=""s32"">3</kind>
</event>
<event>
<stime_ms __type=""s64"">20000</stime_ms>
<etime_ms __type=""s64"">20000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">21000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">6</type>
<kind __type=""s32"">4</kind>
<!--  large spin/tornado right  -->
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">7</type>
<kind __type=""s32"">4</kind>
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">8</type>
<kind __type=""s32"">4</kind>
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">9</type>
<kind __type=""s32"">4</kind>
</event>
<event>
<stime_ms __type=""s64"">21000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">10</type>
<kind __type=""s32"">4</kind>
</event>
<event>
<stime_ms __type=""s64"">22000</stime_ms>
<etime_ms __type=""s64"">22000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">23000</stime_ms>
<etime_ms __type=""s64"">23000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">24000</etime_ms>
<type __type=""s32"">11</type>
<kind __type=""s32"">11</kind>
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">6</type>
<!--  spinner numbers change to 6,7,8,9,10  -->
<kind __type=""s32"">1</kind>
<!--  regular hold note   -->
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">7</type>
<kind __type=""s32"">1</kind>
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">8</type>
<kind __type=""s32"">1</kind>
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">9</type>
<kind __type=""s32"">1</kind>
</event>
<event>
<stime_ms __type=""s64"">24000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">10</type>
<kind __type=""s32"">1</kind>
</event>
<event>
<stime_ms __type=""s64"">25000</stime_ms>
<etime_ms __type=""s64"">25000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">26000</stime_ms>
<etime_ms __type=""s64"">26000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
<event>
<stime_ms __type=""s64"">27000</stime_ms>
<etime_ms __type=""s64"">27000</etime_ms>
<type __type=""s32"">12</type>
<kind __type=""s32"">12</kind>
</event>
</data>";
    }
}
