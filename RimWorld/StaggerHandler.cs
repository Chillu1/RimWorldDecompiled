using UnityEngine;
using Verse;

namespace RimWorld;

public class StaggerHandler : IExposable
{
	public const float DefaultStaggerMoveSpeedFactor = 0.17f;

	public Pawn parent;

	private int staggerTicksLeft;

	private float staggerMoveSpeedFactor;

	private int staggerEffectTicksLeft;

	private Effecter effecter;

	public bool Staggered => staggerTicksLeft > 0;

	public float StaggerMoveSpeedFactor => staggerMoveSpeedFactor;

	public int StaggerTicksLeft => staggerTicksLeft;

	public StaggerHandler(Pawn parent)
	{
		this.parent = parent;
	}

	public bool StaggerFor(int ticks, float moveSpeedFactor = 0.17f)
	{
		if (ModsConfig.AnomalyActive && parent.health.hediffSet.HasHediff(HediffDefOf.AwokenCorpse))
		{
			return false;
		}
		ticks = Mathf.RoundToInt((float)ticks * parent.GetStatValue(StatDefOf.StaggerDurationFactor));
		if (ticks > 0)
		{
			staggerTicksLeft = Mathf.Max(staggerTicksLeft, ticks);
			staggerMoveSpeedFactor = Mathf.Min(staggerMoveSpeedFactor, moveSpeedFactor);
			return true;
		}
		return false;
	}

	public void Notify_BulletImpact(Bullet bullet)
	{
		if (parent.RaceProps.bulletStaggerIgnoreBodySize || parent.BodySize <= bullet.stoppingPower + 0.001f)
		{
			int ticks = parent.RaceProps.bulletStaggerDelayTicks ?? 95;
			if (StaggerFor(ticks, parent.RaceProps.bulletStaggerSpeedFactor ?? 0.17f) && parent.RaceProps.bulletStaggerEffecterDef != null)
			{
				effecter?.Cleanup();
				effecter = parent.RaceProps.bulletStaggerEffecterDef.Spawn();
			}
		}
	}

	public void StaggerHandlerTick()
	{
		if (staggerTicksLeft > 0)
		{
			staggerTicksLeft--;
			if (staggerTicksLeft <= 0)
			{
				staggerMoveSpeedFactor = 0.17f;
			}
		}
		if (staggerEffectTicksLeft > 0)
		{
			staggerEffectTicksLeft--;
			if (staggerEffectTicksLeft <= 0)
			{
				effecter?.Cleanup();
				effecter = null;
			}
			if (effecter != null)
			{
				effecter.EffectTick(parent, parent);
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref staggerTicksLeft, "staggerTicksLeft", 0);
		Scribe_Values.Look(ref staggerMoveSpeedFactor, "staggerMoveSpeedFactor", 0f);
		Scribe_Values.Look(ref staggerEffectTicksLeft, "staggerEffectTicksLeft", 0);
	}
}
