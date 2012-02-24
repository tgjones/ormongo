using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Ormongo.Internal.Proxying
{
	internal static class ProxyManager
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

		public static Type GetUnderlyingType(object value)
		{
			return ProxyUtil.GetUnproxiedType(value);
		}
	}
}