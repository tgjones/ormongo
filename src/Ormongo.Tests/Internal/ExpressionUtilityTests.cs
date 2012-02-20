using System;
using MongoDB.Bson;
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
			public DataDictionary ExtraData { get; set; }

			public string MyMethod()
			{
				throw new NotImplementedException();
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

		[Test]
		public void CanGetPropertyNameFromDataDictionary()
		{
			// Act.
			string name = ExpressionUtility.GetPropertyName<TestClass, BsonValue>(u => u.ExtraData["MyProperty"]);

			// Assert.
			Assert.That(name, Is.EqualTo("ExtraData.MyProperty"));
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

		[Test]
		public void CanGetPropertyNameAndConstantValueFromBinaryExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == "hello",
				out name, out value);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
			Assert.That(value, Is.EqualTo("hello"));
		}

		[Test]
		public void CanGetPropertyNameAndLocalVariableValueFromBinaryExpression()
		{
			// Act.
			string name;
			object value;
			string message = "hello2";
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == message,
				out name, out value);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
			Assert.That(value, Is.EqualTo("hello2"));
		}

		private string GetMessage()
		{
			return "hello3";
		}

		[Test]
		public void CanGetPropertyNameAndInstanceMethodValueFromBinaryExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == GetMessage(),
				out name, out value);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
			Assert.That(value, Is.EqualTo("hello3"));
		}

		private static string GetMessageStatic()
		{
			return "hello4";
		}

		[Test]
		public void CanGetPropertyNameAndStaticMethodValueFromBinaryExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == GetMessageStatic(),
				out name, out value);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
			Assert.That(value, Is.EqualTo("hello4"));
		}

		private string _message = "hello5";

		[Test]
		public void CanGetPropertyNameAndFieldValueFromBinaryExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == _message,
				out name, out value);

			// Assert.
			Assert.That(name, Is.EqualTo("MyProp"));
			Assert.That(value, Is.EqualTo("hello5"));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfNotBinaryExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => true, out name, out value);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ThrowsIfNotMemberExpression()
		{
			// Act.
			string name;
			object value;
			ExpressionUtility.GetPropertyNameAndValue<TestClass>(u => u.MyProp == new string('c', 8),
				out name, out value);
		}
	}
}