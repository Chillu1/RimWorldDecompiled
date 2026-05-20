using Verse;

namespace RimWorld
{
	public class ComTargetEffect_FleckConnecting : CompTargetEffect
	{
		private CompProperties_TargetEffect_FleckConnecting Props => (CompProperties_TargetEffect_FleckConnecting)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (Props.fleckDef != null)
			{
				FleckMaker.ConnectingLine(user.DrawPos, target.DrawPos, Props.fleckDef, user.Map);
			}
		}
	}
}
