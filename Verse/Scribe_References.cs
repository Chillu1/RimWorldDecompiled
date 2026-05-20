using RimWorld.Planet;

namespace Verse;

public static class Scribe_References
{
	public static void Look<T>(ref T refee, string label, bool saveDestroyedThings = false) where T : ILoadReferenceable
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (refee == null)
			{
				Scribe.saver.WriteElement(label, "null");
			}
			else if ((!(refee is Thing th) || !CheckSaveReferenceToDestroyedThing(th, label, saveDestroyedThings)) && (!(refee is WorldObject w) || !CheckSaveReferenceToDestroyedWorldObject(w, label, saveDestroyedThings)))
			{
				string uniqueLoadID = refee.GetUniqueLoadID();
				Scribe.saver.WriteElement(label, uniqueLoadID);
				Scribe.saver.loadIDsErrorsChecker.RegisterReferenced(refee, label);
			}
		}
		else if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (Scribe.loader.curParent != null && Scribe.loader.curParent.GetType().IsValueType)
			{
				Log.Warning("Trying to load reference of an object of type " + typeof(T)?.ToString() + " with label " + label + ", but our current node is a value type. The reference won't be loaded properly. curParent=" + Scribe.loader.curParent);
			}
			string targetLoadID = Scribe.loader.curXmlParent[label]?.InnerText;
			Scribe.loader.crossRefs.loadIDs.RegisterLoadIDReadFromXml(targetLoadID, typeof(T), label);
		}
		else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			refee = Scribe.loader.crossRefs.TakeResolvedRef<T>(label);
		}
	}

	public static void Look<T>(ref WeakReference<T> refee, string label, bool saveDestroyedThings = false) where T : class, ILoadReferenceable
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			T refee2 = ((refee != null) ? refee.Target : null);
			Look(ref refee2, label, saveDestroyedThings);
		}
		else if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			T refee3 = null;
			Look(ref refee3, label, saveDestroyedThings);
		}
		else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			T refee4 = null;
			Look(ref refee4, label, saveDestroyedThings);
			if (refee4 != null)
			{
				refee = new WeakReference<T>(refee4);
			}
		}
	}

	public static bool CheckSaveReferenceToDestroyedThing(Thing th, string label, bool saveDestroyedThings)
	{
		if (!th.Destroyed)
		{
			return false;
		}
		if (!saveDestroyedThings)
		{
			Scribe.saver.WriteElement(label, "null");
			return true;
		}
		if (th.Discarded)
		{
			Log.Warning("Trying to save reference to a discarded thing " + th?.ToString() + " with saveDestroyedThings=true. This means that it's not deep-saved anywhere and is no longer managed by anything in the code, so saving its reference will always fail. , label=" + label);
			Scribe.saver.WriteElement(label, "null");
			return true;
		}
		return false;
	}

	public static bool CheckSaveReferenceToDestroyedWorldObject(WorldObject w, string label, bool saveDestroyedWorldObjects)
	{
		if (!w.Destroyed)
		{
			return false;
		}
		if (!saveDestroyedWorldObjects)
		{
			Scribe.saver.WriteElement(label, "null");
			return true;
		}
		return false;
	}
}
