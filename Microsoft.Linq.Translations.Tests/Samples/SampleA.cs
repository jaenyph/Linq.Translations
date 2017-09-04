namespace Microsoft.Linq.Translations.Tests.Samples
{
    internal class SampleA : SampleBase
    {
        private static readonly CompiledExpression<SampleA, string> FullNameExpression =
            DefaultTranslationOf<SampleA>.Property(e => e.FullName).Is(e => "SampleA : " + e.FirstName + " " + e.LastName);

        internal override string FullName => FullNameExpression.Evaluate(this);
    }
}
