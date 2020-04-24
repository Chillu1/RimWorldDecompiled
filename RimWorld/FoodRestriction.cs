using Verse;

namespace RimWorld
{
	public class FoodRestriction : IExposable, ILoadReferenceable
	{
		public int id;

		public string label;

		public ThingFilter filter = new ThingFilter();

		public FoodRestriction(int id, string label)
		{
			this.id = id;
			this.label = label;
		}

		public FoodRestriction()
		{
		}

		public bool Allows(ThingDef def)
		{
			return filter.Allows(def);
		}

		public bool Allows(Thing thing)
		{
			return filter.Allows(thing);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref id, "id", 0);
			Scribe_Values.Look(ref label, "label");
			Scribe_Deep.Look(ref filter, "filter");
		}

		public string GetUniqueLoadID()
		{
			return "FoodRestriction_" + label + id;
		}
	}
}
