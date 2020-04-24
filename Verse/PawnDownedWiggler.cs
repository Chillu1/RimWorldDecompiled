using RimWorld;

namespace Verse
{
	public class PawnDownedWiggler
	{
		private Pawn pawn;

		public float downedAngle = RandomDownedAngle;

		public int ticksToIncapIcon;

		private bool usingCustomRotation;

		private const float DownedAngleWidth = 45f;

		private const float DamageTakenDownedAngleShift = 10f;

		private const int IncapWigglePeriod = 300;

		private const int IncapWiggleLength = 90;

		private const float IncapWiggleSpeed = 0.35f;

		private const int TicksBetweenIncapIcons = 200;

		private static float RandomDownedAngle
		{
			get
			{
				float num = Rand.Range(45f, 135f);
				if (Rand.Value < 0.5f)
				{
					num += 180f;
				}
				return num;
			}
		}

		public PawnDownedWiggler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void WigglerTick()
		{
			if (!pawn.Downed || !pawn.Spawned || pawn.InBed())
			{
				return;
			}
			ticksToIncapIcon--;
			if (ticksToIncapIcon <= 0)
			{
				MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_IncapIcon);
				ticksToIncapIcon = 200;
			}
			if (pawn.Awake())
			{
				int num = Find.TickManager.TicksGame % 300 * 2;
				if (num < 90)
				{
					downedAngle += 0.35f;
				}
				else if (num < 390 && num >= 300)
				{
					downedAngle -= 0.35f;
				}
			}
		}

		public void SetToCustomRotation(float rot)
		{
			downedAngle = rot;
			usingCustomRotation = true;
		}

		public void Notify_DamageApplied(DamageInfo dam)
		{
			if ((!pawn.Downed && !pawn.Dead) || !dam.Def.hasForcefulImpact)
			{
				return;
			}
			downedAngle += 10f * Rand.Range(-1f, 1f);
			if (!usingCustomRotation)
			{
				if (downedAngle > 315f)
				{
					downedAngle = 315f;
				}
				if (downedAngle < 45f)
				{
					downedAngle = 45f;
				}
				if (downedAngle > 135f && downedAngle < 225f)
				{
					if (downedAngle > 180f)
					{
						downedAngle = 225f;
					}
					else
					{
						downedAngle = 135f;
					}
				}
			}
			else
			{
				if (downedAngle >= 360f)
				{
					downedAngle -= 360f;
				}
				if (downedAngle < 0f)
				{
					downedAngle += 360f;
				}
			}
		}
	}
}
