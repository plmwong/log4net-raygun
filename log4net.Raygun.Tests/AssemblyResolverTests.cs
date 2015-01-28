using log4net.Raygun.Core;
using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class AssemblyResolverTests
    {
        [Test]
        public void WhenHttpContextIsAvailableGetAssemblyFromApplicationInstanceAssembly()
        {
            var fakeHttpContext = FakeHttpContext.For(new FakeHttpApplication());
            var assemblyResolver = new AssemblyResolver(fakeHttpContext, null);

            var resolvedAssembly = assemblyResolver.GetApplicationAssembly();
            var fakeHttpApplicationAssembly = fakeHttpContext.ApplicationInstance.GetType().Assembly;

            Assert.That(resolvedAssembly, Is.EqualTo(fakeHttpApplicationAssembly));
        }

        [Test]
        public void WhenHttpContextIsNotAvailableThenGetAssemblyFromCurrentExecutingAssembly()
        {
            var fakeAssemblyLoader = new FakeAssemblyLoader(GetType().Assembly);
            var assemblyResolver = new AssemblyResolver(null, fakeAssemblyLoader);

            var resolvedAssembly = assemblyResolver.GetApplicationAssembly();

            Assert.That(resolvedAssembly, Is.EqualTo(GetType().Assembly));
        }
    }
}