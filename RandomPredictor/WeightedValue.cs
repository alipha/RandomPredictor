
namespace RandomPredictor
{
    public class WeightedValue
    {
        private decimal _weight;
        private int _value;


        public WeightedValue(decimal weight, int value)
        {
            _weight = weight;
            _value = value;
        }

        public decimal Weight { get { return _weight; } }

        public int Value { get { return _value; } }
    }
}
