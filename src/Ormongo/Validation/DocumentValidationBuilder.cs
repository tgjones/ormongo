using System;
using System.Linq.Expressions;

namespace Ormongo.Validation
{
	public class DocumentValidationBuilder<TDocument, TProperty> : ValidationBuilder<DocumentValidationBuilder<TDocument, TProperty>, TDocument, TProperty>
		where TDocument : Document<TDocument> 
	{
		private readonly Expression<Func<TDocument, TProperty>> _propertyExpression;

		public DocumentValidationBuilder(Expression<Func<TDocument, TProperty>> propertyExpression)
		{
			_propertyExpression = propertyExpression;
		}

		public DocumentValidationBuilder<TDocument, TProperty> Uniqueness(bool caseSensitive, SaveType on = SaveType.Any)
		{
			Validators.Add(new DocumentUniquenessValidator<TDocument, TProperty>(_propertyExpression)
			{
				On = on,
				CaseSensitive = caseSensitive
			});
			return this;
		}
	}
}