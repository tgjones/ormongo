using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ormongo.Validation
{
	public class FormatValidator<T> : ValueValidatorBase<T>
	{
		private readonly string _regex;

		public FormatValidator(string regex)
		{
			_regex = regex;
		}

		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			var typedValue = value as string;
			if (string.IsNullOrEmpty(typedValue))
				yield break;

			if (!Regex.IsMatch(typedValue, _regex))
				yield return new ValidationResult(validationContext.DisplayName + " does not match format");
		}
	}
}