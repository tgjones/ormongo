using System;
using NUnit.Framework;
using Ormongo.Internal;

namespace Ormongo.Tests.Internal
{
	[TestFixture]
	public class ExpressionUtilityTests
	{
		private class TestClass
		{
			public string MyField;

			public string MyProp { get; set; }

			public string MyMethod()
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