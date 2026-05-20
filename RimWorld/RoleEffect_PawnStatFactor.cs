using Verse;

namespace RimWorld
{
	public class RoleEffect_PawnStatFactor : RoleEffect_PawnStatModifier
	{
		public override string Label(Pawn pawn, Precept_Role role)
		{
			return statDef.LabelCap + ": " + statDef.ValueToString(modifier, ToStringNumberSense.Factor, finalized: false);
		}
	}
}
