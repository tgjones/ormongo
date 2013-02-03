using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Ormongo.Validation
{
	public class ValidationBuilder<TDocument, TProperty> : IValidationBuilder<TDocument>
	{
		private readonly List<ValueValidatorBase<TDocument>> _validators;

		protected List<ValueValidatorBase<TDocument>> Validators
		{
			get { return _validators; }
		}

		List<ValueValidatorBase<TDocument>> IValidationBuilder<TDocument>.Validators
		{
			get { return _validators; }
		}

		public ValidationBuilder()
		{
			_validators = new List<ValueValidatorBase<TDocument>>();
		}

		public ValidationBuilder<TDocument, TProperty> Presence(SaveType on = SaveType.Any)
		{
			_validators.Add(new PresenceValidator<TDocument> { On = on });
			return this;
		}
	}

	public class DocumentValidationBuilder<TDocument, TProperty> : ValidationBuilder<TDocument, TProperty>
		where TDocument : Document<TDocument>
	{
		private readonly Expression<Func<TDocument, TProperty>> _propertyExpression;

		public DocumentValidationBuilder(Expression<Func<TDocument, TProperty>> propertyExpression)
		{
			_propertyExpression = propertyExpression;
		}

		public ValidationBuilder<TDocument, TProperty> Uniqueness(SaveType on = SaveType.Any)
		{
			Validators.Add(new UniquenessValidator<TDocument, TProperty>(_propertyExpression) { On = on });
			return this;
		}
	}
}