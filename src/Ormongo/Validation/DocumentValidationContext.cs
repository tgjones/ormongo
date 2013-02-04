using System.ComponentModel.DataAnnotations;

namespace Ormongo.Validation
{
	public class DocumentValidationContext<TDocument>
	{
		public static DocumentValidationContext<TDocument> Create(TDocument instance, SaveType saveType)
		{
			return new DocumentValidationContext<TDocument>(instance, null, null, saveType,
				new ValidationContext(instance, null, null));
		}

		public static DocumentValidationContext<TDocument> Create(TDocument instance, object parentDocument, 
			string parentPropertyName, SaveType saveType)
		{
			return new DocumentValidationContext<TDocument>(instance, parentDocument, parentPropertyName, saveType,
				new ValidationContext(instance, null, null));
		}

		private readonly TDocument _document;
		private readonly object _parentDocument;
		private readonly string _parentPropertyName;
		private readonly SaveType _saveType;
		private readonly ValidationContext _validationContext;

		public TDocument Document
		{
			get { return _document; }
		}

		public object ParentDocument
		{
			get { return _parentDocument; }
		}

		public string ParentPropertyName
		{
			get { return _parentPropertyName; }
		}

		public SaveType SaveType
		{
			get { return _saveType; }
		}

		public string DisplayName
		{
			get { return _validationContext.DisplayName; }
		}

		public DocumentValidationContext(TDocument document,
			object parentDocument, string parentPropertyName,
			SaveType saveType, ValidationContext validationContext)
		{
			_document = document;
			_parentDocument = parentDocument;
			_parentPropertyName = parentPropertyName;
			_saveType = saveType;
			_validationContext = validationContext;
		}
	}
}