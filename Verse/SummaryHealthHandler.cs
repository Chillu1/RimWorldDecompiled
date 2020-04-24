using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SummaryHealthHandler
	{
		private Pawn pawn;

		private float cachedSummaryHealthPercent = 1f;

		private bool dirty = true;

		public float SummaryHealthPercent
		{
			get
			{
				if (pawn.Dead)
				{
					return 0f;
				}
				if (dirty)
				{
					List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
					float num = 1f;
					for (int i = 0; i < hediffs.Count; i++)
					{
						if (!(hediffs[i] is Hediff_MissingPart))
						{
							float num2 = Mathf.Min(hediffs[i].SummaryHealthPercentImpact, 0.95f);
							num *= 1f - num2;
						}
					}
					List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
					for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
					{
						float num3 = Mathf.Min(missingPartsCommonAncestors[j].SummaryHealthPercentImpact, 0.95f);
						num *= 1f - num3;
					}
					cachedSummaryHealthPercent = Mathf.Clamp(num, 0.05f, 1f);
					dirty = false;
				}
				return cachedSummaryHealthPercent;
			}
		}

		public SummaryHealthHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Notify_HealthChanged()
		{
			dirty = true;
		}
	}
}
