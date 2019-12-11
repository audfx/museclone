using theori.Charting;

namespace Museclone.Charting
{
    public sealed class MusecloneChartFactory : ChartFactory
    {
        public override Chart CreateNew()
        {
            var chart = new Chart(MusecloneGameMode.Instance);
            chart.CreateTypedLane<PedalEntity>(0, EntityRelation.Equal);
            for (int i = 1; i < 6; i++)
                chart.CreateTypedLane<SpinnerEntity>(i, EntityRelation.Equal);

            return chart;
        }
    }
}
