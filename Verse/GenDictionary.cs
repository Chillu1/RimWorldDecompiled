using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class GenDictionary
	{
		public static string ToStringFullContents<K, V>(this Dictionary<K, V> dict)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<K, V> item in dict)
			{
				stringBuilder.AppendLine(item.Key.ToString() + ": " + item.Value.ToString());
			}
			return stringBuilder.ToString();
		}

		public static bool NullOrEmpty<K, V>(this Dictionary<K, V> dict)
		{
			if (dict != null)
			{
				return dict.Count == 0;
			}
			return true;
		}
	}
}
