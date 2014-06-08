using System;
using System.Reflection;

namespace log4net.Raygun.Tests.Fakes
{
	public class FakeAppDomainManager : AppDomainManager
	{
		private readonly Assembly _entryAssembly;

		public FakeAppDomainManager(Assembly entryAssembly)
		{
			_entryAssembly = entryAssembly;
		}

		public override Assembly EntryAssembly
		{
			get
			{
				return _entryAssembly;
			}
		}
	}
}
