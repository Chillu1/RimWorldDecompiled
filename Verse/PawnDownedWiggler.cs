using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnDownedWiggler
{
	private Pawn pawn;

	public float downedAngle = RandomDownedAngle;

	public int ticksToIncapIcon;

	private bool usingCustomRotation;

	private int wiggleOffset;

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
		wiggleOffset = Mathf.Abs(pawn.HashOffset()) % 600;
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (!pawn.Downed || !pawn.Spawned || pawn.InBed())
		{
			return;
		}
		ticksToIncapIcon -= ticksPassed;
		if (ticksToIncapIcon <= 0)
		{
			ticksToIncapIcon = 200 + ticksToIncapIcon;
			if (HealthAIUtility.WantsToBeRescued(pawn))
			{
				FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.IncapIcon);
			}
		}
		if (pawn.Awake() && (!ModsConfig.BiotechActive || pawn.CurJob?.def != JobDefOf.Breastfeed))
		{
			if (ticksPassed > 600)
			{
				Log.Warning("Too many ticks passed during a single frame for sensical wiggling");
			}
			int num = (Find.TickManager.TicksGame + wiggleOffset) % 600;
			ProcessWigglePeriod(num, ticksPassed, positiveAngleWiggle: true);
			ProcessWigglePeriod((num + 300) % 600, ticksPassed, positiveAngleWiggle: false);
		}
	}

	private void ProcessWigglePeriod(int wigglePeriodTick, int ticksPassed, bool positiveAngleWiggle)
	{
		int a = 0;
		int b = wigglePeriodTick - ticksPassed;
		int num = Mathf.Min(90, wigglePeriodTick) - Mathf.Max(a, b);
		if (num > 0)
		{
			downedAngle += 0.35f * (float)num * (float)(positiveAngleWiggle ? 1 : (-1));
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
