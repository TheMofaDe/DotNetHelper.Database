namespace DotNetHelper.Database.Helper
{
    internal static class AlphabetHelper
    {
        public enum CaseType
        {
            Upper,
            Lower,
        }

        public static char GetNextLetter(char letter, CaseType type = CaseType.Upper)
        {
            char nextChar;
            switch (letter)
            {
                case 'z':
                    nextChar = 'a';
                    break;
                case 'Z':
                    nextChar = 'A';
                    break;
                default:
                    nextChar = (char)(letter + 1);
                    break;
            }

            return type == CaseType.Upper ? char.ToUpper(nextChar) : char.ToLower(nextChar);
        }
        public static char GetPreviousLetter(char letter, CaseType type = CaseType.Upper)
        {
            char nextChar;
            switch (letter)
            {
                case 'a':
                    nextChar = 'z';
                    break;
                case 'A':
                    nextChar = 'Z';
                    break;
                default:
                    nextChar = (char)(letter - 1);
                    break;
            }
            return type == CaseType.Upper ? char.ToUpper(nextChar) : char.ToLower(nextChar);
        }
    }
}
