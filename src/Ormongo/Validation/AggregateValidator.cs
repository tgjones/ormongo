using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class AggregateValidator<T> : ValueValidatorBase<T>
	{
		private readonly List<ValueValidatorBase<T>> _validators;

		public AggregateValidator()
		{
			_validators = new List<ValueValidatorBase<T>>();
		}

		protected void AddValidator(ValueValidatorBase<T> validator)
		{
			_validators.Add(validator);
		}

		protected override sealed IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			var result = new List<ValidationResult>();
			foreach (var validator in _validators)
				result.AddRange(validator.Validate(value, validationContext));
			return result;
		}
	}
}