using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.QuestGen;

namespace Verse;

public static class GenTypes
{
	private struct TypeCacheKey : IEquatable<TypeCacheKey>
	{
		public string typeName;

		public string namespaceIfAmbiguous;

		public override int GetHashCode()
		{
			if (namespaceIfAmbiguous == null)
			{
				return typeName.GetHashCode();
			}
			return (17 * 31 + typeName.GetHashCode()) * 31 + namespaceIfAmbiguous.GetHashCode();
		}

		public bool Equals(TypeCacheKey other)
		{
			if (string.Equals(typeName, other.typeName))
			{
				return string.Equals(namespaceIfAmbiguous, other.namespaceIfAmbiguous);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TypeCacheKey)
			{
				return Equals((TypeCacheKey)obj);
			}
			return false;
		}

		public TypeCacheKey(string typeName, string namespaceIfAmbigous = null)
		{
			this.typeName = typeName;
			namespaceIfAmbiguous = namespaceIfAmbigous;
		}
	}

	public static readonly HashSet<string> IgnoredNamespaceNames = new HashSet<string>
	{
		"RimWorld", "Verse", "LudeonTK", "Verse.AI", "Verse.AI.Group", "Verse.Sound", "Verse.Grammar", "RimWorld.Planet", "RimWorld.BaseGen", "RimWorld.QuestGen",
		"RimWorld.SketchGen", "System"
	};

	private static List<Type> allTypesCached;

	private static HashSet<Type> tmpAllTypesHashSet = new HashSet<Type>();

	private static Dictionary<Type, List<Type>> cachedTypesWithAttribute = new Dictionary<Type, List<Type>>();

	private static Dictionary<Type, List<Type>> cachedSubclasses = new Dictionary<Type, List<Type>>();

	private static Dictionary<Type, List<Type>> cachedSubclassesNonAbstract = new Dictionary<Type, List<Type>>();

	private static readonly Dictionary<Type, List<Type>> cachedAssignableNonAbstract = new Dictionary<Type, List<Type>>();

	private static readonly Dictionary<string, Type> typesInIgnoredNamespacesByName = new Dictionary<string, Type>();

	private static bool initializedTypesInIgnoredNamespaces = false;

	private static Dictionary<TypeCacheKey, Type> typeCache = new Dictionary<TypeCacheKey, Type>(EqualityComparer<TypeCacheKey>.Default);

	private static Dictionary<Type, bool> hasFlagsAttributeCache = new Dictionary<Type, bool>();

	private static Dictionary<Type, bool> isListCache = new Dictionary<Type, bool>();

	private static Dictionary<Type, bool> isDictionaryCache = new Dictionary<Type, bool>();

	private static Dictionary<Type, bool> isSlateRefCache = new Dictionary<Type, bool>();

	private static Dictionary<Type, bool> isListHashSetOrDictionaryCached = new Dictionary<Type, bool>();

	private static Dictionary<Type, bool> isDefCache = new Dictionary<Type, bool>();

	private static object isDefCacheLock = new object();

	private static readonly Dictionary<Type, bool> isSealedCache = new Dictionary<Type, bool>();

	private static IEnumerable<Assembly> AllActiveAssemblies
	{
		get
		{
			yield return Assembly.GetExecutingAssembly();
			foreach (ModContentPack mod in LoadedModManager.RunningMods)
			{
				for (int i = 0; i < mod.assemblies.loadedAssemblies.Count; i++)
				{
					yield return mod.assemblies.loadedAssemblies[i];
				}
			}
		}
	}

	public static List<Type> AllTypes
	{
		get
		{
			if (allTypesCached == null)
			{
				allTypesCached = new List<Type>();
				tmpAllTypesHashSet.Clear();
				foreach (Assembly allActiveAssembly in AllActiveAssemblies)
				{
					Type[] array = null;
					try
					{
						array = allActiveAssembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						Log.Error("Exception getting types in assembly " + allActiveAssembly.ToString() + ". Some types may not work correctly. Exception: " + ex);
						try
						{
							Type[] types = ex.Types;
							if (types != null)
							{
								array = types.Where((Type x) => x != null && x.TypeInitializer != null).ToArray();
							}
						}
						catch (Exception ex2)
						{
							Log.Error("Could not resolve assembly types fallback. Exception: " + ex2);
						}
					}
					if (array == null)
					{
						continue;
					}
					for (int num = 0; num < array.Length; num++)
					{
						if (array[num] != null && tmpAllTypesHashSet.Add(array[num]))
						{
							allTypesCached.Add(array[num]);
						}
					}
				}
				tmpAllTypesHashSet.Clear();
			}
			return allTypesCached;
		}
	}

	public static List<Type> AllTypesWithAttribute<TAttr>() where TAttr : Attribute
	{
		if (cachedTypesWithAttribute.TryGetValue(typeof(TAttr), out var value))
		{
			return value;
		}
		List<Type> list = (from x in AllTypes.AsParallel()
			where x.HasAttribute<TAttr>()
			select x).ToList();
		cachedTypesWithAttribute.Add(typeof(TAttr), list);
		return list;
	}

	public static List<Type> AllSubclasses(this Type baseType)
	{
		if (!cachedSubclasses.ContainsKey(baseType))
		{
			cachedSubclasses.Add(baseType, (from x in AllTypes.AsParallel()
				where x.IsSubclassOf(baseType)
				select x).ToList());
		}
		return cachedSubclasses[baseType];
	}

	public static bool SameOrSubclassOf<T>(this Type baseType)
	{
		return baseType.SameOrSubclassOf(typeof(T));
	}

	public static bool SameOrSubclassOf(this Type baseType, Type parentType)
	{
		if (!(baseType == parentType))
		{
			return baseType.IsSubclassOf(parentType);
		}
		return true;
	}

	public static List<Type> AllSubclassesNonAbstract(this Type baseType)
	{
		if (!cachedSubclassesNonAbstract.ContainsKey(baseType))
		{
			cachedSubclassesNonAbstract.Add(baseType, (from x in AllTypes.AsParallel()
				where x.IsSubclassOf(baseType) && !x.IsAbstract
				select x).ToList());
		}
		return cachedSubclassesNonAbstract[baseType];
	}

	public static List<Type> AllAssignableOfNonAbstract(this Type baseType)
	{
		if (!cachedAssignableNonAbstract.ContainsKey(baseType))
		{
			cachedAssignableNonAbstract.Add(baseType, (from x in AllTypes.AsParallel()
				where baseType.IsAssignableFrom(x) && !x.IsAbstract
				select x).ToList());
		}
		return cachedAssignableNonAbstract[baseType];
	}

	public static void ClearCache()
	{
		cachedSubclasses.Clear();
		cachedSubclassesNonAbstract.Clear();
		cachedAssignableNonAbstract.Clear();
		cachedTypesWithAttribute.Clear();
		allTypesCached = null;
		typesInIgnoredNamespacesByName.Clear();
		isSealedCache.Clear();
		initializedTypesInIgnoredNamespaces = false;
		AlertsReadout.allAlertTypesCached = null;
	}

	public static IEnumerable<Type> AllLeafSubclasses(this Type baseType)
	{
		return from type in baseType.AllSubclasses()
			where !type.AllSubclasses().Any()
			select type;
	}

	public static IEnumerable<Type> InstantiableDescendantsAndSelf(this Type baseType)
	{
		if (!baseType.IsAbstract)
		{
			yield return baseType;
		}
		foreach (Type item in baseType.AllSubclasses())
		{
			if (!item.IsAbstract)
			{
				yield return item;
			}
		}
	}

	private static Type TryGetTypeInIgnoredNamespace(string typeName)
	{
		if (!initializedTypesInIgnoredNamespaces)
		{
			foreach (Type allType in AllTypes)
			{
				if (allType.Namespace == null || IgnoredNamespaceNames.Contains(allType.Namespace))
				{
					typesInIgnoredNamespacesByName[allType.Name] = allType;
				}
			}
			initializedTypesInIgnoredNamespaces = true;
		}
		if (typesInIgnoredNamespacesByName.TryGetValue(typeName, out var value))
		{
			return value;
		}
		return null;
	}

	public static Type GetTypeInAnyAssembly(string typeName, string namespaceIfAmbiguous = null)
	{
		if (namespaceIfAmbiguous == null || IgnoredNamespaceNames.Contains(namespaceIfAmbiguous))
		{
			Type type = TryGetTypeInIgnoredNamespace(typeName);
			if (type != null)
			{
				return type;
			}
		}
		TypeCacheKey key = new TypeCacheKey(typeName, namespaceIfAmbiguous);
		Type value = null;
		if (!typeCache.TryGetValue(key, out value))
		{
			value = GetTypeInAnyAssemblyInt(typeName, namespaceIfAmbiguous);
			typeCache.Add(key, value);
		}
		return value;
	}

	public static bool HasFlagsAttribute(Type type)
	{
		if (hasFlagsAttributeCache.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = Attribute.IsDefined(type, typeof(FlagsAttribute));
		hasFlagsAttributeCache.Add(type, flag);
		return flag;
	}

	public static bool IsList(Type type)
	{
		if (isListCache.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = type.HasGenericDefinition(typeof(List<>));
		isListCache.Add(type, flag);
		return flag;
	}

	public static bool IsDictionary(Type type)
	{
		if (isDictionaryCache.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = type.HasGenericDefinition(typeof(Dictionary<, >));
		isDictionaryCache.Add(type, flag);
		return flag;
	}

	public static bool IsSlateRef(Type type)
	{
		if (isSlateRefCache.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = typeof(ISlateRef).IsAssignableFrom(type);
		isSlateRefCache.Add(type, flag);
		return flag;
	}

	public static bool IsListHashSetOrDictionary(Type type)
	{
		if (isListHashSetOrDictionaryCached.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = false;
		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(HashSet<>) || genericTypeDefinition == typeof(Dictionary<, >))
			{
				flag = true;
			}
		}
		isListHashSetOrDictionaryCached.Add(type, flag);
		return flag;
	}

	public static bool IsDef(Type type)
	{
		if (isDefCache.TryGetValue(type, out var value))
		{
			return value;
		}
		bool flag = typeof(Def).IsAssignableFrom(type);
		isDefCache.Add(type, flag);
		return flag;
	}

	public static bool IsDefThreaded(Type type)
	{
		lock (isDefCacheLock)
		{
			return IsDef(type);
		}
	}

	private static Type GetTypeInAnyAssemblyInt(string typeName, string namespaceIfAmbiguous = null)
	{
		Type type = GetTypeInAnyAssemblyRaw(typeName);
		if (type != null)
		{
			return type;
		}
		if (!namespaceIfAmbiguous.NullOrEmpty() && IgnoredNamespaceNames.Contains(namespaceIfAmbiguous))
		{
			type = GetTypeInAnyAssemblyRaw(namespaceIfAmbiguous + "." + typeName);
			if (type != null)
			{
				return type;
			}
		}
		foreach (string ignoredNamespaceName in IgnoredNamespaceNames)
		{
			type = GetTypeInAnyAssemblyRaw(ignoredNamespaceName + "." + typeName);
			if (type != null)
			{
				return type;
			}
		}
		if (TryGetMixedAssemblyGenericType(typeName, out type))
		{
			return type;
		}
		return null;
	}

	private static bool TryGetMixedAssemblyGenericType(string typeName, out Type type)
	{
		type = GetTypeInAnyAssemblyRaw(typeName);
		if (type == null && typeName.Contains("`"))
		{
			try
			{
				Match match = Regex.Match(typeName, "(?<MainType>.+`(?<ParamCount>[0-9]+))(?<Types>\\[.*\\])");
				if (match.Success)
				{
					int capacity = int.Parse(match.Groups["ParamCount"].Value);
					string value = match.Groups["Types"].Value;
					List<string> list = new List<string>(capacity);
					foreach (Match item in Regex.Matches(value, "\\[(?<Type>.*?)\\],?"))
					{
						if (item.Success)
						{
							list.Add(item.Groups["Type"].Value.Trim());
						}
					}
					Type[] array = new Type[list.Count];
					for (int i = 0; i < list.Count; i++)
					{
						if (TryGetMixedAssemblyGenericType(list[i], out var type2))
						{
							array[i] = type2;
							continue;
						}
						return false;
					}
					if (TryGetMixedAssemblyGenericType(match.Groups["MainType"].Value, out var type3))
					{
						type = type3.MakeGenericType(array);
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce("Error in TryGetMixedAssemblyGenericType with typeName=" + typeName + ": " + ex, typeName.GetHashCode());
			}
		}
		return type != null;
	}

	private static Type GetTypeInAnyAssemblyRaw(string typeName)
	{
		switch (typeName)
		{
		case "int":
			return typeof(int);
		case "uint":
			return typeof(uint);
		case "short":
			return typeof(short);
		case "ushort":
			return typeof(ushort);
		case "float":
			return typeof(float);
		case "double":
			return typeof(double);
		case "long":
			return typeof(long);
		case "ulong":
			return typeof(ulong);
		case "byte":
			return typeof(byte);
		case "sbyte":
			return typeof(sbyte);
		case "char":
			return typeof(char);
		case "bool":
			return typeof(bool);
		case "decimal":
			return typeof(decimal);
		case "string":
			return typeof(string);
		case "int?":
			return typeof(int?);
		case "uint?":
			return typeof(uint?);
		case "short?":
			return typeof(short?);
		case "ushort?":
			return typeof(ushort?);
		case "float?":
			return typeof(float?);
		case "double?":
			return typeof(double?);
		case "long?":
			return typeof(long?);
		case "ulong?":
			return typeof(ulong?);
		case "byte?":
			return typeof(byte?);
		case "sbyte?":
			return typeof(sbyte?);
		case "char?":
			return typeof(char?);
		case "bool?":
			return typeof(bool?);
		case "decimal?":
			return typeof(decimal?);
		default:
		{
			Type type = typeof(GenTypes).Assembly.GetType(typeName, throwOnError: false, ignoreCase: true);
			if (type != null)
			{
				return type;
			}
			foreach (Assembly allActiveAssembly in AllActiveAssemblies)
			{
				Type type2 = allActiveAssembly.GetType(typeName, throwOnError: false, ignoreCase: true);
				if (type2 != null)
				{
					return type2;
				}
			}
			Type type3 = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
			if (type3 != null)
			{
				return type3;
			}
			return null;
		}
		}
	}

	public static string GetTypeNameWithoutIgnoredNamespaces(Type type)
	{
		if (type.IsGenericType)
		{
			return type.ToString();
		}
		if (IgnoredNamespaceNames.Contains(type.Namespace))
		{
			return type.Name;
		}
		return type.FullName;
	}

	public static bool IsCustomType(Type type)
	{
		string text = type.Namespace;
		if (text != null)
		{
			if (!text.StartsWith("System") && !text.StartsWith("UnityEngine"))
			{
				return !text.StartsWith("Steamworks");
			}
			return false;
		}
		return true;
	}

	public static bool Isnt<T>(this object obj)
	{
		return !(obj is T);
	}

	public static bool Isnt<T>(this object obj, out T casted) where T : class
	{
		casted = obj as T;
		return casted == null;
	}

	public static bool IsSealedWithCache(this Type type)
	{
		lock (isSealedCache)
		{
			if (isSealedCache.TryGetValue(type, out var value))
			{
				return value;
			}
			return isSealedCache[type] = type.IsSealed;
		}
	}

	public static bool IsMethodOverriden(this Type type, string method)
	{
		return type.GetMethod(method).IsOverriden();
	}

	public static bool IsOverriden(this MethodInfo method)
	{
		if (method.IsVirtual)
		{
			return method.DeclaringType != method.GetBaseDefinition().DeclaringType;
		}
		return false;
	}
}
