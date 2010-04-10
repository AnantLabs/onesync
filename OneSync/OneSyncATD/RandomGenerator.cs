using System;
using System.Text;

namespace OneSyncATD
{
    public static class RandomGenerator
    {
        /****************************************************
         * Notes:
         * On Windows-based platforms, paths must be less
         * than 248 characters, and file names must be less
         * than 260 characters.
         *****************************************************/

        public enum NameType
        {
            Letters,
            LettersAndSymbols,
            Unicode
        };


        // For generating random numbers
        public static Random random = new Random();

        // Character set
        private static char[] symbolChars = new char[] {
            ' ', '_', '(', ')', '#', '@', '+', '-'
        };


        public static string GetFilename(NameType type)
        {
            return string.Format("{0}.{1}{2}{3}",
                                 GetDirectoryName(type),
                                 GetLowerCaseChar(), GetLowerCaseChar(), GetLowerCaseChar());
        }

        public static string GetDirectoryName(NameType type)
        {
            // Generate the length of Directory/Folder name
            int len = random.Next(0, 6) + random.Next(1, 7);

            if (type == NameType.Letters)
                return GetString(len, type);
            else if (type == NameType.LettersAndSymbols)
                return GetString(len, type);
            else if (type == NameType.Unicode)
                return GetUnicodeString(len);

            return null;
        }

        public static int GetFileSize(int min, int max)
        {
            if (max < min) throw new ArgumentException("max must not be less than min");

            return random.Next(min, max);
        }


        #region private methods

        private static char GetLowerCaseChar()
        {
            return (char)('a' + random.Next(0, 26));
        }

        private static char GetUpperCaseChar()
        {
            return (char)('A' + random.Next(0, 26));
        }

        private static char GetLetter()
        {
            if (random.Next(0, 2) == 0) return GetLowerCaseChar();
            return GetUpperCaseChar();
        }

        private static char GetLetterOrSymbol()
        {
            int rnd = random.Next(0, symbolChars.Length + 52);

            if (rnd < symbolChars.Length)
                return symbolChars[rnd];
            else
                return GetLetter();
        }

        private static string GetString(int length, NameType type)
        {
            if (type == NameType.Unicode) return GetUnicodeString(length);

            StringBuilder sb = new StringBuilder(length);

            if (type == NameType.Letters)
            {
                // First character is uppercase
                sb.Append(GetUpperCaseChar());

                // Subsequent characters are lowercase
                while (--length > 0)
                    sb.Append(GetLowerCaseChar());

            }
            else if (type == NameType.LettersAndSymbols)
            {
                // Ensure first character is not a 'space' (or symbol).
                sb.Append(GetLetter());

                while (--length > 1)
                    sb.Append(GetLetterOrSymbol());

                // Ensure last character is not a 'space'.
                sb.Append(GetLetter());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a random unicode string with specified length
        /// </summary>
        private static string GetUnicodeString(int length)
        {
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                char ch = Convert.ToChar(random.Next(UInt16.MaxValue));
                sb.Append(ch);
            }
            return sb.ToString();
        }

        #endregion

        
    }
}
