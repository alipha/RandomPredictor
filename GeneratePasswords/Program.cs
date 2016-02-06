using System;
using PasswordPredictor.PasswordGenerators;
using PasswordPredictor.RandomGenerators;

namespace GeneratePasswords
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new SimplePasswordGenerator(new SystemRandom(new Random()));

            Console.WriteLine("Generating 6 passwords...");

            for (var i = 0; i < 6; i++)
                Console.WriteLine(generator.GeneratePassword());

            Console.WriteLine("Press enter to generate the 7th.");
            Console.ReadLine();

            Console.WriteLine(generator.GeneratePassword());
            Console.ReadLine();
        }
    }
}
