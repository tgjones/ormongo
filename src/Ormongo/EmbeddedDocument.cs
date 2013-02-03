using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
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
			Validators = new Dictionary<Func<T, object>, ValueValidatorBase<T>[]>();
		}

		private static readonly Dictionary<Func<T, object>, ValueValidatorBase<T>[]> Validators;

		protected static void Validates<TProperty>(Expression<Func<T, TProperty>> propertyExpression, params ValueValidatorBase<T>[] validators)
		{
			foreach (var validator in validators)
				validator.Initialize(propertyExpression);
			Validators.Add(x => propertyExpression.Compile()(x), validators);
		}

		IEnumerable<ValidationResult> IValidatableDocument.Validate(SaveType saveType)
		{
			return ValidationUtility.Validate(this, saveType, Validators);
		}

		#endregion
	}
}