namespace Microsoft.Linq.Translations.Tests.Samples
{
    /// <summary>
    ///     A test class
    /// </summary>
    internal abstract class SampleBase
    {
        internal string FirstName { get; set; }
        internal string LastName { get; set; }
        internal abstract string FullName { get; }
    }
}