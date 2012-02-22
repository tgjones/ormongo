using System;

namespace Ormongo.Internal
{
	public static class CollectionUtility
	{
		 public static string GetCollectionName(Type type)
		 {
		 	var documentType = typeof(Document<>);
			 while (type != null && type != typeof(object) && type.BaseType != null)
			 {
				 var cur = type.BaseType.IsGenericType ? type.BaseType.GetGenericTypeDefinition() : type.BaseType;
				 if (documentType == cur)
					 return type.Name;
				 type = type.BaseType;
			 }
		 	throw new Exception("Type doesn't inherit from Document<>");
		 }
	}
}