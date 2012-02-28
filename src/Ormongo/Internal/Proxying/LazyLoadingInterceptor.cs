using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using MongoDB.Bson;

namespace Ormongo.Internal.Proxying
{
	internal class LazyLoadingInterceptor : IInterceptor
	{
		private readonly List<string> _loadedProperties = new List<string>();

		public void Intercept(IInvocation invocation)
		{
			var document = invocation.InvocationTarget as IDocument;
			if (document != null)
			{
				// Intercept calls to ReferencesOn relations.
				if (invocation.Method.Name.StartsWith("get_") &&
					ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), invocation.Method.ReturnType))
				{
					var propertyName = invocation.Method.Name.Substring(4);
					if (!_loadedProperties.Contains(propertyName))
					{
						if (document.ReferencesOneIDs.ContainsKey(propertyName))
						{
							var otherID = document.ReferencesOneIDs[propertyName];

							var otherType = invocation.Method.ReturnType;
							var findMethod = typeof(Document<>).MakeGenericType(otherType)
								.GetMethods(BindingFlags.Public | BindingFlags.Static)
								.Single(m => m.Name == "Find" && !m.IsGenericMethod && m.GetParameters().Count() == 1 && m.GetParameters()[0].ParameterType == typeof(ObjectId));
							var other = findMethod.Invoke(null, new object[] { otherID });

							var propertySetter = document.GetType().GetProperty(propertyName);
							propertySetter.SetValue(document, other, null);
						}
						_loadedProperties.Add(propertyName);
					}
				}

				// Intercept calls to ReferencesMany relations.
				else if (invocation.Method.Name.StartsWith("get_") &&
					ReflectionUtility.IsListOfRawGeneric(typeof(Document<>), invocation.Method.ReturnType))
				{
					var propertyName = invocation.Method.Name.Substring(4);
					if (!_loadedProperties.Contains(propertyName))
					{
						if (document.ReferencesManyIDs.ContainsKey(propertyName))
						{
							var otherIDs = document.ReferencesManyIDs[propertyName].ToArray();

							var otherType = ReflectionUtility.GetTypeOfGenericList(invocation.Method.ReturnType);
							var findMethod = typeof(Document<>).MakeGenericType(otherType).GetMethod("Find", new[] { typeof(ObjectId[]) });
							var other = findMethod.Invoke(null, new object[] { otherIDs});

							other = typeof(Enumerable).GetMethod("ToList", BindingFlags.Public | BindingFlags.Static)
								.MakeGenericMethod(otherType)
								.Invoke(null, new[] { other });

							var propertySetter = document.GetType().GetProperty(propertyName);
							propertySetter.SetValue(document, other, null);
						}
						_loadedProperties.Add(propertyName);
					}
				}
			}

			invocation.Proceed();
		}
	}
}