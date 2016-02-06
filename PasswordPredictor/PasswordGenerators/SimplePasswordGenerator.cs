using PasswordPredictor.RandomGenerators;

namespace PasswordPredictor.PasswordGenerators
{
    public class SimplePasswordGenerator : IPasswordGenerator
    {
        private IRandom random;

        public int PasswordLength { get; set; }


        public SimplePasswordGenerator(IRandom random)
        {
            this.random = random;
            PasswordLength = 10;
        }

        public string GeneratePassword()
        {
            var chars = new char[PasswordLength];

            for (int i = 0; i < chars.Length; i++)
                chars[i] = Password.AllChars[random.Next(Password.AllChars.Length)];

            return new string(chars);
        }
    }
}
