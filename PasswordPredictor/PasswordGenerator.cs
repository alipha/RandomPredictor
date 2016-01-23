using System;
using PasswordPredictor.RandomGenerators;

namespace PasswordPredictor
{
    public class PasswordGenerator
    {
        private IRandom random;

        public PasswordGenerator(IRandom random)
        {
            this.random = random;
        }

        public string GeneratePassword()
        {
            const int passwordLength = 8;
            int index;
            
            var chars = new char[passwordLength];

            index = GetAvailablePosition(chars, passwordLength);
            chars[index] = Password.SpecialChars[random.Next(Password.SpecialChars.Length)];

            index = GetAvailablePosition(chars, passwordLength - 1);
            chars[index] = Password.UpperCaseChars[random.Next(Password.UpperCaseChars.Length)];

            index = GetAvailablePosition(chars, passwordLength - 2);
            chars[index] = Password.LowerCaseChars[random.Next(Password.LowerCaseChars.Length)];

            index = GetAvailablePosition(chars, passwordLength - 3);
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
