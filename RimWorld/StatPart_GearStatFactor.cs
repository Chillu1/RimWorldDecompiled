using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_GearStatFactor : StatPart
{
	private StatDef apparelStat;

	private bool includeWeapon;

	public override void TransformValue(StatRequest req, ref float val)
	{
		if (!req.HasThing || req.Thing == null || !(req.Thing is Pawn pawn))
		{
			return;
		}
		if (pawn.apparel != null)
		{
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				val *= pawn.apparel.WornApparel[i].GetStatValue(apparelStat);
			}
		}
		if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null)
		{
			val *= pawn.equipment.Primary.GetStatValue(apparelStat);
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (!req.HasThing || req.Thing == null)
		{
			return null;
		}
		if (!(req.Thing is Pawn pawn) || !PawnWearingRelevantGear(pawn))
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
		if (pawn.apparel != null)
		{
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				Apparel apparel = pawn.apparel.WornApparel[i];
				float statValue = apparel.GetStatValue(apparelStat);
				if (!Mathf.Approximately(statValue, 1f))
				{
					stringBuilder.AppendLine(InfoTextLineFrom(apparel, statValue));
				}
			}
		}
		if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null)
		{
			float statValue2 = pawn.equipment.Primary.GetStatValue(apparelStat);
			if (!Mathf.Approximately(statValue2, 1f))
			{
				stringBuilder.AppendLine(InfoTextLineFrom(pawn.equipment.Primary, statValue2));
			}
		}
		return stringBuilder.ToString();
	}

	private string InfoTextLineFrom(Thing gear, float value)
	{
		return "    " + gear.LabelCap + ": " + value.ToStringByStyle(apparelStat.toStringStyle, ToStringNumberSense.Factor);
	}

	private bool PawnWearingRelevantGear(Pawn pawn)
	{
		if (pawn.apparel != null)
		{
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				if (!Mathf.Approximately(pawn.apparel.WornApparel[i].GetStatValue(apparelStat), 1f))
				{
					return true;
				}
			}
		}
		if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null && !Mathf.Approximately(pawn.equipment.Primary.GetStatValue(apparelStat), 1f))
		{
			return true;
		}
		return false;
	}

	public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
	{
		Thing thing = req.Thing;
		if (!(thing is Pawn { apparel: not null } pawn))
		{
			yield break;
		}
		for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
		{
			Apparel thing2 = pawn.apparel.WornApparel[i];
			if (!Mathf.Approximately(thing2.GetStatValue(apparelStat), 1f))
			{
				yield return new Dialog_InfoCard.Hyperlink(thing2);
			}
		}
	}
}
