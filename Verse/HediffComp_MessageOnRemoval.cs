using RimWorld;

namespace Verse;

public class HediffComp_MessageOnRemoval : HediffComp_MessageBase
{
	protected HediffCompProperties_MessageOnRemoval Props => (HediffCompProperties_MessageOnRemoval)props;

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		if (((Props.messageOnZeroSeverity && parent.Severity <= 0f) || (Props.messageOnNonZeroSeverity && parent.Severity > 0f)) && PawnUtility.ShouldSendNotificationAbout(parent.pawn))
		{
			Message();
		}
	}
}
