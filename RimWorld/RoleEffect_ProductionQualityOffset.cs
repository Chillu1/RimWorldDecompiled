using Verse;

namespace RimWorld
{
	public class RoleEffect_ProductionQualityOffset : RoleEffect
	{
		public int offset;

		public RoleEffect_ProductionQualityOffset()
		{
			labelKey = "RoleEffectProductionQualityOffset";
		}

		public override string Label(Pawn pawn, Precept_Role role)
		{
			return labelKey.Translate(offset.ToStringWithSign());
		}
	}
}
