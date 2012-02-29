using System;

namespace Ormongo
{
	public interface IProxy
	{
		Type GetUnderlyingType();
	}
}