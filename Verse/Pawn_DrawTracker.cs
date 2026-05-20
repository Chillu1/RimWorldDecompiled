using RimWorld;
using UnityEngine;

namespace Verse;

public class Pawn_DrawTracker
{
	private Pawn pawn;

	public PawnTweener tweener;

	private JitterHandler jitterer;

	public PawnLeaner leaner;

	public PawnRenderer renderer;

	public PawnUIOverlay ui;

	private PawnFootprintMaker footprintMaker;

	private PawnBreathMoteMaker breathMoteMaker;

	private PawnWaterRippleMaker waterRippleMaker;

	private const float MeleeJitterDistance = 0.5f;

	public Vector3 DrawPos
	{
		get
		{
			tweener.PreDrawPosCalculation();
			return (tweener.TweenedPos + jitterer.CurrentOffset + leaner.LeanOffset + OffsetForcedByJob() + FlyingOffset()).WithY(pawn.def.Altitude + FlightYOffset() + SeededYOffset);
		}
	}

	public float SeededYOffset { get; }

	public Pawn_DrawTracker(Pawn pawn)
	{
		this.pawn = pawn;
		tweener = new PawnTweener(pawn);
		jitterer = new JitterHandler();
		leaner = new PawnLeaner(pawn);
		renderer = new PawnRenderer(pawn);
		ui = new PawnUIOverlay(pawn);
		footprintMaker = new PawnFootprintMaker(pawn);
		breathMoteMaker = new PawnBreathMoteMaker(pawn);
		waterRippleMaker = new PawnWaterRippleMaker(pawn);
		SeededYOffset = Rand.RangeSeeded(-0.03658537f, 0.03658537f, pawn.thingIDNumber);
	}

	public void ProcessPostTickVisuals(int ticksPassed)
	{
		if (pawn.Spawned)
		{
			jitterer.ProcessPostTickVisuals(ticksPassed);
			footprintMaker.ProcessPostTickVisuals(ticksPassed);
			breathMoteMaker.ProcessPostTickVisuals(ticksPassed);
			waterRippleMaker.ProcessPostTickVisuals(ticksPassed);
			leaner.ProcessPostTickVisuals(ticksPassed);
			renderer.ProcessPostTickVisuals(ticksPassed);
		}
	}

	public void DrawShadowAt(Vector3 loc)
	{
		using (new ProfilerBlock("Draw Shadow At()"))
		{
			renderer.RenderShadowOnlyAt(loc);
		}
	}

	private Vector3 OffsetForcedByJob()
	{
		if (pawn.jobs?.curDriver != null)
		{
			return pawn.jobs.curDriver.ForcedBodyOffset;
		}
		return Vector3.zero;
	}

	private Vector3 FlyingOffset()
	{
		if (pawn.flight == null)
		{
			return Vector3.zero;
		}
		return new Vector3(0f, 0f, 0.6f) * pawn.flight.PositionOffsetFactor;
	}

	private float FlightYOffset()
	{
		if (pawn.flight == null)
		{
			return 0f;
		}
		return 0.03658537f * pawn.flight.PositionOffsetFactor;
	}

	public void Notify_Spawned()
	{
		tweener.ResetTweenedPosToRoot();
	}

	public void Notify_WarmingCastAlongLine(ShootLine newShootLine, IntVec3 ShootPosition)
	{
		leaner.Notify_WarmingCastAlongLine(newShootLine, ShootPosition);
	}

	public void Notify_DamageApplied(DamageInfo dinfo)
	{
		if (!pawn.Destroyed && pawn.Spawned)
		{
			jitterer.Notify_DamageApplied(dinfo);
			renderer.Notify_DamageApplied(dinfo);
		}
	}

	public void Notify_DamageDeflected(DamageInfo dinfo)
	{
		if (!pawn.Destroyed)
		{
			jitterer.Notify_DamageDeflected(dinfo);
		}
	}

	public void Notify_MeleeAttackOn(Thing Target)
	{
		if (Target.Position != pawn.Position)
		{
			jitterer.AddOffset(0.5f, (Target.Position - pawn.Position).AngleFlat);
		}
		else if (Target.DrawPos != pawn.DrawPos)
		{
			jitterer.AddOffset(0.25f, (Target.DrawPos - pawn.DrawPos).AngleFlat());
		}
	}

	public void Notify_DebugAffected()
	{
		for (int i = 0; i < 10; i++)
		{
			FleckMaker.ThrowAirPuffUp(pawn.DrawPosHeld.Value, pawn.MapHeld);
		}
		jitterer.AddOffset(0.05f, Rand.Range(0, 360));
	}
}
