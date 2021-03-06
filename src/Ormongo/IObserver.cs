namespace Ormongo
{
	public interface IObserver<in T>
		where T : Document<T>
	{
		void AfterInitialize(T document);
		void AfterFind(T document);

		bool BeforeCreate(T document);
		void AfterCreate(T document);

		bool BeforeSave(T document);
		void AfterSave(T document);

		bool BeforeUpdate(T document);
		void AfterUpdate(T document);

		bool BeforeDestroy(T document);
		void AfterDestroy(T document);

		bool BeforeValidation(T document);
		void AfterValidation(T document);
	}
}