using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GenDefDatabase
	{
		public static Def GetDef(Type defType, string defName, bool errorOnFail = true)
		{
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "GetNamed", defName, errorOnFail);
		}

		public static Def GetDefSilentFail(Type type, string targetDefName, bool specialCaseForSoundDefs = true)
		{
			if (specialCaseForSoundDefs && type == typeof(SoundDef))
			{
				return SoundDef.Named(targetDefName);
			}
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, "GetNamedSilentFail", targetDefName);
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
}
