using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_Burnable : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!CanEverMatch(t.def))
			{
				return false;
			}
			return t.BurnableByRecipe;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return def.burnableByRecipe;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			if (def.burnableByRecipe)
			{
				return !def.MadeFromStuff;
			}
			return false;
		}
	}
}
