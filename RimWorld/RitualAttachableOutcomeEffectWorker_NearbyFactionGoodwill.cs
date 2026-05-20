using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_NearbyFactionGoodwill : RitualAttachableOutcomeEffectWorker
{
	public static readonly IntRange GoodwillRange = new IntRange(10, 20);

	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		List<RitualOutcomePossibility> outcomeChances = jobRitual.Ritual.outcomeEffect.def.outcomeChances;
		int positivityIndex = outcomeChances.MaxBy((RitualOutcomePossibility c) => c.positivityIndex).positivityIndex;
		int positivityIndex2 = outcomeChances.Where((RitualOutcomePossibility c) => c.positivityIndex >= 0).MinBy((RitualOutcomePossibility c) => c.positivityIndex).positivityIndex;
		int num = GoodwillRange.Lerped((float)(outcome.positivityIndex - positivityIndex2) / (float)(positivityIndex - positivityIndex2));
		PlanetTile tile = jobRitual.Map.Tile;
		float num2 = float.PositiveInfinity;
		Settlement settlement = null;
		foreach (WorldObject item in Find.WorldObjects.AllSettlementsOnLayer(tile.Layer))
		{
			if (item is Settlement settlement2 && item.Faction.CanChangeGoodwillFor(Faction.OfPlayer, num))
			{
				float num3 = Find.WorldGrid.ApproxDistanceInTiles(tile, item.Tile);
				if (num3 < num2)
				{
					num2 = num3;
					settlement = settlement2;
				}
			}
		}
		if (settlement != null && settlement.Faction.TryAffectGoodwillWith(Faction.OfPlayer, num, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.RitualDone))
		{
			letterLookTargets = new LookTargets((letterLookTargets.targets ?? new List<GlobalTargetInfo>()).Concat(Gen.YieldSingle((GlobalTargetInfo)settlement)));
			extraOutcomeDesc = def.letterInfoText.Formatted(num.Named("AMOUNT"), settlement.Faction.Named("FACTION"));
		}
		else
		{
			extraOutcomeDesc = null;
		}
	}
}
