using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Ormongo.Internal;

namespace Ormongo.Validation
{
	public class UniquenessValidator<TDocument> : ValueValidatorBase<TDocument>
		where TDocument : Document<TDocument>
	{
		private string _propertyName;

		public bool CaseSensitive { get; set; }
		public Expression<Func<TDocument, object>>[] Scope { get; set; }

		public UniquenessValidator()
		{
			CaseSensitive = true;
		}

		internal override void Initialize<TProperty>(Expression<Func<TDocument, TProperty>> propertyExpression)
		{
			_propertyName = ExpressionUtility.GetPropertyName(propertyExpression);
		}

		protected override IEnumerable<ValidationResult> ValidateInternal(object value, DocumentValidationContext<TDocument> validationContext)
		{
			var typedValue = value as string;

			if (string.IsNullOrEmpty(typedValue))
				yield break;

			typedValue = typedValue.ToLower();

			var andClauses = new List<IMongoQuery>();
			andClauses.Add(Query.NE("_id", validationContext.Document.ID));

			andClauses.Add(CaseSensitive
				? Query.EQ(_propertyName, BsonValue.Create(typedValue))
				: Query.Matches(_propertyName, new BsonRegularExpression("^" + Regex.Escape(typedValue) + "$", "i")));

			var query = Query.And(andClauses.ToArray());
			var results = Document<TDocument>.GetCollection().Find(query);

			if (results.Any())
				yield return new ValidationResult(validationContext.DisplayName + " must be unique");
		}
	}
}