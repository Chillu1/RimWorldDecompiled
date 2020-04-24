using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_NonBurnable : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			if (!CanEverMatch(t.def))
			{
				return false;
			}
			return !t.BurnableByRecipe;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			if (def.burnableByRecipe)
			{
				return def.MadeFromStuff;
			}
			return true;
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			if (!def.burnableByRecipe)
			{
				return !def.MadeFromStuff;
			}
			return false;
		}
	}
}
