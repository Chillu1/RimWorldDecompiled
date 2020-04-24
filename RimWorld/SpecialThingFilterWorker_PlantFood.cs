using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_PlantFood : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			return AlwaysMatches(t.def);
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			if (def.ingestible != null)
			{
				return (def.ingestible.foodType & FoodTypeFlags.Plant) != 0;
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return AlwaysMatches(def);
		}
	}
}
