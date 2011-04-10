using System;

namespace Ormongo
{
	public class DocumentEventArgs<T> : EventArgs
	{
		public T Document { get; private set; }

		public DocumentEventArgs(T document)
		{
			Document = document;
		}
	}
}