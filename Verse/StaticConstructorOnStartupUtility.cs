using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Verse;

public static class StaticConstructorOnStartupUtility
{
	public static bool coreStaticAssetsLoaded;

	public static void CallAll()
	{
		DeepProfiler.Start("StaticConstructorOnStartupUtility.CallAll()");
		foreach (Type item in GenTypes.AllTypesWithAttribute<StaticConstructorOnStartup>())
		{
			try
			{
				RuntimeHelpers.RunClassConstructor(item.TypeHandle);
			}
			catch (Exception ex)
			{
				Log.Error("Error in static constructor of " + item?.ToString() + ": " + ex);
			}
		}
		DeepProfiler.End();
		coreStaticAssetsLoaded = true;
	}

	public static void ReportProbablyMissingAttributes()
	{
		BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		Parallel.ForEach(GenTypes.AllTypes, delegate(Type t)
		{
			if (!t.HasAttribute<StaticConstructorOnStartup>())
			{
				FieldInfo fieldInfo = t.GetFields(bindingFlags).FirstOrDefault(delegate(FieldInfo x)
				{
					Type type = x.FieldType;
					if (type.IsArray)
					{
						type = type.GetElementType();
					}
					return typeof(Texture).IsAssignableFrom(type) || typeof(Material).IsAssignableFrom(type) || typeof(Shader).IsAssignableFrom(type) || typeof(Graphic).IsAssignableFrom(type) || typeof(GameObject).IsAssignableFrom(type) || typeof(MaterialPropertyBlock).IsAssignableFrom(type);
				});
				if (fieldInfo != null)
				{
					Log.Warning("Type " + t.Name + " probably needs a StaticConstructorOnStartup attribute, because it has a field " + fieldInfo.Name + " of type " + fieldInfo.FieldType.Name + ". All assets must be loaded in the main thread.");
				}
			}
		});
	}
}
