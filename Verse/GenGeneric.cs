using System;
using System.Reflection;

namespace Verse
{
	public static class GenGeneric
	{
		public const BindingFlags BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static MethodInfo MethodOnGenericType(Type genericBase, Type genericParam, string methodName)
		{
			return genericBase.MakeGenericType(genericParam).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static void InvokeGenericMethod(object objectToInvoke, Type genericParam, string methodName, params object[] args)
		{
			objectToInvoke.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).MakeGenericMethod(genericParam)
				.Invoke(objectToInvoke, args);
		}

		public static object InvokeStaticMethodOnGenericType(Type genericBase, Type genericParam, string methodName, params object[] args)
		{
			return MethodOnGenericType(genericBase, genericParam, methodName).Invoke(null, args);
		}

		public static object InvokeStaticMethodOnGenericType(Type genericBase, Type genericParam, string methodName)
		{
			return MethodOnGenericType(genericBase, genericParam, methodName).Invoke(null, null);
		}

		public static object InvokeStaticGenericMethod(Type baseClass, Type genericParam, string methodName)
		{
			return baseClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).MakeGenericMethod(genericParam).Invoke(null, null);
		}

		public static object InvokeStaticGenericMethod(Type baseClass, Type genericParam, string methodName, params object[] args)
		{
			return baseClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).MakeGenericMethod(genericParam).Invoke(null, args);
		}

		private static PropertyInfo PropertyOnGenericType(Type genericBase, Type genericParam, string propertyName)
		{
			return genericBase.MakeGenericType(genericParam).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static object GetStaticPropertyOnGenericType(Type genericBase, Type genericParam, string propertyName)
		{
			return PropertyOnGenericType(genericBase, genericParam, propertyName).GetGetMethod().Invoke(null, null);
		}

		public static bool HasGenericDefinition(this Type type, Type Def)
		{
			return type.GetTypeWithGenericDefinition(Def) != null;
		}

		public static Type GetTypeWithGenericDefinition(this Type type, Type Def)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (Def == null)
			{
				throw new ArgumentNullException("Def");
			}
			if (!Def.IsGenericTypeDefinition)
			{
				throw new ArgumentException("The Def needs to be a GenericTypeDefinition", "Def");
			}
			if (Def.IsInterface)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					if (type2.IsGenericType && type2.GetGenericTypeDefinition() == Def)
					{
						return type2;
					}
				}
			}
			Type type3 = type;
			while (type3 != null)
			{
				if (type3.IsGenericType && type3.GetGenericTypeDefinition() == Def)
				{
					return type3;
				}
				type3 = type3.BaseType;
			}
			return null;
		}
	}
}
