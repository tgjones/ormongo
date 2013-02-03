using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	internal static class ValidationUtility
	{
		public static bool GetIsValid(object instance)
		{
			return Validator.TryValidateObject(instance, new ValidationContext(instance, null, null), new Collection<ValidationResult>());
		}

		public static IEnumerable<ValidationResult> Validate<T>(object instance, 
			Dictionary<Func<T, object>, ValueValidatorBase<T>[]> validators, 
			ValidationContext validationContext)
		{
			var documentValidationContext = DocumentValidationContext<T>.Create((T) instance);
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