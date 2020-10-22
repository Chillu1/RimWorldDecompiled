using System.Text;
using Verse;

namespace RimWorld
{
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
			CompReloadable compReloadable = req.Thing?.TryGetComp<CompReloadable>();
			if (compReloadable != null && compReloadable.RemainingCharges != compReloadable.MaxCharges)
			{
				if (compReloadable.AmmoDef != null)
				{
					int num = compReloadable.MaxAmmoNeeded(allowForcedReload: true);
					float num2 = (0f - compReloadable.AmmoDef.BaseMarketValue) * (float)num;
					val += num2;
					explanation?.AppendLine("StatsReport_ReloadMarketValue".Translate(NamedArgumentUtility.Named(compReloadable.AmmoDef, "AMMO"), num.Named("COUNT")) + ": " + num2.ToStringMoneyOffset());
				}
				else if (compReloadable.Props.destroyOnEmpty)
				{
					float num3 = (float)compReloadable.RemainingCharges / (float)compReloadable.MaxCharges;
					explanation?.AppendLine("StatsReport_ReloadRemainingChargesMultipler".Translate(compReloadable.Props.ChargeNounArgument, compReloadable.LabelRemaining) + ": x" + num3.ToStringPercent());
					val *= num3;
				}
				if (val < 0f)
				{
					val = 0f;
				}
			}
		}
	}
}
