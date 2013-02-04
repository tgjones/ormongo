using System;
using System.Collections.Generic;

namespace Ormongo.Validation
{
	public class ValidationBuilder<TValidationBuilder, TDocument, TProperty> : IValidationBuilder<TDocument>
		where TValidationBuilder : ValidationBuilder<TValidationBuilder, TDocument, TProperty>
	{
		private readonly List<ValueValidatorBase<TDocument>> _validators;

		public List<ValueValidatorBase<TDocument>> Validators
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

		public TValidationBuilder Format(string regex, SaveType on = SaveType.Any)
		{
			_validators.Add(new FormatValidator<TDocument>(regex) { On = on });
			return (TValidationBuilder) this;
		}

		public TValidationBuilder Inclusion(IEnumerable<TProperty> items, SaveType on = SaveType.Any)
		{
			_validators.Add(new InclusionValidator<TDocument, TProperty>(items) { On = on });
			return (TValidationBuilder) this;
		}

		public TValidationBuilder Length(int minimum = 0, int maximum = int.MaxValue,
			Func<TDocument, bool> unless = null, SaveType on = SaveType.Any)
		{
			_validators.Add(new StringLengthValidator<TDocument>(minimum, maximum)
			{
				Unless = unless,
				On = on
			});
			return (TValidationBuilder) this;
		}

		public TValidationBuilder Presence(Func<TDocument, bool> unless = null, SaveType on = SaveType.Any)
		{
			_validators.Add(new PresenceValidator<TDocument>
			{
				Unless = unless,
				On = on
			});
			return (TValidationBuilder) this;
		}

		public TValidationBuilder Numericality(
			int? greaterThanOrEqualTo = null, int? greaterThan = null,
			int? lessThanOrEqualTo = null, int? lessThan = null,
			Func<TDocument, bool> unless = null, SaveType on = SaveType.Any)
		{
			_validators.Add(new NumericalityValidator<TDocument>
			{
				GreaterThanOrEqualTo = greaterThanOrEqualTo,
				GreaterThan = greaterThan,
				LessThanOrEqualTo = lessThanOrEqualTo,
				LessThan = lessThan,
				Unless = unless,
				On = on
			});
			return (TValidationBuilder) this;
		}
	}
}