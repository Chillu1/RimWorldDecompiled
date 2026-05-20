using Verse;

namespace RimWorld
{
	public class RoleEffect_HuntingRevengeChanceFactor : RoleEffect
	{
		public float factor;

		public RoleEffect_HuntingRevengeChanceFactor()
		{
			labelKey = "RoleEffectHuntingRevengeChance";
		}

		public override string Label(Pawn pawn, Precept_Role role)
		{
			return labelKey.Translate("x" + factor.ToStringPercent());
		}
	}
}
