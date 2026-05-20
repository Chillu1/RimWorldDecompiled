using System;
using System.Collections.Generic;

namespace Verse;

public class CrossRefHandler
{
	private LoadedObjectDirectory loadedObjectDirectory = new LoadedObjectDirectory();

	public LoadIDsWantedBank loadIDs = new LoadIDsWantedBank();

	public List<IExposable> crossReferencingExposables = new List<IExposable>();

	public void RegisterForCrossRefResolve(IExposable s)
	{
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			Log.Error("Registered " + s?.ToString() + " for cross ref resolve, but current mode is " + Scribe.mode);
		}
		else if (s != null)
		{
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("RegisterForCrossRefResolve " + ((s != null) ? s.GetType().ToString() : "null"));
			}
			crossReferencingExposables.Add(s);
		}
	}

	public void ResolveAllCrossReferences()
	{
		Scribe.mode = LoadSaveMode.ResolvingCrossRefs;
		if (DebugViewSettings.logMapLoad)
		{
			LogSimple.Message("==================Register the saveables all so we can find them later");
		}
		foreach (IExposable crossReferencingExposable in crossReferencingExposables)
		{
			if (crossReferencingExposable is ILoadReferenceable loadReferenceable)
			{
				if (DebugViewSettings.logMapLoad)
				{
					LogSimple.Message("RegisterLoaded " + loadReferenceable.GetType());
				}
				loadedObjectDirectory.RegisterLoaded(loadReferenceable);
			}
		}
		if (DebugViewSettings.logMapLoad)
		{
			LogSimple.Message("==================Fill all cross-references to the saveables");
		}
		foreach (IExposable crossReferencingExposable2 in crossReferencingExposables)
		{
			if (DebugViewSettings.logMapLoad)
			{
				LogSimple.Message("ResolvingCrossRefs ExposeData " + crossReferencingExposable2.GetType());
			}
			try
			{
				Scribe.loader.curParent = crossReferencingExposable2;
				Scribe.loader.curPathRelToParent = null;
				crossReferencingExposable2.ExposeData();
			}
			catch (Exception ex)
			{
				Log.Error("Could not resolve cross refs: " + ex);
			}
		}
		Scribe.loader.curParent = null;
		Scribe.loader.curPathRelToParent = null;
		Scribe.mode = LoadSaveMode.Inactive;
		Clear(errorIfNotEmpty: true);
	}

	public T TakeResolvedRef<T>(string pathRelToParent, IExposable parent) where T : ILoadReferenceable
	{
		string loadID = loadIDs.Take<T>(pathRelToParent, parent);
		return loadedObjectDirectory.ObjectWithLoadID<T>(loadID);
	}

	public T TakeResolvedRef<T>(string toAppendToPathRelToParent) where T : ILoadReferenceable
	{
		string text = Scribe.loader.curPathRelToParent;
		if (!toAppendToPathRelToParent.NullOrEmpty())
		{
			text = text + "/" + toAppendToPathRelToParent;
		}
		return TakeResolvedRef<T>(text, Scribe.loader.curParent);
	}

	public List<T> TakeResolvedRefList<T>(string pathRelToParent, IExposable parent)
	{
		List<string> list = loadIDs.TakeList(pathRelToParent, parent);
		List<T> list2 = new List<T>();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(loadedObjectDirectory.ObjectWithLoadID<T>(list[i]));
			}
		}
		return list2;
	}

	public List<T> TakeResolvedRefList<T>(string toAppendToPathRelToParent)
	{
		string text = Scribe.loader.curPathRelToParent;
		if (!toAppendToPathRelToParent.NullOrEmpty())
		{
			text = text + "/" + toAppendToPathRelToParent;
		}
		return TakeResolvedRefList<T>(text, Scribe.loader.curParent);
	}

	public void Clear(bool errorIfNotEmpty)
	{
		if (errorIfNotEmpty)
		{
			loadIDs.ConfirmClear();
		}
		else
		{
			loadIDs.Clear();
		}
		crossReferencingExposables.Clear();
		loadedObjectDirectory.Clear();
	}
}
