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
			var plugins = PluginManager.FindPlugins();

			// Assert.
			Assert.That(plugins, Has.Count.EqualTo(3));
			Assert.That(plugins, Has.Some.InstanceOf<AssociationPlugin>());
			Assert.That(plugins, Has.Some.InstanceOf<ValidationPlugin>());
			Assert.That(plugins, Has.Some.InstanceOf<AncestryPlugin>());
		}
	}
}