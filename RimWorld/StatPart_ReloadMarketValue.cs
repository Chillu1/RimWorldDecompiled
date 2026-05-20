using System.Text;
using RimWorld.Utility;
using Verse;

namespace RimWorld;

public class StatPart_ReloadMarketValue : StatPart
{
	public override void TransformValue(StatRequest req, ref float val)
	{
		TransformAndExplain(req, ref val, null);
	}

	public override string ExplanationPart(StatRequest req)
	{
		float val = 1f;
		StringBuilder stringBuilder = new StringBuilder();
		TransformAndExplain(req, ref val, stringBuilder);
		return stringBuilder.ToString().TrimEndNewlines();
	}

	private static void TransformAndExplain(StatRequest req, ref float val, StringBuilder explanation)
	{
		IReloadableComp reloadableComp = req.Thing?.TryGetComp<CompApparelReloadable>();
		IReloadableComp reloadableComp2 = reloadableComp ?? req.Thing?.TryGetComp<CompEquippableAbilityReloadable>();
		if (reloadableComp2 != null && reloadableComp2.RemainingCharges != reloadableComp2.MaxCharges)
		{
			if (reloadableComp2.AmmoDef != null)
			{
				int num = reloadableComp2.MaxAmmoNeeded(allowForcedReload: true);
				float num2 = (0f - reloadableComp2.AmmoDef.BaseMarketValue) * (float)num;
				val += num2;
				explanation?.AppendLine("StatsReport_ReloadMarketValue".Translate(NamedArgumentUtility.Named(reloadableComp2.AmmoDef, "AMMO"), num.Named("COUNT")) + ": " + num2.ToStringMoneyOffset());
			}
			else if (reloadableComp2 is CompApparelVerbOwner_Charged compApparelVerbOwner_Charged && compApparelVerbOwner_Charged.Props.destroyOnEmpty)
			{
				float num3 = (float)reloadableComp2.RemainingCharges / (float)reloadableComp2.MaxCharges;
				explanation?.AppendLine("StatsReport_ReloadRemainingChargesMultipler".Translate(compApparelVerbOwner_Charged.Props.ChargeNounArgument, reloadableComp2.LabelRemaining) + ": x" + num3.ToStringPercent());
				val *= num3;
			}
			if (val < 0f)
			{
				val = 0f;
			}
		}
	}
}
