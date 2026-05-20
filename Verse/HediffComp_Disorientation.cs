using System.Linq;
using RimWorld;
using Verse.AI;

namespace Verse;

public class HediffComp_Disorientation : HediffComp
{
	private const string MoteTexPath = "Things/Mote/Disoriented";

	private HediffCompProperties_Disorientation Props => (HediffCompProperties_Disorientation)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (Props.wanderMtbHours > 0f && base.Pawn.Spawned && !base.Pawn.Downed && base.Pawn.Awake() && base.Pawn.CurJobDef.suspendable && Props.wanderMtbHours > 0f && base.Pawn.IsHashIntervalTick(60, delta) && Rand.MTBEventOccurs(Props.wanderMtbHours, 2500f, 60f) && base.Pawn.CurJob.def != JobDefOf.GotoMindControlled)
		{
			IntVec3 intVec = (from c in GenRadial.RadialCellsAround(base.Pawn.Position, Props.wanderRadius, useCenter: false)
				where c.Standable(base.Pawn.MapHeld) && base.Pawn.CanReach(c, PathEndMode.OnCell, Danger.Unspecified)
				select c).RandomElementWithFallback(IntVec3.Invalid);
			if (intVec.IsValid)
			{
				MoteMaker.MakeThoughtBubble(base.Pawn, "Things/Mote/Disoriented");
				Job job = JobMaker.MakeJob(JobDefOf.GotoMindControlled, intVec);
				job.expiryInterval = Props.singleWanderDurationTicks;
				base.Pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
			}
		}
	}
}
