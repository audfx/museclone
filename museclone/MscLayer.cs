using theori;
using theori.Resources;

using MoonSharp.Interpreter;

namespace Museclone
{
    public class MscLayer : Layer
    {
        public readonly Table tblMsc;

        public MscLayer(ClientResourceLocator locator, string layerPath, DynValue[] args)
            : base(locator, layerPath, args)
        {
            tblMsc = m_script.NewTable();
        }

        protected override Layer CreateNewLuaLayer(string layerPath, DynValue[] args) =>
            new MscLayer(ResourceLocator, layerPath, args);
    }
}
