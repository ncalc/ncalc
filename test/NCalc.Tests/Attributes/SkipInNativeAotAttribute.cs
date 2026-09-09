namespace NCalc.Tests.Attributes
{
    public class SkipInNativeAotAttribute : SkipAttribute
    {
        public SkipInNativeAotAttribute() : base("Reflection is not supported under NativeAOT.")
        {
        }

        public override Task<bool> ShouldSkip(TestRegisteredContext context)
        {
            bool isAot = !System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled;
            return Task.FromResult(isAot);
        }
    }
}
