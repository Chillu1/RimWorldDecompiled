using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Beauty : Need_Seeker
	{
		private const float BeautyImpactFactor = 0.1f;

		private const float ThreshVeryUgly = 0.01f;

		private const float ThreshUgly = 0.15f;

		private const float ThreshNeutral = 0.35f;

		private const float ThreshPretty = 0.65f;

		private const float ThreshVeryPretty = 0.85f;

		private const float ThreshBeautiful = 0.99f;

		public override float CurInstantLevel
		{
			get
			{
				if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
				{
					return 0.5f;
				}
				if (!pawn.Spawned)
				{
					return 0.5f;
				}
				return LevelFromBeauty(CurrentInstantBeauty());
			}
		}

		public BeautyCategory CurCategory
		{
			get
			{
				if (CurLevel > 0.99f)
				{
					return BeautyCategory.Beautiful;
				}
				if (CurLevel > 0.85f)
				{
					return BeautyCategory.VeryPretty;
				}
				if (CurLevel > 0.65f)
				{
					return BeautyCategory.Pretty;
				}
				if (CurLevel > 0.35f)
				{
					return BeautyCategory.Neutral;
				}
				if (CurLevel > 0.15f)
				{
					return BeautyCategory.Ugly;
				}
				if (CurLevel > 0.01f)
				{
					return BeautyCategory.VeryUgly;
				}
				return BeautyCategory.Hideous;
			}
		}

		public Need_Beauty(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.15f);
			threshPercents.Add(0.35f);
			threshPercents.Add(0.65f);
			threshPercents.Add(0.85f);
		}

		private float LevelFromBeauty(float beauty)
		{
			return Mathf.Clamp01(def.baseLevel + beauty * 0.1f);
		}

		public float CurrentInstantBeauty()
		{
			if (!pawn.SpawnedOrAnyParentSpawned)
			{
				return 0.5f;
			}
			return BeautyUtility.AverageBeautyPerceptible(pawn.PositionHeld, pawn.MapHeld);
		}
	}
}
