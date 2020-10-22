using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse
{
	public static class ShortHashGiver
	{
		private static Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = new Dictionary<Type, HashSet<ushort>>();

		public static void GiveAllShortHashes()
		{
			takenHashesPerDeftype.Clear();
			List<Def> list = new List<Def>();
			foreach (Type item2 in GenDefDatabase.AllDefTypesWithDatabases())
			{
				IEnumerable obj = (IEnumerable)typeof(DefDatabase<>).MakeGenericType(item2).GetProperty("AllDefs").GetGetMethod()
					.Invoke(null, null);
				list.Clear();
				foreach (Def item3 in obj)
				{
					list.Add(item3);
				}
				list.SortBy((Def d) => d.defName);
				for (int i = 0; i < list.Count; i++)
				{
					GiveShortHash(list[i], item2);
				}
			}
		}

		private static void GiveShortHash(Def def, Type defType)
		{
			if (def.shortHash != 0)
			{
				Log.Error(string.Concat(def, " already has short hash."));
				return;
			}
			if (!takenHashesPerDeftype.TryGetValue(defType, out var value))
			{
				value = new HashSet<ushort>();
				takenHashesPerDeftype.Add(defType, value);
			}
			ushort num = (ushort)(GenText.StableStringHash(def.defName) % 65535);
			int num2 = 0;
			while (num == 0 || value.Contains(num))
			{
				num = (ushort)(num + 1);
				num2++;
				if (num2 > 5000)
				{
					Log.Message("Short hashes are saturated. There are probably too many Defs.");
				}
			}
			def.shortHash = num;
			value.Add(num);
		}
	}
}
