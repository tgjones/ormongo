using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Ormongo.Internal;
using Ormongo.Validation;

namespace Ormongo
{
	public abstract class EmbeddedDocument<T, TEmbeddedIn> : IValidatableDocument
		where T : EmbeddedDocument<T, TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		[BsonIgnore, ScaffoldColumn(false)]
		public TEmbeddedIn Parent
		{
			get;
			set;
		}

		#region Validation

		static EmbeddedDocument()
		{
			Validators = new Dictionary<Func<T, object>, IValidationBuilder<T>>();
		}

		private static readonly Dictionary<Func<T, object>, IValidationBuilder<T>> Validators;

		protected static EmbeddedDocumentValidationBuilder<T, TEmbeddedIn, TProperty> Validates<TProperty>(Func<T, TProperty> propertyExpression)
		{
			var validationBuilder = new EmbeddedDocumentValidationBuilder<T, TEmbeddedIn, TProperty>(propertyExpression);
			Validators.Add(x => propertyExpression(x), validationBuilder);
			return validationBuilder;
		}

		IEnumerable<ValidationResult> IValidatableDocument.Validate(SaveType saveType)
		{
			var parentPropertyName = EmbeddedDocumentUtility.GetParentPropertyName<T, TEmbeddedIn>((T) this);
			var validationContext = DocumentValidationContext<T>.Create((T) this, Parent, parentPropertyName, saveType);
			return ValidationUtility.Validate(Validators, validationContext);
		}

		#endregion
	}
}