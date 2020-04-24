using System;
using System.Collections.Generic;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DefaultEmptyListAttribute : DefaultValueAttribute
	{
		public DefaultEmptyListAttribute(Type type)
			: base(type)
		{
		}

		public override bool ObjIsDefault(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType().GetGenericTypeDefinition() != typeof(List<>))
			{
				return false;
			}
			Type[] genericArguments = obj.GetType().GetGenericArguments();
			if (genericArguments.Length != 1 || genericArguments[0] != (Type)value)
			{
				return false;
			}
			if ((int)obj.GetType().GetProperty("Count").GetValue(obj, null) != 0)
			{
				return false;
			}
			return true;
		}
	}
}
