using PasswordPredictor.RandomGenerators;

namespace PasswordPredictor.PasswordGenerators
{
    public class GuaranteedCharPasswordGenerator : IPasswordGenerator
    {
        private IRandom random;

        public int PasswordLength { get; set; }


        public GuaranteedCharPasswordGenerator(IRandom random)
        {
            this.random = random;
            PasswordLength = 8;
        }

        public string GeneratePassword()
        {
            var chars = new char[PasswordLength];

            var index = GetAvailablePosition(chars, chars.Length);
            chars[index] = Password.SpecialChars[random.Next(Password.SpecialChars.Length)];

            index = GetAvailablePosition(chars, chars.Length - 1);
            chars[index] = Password.UpperCaseChars[random.Next(Password.UpperCaseChars.Length)];

            index = GetAvailablePosition(chars, chars.Length - 2);
            chars[index] = Password.LowerCaseChars[random.Next(Password.LowerCaseChars.Length)];

            index = GetAvailablePosition(chars, chars.Length - 3);
            chars[index] = Password.NumberChars[random.Next(Password.NumberChars.Length)];

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\0')
                    chars[i] = Password.AllChars[random.Next(Password.AllChars.Length)];
            }

            return new string(chars);
        }

        private int GetAvailablePosition(char[] chars, int availableCount)
        {
            int targetAvailableIndex = random.Next(availableCount);
            int availableIndex = -1;
            int arrayIndex = 0;

            while (true)
            {
                if (chars[arrayIndex] == '\0')
                {
                    availableIndex++;

                    if (availableIndex == targetAvailableIndex)
                        return arrayIndex;
                }

                arrayIndex++;
            }
        }
    }
}
