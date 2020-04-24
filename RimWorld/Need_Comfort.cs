using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Comfort : Need_Seeker
	{
		public float lastComfortUsed;

		public int lastComfortUseTick;

		private const float MinNormal = 0.1f;

		private const float MinComfortable = 0.6f;

		private const float MinVeryComfortable = 0.7f;

		private const float MinExtremelyComfortablee = 0.8f;

		private const float MinLuxuriantlyComfortable = 0.9f;

		public const int ComfortUseInterval = 10;

		public override float CurInstantLevel
		{
			get
			{
				if (lastComfortUseTick >= Find.TickManager.TicksGame - 10)
				{
					return Mathf.Clamp01(lastComfortUsed);
				}
				return 0f;
			}
		}

		public ComfortCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.1f)
				{
					return ComfortCategory.Uncomfortable;
				}
				if (CurLevel < 0.6f)
				{
					return ComfortCategory.Normal;
				}
				if (CurLevel < 0.7f)
				{
					return ComfortCategory.Comfortable;
				}
				if (CurLevel < 0.8f)
				{
					return ComfortCategory.VeryComfortable;
				}
				if (CurLevel < 0.9f)
				{
					return ComfortCategory.ExtremelyComfortable;
				}
				return ComfortCategory.LuxuriantlyComfortable;
			}
		}

		public Need_Comfort(Pawn pawn)
			: base(pawn)
		{
			threshPercents = new List<float>();
			threshPercents.Add(0.1f);
			threshPercents.Add(0.6f);
			threshPercents.Add(0.7f);
			threshPercents.Add(0.8f);
			threshPercents.Add(0.9f);
		}

		public void ComfortUsed(float comfort)
		{
			lastComfortUsed = comfort;
			lastComfortUseTick = Find.TickManager.TicksGame;
		}
	}
}
