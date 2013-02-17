namespace Ormongo
{
	public enum CallbackType
	{
		AfterInitialize,
		AfterFind,

		BeforeCreate,
		AfterCreate,

		BeforeSave,
 		AfterSave,

		BeforeUpdate,
		AfterUpdate,

		BeforeDestroy,
		AfterDestroy,

		BeforeValidation,
		AfterValidation
	}
}