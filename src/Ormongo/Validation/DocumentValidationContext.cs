using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class DocumentValidationContext<TDocument>
	{
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