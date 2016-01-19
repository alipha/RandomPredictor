
using System;

namespace RandomPredictor
{
    public class PossibleRange
    {
        private decimal _probability;
        private int _lowerBound;
        private int _upperBound;


        public PossibleRange(decimal probability, int lowerBound, int upperBound)
        {
            //if (probability <= 0 || probability > 1)
            //    throw new ArgumentOutOfRangeException($"Probability {probability} is not between 0 and 1.");

            if(lowerBound < 0) //|| lowerBound == Int32.MaxValue)
                throw new ArgumentOutOfRangeException(string.Format("LowerBound {0} is less than 0 or equal to {1}.", lowerBound, Int32.MaxValue));

            if (upperBound < 0) //|| upperBound == Int32.MaxValue)
                throw new ArgumentOutOfRangeException(string.Format("UpperBound {0} is less than 0 or equal to {1}.", upperBound, Int32.MaxValue));

            _probability = probability;
            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public PossibleRange(PossibleRange source)
        {
            _probability = source.Probability;
            _lowerBound = source.LowerBound;
            _upperBound = source.UpperBound;
        }

        public decimal Probability { get { return _probability; } }

        public int LowerBound { get { return _lowerBound; } }

        public int UpperBound { get { return _upperBound; } }

        public int Range { get { return UpperBound - LowerBound + 1; } }

        public bool IsFullRange { get { return LowerBound == 0 && UpperBound == Int32.MaxValue - 1; } }


        public decimal SubProbability(int lowerBound, int upperBound)
        {
            if(upperBound < lowerBound)
                throw new ArgumentOutOfRangeException(string.Format("upperBound {0} is less than lowerBound {1}.", upperBound, lowerBound));

            if(lowerBound < LowerBound)
                throw new ArgumentOutOfRangeException(string.Format("{0} is less than LowerBound {1}.", lowerBound, LowerBound));

            if (upperBound > UpperBound)
                throw new ArgumentOutOfRangeException(string.Format("{0} is greater than UpperBound {1}.", upperBound, UpperBound));

            return Probability * (upperBound - lowerBound + 1) / Range;
        }

        public PossibleRange SubRange(int lowerBound, int upperBound)
        {
            var probability = SubProbability(lowerBound, upperBound);
            return new PossibleRange(probability, lowerBound, upperBound);
        }

        public override string ToString()
        {
            return string.Format("<{0:F2}% [{1}, {2}]>", Probability*100, LowerBound, UpperBound);
        }
    }
}
