using System;
using System.Collections.Generic;

namespace Verse;

public class DebugLoadIDsSavingErrorsChecker
{
	private struct ReferencedObject : IEquatable<ReferencedObject>
	{
		public string loadID;

		public string label;

		public ReferencedObject(string loadID, string label)
		{
			this.loadID = loadID;
			this.label = label;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ReferencedObject))
			{
				return false;
			}
			return Equals((ReferencedObject)obj);
		}

		public bool Equals(ReferencedObject other)
		{
			if (loadID == other.loadID)
			{
				return label == other.label;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(Gen.HashCombine(0, loadID), label);
		}

		public static bool operator ==(ReferencedObject lhs, ReferencedObject rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(ReferencedObject lhs, ReferencedObject rhs)
		{
			return !(lhs == rhs);
		}
	}

	private HashSet<string> deepSaved = new HashSet<string>();

	private Dictionary<string, string> deepSavedInfo = new Dictionary<string, string>();

	private HashSet<ReferencedObject> referenced = new HashSet<ReferencedObject>();

	public void Clear()
	{
		if (Prefs.DevMode)
		{
			deepSaved.Clear();
			deepSavedInfo.Clear();
			referenced.Clear();
		}
	}

	public void CheckForErrorsAndClear()
	{
		if (!Prefs.DevMode)
		{
			return;
		}
		if (!Scribe.saver.savingForDebug)
		{
			foreach (ReferencedObject item in referenced)
			{
				if (!deepSaved.Contains(item.loadID))
				{
					Log.Warning("Object with load ID " + item.loadID + " is referenced (xml node name: " + item.label + ") but is not deep-saved. This will cause errors during loading.");
				}
			}
		}
		Clear();
	}

	public void RegisterDeepSaved(object obj, string label)
	{
		if (!Prefs.DevMode)
		{
			return;
		}
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			Log.Error("Registered " + obj?.ToString() + ", but current mode is " + Scribe.mode);
		}
		else
		{
			if (obj == null || !(obj is ILoadReferenceable loadReferenceable))
			{
				return;
			}
			try
			{
				string uniqueLoadID = loadReferenceable.GetUniqueLoadID();
				if (!deepSaved.Add(uniqueLoadID))
				{
					Log.Warning("DebugLoadIDsSavingErrorsChecker error: tried to register deep-saved object with loadID " + uniqueLoadID + ", but it's already here. label=" + label + " (not cleared after the previous save? different objects have the same load ID? the same object is deep-saved twice?)");
					if (deepSavedInfo.TryGetValue(uniqueLoadID, out var value))
					{
						Log.Warning(loadReferenceable.GetType()?.ToString() + " was already deepsaved at " + value + ".");
					}
				}
				else
				{
					deepSavedInfo.Add(uniqueLoadID, Scribe.saver.CurPath);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error in GetUniqueLoadID(): " + ex);
			}
		}
	}

	public void RegisterReferenced(ILoadReferenceable obj, string label)
	{
		if (!Prefs.DevMode)
		{
			return;
		}
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			Log.Error("Registered " + obj?.ToString() + ", but current mode is " + Scribe.mode);
		}
		else if (obj != null)
		{
			try
			{
				referenced.Add(new ReferencedObject(obj.GetUniqueLoadID(), label));
			}
			catch (Exception ex)
			{
				Log.Error("Error in GetUniqueLoadID(): " + ex);
			}
		}
	}
}
