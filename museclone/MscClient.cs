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

            host.Exited += OnExited;
        }

        private void OnExited()
        {
        }
    }
}
