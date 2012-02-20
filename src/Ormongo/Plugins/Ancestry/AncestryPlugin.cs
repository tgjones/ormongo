using System;
using System.Collections.Generic;
using System.Linq;

namespace Ormongo.Plugins.Ancestry
{
	public class AncestryPlugin : PluginBase
	{
		private readonly Dictionary<Type, AncestryAttribute> _ancestrySettings = new Dictionary<Type, AncestryAttribute>();

		public override void BeforeSave(object document)
		{
			if (HasAncestry(document))
				GetAncestryProxy(document).UpdateDescendantsWithNewAncestry();
			base.BeforeSave(document);
		}

		public override void AfterSave(object document)
		{
			if (HasAncestry(document))
				GetAncestryProxy(document).ResetChangedFields();
			base.AfterSave(document);
		}

		public override void BeforeDestroy(object document)
		{
			if (HasAncestry(document))
			{
				var ancestrySettings = GetAncestrySettings(document.GetType());
				GetAncestryProxy(document).ApplyOrphanStrategy(ancestrySettings.OrphanStrategy);
			}
			base.BeforeDestroy(document);
		}

		private AncestryAttribute GetAncestrySettings(Type documentType)
		{
			if (!_ancestrySettings.ContainsKey(documentType))
			{
				var attributes = documentType.GetCustomAttributes(typeof(AncestryAttribute), true);
				var attribute = attributes.OfType<AncestryAttribute>().FirstOrDefault() ?? new AncestryAttribute
				{
					OrphanStrategy = OrphanStrategy.Destroy,
					CacheDepth = false
				};
				_ancestrySettings.Add(documentType, attribute);
			}
			return _ancestrySettings[documentType];
		}

		private static bool HasAncestry(object document)
		{
			return document.GetType().GetInterface(typeof(IHasAncestry).Name) != null;
		}

		private static IAncestryProxy GetAncestryProxy(object document)
		{
			return (IAncestryProxy) document.GetType().GetProperty("Ancestry").GetValue(document, null);
		}
	}
}