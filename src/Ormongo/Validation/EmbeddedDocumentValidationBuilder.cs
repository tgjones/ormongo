using System;

namespace Ormongo.Validation
{
	public class EmbeddedDocumentValidationBuilder<TDocument, TEmbeddedIn, TProperty> : 
		ValidationBuilder<EmbeddedDocumentValidationBuilder<TDocument, TEmbeddedIn, TProperty>, TDocument, TProperty>
		where TDocument : EmbeddedDocument<TDocument, TEmbeddedIn>
		where TEmbeddedIn : Document<TEmbeddedIn>
	{
		private readonly Func<TDocument, TProperty> _propertyExpression;

		public EmbeddedDocumentValidationBuilder(Func<TDocument, TProperty> propertyExpression)
		{
			_propertyExpression = propertyExpression;
		}

		public EmbeddedDocumentValidationBuilder<TDocument, TEmbeddedIn, TProperty> Uniqueness(bool caseSensitive, SaveType on = SaveType.Any)
		{
			Validators.Add(new EmbeddedDocumentUniquenessValidator<TDocument, TEmbeddedIn, TProperty>(_propertyExpression)
			{
				On = on
			});
			return this;
		}
	}
}