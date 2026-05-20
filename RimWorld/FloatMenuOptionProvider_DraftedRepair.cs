using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_DraftedRepair : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => false;

	protected override bool Multiselect => false;

	protected override bool MechanoidCanDo => true;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (context.FirstSelectedPawn.skills != null)
		{
			return !context.FirstSelectedPawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled;
		}
		return false;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!RepairUtility.PawnCanRepairNow(context.FirstSelectedPawn, clickedThing))
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.Touch, Danger.Deadly))
		{
			return new FloatMenuOption("CannotRepair".Translate(clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(clickedThing), delegate
		{
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Repair, clickedThing), JobTag.Misc);
		}), context.FirstSelectedPawn, clickedThing);
	}
}
