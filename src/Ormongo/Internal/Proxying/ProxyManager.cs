using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Ormongo.Internal.Proxying
{
	public static class ProxyManager
	{
		private static readonly ProxyGenerator Generator = new ProxyGenerator();

		internal static object GetProxy(Type type)
		{
			return Generator.CreateClassProxy(type, new[] { typeof(IProxy) }, 
				new LazyLoadingInterceptor());
		}

		internal static bool AreSameTypes(Type left, Type right)
		{
			if (left == right)
				return true;

			if (left.BaseType == right && left.GetInterfaces().Contains(typeof(IProxy)))
				return true;

			if (right.BaseType == left && right.GetInterfaces().Contains(typeof(IProxy)))
				return true;

			return false;
		}

		public static Type GetUnderlyingType(Type type)
		{
			if (type.GetInterfaces().Contains(typeof(IProxy)))
				return type.BaseType;
			return type;
		}
	}
}