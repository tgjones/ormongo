using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Ormongo.Validation
{
	public class InclusionValidator<T, TProperty> : ValueValidatorBase<T>
	{
		private readonly IEnumerable<TProperty> _items;

		public InclusionValidator(IEnumerable<TProperty> items)
		{
			_items = items;
		}

		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			if (!_items.Contains((TProperty) value))
				yield return new ValidationResult(validationContext.DisplayName + " is not valid");
		}
	}
}