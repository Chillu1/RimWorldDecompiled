using RimWorld;

namespace Verse;

public class Hediff_Mechlink : HediffWithComps
{
	private const int LearningOpportunityCheckInterval = 300;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		if (!ModLister.CheckBiotech("Mechlink"))
		{
			pawn.health.RemoveHediff(this);
			return;
		}
		PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
		if (pawn.Spawned)
		{
			Find.LetterStack.ReceiveLetter("LetterLabelMechlinkInstalled".Translate() + ": " + pawn.LabelShortCap, "LetterMechlinkInstalled".Translate(pawn.Named("PAWN")), LetterDefOf.PositiveEvent, pawn);
		}
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		pawn.mechanitor?.Notify_MechlinkRemoved();
	}

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		if (pawn.Spawned && pawn.IsHashIntervalTick(300, delta))
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Mechanitors, OpportunityType.Important);
		}
	}
}
