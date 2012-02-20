using System.Linq;
using NUnit.Framework;
using Ormongo.Plugins;
using Ormongo.Plugins.Ancestry;

namespace Ormongo.Tests.Plugins
{
	[TestFixture]
	public class PluginManagerTests
	{
		[Test]
		public void CanFindPlugins()
		{
			// Act.
			var plugins = PluginManager.FindPlugins().ToList();

			// Assert.
			Assert.That(plugins, Has.Count.EqualTo(2));
			Assert.That(plugins, Has.Some.InstanceOf<EmbeddedDocumentPlugin>());
			Assert.That(plugins, Has.Some.InstanceOf<ValidationPlugin>());
		}
	}
}