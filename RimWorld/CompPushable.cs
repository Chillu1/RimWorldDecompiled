using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompPushable : ThingComp
{
	public bool canBePushed = true;

	public Vector3 drawPos;

	public Vector3 drawVel;

	public CompProperties_Pushable Props => (CompProperties_Pushable)props;

	public Pawn Pawn => (Pawn)parent;

	public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!canBePushed || !Props.givePushOption)
		{
			yield break;
		}
		FloatMenuOption floatMenuOption = new FloatMenuOption("CommandPushTo".Translate(), delegate
		{
			Find.Targeter.BeginTargeting(TargetingParameters.ForCell(), delegate(LocalTargetInfo target)
			{
				Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, Pawn, target.Cell);
				job.count = 1;
				selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
			});
		});
		if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			floatMenuOption.Label = string.Format("{0} ({1})", floatMenuOption.Label, "RequiredCapacity".Translate(PawnCapacityDefOf.Manipulation.label));
			floatMenuOption.Disabled = true;
		}
		yield return floatMenuOption;
	}

	public void OnStartedCarrying(Pawn pawn)
	{
		Vector3 v = (pawn.Position - Pawn.Position).ToVector3();
		Vector2 vector = new Vector2(0f, 0f - Props.offsetDistance).RotatedBy(v.AngleFlat());
		drawPos = new Vector3(vector.x, 0f, 0f - vector.y);
		drawVel = Vector3.zero;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		_ = Prefs.DevMode;
		yield break;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref canBePushed, "canBePushed", defaultValue: false);
		Scribe_Values.Look(ref drawPos, "drawPos");
		Scribe_Values.Look(ref drawVel, "drawVel");
	}
}
