using Verse;

namespace RimWorld
{
	public class CompUseEffect_StartWick : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			parent.GetComp<CompExplosive>().StartWick();
		}
	}
}
