using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ormongo.Validation;

namespace Ormongo.Internal
{
	internal interface IValidatableDocument
	{
		IEnumerable<ValidationResult> Validate(SaveType saveType);
	}
}