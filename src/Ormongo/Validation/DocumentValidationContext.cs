using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class DocumentValidationContext<TDocument>
	{
		public static DocumentValidationContext<TDocument> Create(TDocument instance, SaveType saveType)
		{
			return new DocumentValidationContext<TDocument>(instance, saveType,
				new ValidationContext(instance, null, null));
		}

		private readonly TDocument _document;
		private readonly SaveType _saveType;
		private readonly ValidationContext _validationContext;

		public TDocument Document
		{
			get { return _document; }
		}

		public SaveType SaveType
		{
			get { return _saveType; }
		}

		public string DisplayName
		{
			get { return _validationContext.DisplayName; }
		}

		public DocumentValidationContext(TDocument document, SaveType saveType, ValidationContext validationContext)
		{
			_document = document;
			_saveType = saveType;
			_validationContext = validationContext;
		}
	}
}