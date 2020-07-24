using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
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
				stringBuilder.AppendLine(string.Concat(item.Key, " - ", item.Value));
			}
			return stringBuilder.ToString();
		}
	}
}
