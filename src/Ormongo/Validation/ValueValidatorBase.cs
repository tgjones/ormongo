using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public abstract class ValueValidatorBase<T>
	{
		public bool AllowNull { get; set; }
		public Func<T, bool> Unless { get; set; }
		public SaveType On { get; set; }

		public IEnumerable<ValidationResult> Validate(object value, DocumentValidationContext<T> validationContext)
		{
			if (Unless != null && Unless(validationContext.Document))
				yield break;

			if (On != SaveType.Any && On != validationContext.SaveType)
				yield break;

			// TODO: Move this to child validators.
			if (value == null && !AllowNull)
			{
				yield return new ValidationResult(validationContext.DisplayName + " is required");
				yield break;
			}

			foreach (var validationResult in ValidateInternal(value, validationContext))
				yield return validationResult;
		}

		protected abstract IEnumerable<ValidationResult> ValidateInternal(object value,
			DocumentValidationContext<T> validationContext);
	}
}