using System.Collections.Generic;
using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class ReflectionUtilityTests
	{
		[Test]
		public void CanCheckSubclassOfRawGeneric()
		{
			// Arrange.
			var myList = new List<int>();

			// Act.
			bool result = ReflectionUtility.IsSubclassOfRawGeneric(typeof (List<>), myList.GetType());

			// Assert.
			Assert.That(result, Is.True);
		}
	}
}