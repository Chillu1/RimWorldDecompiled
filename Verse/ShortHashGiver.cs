using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Verse;

public static class ShortHashGiver
{
	private static Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = new Dictionary<Type, HashSet<ushort>>();

	public static void GiveAllShortHashes()
	{
		takenHashesPerDeftype.Clear();
		Parallel.ForEach(GenDefDatabase.AllDefTypesWithDatabases().Select(delegate(Type defType)
		{
			if (!takenHashesPerDeftype.TryGetValue(defType, out var value))
			{
				value = new HashSet<ushort>();
				takenHashesPerDeftype.Add(defType, value);
			}
			return (defType: defType, takenHashes: value);
		}).ToArray(), delegate((Type defType, HashSet<ushort> takenHashes) pair)
		{
			(Type defType, HashSet<ushort> takenHashes) tuple = pair;
			Type item = tuple.defType;
			HashSet<ushort> item2 = tuple.takenHashes;
			Type type = typeof(DefDatabase<>).MakeGenericType(item);
			IEnumerable obj = (IEnumerable)type.GetProperty("AllDefs").GetGetMethod().Invoke(null, null);
			List<Def> list = new List<Def>();
			foreach (Def item3 in obj)
			{
				list.Add(item3);
			}
			list.SortBy((Def d) => d.defName);
			for (int num = 0; num < list.Count; num++)
			{
				GiveShortHash(list[num], item, item2);
			}
			type.GetMethod("InitializeShortHashDictionary").Invoke(null, null);
		});
	}

	private static void GiveShortHash(Def def, Type defType, HashSet<ushort> takenHashes)
	{
		if (def.shortHash != 0)
		{
			Log.Error(def?.ToString() + " already has short hash.");
			return;
		}
		ushort num = (ushort)(GenText.StableStringHash(def.defName) % 65535);
		int num2 = 0;
		while (num == 0 || takenHashes.Contains(num))
		{
			num++;
			num2++;
			if (num2 > 5000)
			{
				Log.Message("Short hashes are saturated. There are probably too many Defs.");
			}
		}
		def.shortHash = num;
		takenHashes.Add(num);
	}
}
