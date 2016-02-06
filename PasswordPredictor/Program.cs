using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PasswordPredictor.PasswordGenerators;
using RandomPredictor;
using PasswordPredictor.RandomGenerators;

namespace PasswordPredictor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var predictor = new SubGenPredictor();
            int passwordLength = 0;

            Console.WriteLine("Enter in 6 passwords:");

            for (var i = 0; i < 6; i++)
            {
                var password = Console.ReadLine();
                passwordLength = password.Length;
                
                RecordPassword(predictor, password);
            }


            var possibleOutputs = new List<WeightedValue>[passwordLength];

            for (var i = 0; i < passwordLength; i++)
                possibleOutputs[i] = predictor.PredictWeightedNext(Password.AllChars.Length).ToList();

            List<string> possiblePasswords = GetPossiblePasswords(possibleOutputs, new int[passwordLength], 0);
            File.WriteAllLines("passwords.txt", possiblePasswords);

            Console.WriteLine($"{possiblePasswords.Count} possible next passwords written to passwords.txt");
            Console.ReadLine();
        }

        private static void RecordPassword(SubGenPredictor predictor, string password)
        {
            foreach (char ch in password)
            {
                var index = Password.AllChars.IndexOf(ch);
                predictor.CollectNext(index, Password.AllChars.Length);
            }
        }

        private static List<string> GetPossiblePasswords(List<WeightedValue>[] possibleOutputs, int[] currentIndexes, int depth)
        {
            var password = "";
            var passwords = new List<string>();
            var indexes = (int[])currentIndexes.Clone();

            if (depth == possibleOutputs.Length - 1)
            {
                for (var i = 0; i < possibleOutputs.Length - 1; i++)
                    password += Password.AllChars[possibleOutputs[i][indexes[i]].Value];
            }

            for(var i = 0; i < possibleOutputs[depth].Count; i++)
            {
                if (depth == possibleOutputs.Length - 1)
                {
                    passwords.Add(password + Password.AllChars[possibleOutputs[depth][i].Value]);
                }
                else
                {
                    indexes[depth] = i;
                    passwords.AddRange(GetPossiblePasswords(possibleOutputs, indexes, depth + 1));
                }
            }

            return passwords;
        }



        #region Old

        private static int stateCount = 0;


        private static IPasswordGenerator CreatePasswordGenerator(IRandom random)
        {
            return new GuaranteedCharPasswordGenerator(random);
        }

        private static void GetPassword(IPasswordGenerator generator, List<Password> passwords)
        {
            string password = generator.GeneratePassword();
            Console.WriteLine(password);
            passwords.Add(new Password(password));
        }



        public static void Test5()
        {
            var generator = new SimplePasswordGenerator(new SystemRandom(new Random()));
            var predictor = new SubGenPredictor();
            int passwordLength = 0;

            Console.WriteLine("Generating passwords...");

            for (var i = 0; i < 6; i++)
            {
                var password = generator.GeneratePassword();
                passwordLength = password.Length;
                Console.WriteLine(password);

                RecordPassword(predictor, password);
            }


            var possibleOutputs = new List<WeightedValue>[passwordLength];

            for (var i = 0; i < passwordLength; i++)
                possibleOutputs[i] = predictor.PredictWeightedNext(Password.AllChars.Length).ToList();

            List<string> possiblePasswords = GetPossiblePasswords(possibleOutputs, new int[passwordLength], 0);
            File.WriteAllLines("passwords.txt", possiblePasswords);

            Console.WriteLine($"{possiblePasswords.Count} possible next passwords written to passwords.txt");
            Console.WriteLine("Next password:");

            var nextPassword = generator.GeneratePassword();
            Console.WriteLine(nextPassword);

            int index = possiblePasswords.IndexOf(nextPassword);

            if (index == -1)
                Console.WriteLine("This password was not found in the list.");
            else
                Console.WriteLine($"Password was found at index {index} in the list.");

            Console.ReadLine();
        }


        private static void Test4()
        {
            var auditor = new RandomAuditor(new Random(50), new SubGenPredictor());
            var generator = CreatePasswordGenerator(auditor);
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

            /*
            var predictor = new SubGenPredictor(currentStates[0].Predictor);
            var possibleRandomOutputs = new List<WeightedValue>[generator.PasswordLength + 4];
            possibleRandomOutputs[0] = predictor.PredictWeightedNext(generator.PasswordLength).ToList();
            possibleRandomOutputs[1] = predictor.PredictWeightedNext(Password.SpecialChars.Length).ToList();
            possibleRandomOutputs[2] = predictor.PredictWeightedNext(generator.PasswordLength - 1).ToList();
            possibleRandomOutputs[3] = predictor.PredictWeightedNext(Password.UpperCaseChars.Length).ToList();
            possibleRandomOutputs[4] = predictor.PredictWeightedNext(generator.PasswordLength - 2).ToList();
            possibleRandomOutputs[5] = predictor.PredictWeightedNext(Password.LowerCaseChars.Length).ToList();
            possibleRandomOutputs[6] = predictor.PredictWeightedNext(generator.PasswordLength - 3).ToList();
            possibleRandomOutputs[7] = predictor.PredictWeightedNext(Password.NumberChars.Length).ToList();

            for(var i = 0; i < generator.PasswordLength - 4; i++)
                possibleRandomOutputs[i + 8] = predictor.PredictWeightedNext(Password.AllChars.Length).ToList();
            */

            while (true)
            {
                auditor.Predictor = currentStates[0].Predictor;
                auditor.DebugOutput = true;
                GetPassword(generator, passwords);
                Console.ReadLine();
            }
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
            var generator = CreatePasswordGenerator(auditor);

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

        #endregion
    }
}
