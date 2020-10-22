using System;
using System.Reflection;

namespace Verse
{
	public static class GenAttribute
	{
		public static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
		{
			T customAttribute;
			return memberInfo.TryGetAttribute<T>(out customAttribute);
		}

		public static bool TryGetAttribute<T>(this MemberInfo memberInfo, out T customAttribute) where T : Attribute
		{
			object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherit: true);
			if (customAttributes.Length == 0)
			{
				customAttribute = null;
				return false;
			}
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (customAttributes[i] is T)
				{
					customAttribute = (T)customAttributes[i];
					return true;
				}
			}
			customAttribute = null;
			return false;
		}

		public static T TryGetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
		{
			T customAttribute = null;
			memberInfo.TryGetAttribute<T>(out customAttribute);
			return customAttribute;
		}
	}
}
