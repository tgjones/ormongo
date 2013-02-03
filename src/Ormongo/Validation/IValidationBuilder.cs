using System.Collections.Generic;

namespace Ormongo.Validation
{
	internal interface IValidationBuilder<TDocument>
	{
		List<ValueValidatorBase<TDocument>> Validators { get; }
	}
}