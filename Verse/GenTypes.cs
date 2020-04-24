using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse
{
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

		public static readonly List<string> IgnoredNamespaceNames = new List<string>
		{
			"RimWorld",
			"Verse",
			"Verse.AI",
			"Verse.Sound",
			"Verse.Grammar",
			"RimWorld.Planet",
			"RimWorld.BaseGen",
			"RimWorld.QuestGen",
			"RimWorld.SketchGen",
			"System"
		};

		private static Dictionary<TypeCacheKey, Type> typeCache = new Dictionary<TypeCacheKey, Type>(EqualityComparer<TypeCacheKey>.Default);

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

		public static IEnumerable<Type> AllTypes
		{
			get
			{
				foreach (Assembly allActiveAssembly in AllActiveAssemblies)
				{
					Type[] array = null;
					try
					{
						array = allActiveAssembly.GetTypes();
					}
					catch (ReflectionTypeLoadException)
					{
						Log.Error("Exception getting types in assembly " + allActiveAssembly.ToString());
					}
					if (array != null)
					{
						Type[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							yield return array2[i];
						}
					}
				}
			}
		}

		public static IEnumerable<Type> AllTypesWithAttribute<TAttr>() where TAttr : Attribute
		{
			return AllTypes.Where((Type x) => x.HasAttribute<TAttr>());
		}

		public static IEnumerable<Type> AllSubclasses(this Type baseType)
		{
			return AllTypes.Where((Type x) => x.IsSubclassOf(baseType));
		}

		public static IEnumerable<Type> AllSubclassesNonAbstract(this Type baseType)
		{
			return AllTypes.Where((Type x) => x.IsSubclassOf(baseType) && !x.IsAbstract);
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

		public static Type GetTypeInAnyAssembly(string typeName, string namespaceIfAmbiguous = null)
		{
			TypeCacheKey key = new TypeCacheKey(typeName, namespaceIfAmbiguous);
			Type value = null;
			if (!typeCache.TryGetValue(key, out value))
			{
				value = GetTypeInAnyAssemblyInt(typeName, namespaceIfAmbiguous);
				typeCache.Add(key, value);
			}
			return value;
		}

		private static Type GetTypeInAnyAssemblyInt(string typeName, string namespaceIfAmbiguous = null)
		{
			Type typeInAnyAssemblyRaw = GetTypeInAnyAssemblyRaw(typeName);
			if (typeInAnyAssemblyRaw != null)
			{
				return typeInAnyAssemblyRaw;
			}
			if (!namespaceIfAmbiguous.NullOrEmpty() && IgnoredNamespaceNames.Contains(namespaceIfAmbiguous))
			{
				typeInAnyAssemblyRaw = GetTypeInAnyAssemblyRaw(namespaceIfAmbiguous + "." + typeName);
				if (typeInAnyAssemblyRaw != null)
				{
					return typeInAnyAssemblyRaw;
				}
			}
			for (int i = 0; i < IgnoredNamespaceNames.Count; i++)
			{
				typeInAnyAssemblyRaw = GetTypeInAnyAssemblyRaw(IgnoredNamespaceNames[i] + "." + typeName);
				if (typeInAnyAssemblyRaw != null)
				{
					return typeInAnyAssemblyRaw;
				}
			}
			return null;
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
				foreach (Assembly allActiveAssembly in AllActiveAssemblies)
				{
					Type type = allActiveAssembly.GetType(typeName, throwOnError: false, ignoreCase: true);
					if (type != null)
					{
						return type;
					}
				}
				Type type2 = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
				if (type2 != null)
				{
					return type2;
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
			for (int i = 0; i < IgnoredNamespaceNames.Count; i++)
			{
				if (type.Namespace == IgnoredNamespaceNames[i])
				{
					return type.Name;
				}
			}
			return type.FullName;
		}

		public static bool IsCustomType(Type type)
		{
			string @namespace = type.Namespace;
			if (!@namespace.StartsWith("System") && !@namespace.StartsWith("UnityEngine"))
			{
				return !@namespace.StartsWith("Steamworks");
			}
			return false;
		}
	}
}
