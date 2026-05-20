using Verse;

namespace RimWorld;

public class CompAbilityEffect_ConnectingFleckLine : CompAbilityEffect
{
	public new CompProperties_AbilityConnectingFleckLine Props => (CompProperties_AbilityConnectingFleckLine)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (Props.fleckDef != null)
		{
			FleckMaker.ConnectingLine(parent.pawn.DrawPos, target.CenterVector3, Props.fleckDef, parent.pawn.Map);
		}
	}
}
