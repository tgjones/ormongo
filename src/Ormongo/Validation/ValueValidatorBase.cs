using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Ormongo.Validation
{
	public abstract class ValueValidatorBase<T> 
	{
		public bool AllowNull { get; set; }
		public Func<T, bool> Unless { get; set; }

		internal virtual void Initialize<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			
		}

		public IEnumerable<ValidationResult> Validate(object value, DocumentValidationContext<T> validationContext)
		{
			if (Unless != null && Unless(validationContext.Document))
				yield break;

			if (value == null && !AllowNull)
			{
				yield return new ValidationResult(validationContext.DisplayName + " is required");
				yield break;
			}

			foreach (var validationResult in ValidateInternal(value, validationContext))
				yield return validationResult;
		}

		protected abstract IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext);
	}
}