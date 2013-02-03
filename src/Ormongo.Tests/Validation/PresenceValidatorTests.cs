using System.Linq;
using NUnit.Framework;
using Ormongo.Validation;

namespace Ormongo.Tests.Validation
{
	[TestFixture]
	public class PresenceValidatorTests : ValidatorTestsBase
	{
		[TestCase("foo", Result = 0)]
		[TestCase("", Result = 1)]
		[TestCase(null, Result = 1)]
		public int TestValidation(string value)
		{
			// Arrange.
			var validator = new PresenceValidator<TestEntity>();
			var instance = new TestEntity();
			var context = DocumentValidationContext<TestEntity>.Create(instance); 

			// Act.
			return validator.Validate(value, context).Count();
		}
	}
}