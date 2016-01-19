
using System;

namespace RandomPredictor
{
    public class PossibleValue : PossibleRange
    {
        public PossibleValue(decimal probability, int value) : base(probability, value, value) { }

        public int Value { get { return LowerBound; } }

        public override string ToString()
        {
            return string.Format("<{0:F2}% [{1}]>", Probability*100, Value);
        }
    }
}
