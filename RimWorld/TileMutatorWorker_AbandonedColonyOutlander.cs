using Verse;

namespace RimWorld;

public class TileMutatorWorker_AbandonedColonyOutlander : TileMutatorWorker_AbandonedColony
{
	public TileMutatorWorker_AbandonedColonyOutlander(TileMutatorDef def)
		: base(def)
	{
	}

	protected override Faction GetFaction()
	{
		if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: false, allowDefeated: true, TechLevel.Medieval, TechLevel.Spacer))
		{
			return faction;
		}
		return null;
	}
}
