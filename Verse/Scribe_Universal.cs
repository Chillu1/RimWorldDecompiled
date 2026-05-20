using System;
using System.Reflection;
using RimWorld.Planet;

namespace Verse;

public static class Scribe_Universal
{
	private const string LookInternalMiscName = "LookInternalMisc";

	private const string LookInternalDefName = "LookInternalDef";

	private const string LookInternalReferenceName = "LookInternalReference";

	public static void Look<TAny>(ref TAny obj, string label, LookMode lookMode, ref Type type)
	{
		if (lookMode == LookMode.Undefined)
		{
			Log.Error("Look mode can't be Undefined in Scribe_Universal.Look() if \"ref\" isn't used.");
			return;
		}
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			type = ((obj != null) ? obj.GetType() : typeof(TAny));
		}
		Scribe_Values.Look(ref type, label + "_type");
		if (Scribe.mode == LoadSaveMode.LoadingVars && type == null)
		{
			type = typeof(TAny);
		}
		object obj2 = obj;
		LookInt(ref obj2, label, lookMode, type);
		obj = ((obj2 is TAny) ? ((TAny)obj2) : default(TAny));
	}

	public static void Look<TAny>(ref TAny obj, string label, ref LookMode lookMode, ref Type type, bool forceDeepSave = false)
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			type = ((obj != null) ? obj.GetType() : typeof(TAny));
			if (!TryResolveLookMode(type, out lookMode, desperate: true, forceDeepSave))
			{
				Log.Error("Scribe_Universal could not resolve look mode for " + obj.ToStringSafe());
				return;
			}
		}
		Scribe_Values.Look(ref lookMode, label + "_lookMode", LookMode.Undefined);
		Scribe_Values.Look(ref type, label + "_type");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (type == null)
			{
				type = typeof(TAny);
			}
			if (lookMode == LookMode.Undefined && !TryResolveLookMode(type, out lookMode, desperate: true, forceDeepSave))
			{
				Log.Error("Scribe_Universal could not resolve look mode for " + obj.ToStringSafe());
				return;
			}
		}
		object obj2 = obj;
		LookInt(ref obj2, label, lookMode, type);
		obj = ((obj2 is TAny) ? ((TAny)obj2) : default(TAny));
	}

	private static void LookInt(ref object obj, string label, LookMode lookMode, Type type)
	{
		if (type == typeof(object))
		{
			type = typeof(string);
			lookMode = LookMode.Value;
		}
		string name = lookMode switch
		{
			LookMode.Def => "LookInternalDef", 
			LookMode.Reference => "LookInternalReference", 
			_ => "LookInternalMisc", 
		};
		object[] array = new object[3] { obj, label, lookMode };
		typeof(Scribe_Universal).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(null, array);
		obj = array[0];
	}

	private static void LookInternalMisc<T>(ref T obj, string label, LookMode lookMode)
	{
		switch (lookMode)
		{
		case LookMode.Value:
			Scribe_Values.Look(ref obj, label);
			break;
		case LookMode.Deep:
			Scribe_Deep.Look(ref obj, label);
			break;
		case LookMode.LocalTargetInfo:
		{
			LocalTargetInfo value3 = (LocalTargetInfo)(object)obj;
			Scribe_TargetInfo.Look(ref value3, label);
			obj = (T)(object)value3;
			break;
		}
		case LookMode.TargetInfo:
		{
			TargetInfo value2 = (TargetInfo)(object)obj;
			Scribe_TargetInfo.Look(ref value2, label);
			obj = (T)(object)value2;
			break;
		}
		case LookMode.GlobalTargetInfo:
		{
			GlobalTargetInfo value = (GlobalTargetInfo)(object)obj;
			Scribe_TargetInfo.Look(ref value, label);
			obj = (T)(object)value;
			break;
		}
		case LookMode.BodyPart:
		{
			BodyPartRecord part = (BodyPartRecord)(object)obj;
			Scribe_BodyParts.Look(ref part, label);
			obj = (T)(object)part;
			break;
		}
		case LookMode.Reference:
		case LookMode.Def:
			break;
		}
	}

	private static void LookInternalDef<T>(ref T obj, string label, LookMode lookMode) where T : Def, new()
	{
		Scribe_Defs.Look(ref obj, label);
	}

	private static void LookInternalReference<T>(ref T obj, string label, LookMode lookMode) where T : ILoadReferenceable
	{
		Scribe_References.Look(ref obj, label);
	}

	public static bool TryResolveLookMode(Type type, out LookMode lookMode, bool desperate = false, bool preferDeepIfDesperateAndAmbiguous = false)
	{
		if (type == null)
		{
			if (desperate)
			{
				lookMode = LookMode.Value;
				return true;
			}
			lookMode = LookMode.Undefined;
			return false;
		}
		if (type == typeof(object) && desperate)
		{
			lookMode = LookMode.Value;
			return true;
		}
		if (ParseHelper.HandlesType(type))
		{
			lookMode = LookMode.Value;
			return true;
		}
		if (type == typeof(LocalTargetInfo))
		{
			lookMode = LookMode.LocalTargetInfo;
			return true;
		}
		if (type == typeof(TargetInfo))
		{
			lookMode = LookMode.TargetInfo;
			return true;
		}
		if (type == typeof(GlobalTargetInfo))
		{
			lookMode = LookMode.GlobalTargetInfo;
			return true;
		}
		if (GenTypes.IsDef(type))
		{
			lookMode = LookMode.Def;
			return true;
		}
		if (type == typeof(BodyPartRecord))
		{
			lookMode = LookMode.BodyPart;
			return true;
		}
		if (typeof(IExposable).IsAssignableFrom(type) && !typeof(ILoadReferenceable).IsAssignableFrom(type))
		{
			lookMode = LookMode.Deep;
			return true;
		}
		if (desperate && typeof(ILoadReferenceable).IsAssignableFrom(type))
		{
			if (preferDeepIfDesperateAndAmbiguous)
			{
				lookMode = LookMode.Deep;
			}
			else
			{
				lookMode = LookMode.Reference;
			}
			return true;
		}
		lookMode = LookMode.Undefined;
		return false;
	}
}
