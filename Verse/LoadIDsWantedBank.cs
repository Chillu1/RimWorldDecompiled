using System;
using System.Collections.Generic;
using System.Text;

namespace Verse;

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

	private Dictionary<(IExposable, string), IdRecord> idsRead = new Dictionary<(IExposable, string), IdRecord>();

	private Dictionary<(IExposable, string), IdListRecord> idListsRead = new Dictionary<(IExposable, string), IdListRecord>();

	public void ConfirmClear()
	{
		if (idsRead.Count > 0 || idListsRead.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Not all loadIDs which were read were consumed.");
			if (idsRead.Count > 0)
			{
				stringBuilder.AppendLine("Singles:");
				foreach (KeyValuePair<(IExposable, string), IdRecord> item in idsRead)
				{
					stringBuilder.AppendLine("  " + item.Value.targetLoadID.ToStringSafe() + " of type " + item.Value.targetType?.ToString() + ". pathRelToParent=" + item.Value.pathRelToParent + ", parent=" + item.Value.parent.ToStringSafe());
				}
			}
			if (idListsRead.Count > 0)
			{
				stringBuilder.AppendLine("Lists:");
				foreach (KeyValuePair<(IExposable, string), IdListRecord> item2 in idListsRead)
				{
					stringBuilder.AppendLine("  List with " + ((item2.Value.targetLoadIDs != null) ? item2.Value.targetLoadIDs.Count : 0) + " elements. pathRelToParent=" + item2.Value.pathRelToParent + ", parent=" + item2.Value.parent.ToStringSafe());
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
		if (idsRead.ContainsKey((parent, pathRelToParent)))
		{
			Log.Error("Tried to register the same load ID twice: " + targetLoadID + ", pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
		}
		else
		{
			IdRecord value = new IdRecord(targetLoadID, targetType, pathRelToParent, parent);
			idsRead.Add((parent, pathRelToParent), value);
		}
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
		if (idListsRead.ContainsKey((parent, pathRelToParent)))
		{
			Log.Error("Tried to register the same list of load IDs twice. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			return;
		}
		IdListRecord value = new IdListRecord(targetLoadIDList, pathRelToParent, parent);
		idListsRead.Add((parent, pathRelToParent), value);
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
		if (idsRead.TryGetValue((parent, pathRelToParent), out var value))
		{
			string targetLoadID = value.targetLoadID;
			if (typeof(T) != value.targetType)
			{
				Log.Error("Trying to get load ID of object of type " + typeof(T)?.ToString() + ", but it was registered as " + value.targetType?.ToString() + ". pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			}
			idsRead.Remove((parent, pathRelToParent));
			return targetLoadID;
		}
		Log.Error("Could not get load ID. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
		return null;
	}

	public List<string> TakeList(string pathRelToParent, IExposable parent)
	{
		if (idListsRead.TryGetValue((parent, pathRelToParent), out var value))
		{
			List<string> targetLoadIDs = value.targetLoadIDs;
			idListsRead.Remove((parent, pathRelToParent));
			return targetLoadIDs;
		}
		Log.Error("Could not get load IDs list. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
		return new List<string>();
	}
}
