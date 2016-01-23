using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordPredictor.RandomGenerators
{
    public class SystemRandom : IRandom
    {
        private Random _random;

        public SystemRandom(Random random)
        {
            _random = random;
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }
    }
}
