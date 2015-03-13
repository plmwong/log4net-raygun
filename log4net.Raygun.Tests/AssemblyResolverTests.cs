using log4net.Raygun.Core;
using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class AssemblyResolverTests
    {
        [Test]
        public void WhenHttpContextIsNotAvailableThenGetAssemblyFromCurrentExecutingAssembly()
        {
            var fakeAssemblyLoader = new FakeAssemblyLoader(GetType().Assembly);
            var assemblyResolver = new AssemblyResolver(fakeAssemblyLoader);

            var resolvedAssembly = assemblyResolver.GetApplicationAssembly();

            Assert.That(resolvedAssembly, Is.EqualTo(GetType().Assembly));
        }
    }
}