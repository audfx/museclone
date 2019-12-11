using theori.Charting;
using theori.GameModes;

using Museclone.Charting;

namespace Museclone
{
    public sealed class MusecloneGameMode : GameMode
    {
        public static readonly MusecloneGameMode Instance = new MusecloneGameMode();

        public MusecloneGameMode()
            : base("Museclone")
        {
        }

        public override bool SupportsStandaloneUsage => true;

        public override bool SupportsSharedUsage => true;

        public override ChartFactory CreateChartFactory() => MusecloneChartFactory.Instance;
    }
}
