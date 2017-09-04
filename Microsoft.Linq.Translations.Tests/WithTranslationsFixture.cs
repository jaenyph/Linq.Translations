using System.Linq;
using Microsoft.Linq.Translations.Tests.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Linq.Translations.Tests
{
    [TestClass]
    public class WithTranslationsFixture
    {
        [TestMethod]
        public void TestMethod1()
        {
            var samples = new SampleBase[] {new SampleA(), new SampleB()}.AsQueryable().Select(e => new ProjectionSample{FullName = e.FullName});

            samples = samples.WithTranslations();

            var map = TranslationMap.DefaultMap;

            Assert.IsNotNull(map);

            Assert.IsTrue(map.ToList().Count == 2);
        }
    }
}