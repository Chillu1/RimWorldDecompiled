using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Need_Seeker : Need
	{
		private const float GUIArrowTolerance = 0.05f;

		public override int GUIChangeArrow
		{
			get
			{
				if (!pawn.Awake())
				{
					return 0;
				}
				float curInstantLevelPercentage = base.CurInstantLevelPercentage;
				if (curInstantLevelPercentage > base.CurLevelPercentage + 0.05f)
				{
					return 1;
				}
				if (curInstantLevelPercentage < base.CurLevelPercentage - 0.05f)
				{
					return -1;
				}
				return 0;
			}
		}

		public Need_Seeker(Pawn pawn)
			: base(pawn)
		{
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				float curInstantLevel = CurInstantLevel;
				if (curInstantLevel > CurLevel)
				{
					CurLevel += def.seekerRisePerHour * 0.06f;
					CurLevel = Mathf.Min(CurLevel, curInstantLevel);
				}
				if (curInstantLevel < CurLevel)
				{
					CurLevel -= def.seekerFallPerHour * 0.06f;
					CurLevel = Mathf.Max(CurLevel, curInstantLevel);
				}
			}
		}
	}
}
