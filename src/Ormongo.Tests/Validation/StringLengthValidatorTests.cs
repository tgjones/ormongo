using System.Linq;
using NUnit.Framework;
using Ormongo.Validation;

namespace Ormongo.Tests.Validation
{
	[TestFixture]
	public class StringLengthValidatorTests : ValidatorTestsBase
	{
		[TestCase("foo", 4, 6, Result = 1)]
		[TestCase("foo", 0, 6, Result = 0)]
		[TestCase(null, 0, 6, Result = 0)]
		public int TestValidation(string value, int minimumLength, int maximumLength)
		{
			// Arrange.
			var validator = new StringLengthValidator<TestEntity>(minimumLength, maximumLength)
			{
				AllowNull = true
			};
			var instance = new TestEntity();
			var context = DocumentValidationContext<TestEntity>.Create(instance, SaveType.Any); 

			// Act.
			return validator.Validate(value, context).Count();
		}
	}
}