using System;
using System.Collections.Generic;
using System.Linq;
using RandomPredictor;
using PasswordPredictor.RandomGenerators;

namespace PasswordPredictor
{
    public class Program
    {
        private static int stateCount = 0;


        public static void Main(string[] args)
        {
            var auditor = new RandomAuditor(new Random(50), new SubGenPredictor());
            var generator = new PasswordGenerator(auditor);
            //var generator = new PasswordGenerator(new Random(50));

            var passwords = new List<Password>();
            var currentStates = new List<State>();
            var passwordCount = 0;

            GetPassword(generator, passwords);
            AddSiblings(currentStates, new State(passwords, null));

            while (currentStates.Count > 1)
            {
                passwordCount++;
                Console.WriteLine(string.Format("Observed: {0}\tPossible states: {1}", passwordCount, currentStates.Count));
                //Console.ReadLine();

                GetPassword(generator, passwords);
                var newStates = new List<State>();

                foreach (var state in currentStates)
                    AddSiblings(newStates, new State(state, null));

                currentStates = newStates;
            }

            while (true)
            {
                auditor.Predictor = currentStates[0].Predictor;
                auditor.DebugOutput = true;
                GetPassword(generator, passwords);
                Console.ReadLine();
            }
        }


        private static void GetPassword(PasswordGenerator generator, List<Password> passwords)
        {
            string password = generator.GeneratePassword();
            Console.WriteLine(password);
            passwords.Add(new Password(password));
        }


        private static void AddSiblings(List<State> states, State state)
        {
            if(!state.ImpossibleState)
                states.Add(state);

            while ((state = state.NextSibling()) != null)
            {
                if(!state.ImpossibleState)
                    states.Add(state);
            }
        }


        private static void AddChildren(State state, int level)
        {
            if (level > 1)
            {
                foreach (var ch in state.Children)
                    AddChildren(ch, level - 1);
                return;
            }

            var child = new State(state, null);
            stateCount++;
            if(!child.ImpossibleState)
                state.Children.Add(child);

            while ((child = child.NextSibling()) != null)
            {
                stateCount++;
                if(!child.ImpossibleState)
                    state.Children.Add(child);
            }
        }


        private static void Test3()
        {
            var auditor = new RandomAuditor(new Random(73), new SubGenPredictor());
            var generator = new PasswordGenerator(auditor);

            for (var i = 0; i < 4; i++)
            {
                Console.WriteLine(string.Format("{0}\t{1}", i, generator.GeneratePassword()));
                //Console.ReadLine();
            }

            auditor.DebugOutput = true;
            Console.WriteLine(generator.GeneratePassword());
            Console.ReadLine();
        }

        private static void Test2()
        { 
            var random = new Random(42);
            var predictor = new SubGenPredictor();
            var max = 10;
            var step = (double)max / Int32.MaxValue;

            for (var i = 0; i < 55; ++i)
            {
                var value = random.Next(max);
                predictor.CollectNext(value, max);
                var lastLaggers = string.Join(", ", predictor.LastLaggers);
                var lastLeaders = string.Join(", ", predictor.LastLeaders);
                Console.WriteLine(string.Format("{0}:{1}\t{2}\t{3}", i, value, lastLaggers, lastLeaders));
            }

            for (var i = 0; i < 110; ++i)
            {
                var value = random.Next(max);
                var rawPredicted = string.Join(", ", predictor.PredictNext(increment: false));
                    //.Select(p => $"[{p.LowerBound * step:F2}, {p.UpperBound * step:F2}]"));
                var predicted = predictor.PredictNext(max, increment: false).ToList();
                var predictedStr = string.Join(", ", predicted);
                Console.WriteLine(string.Format("{0}:{1}\t{2}\t{3}", i, value == predicted.First().LowerBound, value, predictedStr)); //\t{rawPredicted}");
                //Console.WriteLine($"{i}\t{value}\t{rawPredicted}");
                predictor.CollectNext(value, max);
            }

            Console.ReadLine();
        }

        private static void Test()
        { 
            var random = new Random(42);
            var random2 = new Random(43);
            var predictor = new SubGenPredictor();


            for (var i = 0; i < 54; ++i)
            {
                var value = random.Next();
                var value2 = random2.Next();

                predictor.CollectNext(new[]
                {
                    new PossibleValue ( .5M, value ),
                    new PossibleValue ( .5M, value2 )
                });
                Console.WriteLine(string.Format("{0}\t{1}", value, value2));
            }

            Console.WriteLine(random.Next());
            predictor.Skip();

            for (var i = 0; i < 115; ++i)
            {
                var value = random.Next();
                var predicted = predictor.PredictNext(increment: false).ToList();
                var predictedStr = string.Join(", ", predicted);
                Console.WriteLine(string.Format("{0}\t{1}\t{2}", value == predicted.First().LowerBound, value, predictedStr));

                predictor.CollectNext(value);
            }

            Console.ReadLine();
        }
    }
}
