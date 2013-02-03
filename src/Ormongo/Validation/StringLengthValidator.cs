using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class StringLengthValidator<T> : ValueValidatorBase<T>
	{
		private readonly int _minimumLength;
		private readonly int _maximumLength;

		public StringLengthValidator(int minimumLength, int maximumLength)
		{
			_minimumLength = minimumLength;
			_maximumLength = maximumLength;
		}

		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			var typedValue = value as string;
			if (string.IsNullOrEmpty(typedValue))
				yield break;

			if (typedValue.Length < _minimumLength)
				yield return new ValidationResult(validationContext.DisplayName + " must be at least " + _minimumLength + " characters long");
			if (typedValue.Length > _maximumLength)
				yield return new ValidationResult(validationContext.DisplayName + " must be at most " + _maximumLength + " characters long");
		}
	}
}