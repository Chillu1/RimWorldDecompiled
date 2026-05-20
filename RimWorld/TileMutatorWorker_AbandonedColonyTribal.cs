using Verse;

namespace RimWorld;

public class TileMutatorWorker_AbandonedColonyTribal : TileMutatorWorker_AbandonedColony
{
	public TileMutatorWorker_AbandonedColonyTribal(TileMutatorDef def)
		: base(def)
	{
	}

	protected override Faction GetFaction()
	{
		if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: false, allowDefeated: true, TechLevel.Animal, TechLevel.Neolithic))
		{
			return faction;
		}
		return null;
	}
}
