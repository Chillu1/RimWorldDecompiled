using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompObeliskDeactivationInteractor : CompInteractable
{
	private CompObelisk obeliskComp;

	private CompObelisk ObeliskComp => obeliskComp ?? (obeliskComp = parent.GetComp<CompObelisk>());

	private new CompProperties_ObeliskDeactivationInteractor Props => (CompProperties_ObeliskDeactivationInteractor)props;

	public override string ExposeKey => "Deactivation";

	public override bool CanCooldown => false;

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (!ObeliskComp.StudyFinished || ObeliskComp.Activated || ObeliskComp.ActivityComp.Deactivated)
		{
			return false;
		}
		if (activateBy != null)
		{
			if (checkOptionalItems && !activateBy.HasReserved(ThingDefOf.Shard) && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired, (Thing t) => activateBy.CanReserveAndReach(t, PathEndMode.Touch, Danger.None)))
			{
				return "ObeliskDeactivateMissingShards".Translate(Props.shardsRequired);
			}
		}
		else if (checkOptionalItems && !ReservationUtility.ExistsUnreservedAmountOfDef(parent.MapHeld, ThingDefOf.Shard, Faction.OfPlayer, Props.shardsRequired))
		{
			return "ObeliskDeactivateMissingShards".Translate(Props.shardsRequired);
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!ObeliskComp.StudyFinished || ObeliskComp.Activated || ObeliskComp.ActivityComp.Deactivated)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!ObeliskComp.StudyFinished || ObeliskComp.Activated || ObeliskComp.ActivityComp.Deactivated)
		{
			yield break;
		}
		foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
		{
			yield return item;
		}
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			OrderDeactivation(target.Pawn);
		}
	}

	private void OrderDeactivation(Pawn pawn)
	{
		List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, ThingDefOf.Shard, Props.shardsRequired);
		if (!list.NullOrEmpty())
		{
			Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent, list[0]);
			job.targetQueueB = (from i in list.Skip(1)
				select new LocalTargetInfo(i)).ToList();
			job.count = Props.shardsRequired;
			job.playerForced = true;
			job.interactableIndex = 1;
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}

	protected override void OnInteracted(Pawn caster)
	{
		if (!obeliskComp.Activated)
		{
			obeliskComp.ActivityComp.Deactivate();
			parent.GetComp<CompObeliskTriggerInteractor>()?.ResetCooldown();
		}
	}
}
