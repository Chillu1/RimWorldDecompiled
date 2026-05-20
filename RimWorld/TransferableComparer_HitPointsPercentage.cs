using Verse;

namespace RimWorld;

public class TransferableComparer_HitPointsPercentage : TransferableComparer
{
	public override int Compare(Transferable lhs, Transferable rhs)
	{
		return GetValueFor(lhs).CompareTo(GetValueFor(rhs));
	}

	private float GetValueFor(Transferable t)
	{
		Thing anyThing = t.AnyThing;
		if (anyThing is Pawn pawn)
		{
			return pawn.health.summaryHealth.SummaryHealthPercent;
		}
		if (!anyThing.def.useHitPoints || !anyThing.def.healthAffectsPrice)
		{
			return 1f;
		}
		return (float)anyThing.HitPoints / (float)anyThing.MaxHitPoints;
	}
}
