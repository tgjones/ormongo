using System;
using System.Collections.Generic;
using System.Linq;

namespace Ormongo.Plugins
{
	internal static class PluginManager
	{
		private static List<IPlugin> _plugins;

		private static List<IPlugin> Plugins
		{
			get { return _plugins ?? (_plugins = FindPlugins().ToList()); }
		}

		internal static IEnumerable<IPlugin> FindPlugins()
		{
			yield return new ValidationPlugin();
			yield return new EmbeddedDocumentPlugin();
		}

		public static void Execute(Action<IPlugin> action)
		{
			Plugins.ForEach(action);
		}
	}
}