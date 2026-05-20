using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Play : Need
	{
		public const float BaseFallPerInterval = 0.00025f;

		public const float ThresholdEmpty = 0.01f;

		public const float ThresholdLow = 0.15f;

		public const float ThresholdSatisfied = 0.3f;

		public const float ThresholdHigh = 0.7f;

		public const float ThresholdVeryHigh = 0.85f;

		public bool IsLow => base.CurLevelPercentage <= 0.15f;

		public PlayCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.01f)
				{
					return PlayCategory.Empty;
				}
				if (CurLevel < 0.15f)
				{
					return PlayCategory.VeryLow;
				}
				if (CurLevel < 0.3f)
				{
					return PlayCategory.Low;
				}
				if (CurLevel < 0.7f)
				{
					return PlayCategory.Satisfied;
				}
				if (CurLevel < 0.85f)
				{
					return PlayCategory.High;
				}
				return PlayCategory.Extreme;
			}
		}

		public Need_Play(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float> { 0.15f, 0.3f, 0.7f, 0.85f };
		}

		public void Play(float amount)
		{
			base.CurLevelPercentage += amount;
			base.CurLevelPercentage = Mathf.Clamp01(base.CurLevelPercentage);
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				CurLevel -= 0.00025f;
			}
		}
	}
}
