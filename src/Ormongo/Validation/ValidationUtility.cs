using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	internal static class ValidationUtility
	{
		public static IEnumerable<ValidationResult> Validate<T>(
			Dictionary<Func<T, object>, IValidationBuilder<T>> validators,
			DocumentValidationContext<T> validationContext)
		{
			var results = new List<ValidationResult>();
			foreach (var kvp in validators)
			{
				var value = kvp.Key(validationContext.Document);
				foreach (var validator in kvp.Value.Validators)
					results.AddRange(validator.Validate(value, validationContext));
			}
			return results;
		}
	}
}