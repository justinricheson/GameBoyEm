using System;

namespace GameBoyEm
{
    public static class Throw
    {
        public static void IfNotEqual(object first, object second, string argumentName)
        {
            if (!ReferenceEquals(first, second))
            {
                throw new ArgumentException(argumentName);
            }
        }
    }
}
