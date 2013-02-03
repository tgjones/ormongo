using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization.Attributes;
using Ormongo.Validation;

namespace Ormongo
{
	public abstract class EmbeddedDocument<T, TEmbeddedIn> : IValidatableObject
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

		public bool IsValid
		{
			get { return ValidationUtility.GetIsValid(this); }
		}

		protected static void Validates<TProperty>(Expression<Func<T, TProperty>> propertyExpression, params ValueValidatorBase<T>[] validators)
		{
			foreach (var validator in validators)
				validator.Initialize(propertyExpression);
			Validators.Add(x => propertyExpression.Compile()(x), validators);
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return ValidationUtility.Validate(this, Validators, validationContext);
		}

		#endregion
	}
}