using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class LoadIDsWantedBank
	{
		private struct IdRecord
		{
			public string targetLoadID;

			public Type targetType;

			public string pathRelToParent;

			public IExposable parent;

			public IdRecord(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
			{
				this.targetLoadID = targetLoadID;
				this.targetType = targetType;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private struct IdListRecord
		{
			public List<string> targetLoadIDs;

			public string pathRelToParent;

			public IExposable parent;

			public IdListRecord(List<string> targetLoadIDs, string pathRelToParent, IExposable parent)
			{
				this.targetLoadIDs = targetLoadIDs;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private List<IdRecord> idsRead = new List<IdRecord>();

		private List<IdListRecord> idListsRead = new List<IdListRecord>();

		public void ConfirmClear()
		{
			if (idsRead.Count > 0 || idListsRead.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Not all loadIDs which were read were consumed.");
				if (idsRead.Count > 0)
				{
					stringBuilder.AppendLine("Singles:");
					for (int i = 0; i < idsRead.Count; i++)
					{
						stringBuilder.AppendLine(string.Concat("  ", idsRead[i].targetLoadID.ToStringSafe(), " of type ", idsRead[i].targetType, ". pathRelToParent=", idsRead[i].pathRelToParent, ", parent=", idsRead[i].parent.ToStringSafe()));
					}
				}
				if (idListsRead.Count > 0)
				{
					stringBuilder.AppendLine("Lists:");
					for (int j = 0; j < idListsRead.Count; j++)
					{
						stringBuilder.AppendLine("  List with " + ((idListsRead[j].targetLoadIDs != null) ? idListsRead[j].targetLoadIDs.Count : 0) + " elements. pathRelToParent=" + idListsRead[j].pathRelToParent + ", parent=" + idListsRead[j].parent.ToStringSafe());
					}
				}
				Log.Warning(stringBuilder.ToString().TrimEndNewlines());
			}
			Clear();
		}

		public void Clear()
		{
			idsRead.Clear();
			idListsRead.Clear();
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idsRead.Count; i++)
			{
				if (idsRead[i].parent == parent && idsRead[i].pathRelToParent == pathRelToParent)
				{
					Log.Error("Tried to register the same load ID twice: " + targetLoadID + ", pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
					return;
				}
			}
			idsRead.Add(new IdRecord(targetLoadID, targetType, pathRelToParent, parent));
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + "/" + toAppendToPathRelToParent;
			}
			RegisterLoadIDReadFromXml(targetLoadID, targetType, text, Scribe.loader.curParent);
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idListsRead.Count; i++)
			{
				if (idListsRead[i].parent == parent && idListsRead[i].pathRelToParent == pathRelToParent)
				{
					Log.Error("Tried to register the same list of load IDs twice. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
					return;
				}
			}
			idListsRead.Add(new IdListRecord(targetLoadIDList, pathRelToParent, parent));
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + "/" + toAppendToPathRelToParent;
			}
			RegisterLoadIDListReadFromXml(targetLoadIDList, text, Scribe.loader.curParent);
		}

		public string Take<T>(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idsRead.Count; i++)
			{
				if (idsRead[i].parent == parent && idsRead[i].pathRelToParent == pathRelToParent)
				{
					string targetLoadID = idsRead[i].targetLoadID;
					if (typeof(T) != idsRead[i].targetType)
					{
						Log.Error(string.Concat("Trying to get load ID of object of type ", typeof(T), ", but it was registered as ", idsRead[i].targetType, ". pathRelToParent=", pathRelToParent, ", parent=", parent.ToStringSafe()));
					}
					idsRead.RemoveAt(i);
					return targetLoadID;
				}
			}
			Log.Error("Could not get load ID. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			return null;
		}

		public List<string> TakeList(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idListsRead.Count; i++)
			{
				if (idListsRead[i].parent == parent && idListsRead[i].pathRelToParent == pathRelToParent)
				{
					List<string> targetLoadIDs = idListsRead[i].targetLoadIDs;
					idListsRead.RemoveAt(i);
					return targetLoadIDs;
				}
			}
			Log.Error("Could not get load IDs list. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			return new List<string>();
		}
	}
}
