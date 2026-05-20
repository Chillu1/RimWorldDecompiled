using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class HediffComp_PsychicHarmonizer : HediffComp
{
	public HediffCompProperties_PsychicHarmonizer Props => (HediffCompProperties_PsychicHarmonizer)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		Pawn pawn = parent.pawn;
		if (!pawn.IsHashIntervalTick(150, delta) || pawn.needs == null || pawn.needs.mood == null || pawn.Faction == null)
		{
			return;
		}
		if (pawn.Spawned)
		{
			List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
			AffectPawns(pawn, pawns);
			return;
		}
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null)
		{
			AffectPawns(pawn, caravan.pawns.InnerListForReading);
		}
	}

	private void AffectPawns(Pawn p, List<Pawn> pawns)
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			if (p == pawn || !p.RaceProps.Humanlike || pawn?.needs?.mood?.thoughts == null || pawn.Position.DistanceTo(p.Position) > Props.range || pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicHarmonizer))
			{
				continue;
			}
			bool flag = false;
			foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
			{
				if (memory is Thought_PsychicHarmonizer thought_PsychicHarmonizer && thought_PsychicHarmonizer.harmonizer == parent)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Thought_PsychicHarmonizer thought_PsychicHarmonizer2 = (Thought_PsychicHarmonizer)ThoughtMaker.MakeThought(Props.thought);
				thought_PsychicHarmonizer2.harmonizer = parent;
				thought_PsychicHarmonizer2.otherPawn = parent.pawn;
				pawn.needs.mood.thoughts.memories.TryGainMemory(thought_PsychicHarmonizer2);
			}
		}
	}
}
