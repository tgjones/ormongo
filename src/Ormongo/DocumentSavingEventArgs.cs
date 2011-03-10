using System;

namespace Ormongo
{
	public class DocumentSavingEventArgs<T> : EventArgs
	{
		public T Document { get; private set; }

		public DocumentSavingEventArgs(T document)
		{
			Document = document;
		}
	}
}