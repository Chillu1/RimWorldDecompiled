using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Mechanitor : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		return MechanitorUtility.IsMechanitor(context.FirstSelectedPawn);
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
	{
		if (!clickedPawn.IsColonyMech)
		{
			yield break;
		}
		if (clickedPawn.GetOverseer() != context.FirstSelectedPawn)
		{
			if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
			{
				yield return new FloatMenuOption("CannotControlMech".Translate(clickedPawn.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			}
			else if (!MechanitorUtility.CanControlMech(context.FirstSelectedPawn, clickedPawn))
			{
				AcceptanceReport acceptanceReport = MechanitorUtility.CanControlMech(context.FirstSelectedPawn, clickedPawn);
				if (!acceptanceReport.Reason.NullOrEmpty())
				{
					yield return new FloatMenuOption("CannotControlMech".Translate(clickedPawn.LabelShort) + ": " + acceptanceReport.Reason, null);
				}
			}
			else
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ControlMech".Translate(clickedPawn.LabelShort), delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.ControlMech, clickedPawn);
					context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn));
			}
			yield return new FloatMenuOption("CannotDisassembleMech".Translate(clickedPawn.LabelCap) + ": " + "MustBeOverseer".Translate().CapitalizeFirst(), null);
		}
		else
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisconnectMech".Translate(clickedPawn.LabelShort), delegate
			{
				MechanitorUtility.ForceDisconnectMechFromOverseer(clickedPawn);
			}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -10), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn));
			if (!clickedPawn.IsFighting())
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisassembleMech".Translate(clickedPawn.LabelCap), delegate
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisassemblingMech".Translate(clickedPawn.LabelCap) + ":\n" + (from x in MechanitorUtility.IngredientsFromDisassembly(clickedPawn.def)
						select x.Summary).ToLineList("  - "), delegate
					{
						context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DisassembleMech, clickedPawn), JobTag.Misc);
					}, destructive: true));
				}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -20), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn));
			}
		}
		if (!context.FirstSelectedPawn.Drafted || !MechRepairUtility.CanRepair(clickedPawn))
		{
			yield break;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotRepairMech".Translate(clickedPawn.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(clickedPawn.LabelShort), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.RepairMech, clickedPawn);
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn));
	}
}
