using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompMannable : ThingComp
	{
		private int lastManTick = -1;

		private Pawn lastManPawn;

		public bool MannedNow
		{
			get
			{
				if (Find.TickManager.TicksGame - lastManTick <= 1 && lastManPawn != null)
				{
					return lastManPawn.Spawned;
				}
				return false;
			}
		}

		public Pawn ManningPawn
		{
			get
			{
				if (!MannedNow)
				{
					return null;
				}
				return lastManPawn;
			}
		}

		public CompProperties_Mannable Props => (CompProperties_Mannable)props;

		public void ManForATick(Pawn pawn)
		{
			lastManTick = Find.TickManager.TicksGame;
			lastManPawn = pawn;
			pawn.mindState.lastMannedThing = parent;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			CompMannable compMannable = this;
			Pawn pawn2 = pawn;
			if (!pawn2.RaceProps.ToolUser || !pawn2.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
			{
				yield break;
			}
			if (Props.manWorkType != 0 && pawn2.WorkTagIsDisabled(Props.manWorkType))
			{
				if (Props.manWorkType == WorkTags.Violent)
				{
					yield return new FloatMenuOption("CannotManThing".Translate(parent.LabelShort, parent) + " (" + "IsIncapableOfViolenceLower".Translate(pawn2.LabelShort, pawn2) + ")", null);
				}
			}
			else
			{
				yield return new FloatMenuOption("OrderManThing".Translate(parent.LabelShort, parent), delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.ManTurret, compMannable.parent);
					pawn2.jobs.TryTakeOrderedJob(job);
				});
			}
		}
	}
}
