using System.Text.RegularExpressions;
using Verse;

namespace RimWorld
{
	public class Outfit : IExposable, ILoadReferenceable
	{
		public int uniqueId;

		public string label;

		public ThingFilter filter = new ThingFilter();

		public static readonly Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

		public Outfit()
		{
		}

		public Outfit(int uniqueId, string label)
		{
			this.uniqueId = uniqueId;
			this.label = label;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
			Scribe_Values.Look(ref label, "label");
			Scribe_Deep.Look(ref filter, "filter");
		}

		public string GetUniqueLoadID()
		{
			return "Outfit_" + label + uniqueId.ToString();
		}
	}
}
