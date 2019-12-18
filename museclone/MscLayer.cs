using System;
using System.IO;

using MoonSharp.Interpreter;

using theori;
using theori.Charting;
using theori.Resources;

using Museclone.Charting.Conversions;
using Museclone.Graphics;
using Museclone.Charting;

namespace Museclone
{
    public class MscLayer : Layer
    {
        public readonly Table tblMsc;

        public readonly Table tblMscCharts;
        public readonly Table tblMscGraphics;

        public MscLayer(ClientResourceLocator locator, string layerPath, params DynValue[] args)
            : base(locator, layerPath, args)
        {
            m_script["msc"] = tblMsc = m_script.NewTable();

            tblMsc["charts"] = tblMscCharts = m_script.NewTable();
            tblMsc["graphics"] = tblMscGraphics = m_script.NewTable();

            tblMscCharts["create"] = (Func<ChartHandle>)(() => new ChartHandle(m_resources, m_script, Client.DatabaseWorker, MusecloneChartFactory.Instance.CreateNew()));
            tblMscCharts["loadXmlFile"] = (Func<string, ChartHandle>)(path => new ChartHandle(m_resources, m_script, Client.DatabaseWorker, MusecaToTheori.CreateChartFromXml(File.OpenRead(path))));

            tblMscGraphics["createHighway"] = (Func<ChartHandle, Highway>)(chart => new Highway(locator, chart.Chart));
        }

        protected override Layer CreateNewLuaLayer(string layerPath, DynValue[] args) => new MscLayer(ResourceLocator, layerPath, args);
    }
}
