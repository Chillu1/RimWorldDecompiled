using Verse;

namespace RimWorld
{
	public class CompUseEffect_DestroySelf : CompUseEffect
	{
		public override float OrderPriority => -1000f;

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			parent.SplitOff(1).Destroy();
		}
	}
}
