using UnityEngine;
using Verse;

namespace RimWorld;

public class OrbitalStrike : ThingWithComps
{
	public int duration;

	public Thing instigator;

	public ThingDef weaponDef;

	private float angle;

	private int startTick;

	private static readonly FloatRange AngleRange = new FloatRange(-12f, 12f);

	private const int SkyColorFadeInTicks = 30;

	private const int SkyColorFadeOutTicks = 15;

	private const int OrbitalBeamFadeOutTicks = 10;

	protected int TicksPassed => Find.TickManager.TicksGame - startTick;

	protected int TicksLeft => duration - TicksPassed;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref instigator, "instigator");
		Scribe_Defs.Look(ref weaponDef, "weaponDef");
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref angle, "angle", 0f);
		Scribe_Values.Look(ref startTick, "startTick", 0);
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		Comps_PostDraw();
	}

	public virtual void StartStrike()
	{
		if (!base.Spawned)
		{
			Log.Error("Called StartStrike() on unspawned thing.");
			return;
		}
		angle = AngleRange.RandomInRange;
		startTick = Find.TickManager.TicksGame;
		GetComp<CompAffectsSky>()?.StartFadeInHoldFadeOut(30, duration - 30 - 15, 15);
		GetComp<CompOrbitalBeam>().StartAnimation(duration, 10, angle);
	}

	protected override void Tick()
	{
		base.Tick();
		if (TicksPassed >= duration)
		{
			Destroy();
		}
	}
}
