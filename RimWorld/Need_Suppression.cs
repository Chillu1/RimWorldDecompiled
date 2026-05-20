using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Suppression : Need
	{
		private const float CanSuppressMaxThreshold = 0.7f;

		private const float SuppressCriticalThreshold = 0.3f;

		public bool CanBeSuppressedNow => CurLevel < 0.7f;

		public bool IsHigh => CurLevel < 0.3f;

		public Need_Suppression(Pawn pawn)
			: base(pawn)
		{
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				CurLevel -= 0.0025f * pawn.GetStatValue(StatDefOf.SlaveSuppressionFallRate);
			}
		}

		public void DrawSuppressionBar(Rect rect)
		{
			Widgets.FillableBar(rect, base.CurLevelPercentage, GuestUtility.SlaveSuppressionFillTex);
			DrawBarThreshold(rect, 0.3f);
			DrawBarThreshold(rect, 0.15f);
		}
	}
}
