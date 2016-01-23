using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordPredictor.RandomGenerators
{
    public class PredefinedRandom : IRandom
    {
        private IEnumerator<int> _values;


        public PredefinedRandom(IEnumerable<int> values)
        {
            _values = values.GetEnumerator();
        }

        public int Next(int maxValue)
        {
            if(!_values.MoveNext())
                throw new IndexOutOfRangeException(string.Format("There are no more values in the value list."));

            var value = _values.Current;

            if(value >= maxValue)
                throw new ArgumentOutOfRangeException(string.Format("The value {0} returned from the value list is greater than the maxValue {1}.", value, maxValue));

            return value;
        }
    }
}
