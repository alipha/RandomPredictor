using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordPredictor
{
    public class Password
    {
        private static string _allChars = SpecialChars + UpperCaseChars + LowerCaseChars + NumberChars;


        public static string UpperCaseChars { get { return "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; } }

        public static string LowerCaseChars { get { return "abcdefghijklmnopqrstuvwxyz"; } }

        public static string NumberChars { get { return "0123456789"; } }

        public static string SpecialChars { get { return @"!@#$%*()'+,-./:;=?[\]^_`{|}~"; } }

        public static string AllChars { get { return _allChars; } }


        private string _text;
        private List<Tuple<int, int>> _upperIndexes;
        private List<Tuple<int, int>> _lowerIndexes;
        private List<Tuple<int, int>> _numericIndexes;
        private List<Tuple<int, int>> _specialIndexes;


        public Password(string text)
        {
            _text = text;

            _upperIndexes = new List<Tuple<int, int>>();
            _lowerIndexes = new List<Tuple<int, int>>();
            _numericIndexes = new List<Tuple<int, int>>();
            _specialIndexes = new List<Tuple<int, int>>();

            for(var i = 0; i < text.Length; i++)
            {
                if(Char.IsUpper(text[i]))
                    UpperIndexes.Add(Tuple.Create(i, UpperCaseChars.IndexOf(text[i])));
                else if (Char.IsLower(text[i]))
                    LowerIndexes.Add(Tuple.Create(i, LowerCaseChars.IndexOf(text[i])));
                else if (Char.IsDigit(text[i]))
                    NumericIndexes.Add(Tuple.Create(i, NumberChars.IndexOf(text[i])));
                else
                    SpecialIndexes.Add(Tuple.Create(i, SpecialChars.IndexOf(text[i])));
            }
        }

        public string Text { get { return _text; } }

        public List<Tuple<int, int>> UpperIndexes { get { return _upperIndexes; } }

        public List<Tuple<int, int>> LowerIndexes { get { return _lowerIndexes; } }

        public List<Tuple<int, int>> NumericIndexes { get { return _numericIndexes; } }

        public List<Tuple<int, int>> SpecialIndexes { get { return _specialIndexes; } }
    }
}
