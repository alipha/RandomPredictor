
using System;

namespace RandomPredictor
{
    public class PossibleDouble : PossibleValue
    {
        public PossibleDouble(decimal probability, double value) : base(probability, (int)(value * Int32.MaxValue)) { }

        public double DoubleValue { get { return (double)Value / Int32.MaxValue; } }

        public override string ToString()
        {
            return string.Format("<{0:F2}% [{1}]>", Probability*100, DoubleValue);
        }
    }
}
