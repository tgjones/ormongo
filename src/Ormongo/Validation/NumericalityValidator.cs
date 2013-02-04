using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class NumericalityValidator<T> : ValueValidatorBase<T>
	{
		public float? GreaterThanOrEqualTo { get; set; }
		public float? GreaterThan { get; set; }
		public float? LessThanOrEqualTo { get; set; }
		public float? LessThan { get; set; }

		protected override IEnumerable<ValidationResult> ValidateInternal(object value,
			DocumentValidationContext<T> validationContext)
		{
			var typedValue = (float) Convert.ChangeType(value, typeof(float));

			if (GreaterThanOrEqualTo != null && !(typedValue >= GreaterThanOrEqualTo.Value))
				yield return new ValidationResult(validationContext.DisplayName + " must be greater than or equal to " + GreaterThanOrEqualTo.Value);
			if (GreaterThan != null && !(typedValue > GreaterThan.Value))
				yield return new ValidationResult(validationContext.DisplayName + " must be greater than " + GreaterThan.Value);

			if (LessThanOrEqualTo != null && !(typedValue >= LessThanOrEqualTo.Value))
				yield return new ValidationResult(validationContext.DisplayName + " must be Less than or equal to " + LessThanOrEqualTo.Value);
			if (LessThan != null && !(typedValue > LessThan.Value))
				yield return new ValidationResult(validationContext.DisplayName + " must be Less than " + LessThan.Value);
		}
	}
}