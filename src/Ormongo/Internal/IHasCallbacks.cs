namespace Ormongo.Internal
{
	internal interface IHasCallbacks
	{
		void ExecuteCallbacks(CallbackType callbackType);
	}
}