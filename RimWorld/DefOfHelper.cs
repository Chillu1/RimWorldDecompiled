using System;
using System.Reflection;
using System.Threading.Tasks;
using Verse;

namespace RimWorld;

public static class DefOfHelper
{
	private static bool bindingNow;

	private static bool earlyTry = true;

	public static void RebindAllDefOfs(bool earlyTryMode)
	{
		earlyTry = earlyTryMode;
		bindingNow = true;
		try
		{
			Parallel.ForEach(GenTypes.AllTypesWithAttribute<DefOf>(), BindDefsFor);
		}
		finally
		{
			bindingNow = false;
		}
	}

	private static void BindDefsFor(Type type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (!GenTypes.IsDefThreaded(fieldType))
			{
				Log.Error(fieldType?.ToString() + " is not a Def.");
				continue;
			}
			bool flag = !earlyTry;
			MayRequireAttribute mayRequireAttribute = fieldInfo.TryGetAttribute<MayRequireAttribute>();
			if (mayRequireAttribute != null && !ModsConfig.IsActive(mayRequireAttribute.modId))
			{
				flag = false;
			}
			MayRequireAnyOfAttribute mayRequireAnyOfAttribute = fieldInfo.TryGetAttribute<MayRequireAnyOfAttribute>();
			if (mayRequireAnyOfAttribute != null)
			{
				bool flag2 = false;
				string[] modIds = mayRequireAnyOfAttribute.modIds;
				for (int j = 0; j < modIds.Length; j++)
				{
					if (ModsConfig.IsActive(modIds[j]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
				}
			}
			string text = fieldInfo.Name;
			DefAliasAttribute defAliasAttribute = fieldInfo.TryGetAttribute<DefAliasAttribute>();
			if (defAliasAttribute != null)
			{
				text = defAliasAttribute.defName;
			}
			if (fieldType == typeof(SoundDef))
			{
				SoundDef soundDef = SoundDef.Named(text);
				if (soundDef.isUndefined && flag)
				{
					Log.Error("Could not find SoundDef named " + text);
				}
				fieldInfo.SetValue(null, soundDef);
			}
			else
			{
				Def def = GenDefDatabase.GetDef(fieldType, text, flag);
				fieldInfo.SetValue(null, def);
			}
		}
	}

	public static void EnsureInitializedInCtor(Type defOf)
	{
		if (!bindingNow)
		{
			string text = (DirectXmlToObject.currentlyInstantiatingObjectOfType.Any() ? ("DirectXmlToObject is currently instantiating an object of type " + DirectXmlToObject.currentlyInstantiatingObjectOfType.Peek()) : ((Scribe.mode != LoadSaveMode.LoadingVars) ? "" : ("curParent=" + Scribe.loader.curParent.ToStringSafe() + " curPathRelToParent=" + Scribe.loader.curPathRelToParent)));
			Log.Warning("Tried to use an uninitialized DefOf of type " + defOf.Name + ". DefOfs are initialized right after all defs all loaded. Uninitialized DefOfs will return only nulls. (hint: don't use DefOfs as default field values in Defs, try to resolve them in ResolveReferences() instead)" + (text.NullOrEmpty() ? "" : (" Debug info: " + text)));
		}
		if (GenTypes.IsDefThreaded(defOf))
		{
			Log.Warning("Possible typo: " + defOf.Name + ". Using def type name not preceded by \"Of\"");
		}
	}
}
