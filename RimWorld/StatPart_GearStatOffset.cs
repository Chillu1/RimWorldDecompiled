using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatPart_GearStatOffset : StatPart
{
	private StatDef apparelStat;

	private bool subtract;

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
				float statValue = pawn.apparel.WornApparel[i].GetStatValue(apparelStat);
				statValue += StatWorker.StatOffsetFromGear(pawn.apparel.WornApparel[i], apparelStat);
				if (subtract)
				{
					val -= statValue;
				}
				else
				{
					val += statValue;
				}
			}
		}
		if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null)
		{
			float statValue2 = pawn.equipment.Primary.GetStatValue(apparelStat);
			statValue2 += StatWorker.StatOffsetFromGear(pawn.equipment.Primary, apparelStat);
			if (subtract)
			{
				val -= statValue2;
			}
			else
			{
				val += statValue2;
			}
		}
	}

	public override string ExplanationPart(StatRequest req)
	{
		if (req.HasThing && req.Thing != null)
		{
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
					Apparel gear = pawn.apparel.WornApparel[i];
					float num = GetStatValueForGear(gear);
					if (subtract)
					{
						num = 0f - num;
					}
					if (!Mathf.Approximately(num, 0f))
					{
						stringBuilder.AppendLine(InfoTextLineFrom(gear, num));
					}
				}
			}
			if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null)
			{
				float num2 = GetStatValueForGear(pawn.equipment.Primary);
				if (subtract)
				{
					num2 = 0f - num2;
				}
				if (!Mathf.Approximately(num2, 0f))
				{
					stringBuilder.AppendLine(InfoTextLineFrom(pawn.equipment.Primary, num2));
				}
			}
			return stringBuilder.ToString();
		}
		return null;
	}

	private string InfoTextLineFrom(Thing gear, float value)
	{
		return "    " + gear.LabelCap + ": " + value.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset);
	}

	private float GetStatValueForGear(Thing gear)
	{
		return gear.GetStatValue(apparelStat) + StatWorker.StatOffsetFromGear(gear, apparelStat);
	}

	private bool PawnWearingRelevantGear(Pawn pawn)
	{
		if (pawn.apparel != null)
		{
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				Apparel apparel = pawn.apparel.WornApparel[i];
				if (apparel.GetStatValue(apparelStat) != 0f)
				{
					return true;
				}
				if (StatWorker.StatOffsetFromGear(apparel, apparelStat) != 0f)
				{
					return true;
				}
			}
		}
		if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null && StatWorker.StatOffsetFromGear(pawn.equipment.Primary, apparelStat) != 0f)
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
			if (!Mathf.Approximately(thing2.GetStatValue(apparelStat), 0f))
			{
				yield return new Dialog_InfoCard.Hyperlink(thing2);
			}
		}
	}
}
