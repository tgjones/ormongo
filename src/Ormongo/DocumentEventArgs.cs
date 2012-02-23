using System;
using System.ComponentModel;

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

	public class CancelDocumentEventArgs<T> : CancelEventArgs
	{
		public T Document { get; private set; }

		public CancelDocumentEventArgs(T document)
		{
			Document = document;
		}
	}
}