using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class DocumentValidationContext<TDocument>
	{
		public static DocumentValidationContext<TDocument> Create(TDocument instance)
		{
			return new DocumentValidationContext<TDocument>(instance,
				new ValidationContext(instance, null, null));
		}

		private readonly ValidationContext _validationContext;

		public TDocument Document { get; private set; }

		public string DisplayName
		{
			get { return _validationContext.DisplayName; }
		}

		public DocumentValidationContext(TDocument document, ValidationContext validationContext)
		{
			_validationContext = validationContext;
			Document = document;
		}
	}
}