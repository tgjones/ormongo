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
			Assert.That(plugins.Count, Is.EqualTo(1));
			Assert.That(plugins[0], Is.TypeOf<AttachmentPlugin>());
		}
	}
}