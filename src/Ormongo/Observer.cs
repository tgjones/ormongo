namespace Ormongo
{
	public abstract class Observer<T> : IObserver<T>
		where T : Document<T>
	{
		public virtual void AfterInitialize(T document)
		{

		}

		public virtual void AfterFind(T document)
		{

		}

		public virtual bool BeforeCreate(T document)
		{
			return true;
		}

		public virtual void AfterCreate(T document)
		{
			
		}

		public virtual bool BeforeSave(T document)
		{
			return true;
		}

		public virtual void AfterSave(T document)
		{
			
		}

		public virtual bool BeforeUpdate(T document)
		{
			return true;
		}

		public virtual void AfterUpdate(T document)
		{
			
		}

		public virtual bool BeforeDestroy(T document)
		{
			return true;
		}

		public virtual void AfterDestroy(T document)
		{
			
		}

		public virtual bool BeforeValidation(T document)
		{
			return true;
		}

		public virtual void AfterValidation(T document)
		{

		}
	}
}