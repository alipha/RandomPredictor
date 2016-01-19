using System;

namespace RandomPredictor
{
    public class InvalidPredictorStateException : ArgumentException
    {
        public InvalidPredictorStateException(string message) : base(message) { }
    }
}
