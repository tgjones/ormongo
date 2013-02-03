using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	internal static class ValidationUtility
	{
		public static IEnumerable<ValidationResult> Validate<T>(object instance, SaveType saveType,
			Dictionary<Func<T, object>, ValueValidatorBase<T>[]> validators)
		{
			var documentValidationContext = DocumentValidationContext<T>.Create((T) instance, saveType);
			var results = new List<ValidationResult>();
			foreach (var kvp in validators)
			{
				var value = kvp.Key((T) instance);
				foreach (var validator in kvp.Value)
					results.AddRange(validator.Validate(value, documentValidationContext));
			}
			return results;
		}
	}
}