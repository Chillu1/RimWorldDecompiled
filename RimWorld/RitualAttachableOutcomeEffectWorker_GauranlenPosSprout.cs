using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualAttachableOutcomeEffectWorker_GauranlenPosSprout : RitualAttachableOutcomeEffectWorker
{
	public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
	{
		extraOutcomeDesc = null;
		if ((bool)CanApplyNow(jobRitual.Ritual, jobRitual.Map))
		{
			IncidentParms parms = new IncidentParms
			{
				target = jobRitual.Map,
				customLetterText = IncidentDefOf.GauranlenPodSpawn.letterText + "\n\n" + "RitualAttachedOutcome_GauranlenTreePod_ExtraDesc".Translate(jobRitual.RitualLabel)
			};
			if (IncidentDefOf.GauranlenPodSpawn.Worker.TryExecute(parms))
			{
				extraOutcomeDesc = def.letterInfoText;
			}
		}
	}

	public override AcceptanceReport CanApplyNow(Precept_Ritual ritual, Map map)
	{
		if (!IncidentWorker_GauranlenPodSpawn.IsGoodBiome(map.Biome))
		{
			return "RitualAttachedOutcomeCantApply_ExtremeBiome".Translate();
		}
		if (!IncidentWorker_GauranlenPodSpawn.TryFindRootCell(map, out var _))
		{
			return "RitualAttachedOutcomeCantApply_NoValidSpot".Translate();
		}
		return base.CanApplyNow(ritual, map);
	}
}
