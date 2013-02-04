using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ormongo.Internal;

namespace Ormongo.Validation
{
	public class EmbeddedDocumentUniquenessValidator<T, TEmbeddedIn, TProperty> : ValueValidatorBase<T>
		where T : EmbeddedDocument<T, TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		private readonly Func<T, TProperty> _propertyExpression;

		public EmbeddedDocumentUniquenessValidator(Func<T, TProperty> propertyExpression)
		{
			_propertyExpression = propertyExpression;
		}

		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<T> validationContext)
		{
			// Get all documents embeddded in the same collection in the parent document.
			var parentCollection = EmbeddedDocumentUtility.GetEmbeddedCollection(
				validationContext.ParentDocument, 
				validationContext.ParentPropertyName);

			// Loop through those embedded documents, excluding this one.
			foreach (T item in parentCollection.Where(x => x != validationContext.Document))
			{
				// Compare the specified property against this value.
				if (Equals(_propertyExpression(item), (TProperty) value))
				{
					yield return new ValidationResult(validationContext.DisplayName + " must be unique");
					yield break;
				}
			}
		}
	}
}