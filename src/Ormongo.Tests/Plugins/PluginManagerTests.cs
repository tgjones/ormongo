using NUnit.Framework;
using Ormongo.Plugins;

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
			Assert.That(plugins, Has.Count.EqualTo(2));
			Assert.That(plugins, Has.Some.InstanceOf<AssociationPlugin>());
			Assert.That(plugins, Has.Some.InstanceOf<ValidationPlugin>());
		}
	}
}