using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class HediffComp_PsychicHarmonizer : HediffComp
	{
		public HediffCompProperties_PsychicHarmonizer Props => (HediffCompProperties_PsychicHarmonizer)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			Pawn pawn = parent.pawn;
			if (pawn.IsHashIntervalTick(150) || pawn.needs == null || pawn.needs.mood == null || pawn.Faction == null)
			{
				return;
			}
			if (pawn.Spawned)
			{
				List<Pawn> pawns = pawn.Map.mapPawns.PawnsInFaction(pawn.Faction);
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
				if (p != pawn && p.RaceProps.Humanlike && pawn.needs != null && pawn.needs.mood != null && pawn.needs.mood.thoughts != null && (!p.Spawned || !pawn.Spawned || !(pawn.Position.DistanceTo(p.Position) > Props.range)) && !pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicHarmonizer))
				{
					bool flag = false;
					foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
					{
						Thought_PsychicHarmonizer thought_PsychicHarmonizer = memory as Thought_PsychicHarmonizer;
						if (thought_PsychicHarmonizer != null && thought_PsychicHarmonizer.harmonizer == parent)
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
	}
}
