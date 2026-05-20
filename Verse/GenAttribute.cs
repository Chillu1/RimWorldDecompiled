using System;
using System.Reflection;

namespace Verse;

public static class GenAttribute
{
	public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
	{
		return Attribute.IsDefined(memberInfo, typeof(T), inherit: true);
	}

	public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T customAttribute) where T : Attribute
	{
		if (!memberInfo.HasAttribute<T>())
		{
			customAttribute = null;
			return false;
		}
		Attribute[] customAttributes = Attribute.GetCustomAttributes(memberInfo, typeof(T), inherit: true);
		if (customAttributes.Length == 1)
		{
			return (customAttribute = customAttributes[0] as T) != null;
		}
		if (customAttributes.Length == 0)
		{
			customAttribute = null;
			return false;
		}
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is T val)
			{
				customAttribute = val;
				return true;
			}
		}
		customAttribute = null;
		return false;
	}

	public static T TryGetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
	{
		memberInfo.TryGetAttribute<T>(out var customAttribute);
		return customAttribute;
	}
}
