using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class ExpressionUtilityTests
	{
		private class TestClass
		{
			public List<string> MyField;

			public List<string> MyProp { get; set; }

			public List<string> MyMethod()
			{
				return null;
			}
		}

		[Test]
		public void CanGetSimplePropertyName()
		{
			// Act.
			string name = ExpressionUtility.GetPropertyName<TestClass, string>(u => u.MyProp);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void FieldExpressionThrowsException()
		{
			ExpressionUtility.GetPropertyName<TestClass, string>(u => u.MyField);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void MethodExpressionThrowsException()
		{
			ExpressionUtility.GetPropertyName<TestClass, string>(u => u.MyMethod());
		}
	}
}