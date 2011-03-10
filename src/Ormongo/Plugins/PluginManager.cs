using System;
using System.Collections.Generic;
using System.Linq;

namespace Ormongo.Plugins
{
	internal static class PluginManager
	{
		private static List<IPlugin> _plugins;

		internal static List<IPlugin> FindPlugins()
		{
			Type pluginInterfaceType = typeof(IPlugin);
			return typeof(PluginManager).Assembly.GetTypes()
				.Where(t => !t.IsInterface && !t.IsAbstract)
				.Where(pluginInterfaceType.IsAssignableFrom)
				.Select(t => (IPlugin)Activator.CreateInstance(t))
				.ToList();
		}

		public static void Execute(Action<IPlugin> action)
		{
			if (_plugins == null)
				_plugins = FindPlugins();
			_plugins.ForEach(action);
		}
	}
}