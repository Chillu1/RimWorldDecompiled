using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_ApparelStatOffset : StatPart
	{
		private StatDef apparelStat;

		private bool subtract;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (!req.HasThing || req.Thing == null)
			{
				return;
			}
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null || pawn.apparel == null)
			{
				return;
			}
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				float statValue = pawn.apparel.WornApparel[i].GetStatValue(apparelStat);
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

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing != null)
			{
				Pawn pawn = req.Thing as Pawn;
				if (pawn != null && PawnWearingRelevantGear(pawn))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
					if (pawn.apparel != null)
					{
						for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
						{
							Apparel gear = pawn.apparel.WornApparel[i];
							stringBuilder.AppendLine(InfoTextLineFrom(gear));
						}
					}
					return stringBuilder.ToString();
				}
			}
			return null;
		}

		private string InfoTextLineFrom(Thing gear)
		{
			float num = gear.GetStatValue(apparelStat);
			if (subtract)
			{
				num = 0f - num;
			}
			return "    " + gear.LabelCap + ": " + num.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset);
		}

		private bool PawnWearingRelevantGear(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
				{
					if (pawn.apparel.WornApparel[i].GetStatValue(apparelStat) != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
		{
			Pawn pawn = req.Thing as Pawn;
			if (pawn == null || pawn.apparel == null)
			{
				yield break;
			}
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				Apparel thing = pawn.apparel.WornApparel[i];
				if (Mathf.Abs(thing.GetStatValue(apparelStat)) > 0f)
				{
					yield return new Dialog_InfoCard.Hyperlink(thing);
				}
			}
		}
	}
}
