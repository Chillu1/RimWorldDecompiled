using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompLabyrinthDoor : CompInteractable, IJammedDoorDrawer
{
	public Building_JammedDoor Door => (Building_JammedDoor)parent;

	public new CompProperties_LabyrinthDoor Props => (CompProperties_LabyrinthDoor)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Labyrinth door"))
		{
			parent.Destroy();
			return;
		}
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && Rand.Chance(Props.unlockedChance))
		{
			Door.UnlockDoor();
		}
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		OrderActivation(target.Pawn);
	}

	public override string CompInspectStringExtra()
	{
		if (!Door.Jammed)
		{
			return null;
		}
		return "GrayDoorJammed".Translate();
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (Door.Jammed)
		{
			AcceptanceReport acceptanceReport = CanInteract(selPawn);
			FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
			{
				OrderActivation(selPawn);
			});
			if (!acceptanceReport.Accepted)
			{
				floatMenuOption.Disabled = true;
				floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
			}
			yield return floatMenuOption;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Door.Jammed)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	protected override void OnInteracted(Pawn caster)
	{
		Door.UnlockAndOpenDoor();
		if (caster.IsColonist)
		{
			parent.Map.fogGrid.FloodUnfogAdjacent(parent.Position, sendLetters: false);
		}
	}

	private void OrderActivation(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
		job.count = 1;
		job.playerForced = true;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	public void DrawJammed(Rot4 rotation)
	{
		Vector3 drawPos = Door.DrawPos;
		drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();
		Props.jammed.Graphic.Draw(drawPos, rotation, Door);
	}
}
