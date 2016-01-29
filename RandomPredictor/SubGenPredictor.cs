using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomPredictor
{
    public class SubGenPredictor
    {
        #region Private/Protected Fields

        protected const int SPLIT_RANGES = 10;

        protected List<PossibleRange>[] SeedArray;

        protected int LeadIndex;
        protected int LagIndex;

        #endregion


        #region Constructors and Public Properties/Methods

        public SubGenPredictor()
        {
            SeedArray = new List<PossibleRange>[55];

            for (var i = 0; i < SeedArray.Length; i++)
                SeedArray[i] = new List<PossibleRange> { AllPossible };

            LeadIndex = 21;
            LagIndex = 0;
        }

        public SubGenPredictor(SubGenPredictor source)
        {
            SeedArray = new List<PossibleRange>[55];

            for (var i = 0; i < SeedArray.Length; i++)
            {
                SeedArray[i] = new List<PossibleRange>(source.SeedArray[i].Count);

                foreach(var range in source.SeedArray[i])
                    SeedArray[i].Add(new PossibleRange(range));
            }

            LeadIndex = source.LeadIndex;
            LagIndex = source.LagIndex;
        }


        public PossibleRange AllPossible { get { return new PossibleRange(1.0M, 0, Int32.MaxValue - 1); } }

        public void Skip()
        {
            IncrementIndexes();
        }

        public List<PossibleRange> LastLaggers { get { return SeedArray[LagIndex == 0 ? 54 : LagIndex - 1]; } }

        public List<PossibleRange> LastLeaders { get { return SeedArray[LeadIndex == 0 ? 54 : LeadIndex - 1]; } }

        #endregion


        #region CollectNext methods

        public void CollectNext(double value)
        {
            CollectNext(new[] { new PossibleDouble(1.0M, value) });
        }

        public void CollectNext(int value)
        {
            CollectNext(new[] { new PossibleValue(1, value) });
        }

        public void CollectNext(int value, int minValue, int maxValue)
        {
            CollectNext(new[] { new PossibleValue(1, value) }, minValue, maxValue);
        }

        public void CollectNext<TPossible>(IEnumerable<TPossible> possibleValues, int minValue, int maxValue) where TPossible : PossibleRange
        {
            var range = maxValue - minValue;

            CollectNext(possibleValues.Select(v =>
                new PossibleRange
                (
                    v.Probability,
                    (int)((double)(v.LowerBound - minValue) / range * Int32.MaxValue),
                    (int)((double)(v.UpperBound - minValue + 1) / range * Int32.MaxValue) - 1
                )
            ));
        }

        public void CollectNext(int value, int maxValue)
        {
            CollectNext(new[] { new PossibleValue(1, value) }, maxValue);
        }

        public void CollectNext<TPossible>(IEnumerable<TPossible> possibleValues, int maxValue) where TPossible : PossibleRange
        {
            CollectNext(possibleValues.Select(v =>
                new PossibleRange
                (
                    v.Probability,
                    v.LowerBound == 0 ? 0 : (int)((double)v.LowerBound / maxValue * Int32.MaxValue + 1),
                    (int)((double)(v.UpperBound + 1) / maxValue * Int32.MaxValue)
                )
            ));
        }

        public void CollectNext<TPossible>(IEnumerable<TPossible> possibleRanges) where TPossible : PossibleRange
        {
            CollectNext(possibleRanges, modify:true);
        }

        #endregion


        #region PredictNext methods

        public IEnumerable<PossibleRange> PredictNext(bool increment = true)
        {
            return CollectNext(new[] { AllPossible }, increment).OrderByDescending(p => p.Probability);
        }

        public IEnumerable<PossibleRange> PredictNext(int maxValue, bool increment = true)
        {
            var predicted = PredictNext(increment);
            return predicted.Select(p => new PossibleRange(
                p.Probability,
                (int)((double)p.LowerBound * maxValue / Int32.MaxValue),
                (int)((double)p.UpperBound * maxValue / Int32.MaxValue)
            ));
        }

        public IEnumerable<WeightedValue> PredictWeightedNext(int maxValue, bool increment = true)
        {
            var ranges = PredictNext(increment).ToList();
            var weightedValues = new List<WeightedValue>();

            var bottomRange = ranges.SingleOrDefault(r => r.LowerBound == 0);
            var topRange = ranges.SingleOrDefault(r => r.UpperBound == Int32.MaxValue - 1);

            if (bottomRange != null && topRange != null)
            {
                ranges.Remove(bottomRange);
                ranges.Remove(topRange);
                weightedValues.AddRange(CreateWeightedValues(topRange.LowerBound, (long)bottomRange.UpperBound + Int32.MaxValue - 1, maxValue));
            }

            foreach(var range in ranges)
                weightedValues.AddRange(CreateWeightedValues(range.LowerBound, range.UpperBound, maxValue));

            return weightedValues.OrderBy(v => v.Weight);
        }

        #endregion


        #region Private/Protected Methods

        protected void IncrementIndexes()
        {
            LeadIndex++;
            LagIndex++;

            if (LeadIndex >= SeedArray.Length)
                LeadIndex = 0;

            if (LagIndex >= SeedArray.Length)
                LagIndex = 0;
        }


        protected IEnumerable<WeightedValue> CreateWeightedValues(long lowerBound, long upperBound, int maxValue)
        {
            var middleRaw = (upperBound + lowerBound) / 2;
            var range = upperBound - lowerBound + 1;

            var low = (int)((double)lowerBound * maxValue / Int32.MaxValue);
            var middle = (int) ((double)middleRaw * maxValue / Int32.MaxValue);
            var high = (int)((double)(upperBound - 1) * maxValue / Int32.MaxValue);

            var weightedValues = new List<WeightedValue> { new WeightedValue(0.0M, middle) };

            for (var i = middle - 1; i >= low; i--)
                weightedValues.Add(new WeightedValue(CalculateWeight(i + 1, lowerBound, upperBound, middleRaw, range, maxValue), i));

            for (var i = middle; i <= high; i--)
                weightedValues.Add(new WeightedValue(CalculateWeight(i, lowerBound, upperBound, middleRaw, range, maxValue), i));


            for (var i = middle + 1; i < weightedValues.Count; i++)
            {
                if(weightedValues[i].Value >= maxValue)
                    weightedValues[i] = new WeightedValue(weightedValues[i].Weight, weightedValues[i].Value - maxValue);
            }

            return weightedValues;
        }


        protected decimal CalculateWeight(int current, long lowRaw, long highRaw, long middleRaw, long rangeRaw, int maxValue)
        {
            var currentRaw = (int)((double)current / maxValue * Int32.MaxValue);
            var weight = (decimal) (currentRaw - middleRaw) / rangeRaw;

            if (weight < 0.0M)
                weight = -weight;

            if (currentRaw < lowRaw)
                weight /= ((decimal)lowRaw - currentRaw) * maxValue / Int32.MaxValue;

            if (currentRaw > highRaw)
                weight /= ((decimal)currentRaw - highRaw) * maxValue / Int32.MaxValue;

            return weight;
        }


        protected List<PossibleRange> SplitRanges(List<PossibleRange> ranges)
        {
            var result = new List<PossibleRange>();

            foreach (var range in ranges)
            {
                var lastUpperBound = range.LowerBound - 1;
                var increment = (double)range.Range / SPLIT_RANGES;

                for (var i = 1; i < SPLIT_RANGES; i++)
                {
                    var upperBound = (int)(i * increment) + range.LowerBound - 1;

                    result.Add(new PossibleRange(range.Probability / SPLIT_RANGES, lastUpperBound + 1, upperBound));
                    lastUpperBound = upperBound;
                }

                result.Add(new PossibleRange(range.Probability / SPLIT_RANGES, lastUpperBound + 1, range.UpperBound));
            }

            return result;
        } 


        protected IEnumerable<PossibleRange> CollectNext<TPossible>(IEnumerable<TPossible> possibleRanges, bool modify) where TPossible : PossibleRange
        {
            var newLaggers = new List<PossibleRange>();
            var newLeaders = new List<PossibleRange>();
            var possibles = possibleRanges.Select(x => (PossibleRange)x).ToList();
            var splitRange = false && (possibles.Count * SeedArray[LagIndex].Count * SeedArray[LeadIndex].Count * SPLIT_RANGES < 10000 
                && !possibles.Any(x => x.IsFullRange) && !SeedArray[LagIndex].Any(x => x.IsFullRange) && !SeedArray[LeadIndex].Any(x => x.IsFullRange));

            if (!splitRange)
                ;
            else if (SeedArray[LeadIndex].Count < SeedArray[LagIndex].Count)
                SeedArray[LeadIndex] = SplitRanges(SeedArray[LeadIndex]);
            else
                SeedArray[LagIndex] = SplitRanges(SeedArray[LagIndex]);


            foreach (var possible in possibles)
            {
                foreach (var lagger in SeedArray[LagIndex])
                {
                    foreach (var leader in SeedArray[LeadIndex])
                    {
                        var results = SubtractRangesWithExpected(lagger, leader, possible);

                        foreach (var result in results)
                        {
                            AddRange(newLaggers, result);
                            CombineRanges(newLeaders, SubtractRangesWithExpected(lagger, result, leader, updateProbability: false));
                        }
                    }
                }
            }

            if (!newLaggers.Any())
                throw new InvalidPredictorStateException(string.Format("Subtraction of LagIndex {0} and LeadIndex {1} does not produce any of the possible ranges.", LagIndex, LeadIndex));

            if (!newLeaders.Any())
                throw new ArgumentException(string.Format("LeadIndex {0} does not contain any values where when subtracted from LagIndex {1} and it produces the possible ranges.", LeadIndex, LagIndex));

            if (modify)
            {
                SeedArray[LagIndex] = newLaggers;
                SeedArray[LeadIndex] = newLeaders;
                IncrementIndexes();
            }

            return newLaggers;
        }


        protected IEnumerable<PossibleRange> SubtractRanges(PossibleRange lagRange, PossibleRange leadRange)
        {
            var upperBound = lagRange.UpperBound - leadRange.LowerBound;
            var lowerBound = lagRange.LowerBound - leadRange.UpperBound;
            var probability = lagRange.Probability * leadRange.Probability;

            if (upperBound >= 0 && lowerBound >= 0)
            {
                return new[] { new PossibleRange(probability, lowerBound, upperBound) };
            }

            if (upperBound < 0 && lowerBound < 0)
            {
                return new[]
                {
                    new PossibleRange
                    (
                        probability,
                        lowerBound + Int32.MaxValue,
                        upperBound + Int32.MaxValue
                    )
                };
            }

            if (upperBound >= 0 && lowerBound < 0)
            {
                if (probability == 1 && lowerBound + Int32.MaxValue <= upperBound + 1)
                {
                    return new[] { AllPossible };
                }

                decimal range = (decimal)upperBound - lowerBound + 1;
                var lowProbability = -lowerBound / range;
                var highProbability = (upperBound + 1) / range;

                return new[] 
                {
                    new PossibleRange
                    (
                        lowProbability,
                        lowerBound + Int32.MaxValue,
                        Int32.MaxValue
                    ),
                    new PossibleRange
                    (
                        highProbability,
                        0,
                        upperBound
                    )
                };
            }

            throw new ArgumentException(string.Format("UpperBound = {0} and LowerBound = {1} when subtracting [{2}, {3}] and [{4}, {5}].",
                upperBound, lowerBound, lagRange.LowerBound, lagRange.UpperBound, leadRange.LowerBound, leadRange.UpperBound));
        }

        protected IEnumerable<PossibleRange> SubtractRangesWithExpected(PossibleRange lagRange, PossibleRange leadRange,
            PossibleRange expectedRange, bool updateProbability = true)
        {
            var diff = SubtractRanges(lagRange, leadRange);
            return diff.Select(d => IntersectRanges(expectedRange, d, useNewProbability: !updateProbability)).Where(r => r != null);
        } 

        protected IEnumerable<PossibleRange> AddRanges(PossibleRange lagRange, PossibleRange leadRange)
        {
            PossibleRange negatedLeadRange = new PossibleRange
            (
                leadRange.Probability,
                leadRange.UpperBound == 0 ? 0 : Int32.MaxValue - leadRange.UpperBound,
                leadRange.LowerBound == 0 ? 0 : Int32.MaxValue - leadRange.LowerBound
            );

            return SubtractRanges(lagRange, negatedLeadRange);
        }

        protected PossibleRange IntersectRanges(PossibleRange newRange, PossibleRange oldRange, bool useNewProbability = false)
        {
            if (oldRange.UpperBound < newRange.LowerBound || oldRange.LowerBound > newRange.UpperBound)
                return null;

            return new PossibleRange
            (
                useNewProbability ? newRange.Probability : newRange.Probability * oldRange.Probability,
                Math.Max(oldRange.LowerBound, newRange.LowerBound),
                Math.Min(oldRange.UpperBound, newRange.UpperBound)
            );
        }

        protected void AddRange(List<PossibleRange> ranges, PossibleRange newRange, int start = 0)
        {
            for (int i = start, count = ranges.Count; i < count; i++)
            {
                if (!Overlap(newRange, ranges[i]))
                    continue;

                var combined = UnionRanges(newRange, ranges[i]);

                ranges.RemoveAt(i);

                foreach(var range in combined)
                    AddRange(ranges, range, i);

                return;
            }

            ranges.Add(newRange);
        }

        protected void CombineRanges(List<PossibleRange> ranges, IEnumerable<PossibleRange> newRanges)
        {
            foreach(var newRange in newRanges)
                AddRange(ranges, newRange);
        }

        protected bool Overlap(PossibleRange newRange, PossibleRange oldRange)
        {
            if (oldRange.UpperBound < newRange.LowerBound - 1 || oldRange.LowerBound > newRange.UpperBound + 1)
                return false;

            if (oldRange.UpperBound == newRange.LowerBound - 1 || oldRange.LowerBound == newRange.UpperBound + 1)
                return oldRange.Probability == newRange.Probability;

            return true;
        }

        protected IEnumerable<PossibleRange> UnionRanges(PossibleRange newRange, PossibleRange oldRange)
        {
            if (oldRange.UpperBound < newRange.LowerBound - 1 || oldRange.LowerBound > newRange.UpperBound + 1)
                return new[] { newRange, oldRange };

            if (oldRange.UpperBound == newRange.LowerBound - 1 || oldRange.LowerBound == newRange.UpperBound + 1)
            {
                if (oldRange.Probability == newRange.Probability)
                {
                    return new[]
                    {
                        new PossibleRange
                            (
                            newRange.Probability,
                            Math.Min(oldRange.LowerBound, newRange.LowerBound),
                            Math.Max(oldRange.UpperBound, newRange.UpperBound)
                            )
                    };
                }

                return new[] { newRange, oldRange };
            }

            if (oldRange.LowerBound == newRange.LowerBound && oldRange.UpperBound == newRange.UpperBound)
            {
                return new[]
                {
                    new PossibleRange
                    (
                        newRange.Probability + oldRange.Probability, 
                        oldRange.LowerBound,
                        oldRange.UpperBound
                    )
                };
            }

            var lowerRange = newRange;
            var upperRange = oldRange;

            if (lowerRange.LowerBound > upperRange.LowerBound || (lowerRange.LowerBound == upperRange.LowerBound && lowerRange.UpperBound > upperRange.UpperBound))
            {
                lowerRange = oldRange;
                upperRange = newRange;
            }

            var ranges = new List<PossibleRange>();

            // bottom section
            if (lowerRange.LowerBound != upperRange.LowerBound)
                ranges.Add(lowerRange.SubRange(lowerRange.LowerBound, upperRange.LowerBound - 1));

            // top section
            if (lowerRange.UpperBound > upperRange.UpperBound)
            {
                ranges.Add(lowerRange.SubRange(upperRange.UpperBound + 1, lowerRange.UpperBound));
            }
            else if (lowerRange.UpperBound < upperRange.UpperBound)
            {
                ranges.Add(upperRange.SubRange(lowerRange.UpperBound + 1, upperRange.UpperBound));
            }

            // overlapping section
            var overlapUpperBound = Math.Min(lowerRange.UpperBound, upperRange.UpperBound);
            var overlapProb = lowerRange.SubProbability(upperRange.LowerBound, overlapUpperBound) + upperRange.SubProbability(upperRange.LowerBound, overlapUpperBound);
            ranges.Add(new PossibleRange(overlapProb, upperRange.LowerBound, overlapUpperBound));

            return ranges;
        }

        #endregion
    }
}
