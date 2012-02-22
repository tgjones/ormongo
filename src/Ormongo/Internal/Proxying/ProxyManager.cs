using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Ormongo.Internal.Proxying
{
	public static class ProxyManager
	{
		private static readonly ProxyGenerator Generator = new ProxyGenerator();

		public static object GetProxy(Type type)
		{
			return Generator.CreateClassProxy(type, new[] { typeof(IProxy) }, 
				new LazyLoadingInterceptor());
		}

		public static bool AreSameTypes(Type left, Type right)
		{
			if (left == right)
				return true;

			if (left.BaseType == right && left.GetInterfaces().Contains(typeof(IProxy)))
				return true;

			if (right.BaseType == left && right.GetInterfaces().Contains(typeof(IProxy)))
				return true;

			return false;
		}
	}

	internal class LazyLoadingInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			IDocument document = invocation.InvocationTarget as IDocument;
			if (document != null)
			{
				// Intercept calls to HasOne relations.
				if (invocation.Method.Name.StartsWith("get_") &&
					ReflectionUtility.IsSubclassOfRawGeneric(typeof(Document<>), invocation.Method.ReturnType))
				{
					var propertyName = invocation.Method.Name.Substring(4);
					var otherID = document.RelationalIDs[propertyName];

					var otherType = invocation.Method.ReturnType;
					var other = OrmongoConfiguration.GetMongoDatabase()
						.GetCollection(CollectionUtility.GetCollectionName(otherType))
						.FindOneByIdAs(otherType, otherID);

					var propertySetter = document.GetType().GetProperty(propertyName);
					propertySetter.SetValue(document, other, null);
				}
			}

			invocation.Proceed();
		}
	}
}