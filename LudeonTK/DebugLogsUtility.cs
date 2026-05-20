using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LudeonTK;

public class DebugLogsUtility
{
	public static string ThingListToUniqueCountString(IEnumerable<Thing> things)
	{
		if (things == null)
		{
			return "null";
		}
		Dictionary<ThingDef, int> dictionary = new Dictionary<ThingDef, int>();
		foreach (Thing thing in things)
		{
			if (!dictionary.ContainsKey(thing.def))
			{
				dictionary.Add(thing.def, 0);
			}
			dictionary[thing.def]++;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Registered things in dynamic draw list:");
		foreach (KeyValuePair<ThingDef, int> item in dictionary.OrderByDescending((KeyValuePair<ThingDef, int> k) => k.Value))
		{
			stringBuilder.AppendLine(item.Key?.ToString() + " - " + item.Value);
		}
		return stringBuilder.ToString();
	}
}
