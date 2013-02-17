using System;
using System.Collections.Generic;

namespace Ormongo
{
	public class Callbacks<T>
	{
		private readonly Dictionary<CallbackType, Action<T>> _callbacks = new Dictionary<CallbackType, Action<T>>();

		public void AfterInitialize(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterInitialize, callback);
		}

		public void AfterFind(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterFind, callback);
		}

		public void BeforeCreate(Action<T> callback)
		{
			_callbacks.Add(CallbackType.BeforeCreate, callback);
		}

		public void AfterCreate(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterCreate, callback);
		}

		public void BeforeSave(Action<T> callback)
		{
			_callbacks.Add(CallbackType.BeforeSave, callback);
		}

		public void AfterSave(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterSave, callback);
		}

		public void BeforeUpdate(Action<T> callback)
		{
			_callbacks.Add(CallbackType.BeforeUpdate, callback);
		}

		public void AfterUpdate(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterUpdate, callback);
		}

		public void BeforeDestroy(Action<T> callback)
		{
			_callbacks.Add(CallbackType.BeforeDestroy, callback);
		}

		public void AfterDestroy(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterDestroy, callback);
		}

		public void BeforeValidation(Action<T> callback)
		{
			_callbacks.Add(CallbackType.BeforeValidation, callback);
		}

		public void AfterValidation(Action<T> callback)
		{
			_callbacks.Add(CallbackType.AfterValidation, callback);
		}

		public void ExecuteCallbacks(CallbackType callbackType, T instance)
		{
			if (_callbacks.ContainsKey(callbackType))
				_callbacks[callbackType](instance);
		}
	}
}