using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class StatPart_Stuff : StatPart
{
	public StatDef stuffPowerStat;

	public StatDef multiplierStat;

	public override string ExplanationPart(StatRequest req)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (req.BuildableDef.MadeFromStuff)
		{
			string text = ((req.StuffDef != null) ? req.StuffDef.label : "None".TranslateSimple());
			string text2 = ((req.StuffDef != null) ? req.StuffDef.GetStatValueAbstract(stuffPowerStat).ToStringByStyle(parentStat.ToStringStyleUnfinalized) : "0");
			stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + text + "): " + text2);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + ": " + GetMultiplier(req).ToStringPercent("F0"));
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override void TransformValue(StatRequest req, ref float value)
	{
		float num = ((req.StuffDef != null) ? req.StuffDef.GetStatValueAbstract(stuffPowerStat) : 0f);
		value += GetMultiplier(req) * num;
	}

	private float GetMultiplier(StatRequest req)
	{
		if (req.HasThing)
		{
			return req.Thing.GetStatValue(multiplierStat);
		}
		return req.BuildableDef.GetStatValueAbstract(multiplierStat);
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
	{
		if (req.StuffDef != null)
		{
			yield return new Dialog_InfoCard.Hyperlink(req.StuffDef);
		}
	}
}
