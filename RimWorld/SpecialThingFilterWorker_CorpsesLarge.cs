using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesLarge : SpecialThingFilterWorker
{
	private const float MinBodySize = 0.75f;

	public override bool Matches(Thing t)
	{
		if (t is Corpse corpse)
		{
			return corpse.InnerPawn.BodySize >= 0.75f;
		}
		return false;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		if (!def.IsCorpse || def.ingestible?.sourceDef?.race == null)
		{
			return false;
		}
		RaceProperties race = def.ingestible.sourceDef.race;
		for (int i = 0; i < race.lifeStageAges.Count; i++)
		{
			if (race.lifeStageAges[i].def.bodySizeFactor >= 0.75f)
			{
				return true;
			}
		}
		return false;
	}

	public override bool AlwaysMatches(ThingDef def)
	{
		if (!def.IsCorpse || def.ingestible?.sourceDef?.race == null)
		{
			return false;
		}
		RaceProperties race = def.ingestible.sourceDef.race;
		for (int i = 0; i < race.lifeStageAges.Count; i++)
		{
			if (race.lifeStageAges[i].def.bodySizeFactor < 0.75f)
			{
				return false;
			}
		}
		return true;
	}
}
