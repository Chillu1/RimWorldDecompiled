using RimWorld;
using UnityEngine;

namespace Verse;

public class PawnTweener
{
	private Pawn pawn;

	private Vector3 tweenedPos = new Vector3(0f, 0f, 0f);

	private int lastDrawFrame = -1;

	private int lastDrawTick = -1;

	private int lastOffsetTick = -1;

	private Vector3 lastOffset = Vector3.zero;

	private Vector3 lastTickSpringPos;

	private const float SpringTightness = 0.09f;

	private const float CrawlsPerTile = 3f;

	private const float CrawlingLurchFactor = 3f;

	public Vector3 TweenedPos => tweenedPos;

	public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

	public PawnTweener(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void PreDrawPosCalculation()
	{
		if (lastDrawFrame == RealTime.frameCount)
		{
			return;
		}
		if (!pawn.Spawned)
		{
			tweenedPos = pawn.Position.ToVector3Shifted();
			return;
		}
		if (lastDrawFrame < RealTime.frameCount - 1 && lastDrawTick < GenTicks.TicksGame - 1)
		{
			ResetTweenedPosToRoot();
		}
		else
		{
			lastTickSpringPos = tweenedPos;
			float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
			if (tickRateMultiplier < 5f)
			{
				Vector3 vector = TweenedPosRoot() - tweenedPos;
				float num = 0.09f * (RealTime.deltaTime * 60f * tickRateMultiplier);
				if (RealTime.deltaTime > 0.05f)
				{
					num = Mathf.Min(num, 1f);
				}
				tweenedPos += vector * num;
			}
			else
			{
				ResetTweenedPosToRoot();
			}
		}
		lastDrawFrame = RealTime.frameCount;
		lastDrawTick = GenTicks.TicksGame;
	}

	public void ResetTweenedPosToRoot()
	{
		tweenedPos = TweenedPosRoot();
		lastTickSpringPos = tweenedPos;
		lastDrawFrame = RealTime.frameCount;
		lastDrawTick = GenTicks.TicksGame;
	}

	public void Notify_Teleported()
	{
		lastDrawFrame = -1;
		lastDrawTick = -1;
	}

	private Vector3 TweenedPosRoot()
	{
		if (!pawn.Spawned)
		{
			return pawn.Position.ToVector3Shifted();
		}
		float z = 0f;
		if (pawn.Spawned && pawn.ageTracker.CurLifeStage.sittingOffset.HasValue && !pawn.pather.MovingNow && pawn.GetPosture() == PawnPosture.Standing)
		{
			Building edifice = pawn.Position.GetEdifice(pawn.Map);
			if (edifice != null && edifice.def.building != null && edifice.def.building.isSittable)
			{
				z = pawn.ageTracker.CurLifeStage.sittingOffset.Value;
			}
		}
		float num = pawn.pather.MovePercentage;
		if (pawn.Crawling)
		{
			num = num - num % (1f / 3f) + Mathf.Pow(num % (1f / 3f) * 3f, 3f) * (1f / 3f);
		}
		Vector3 vector;
		if (GenTicks.TicksGame == lastOffsetTick)
		{
			vector = lastOffset;
		}
		else
		{
			vector = (lastOffset = PawnCollisionTweenerUtility.PawnCollisionPosOffsetFor(pawn));
			lastOffsetTick = GenTicks.TicksGame;
		}
		return pawn.pather.nextCell.ToVector3Shifted() * num + pawn.Position.ToVector3Shifted() * (1f - num) + new Vector3(0f, 0f, z) + vector;
	}
}
