using System;

namespace Verse
{
	public static class ModToolUtility
	{
		public static bool IsValueEditable(this Type type)
		{
			if (!type.IsValueType)
			{
				return type == typeof(string);
			}
			return true;
		}
	}
}
