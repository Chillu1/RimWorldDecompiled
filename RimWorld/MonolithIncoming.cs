using Verse;

namespace RimWorld;

public class MonolithIncoming : GroundSpawner
{
	protected override IntRange ResultSpawnDelay => new IntRange(EffecterDefOf.VoidStructureIncoming.maintainTicks);

	protected override SoundDef SustainerSound => null;

	protected override bool SpawnRubble => false;

	protected override void Spawn(Map map, IntVec3 pos)
	{
		Building_VoidMonolith building_VoidMonolith = Find.Anomaly.SpawnNewMonolith(pos, map);
		TaggedString text = "MonolithArrivalText".Translate();
		if (Find.Anomaly.Level > 0)
		{
			text += "\n\n" + "MonolithArrivalTextExt".Translate();
		}
		Find.LetterStack.ReceiveLetter("MonolithArrivalLabel".Translate(), text, LetterDefOf.NeutralEvent, building_VoidMonolith);
	}
}
