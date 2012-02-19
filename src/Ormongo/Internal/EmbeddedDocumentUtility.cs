using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ormongo.Internal
{
	public static class EmbeddedDocumentUtility
	{
		/// <summary>
		/// Loop through each property of this document,
		/// and if there are any embedded documents, set
		/// their parent links to this document.
		/// </summary>
		public static void UpdateParentReferences(object document)
		{
			var properties = document.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			// Direct properties.
			var embeddedDocuments = properties
				.Where(pi => ReflectionUtility.IsSubclassOfRawGeneric(typeof(EmbeddedDocument<>), pi.PropertyType))
				.Select(pi => pi.GetValue(document, null));

			// Embedded documents inside lists.
			embeddedDocuments = embeddedDocuments.Union(properties
				.Where(pi => ReflectionUtility.IsSubclassOfRawGeneric(typeof(List<>), pi.PropertyType))
				.Where(pi =>
				{
					var args = pi.PropertyType.GetGenericArguments();
					return args.Length == 1 && ReflectionUtility.IsSubclassOfRawGeneric(typeof (EmbeddedDocument<>), args[0]);
				})
				.SelectMany(pi =>
				{
					var list = pi.GetValue(document, null) as IList;
					return (list != null)
						? list.Cast<object>()
						: new List<object>();
				}))
				.Where(o => o != null);

			foreach (var embeddedDocument in embeddedDocuments)
			{
				var parentProperty = embeddedDocument.GetType().GetProperty("Parent");
				parentProperty.SetValue(embeddedDocument, document, null);
			}
		}
	}
}