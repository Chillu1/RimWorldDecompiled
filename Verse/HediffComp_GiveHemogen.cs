using RimWorld;

namespace Verse;

public class HediffComp_GiveHemogen : HediffComp
{
	public HediffCompProperties_GiveHemogen Props => (HediffCompProperties_GiveHemogen)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		GeneUtility.OffsetHemogen(base.Pawn, Props.amountPerDay / 60000f * (float)delta);
	}
}
