using System;
using System.Collections.Generic;

namespace Verse
{
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

		private HashSet<ReferencedObject> referenced = new HashSet<ReferencedObject>();

		public void Clear()
		{
			if (Prefs.DevMode)
			{
				deepSaved.Clear();
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
				Log.Error(string.Concat("Registered ", obj, ", but current mode is ", Scribe.mode));
			}
			else
			{
				if (obj == null)
				{
					return;
				}
				ILoadReferenceable loadReferenceable = obj as ILoadReferenceable;
				if (loadReferenceable == null)
				{
					return;
				}
				try
				{
					if (!deepSaved.Add(loadReferenceable.GetUniqueLoadID()))
					{
						Log.Warning("DebugLoadIDsSavingErrorsChecker error: tried to register deep-saved object with loadID " + loadReferenceable.GetUniqueLoadID() + ", but it's already here. label=" + label + " (not cleared after the previous save? different objects have the same load ID? the same object is deep-saved twice?)");
					}
				}
				catch (Exception arg)
				{
					Log.Error("Error in GetUniqueLoadID(): " + arg);
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
				Log.Error(string.Concat("Registered ", obj, ", but current mode is ", Scribe.mode));
			}
			else if (obj != null)
			{
				try
				{
					referenced.Add(new ReferencedObject(obj.GetUniqueLoadID(), label));
				}
				catch (Exception arg)
				{
					Log.Error("Error in GetUniqueLoadID(): " + arg);
				}
			}
		}
	}
}
