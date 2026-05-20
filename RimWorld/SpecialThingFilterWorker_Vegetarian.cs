using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Vegetarian : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (FoodUtility.AcceptableVegetarian(t))
			{
				return !FoodUtility.AcceptableCarnivore(t);
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return FoodUtility.UnacceptableCarnivore(def);
		}
	}
}
