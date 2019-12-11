using theori.Charting;

namespace Museclone.Charting
{
    [EntityType("Spinner")]
    public sealed class SpinnerEntity : Entity
    {
        public bool Large = false;
        public LinearDirection Direction = LinearDirection.None;
    }
}
