namespace Microsoft.Linq.Translations.Tests.Samples
{
    internal class SampleB : SampleBase
    {
        private static readonly CompiledExpression<SampleB, string> FullNameExpression =
            DefaultTranslationOf<SampleB>.Property(e => e.FullName).Is(e => "SampleB : " + e.FirstName + " " + e.LastName);

        internal override string FullName => FullNameExpression.Evaluate(this);
    }
}
