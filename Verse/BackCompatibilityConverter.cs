using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse;

public abstract class BackCompatibilityConverter
{
	private readonly Dictionary<object, Dictionary<string, object>> typeToTypeLookup = new Dictionary<object, Dictionary<string, object>>();

	public abstract bool AppliesToVersion(int majorVer, int minorVer);

	public abstract string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null);

	public abstract Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node);

	public virtual int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
	{
		return index;
	}

	public abstract void PostExposeData(object obj);

	public virtual void PostCouldntLoadDef(string defName)
	{
	}

	public virtual void PreLoadSavegame(string loadingVersion)
	{
	}

	public virtual void PostLoadSavegame(string loadingVersion)
	{
	}

	public bool AppliesToLoadedGameVersion(bool allowInactiveScribe = false)
	{
		if (ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty())
		{
			return false;
		}
		if (!allowInactiveScribe && Scribe.mode == LoadSaveMode.Inactive)
		{
			return false;
		}
		return AppliesToVersion(ScribeMetaHeaderUtility.loadedGameVersionMajor, ScribeMetaHeaderUtility.loadedGameVersionMinor);
	}

	protected void Scribe_TypeToType<T1, T2>(object obj, ref T2 v, string label, Func<T1, T2> converter) where T2 : new()
	{
		if (!typeToTypeLookup.ContainsKey(obj))
		{
			typeToTypeLookup[obj] = new Dictionary<string, object>();
		}
		object value = typeToTypeLookup[obj][label];
		Scribe_Values.Look(ref value, label);
		typeToTypeLookup[obj][label] = value;
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			v = converter((T1)typeToTypeLookup[obj][label]);
		}
	}
}
