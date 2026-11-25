using System.Runtime.CompilerServices;

namespace NCalc.Helpers
{
    internal static class BoxedValues
    {
        public static readonly object True = true;
        public static readonly object False = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(bool value) => value ? True : False;
    }
}
