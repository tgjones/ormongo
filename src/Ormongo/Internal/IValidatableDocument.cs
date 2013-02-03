using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ormongo.Internal
{
	internal interface IValidatableDocument
	{
		IEnumerable<ValidationResult> Validate();
	}
}