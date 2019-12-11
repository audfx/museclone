using theori.Charting;

namespace Museclone.Charting
{
    public sealed class MusecloneChartFactory : ChartFactory
    {
        public static readonly MusecloneChartFactory Instance = new MusecloneChartFactory();

        public override Chart CreateNew()
        {
            var chart = new Chart(MusecloneGameMode.Instance);
            for (int i = 0; i < 5; i++)
                chart.CreateMultiTypedLane<ButtonEntity, SpinnerEntity>(i);
            chart.CreateTypedLane<ButtonEntity>(5);

            return chart;
        }
    }
}
