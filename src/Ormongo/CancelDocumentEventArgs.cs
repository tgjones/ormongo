using System.ComponentModel;

namespace Ormongo
{
	public class CancelDocumentEventArgs<T> : CancelEventArgs
	{
		public T Document { get; private set; }

		public CancelDocumentEventArgs(T document)
		{
			Document = document;
		}
	}
}