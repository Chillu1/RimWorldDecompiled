using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_DraftedTend : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => false;

	protected override bool MechanoidCanDo => true;

	protected override bool RequiresManipulation => true;

	protected override bool CanSelfTarget => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (context.FirstSelectedPawn.IsMutant)
		{
			return context.FirstSelectedPawn.mutant.Def.canTend;
		}
		return true;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!IsValidTendTarget(context.FirstSelectedPawn, clickedPawn))
		{
			yield break;
		}
		if (!clickedPawn.health.HasHediffsNeedingTend())
		{
			if (context.FirstSelectedPawn != clickedPawn)
			{
				yield return new FloatMenuOption("CannotTend".Translate(clickedPawn) + ": " + "TendingNotRequired".Translate(clickedPawn), null);
			}
			yield break;
		}
		if (context.FirstSelectedPawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
		{
			yield return new FloatMenuOption("CannotTend".Translate(clickedPawn) + ": " + "CannotPrioritizeWorkTypeDisabled".Translate(WorkTypeDefOf.Doctor.gerundLabel), null);
			yield break;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotTend".Translate(clickedPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (clickedPawn == context.FirstSelectedPawn && context.FirstSelectedPawn.playerSettings != null && !context.FirstSelectedPawn.playerSettings.selfTend)
		{
			yield return new FloatMenuOption("CannotTend".Translate(clickedPawn) + ": " + "SelfTendDisabled".Translate().CapitalizeFirst(), null);
			yield break;
		}
		if (clickedPawn.InAggroMentalState && !clickedPawn.health.hediffSet.HasHediff(HediffDefOf.Scaria))
		{
			yield return new FloatMenuOption("CannotTend".Translate(clickedPawn) + ": " + "PawnIsInAggroMentalState".Translate(clickedPawn).CapitalizeFirst(), null);
			yield break;
		}
		Thing medicine = HealthAIUtility.FindBestMedicine(context.FirstSelectedPawn, clickedPawn, onlyUseInventory: true);
		TaggedString taggedString = "Tend".Translate(clickedPawn);
		if (medicine == null)
		{
			taggedString += " (" + "WithoutMedicine".Translate() + ")";
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, Action), context.FirstSelectedPawn, clickedPawn);
		if (medicine != null && context.FirstSelectedPawn.CanReserve(clickedPawn) && clickedPawn.Spawned)
		{
			yield return new FloatMenuOption("Tend".Translate(clickedPawn) + " (" + "WithoutMedicine".Translate() + ")", delegate
			{
				Job job = JobMaker.MakeJob(JobDefOf.TendPatient, clickedPawn, null);
				job.count = 1;
				job.draftedTend = true;
				context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			});
		}
		void Action()
		{
			Job job = JobMaker.MakeJob(JobDefOf.TendPatient, clickedPawn, medicine);
			job.count = 1;
			job.draftedTend = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}

	private bool IsValidTendTarget(Pawn doctor, Pawn patient)
	{
		if (!doctor.Drafted && patient != doctor)
		{
			return false;
		}
		if (patient.Downed)
		{
			return true;
		}
		if (patient.HostileTo(doctor.Faction))
		{
			return false;
		}
		if (patient.IsColonist || patient.IsQuestLodger() || patient.IsPrisonerOfColony || patient.IsSlaveOfColony || (patient.Faction == Faction.OfPlayer && patient.IsAnimal))
		{
			return true;
		}
		if (patient.IsColonySubhuman && patient.mutant.Def.entitledToMedicalCare)
		{
			return true;
		}
		return false;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Building_HoldingPlatform holdingPlatform = clickedThing as Building_HoldingPlatform;
		if (holdingPlatform == null)
		{
			return null;
		}
		Pawn heldPawn = holdingPlatform.HeldPawn;
		if (heldPawn == null)
		{
			return null;
		}
		if (!HealthAIUtility.ShouldBeTendedNowByPlayer(heldPawn))
		{
			return null;
		}
		if (context.FirstSelectedPawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
		{
			return new FloatMenuOption("CannotTend".Translate(heldPawn) + ": " + "CannotPrioritizeWorkTypeDisabled".Translate(WorkTypeDefOf.Doctor.gerundLabel), null);
		}
		if (!context.FirstSelectedPawn.CanReach(heldPawn, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotTend".Translate(heldPawn) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		Thing medicine = HealthAIUtility.FindBestMedicine(context.FirstSelectedPawn, heldPawn);
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Tend".Translate(heldPawn.LabelShort), delegate
		{
			JobDef tendEntity = JobDefOf.TendEntity;
			LocalTargetInfo targetA = holdingPlatform;
			Thing thing = medicine;
			Job job = JobMaker.MakeJob(tendEntity, targetA, (thing != null) ? ((LocalTargetInfo)thing) : LocalTargetInfo.Invalid);
			job.count = 1;
			job.draftedTend = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, holdingPlatform);
	}
}
