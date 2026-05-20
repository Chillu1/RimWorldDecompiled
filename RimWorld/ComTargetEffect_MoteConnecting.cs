using Verse;

namespace RimWorld;

public class ComTargetEffect_MoteConnecting : CompTargetEffect
{
	private CompProperties_TargetEffect_MoteConnecting Props => (CompProperties_TargetEffect_MoteConnecting)props;

	public override void DoEffectOn(Pawn user, Thing target)
	{
		if (Props.moteDef != null)
		{
			MoteMaker.MakeConnectingLine(user.DrawPos, target.DrawPos, Props.moteDef, user.Map);
		}
	}
}
