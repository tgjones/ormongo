using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class PresenceValidator<T> : ValueValidatorBase<T>
	{
		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			if (value == null || (value is string && string.IsNullOrEmpty((string) value)))
				yield return new ValidationResult(validationContext.DisplayName + " is required");
		}
	}
}