using System;
using System.Linq;
using RandomPredictor;

namespace PasswordPredictor
{
    public class RandomAuditor
    {
        public Random Random { get; set; }

        public SubGenPredictor Predictor { get; set; }

        public bool DebugOutput { get; set; }


        public RandomAuditor(Random random, SubGenPredictor predictor)
        {
            Random = random;
            Predictor = predictor;
            DebugOutput = false;
        }

        public int Next(int maxValue)
        {
            var step = (double)maxValue / Int32.MaxValue;
            var value = Random.Next(maxValue);

            if (DebugOutput)
            {
                //var rawPredicted = string.Join(", ", Predictor.PredictNext(increment: false)
                //    .Select(p => $"[{p.LowerBound * step:F2}, {p.UpperBound * step:F2}]\t{((double)p.LowerBound + p.UpperBound) / 2 * step:F2}"));
                var p = Predictor.PredictNext(increment: false).First();
                var rawPredicted = string.Format("{0:F2}% [{1:F2}, {2:F2}]\t{3:F2}", p.Probability*100, p.LowerBound * step, p.UpperBound * step, ((double)p.LowerBound + p.UpperBound) / 2 * step);
                Console.WriteLine(string.Format("{0}\t{1}", value, rawPredicted));
            }

            Predictor.CollectNext(value, maxValue);
            return value;
        }
    }
}
