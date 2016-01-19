using System;
using System.Collections.Generic;
using RandomPredictor;

namespace PasswordPredictor
{
    public class State
    {
        public static bool Log = false;
        public static int ImpossibleStateCount = 0;

        private List<Password> _observedPasswords;
        private int _observedPasswordIndex;
        private Password _currentPassword;

        private State _parent;
        private List<State> _children;


        public State(List<Password> passwords, State prevSibling)
        {
            _observedPasswords = passwords;
            _currentPassword = passwords[0];
            _children = new List<State>();
            //GuaranteedUpperIndex = GuaranteedLowerIndex = GuaranteedNumericIndex = GuaranteedSpecialIndex = -1;

            Initialize(prevSibling);
        }

        public State(State parent, State prevSibling)
        {
            if (parent != null)
            {
                _parent = parent;
                _observedPasswords = parent.ObservedPasswords;
                _observedPasswordIndex = parent.ObservedPasswordIndex + 1;
                _currentPassword = ObservedPasswords[ObservedPasswordIndex];
            }
            else
            {
                _observedPasswords = prevSibling.ObservedPasswords;
                _currentPassword = ObservedPasswords[0];
            }

            _children = new List<State>();

            Initialize(prevSibling);
        }

        public SubGenPredictor Predictor { get; set; }

        public bool ImpossibleState { get; set; }


        public List<Password> ObservedPasswords { get { return _observedPasswords; } }

        public int ObservedPasswordIndex { get { return _observedPasswordIndex; } }

        public Password CurrentPassword { get { return _currentPassword; } }


        public int GuaranteedUpperIndex { get; set; }

        public int GuaranteedLowerIndex { get; set; }

        public int GuaranteedNumericIndex { get; set; }

        public int GuaranteedSpecialIndex { get; set; }


        public State Parent { get { return _parent; } }

        public List<State> Children { get { return _children; } }


        public State NextSibling()
        {
            var next = new State(Parent, this);

            if (next.GuaranteedUpperIndex >= 0)
                return next;

            return null;
        }


        public void Transverse(int childIndex, Action<State, int> action)
        {
            action(this, childIndex);

            childIndex = 0;

            foreach (var child in Children)
            {
                child.Transverse(childIndex, action);
                childIndex++;
            }
        }


        protected void Initialize(State prevSibling)
        {
            if (prevSibling != null)
            {
                GuaranteedUpperIndex = prevSibling.GuaranteedUpperIndex;
                GuaranteedLowerIndex = prevSibling.GuaranteedLowerIndex;
                GuaranteedNumericIndex = prevSibling.GuaranteedNumericIndex;
                GuaranteedSpecialIndex = prevSibling.GuaranteedSpecialIndex + 1;

                SetGuaranteed();
            }

            if (GuaranteedUpperIndex >= 0)
                UpdatePredictor();
        }


        protected void UpdatePredictor()
        {
            try
            {
                Predictor = Parent != null ? new SubGenPredictor(Parent.Predictor) : new SubGenPredictor();

                var special = CurrentPassword.SpecialIndexes[GuaranteedSpecialIndex];
                Predictor.CollectNext(special.Item1, CurrentPassword.Text.Length);
                Predictor.CollectNext(special.Item2, Password.SpecialChars.Length);
                if(Log) Console.WriteLine(string.Format("{2} special: {0}, {1}", special.Item1, special.Item2, GuaranteedSpecialIndex));

                var upper = CurrentPassword.UpperIndexes[GuaranteedUpperIndex];
                var upperIndex = upper.Item1;

                if (upper.Item1 > special.Item1)
                    upperIndex--;

                Predictor.CollectNext(upperIndex, CurrentPassword.Text.Length - 1);
                Predictor.CollectNext(upper.Item2, Password.UpperCaseChars.Length);
                if(Log) Console.WriteLine(string.Format("{2} upper: {0}, {1}", upperIndex, upper.Item2, GuaranteedUpperIndex));


                var lower = CurrentPassword.LowerIndexes[GuaranteedLowerIndex];
                var lowerIndex = lower.Item1;

                if (lower.Item1 > special.Item1)
                    lowerIndex--;

                if (lower.Item1 > upper.Item1)
                    lowerIndex--;

                Predictor.CollectNext(lowerIndex, CurrentPassword.Text.Length - 2);
                Predictor.CollectNext(lower.Item2, Password.LowerCaseChars.Length);
                if(Log) Console.WriteLine(string.Format("{2} lower: {0}, {1}", lowerIndex, lower.Item2, GuaranteedLowerIndex));


                var numeric = CurrentPassword.NumericIndexes[GuaranteedNumericIndex];
                var numericIndex = numeric.Item1;

                if (numeric.Item1 > special.Item1)
                    numericIndex--;

                if (numeric.Item1 > upper.Item1)
                    numericIndex--;

                if (numeric.Item1 > lower.Item1)
                    numericIndex--;

                Predictor.CollectNext(numericIndex, CurrentPassword.Text.Length - 3);
                Predictor.CollectNext(numeric.Item2, Password.NumberChars.Length);
                if(Log) Console.WriteLine(string.Format("{2} numeric: {0}, {1}", numericIndex, numeric.Item2, GuaranteedNumericIndex));


                for (var i = 0; i < CurrentPassword.Text.Length; i++)
                {
                    if (i != upper.Item1 && i != lower.Item1 && i != numeric.Item1 && i != special.Item1)
                    {
                        var index = Password.AllChars.IndexOf(CurrentPassword.Text[i]);
                        if(Log) Console.WriteLine(index);
                        Predictor.CollectNext(index, Password.AllChars.Length);
                    }
                }
            }
            catch (InvalidPredictorStateException)
            {
                if(Log) Console.WriteLine("impossible");
                ImpossibleState = true;
                ImpossibleStateCount++;
            }
        }

        protected void SetGuaranteed()
        {
            if (GuaranteedSpecialIndex < CurrentPassword.SpecialIndexes.Count)
                return;

            GuaranteedSpecialIndex = 0;
            GuaranteedNumericIndex++;

            if (GuaranteedNumericIndex < CurrentPassword.NumericIndexes.Count)
                return;

            GuaranteedNumericIndex = 0;
            GuaranteedLowerIndex++;

            if (GuaranteedLowerIndex < CurrentPassword.LowerIndexes.Count)
                return;

            GuaranteedLowerIndex = 0;
            GuaranteedUpperIndex++;

            if (GuaranteedUpperIndex < CurrentPassword.UpperIndexes.Count)
                return;

            GuaranteedUpperIndex = -1;
        }
    }
}
