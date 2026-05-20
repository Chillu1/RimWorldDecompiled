using System;
using System.Collections.Generic;

namespace Verse;

public class PostLoadIniter
{
	private HashSet<IExposable> saveablesToPostLoad = new HashSet<IExposable>();

	public void RegisterForPostLoadInit(IExposable s)
	{
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			Log.Error("Registered " + s?.ToString() + " for post load init, but current mode is " + Scribe.mode);
			return;
		}
		if (s == null)
		{
			Log.Warning("Trying to register null in RegisterforPostLoadInit.");
			return;
		}
		try
		{
			if (!saveablesToPostLoad.Add(s))
			{
				Log.Warning("Tried to register in RegisterforPostLoadInit when already registered: " + s);
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not register an object for post load init: " + ex);
		}
	}

	public void DoAllPostLoadInits()
	{
		Scribe.mode = LoadSaveMode.PostLoadInit;
		foreach (IExposable item in saveablesToPostLoad)
		{
			try
			{
				Scribe.loader.curParent = item;
				Scribe.loader.curPathRelToParent = null;
				item.ExposeData();
			}
			catch (Exception ex)
			{
				Log.Error("Could not do PostLoadInit on " + item.ToStringSafe() + ": " + ex);
			}
		}
		Clear();
		Scribe.loader.curParent = null;
		Scribe.loader.curPathRelToParent = null;
		Scribe.mode = LoadSaveMode.Inactive;
	}

	public void Clear()
	{
		saveablesToPostLoad.Clear();
	}
}
