using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse;

public static class GenDefDatabase
{
	private static readonly Dictionary<Type, Func<string, bool, Def>> cachedGetNamed = new Dictionary<Type, Func<string, bool, Def>>();

	private static readonly object cachedGetNamedLock = new object();

	private static readonly Dictionary<Type, Func<string, Def>> cachedGetNamedSilentFail = new Dictionary<Type, Func<string, Def>>();

	private static readonly object cachedGetNamedSilentFailLock = new object();

	public static Def GetDef(Type defType, string defName, bool errorOnFail = true)
	{
		Func<string, bool, Def> value;
		lock (cachedGetNamedLock)
		{
			if (!cachedGetNamed.TryGetValue(defType, out value))
			{
				MethodInfo method = GenGeneric.MethodOnGenericType(typeof(DefDatabase<>), defType, "GetNamed");
				value = (Func<string, bool, Def>)Delegate.CreateDelegate(typeof(Func<string, bool, Def>), method);
				cachedGetNamed.Add(defType, value);
			}
		}
		return value(defName, errorOnFail);
	}

	public static Def GetDefSilentFail(Type type, string targetDefName, bool specialCaseForSoundDefs = true)
	{
		if (specialCaseForSoundDefs && type == typeof(SoundDef))
		{
			return SoundDef.Named(targetDefName);
		}
		Func<string, Def> value;
		lock (cachedGetNamedSilentFailLock)
		{
			if (!cachedGetNamedSilentFail.TryGetValue(type, out value))
			{
				MethodInfo method = GenGeneric.MethodOnGenericType(typeof(DefDatabase<>), type, "GetNamedSilentFail");
				value = (Func<string, Def>)Delegate.CreateDelegate(typeof(Func<string, Def>), method);
				cachedGetNamedSilentFail.Add(type, value);
			}
		}
		return value(targetDefName);
	}

	public static IEnumerable<Def> GetAllDefsInDatabaseForDef(Type defType)
	{
		return ((IEnumerable)GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), defType, "AllDefs")).Cast<Def>();
	}

	public static IEnumerable<Type> AllDefTypesWithDatabases()
	{
		foreach (Type item in typeof(Def).AllSubclasses())
		{
			if (item.IsAbstract || item == typeof(Def))
			{
				continue;
			}
			bool flag = false;
			Type baseType = item.BaseType;
			while (baseType != null && baseType != typeof(Def))
			{
				if (!baseType.IsAbstract)
				{
					flag = true;
					break;
				}
				baseType = baseType.BaseType;
			}
			if (!flag)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> DefsToGoInDatabase<T>(ModContentPack mod)
	{
		return mod.AllDefs.OfType<T>();
	}
}
