using Verse;

namespace RimWorld;

public class StatPart_RoleConversionPower : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		if (req.HasThing && req.Thing is Pawn { Ideo: not null } pawn)
		{
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (role != null)
			{
				val *= role.def.convertPowerFactor;
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing is Pawn { Ideo: not null } pawn)
		{
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (role != null)
			{
				return "AbilityIdeoConvertBreakdownRole".Translate((req.Thing as Pawn).Named("PAWN"), role.Named("ROLE")) + ": " + role.def.convertPowerFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor);
			}
		}
		return null;
	}
}
