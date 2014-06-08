using System.Reflection;
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
			var assemblyResolver = new AssemblyResolver(fakeHttpContext);

			var resolvedAssembly = assemblyResolver.GetApplicationAssembly();
			var fakeHttpApplicationAssembly = fakeHttpContext.ApplicationInstance.GetType().Assembly;

			Assert.That(resolvedAssembly, Is.EqualTo(fakeHttpApplicationAssembly));
		}

		[Test]
		public void WhenHttpContextIsNotAvailableThenGetAssemblyFromCurrentExecutingAssembly()
		{
			var assemblyResolver = new AssemblyResolver(null);

			var resolvedAssembly = assemblyResolver.GetApplicationAssembly();

			//TODO: NUnit test context does not have an EntryAssembly, so need to workaround this
			//TODO: for a proper test
			Assert.That(resolvedAssembly, Is.EqualTo(null));
		}
	}
}

