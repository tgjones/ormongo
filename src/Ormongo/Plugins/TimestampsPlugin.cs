using System;

namespace Ormongo.Plugins
{
	public class TimestampsPlugin<T> : Plugin<T>
		where T : Document<T>
	{
		private const string CreatedAtKey = "Timestamps_CreatedAt";
		private const string UpdatedAtKey = "Timestamps_UpdatedAt";

		public static DateTime GetCreatedAt(T document)
		{
			return document.CatchAll[CreatedAtKey].AsDateTime;
		}

		public static DateTime GetUpdatedAt(T document)
		{
			return document.CatchAll[UpdatedAtKey].AsDateTime;
		}

		public override void Save(T document, ref Action finalAction)
		{
			var now = DateTime.Now;
			if (document.IsNewRecord)
				document.CatchAll[CreatedAtKey] = now;
			document.CatchAll[UpdatedAtKey] = now;

			base.Save(document, ref finalAction);
		}
	}
}