using System.Linq;

namespace NCalc.Playground.Helpers;

public static class IdentifierValidator
{
    public static bool IsValid(string value)
    {
        if (value.Length == 0 || !(char.IsLetter(value[0]) || value[0] == '_'))
            return false;

        return value.Skip(1).All(character => char.IsLetterOrDigit(character) || character == '_');
    }
}
