using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGenerator_Slaves : StockGenerator
	{
		private bool respectPopulationIntent;

		public PawnKindDef slaveKindDef;

		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
			{
				yield break;
			}
			int count = countRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				if (!Find.FactionManager.AllFactionsVisible.Where((Faction fac) => fac != Faction.OfPlayer && fac.def.humanlikeFaction).TryRandomElement(out Faction result))
				{
					break;
				}
				PawnGenerationRequest request = PawnGenerationRequest.MakeDefault();
				request.KindDef = ((slaveKindDef != null) ? slaveKindDef : PawnKindDefOf.Slave);
				request.Faction = result;
				request.Tile = forTile;
				request.ForceAddFreeWarmLayerIfNeeded = !trader.orbital;
				request.RedressValidator = ((Pawn x) => x.royalty == null || !x.royalty.AllTitlesForReading.Any());
				yield return PawnGenerator.GeneratePawn(request);
			}
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			if (thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike)
			{
				return thingDef.tradeability != Tradeability.None;
			}
			return false;
		}
	}
}
