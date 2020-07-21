using NUnit.Framework;

using SimpleInjector;

namespace ComplexUnitOfWork.Tests.DependencyInjection
{
    [TestFixture]
    public class SimpleInjectorConfiguratorTests : TestBase
    {
        [Test]
        public void DiContainerTest()
        {
            var container = (Container)ServiceProvider;
            Assert.DoesNotThrow(() => container.Verify());
        }
    }
}
