using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class CompObelisk_ExplodingSpawner : CompObelisk
{
	protected int explodeTick = -99999;

	protected float pointsRemaining;

	protected float totalCombatPoints;

	private Lord lord;

	protected int nextSpawnTick = -99999;

	private Effecter effecterExplodeWarmup;

	private List<Thing> thingsIgnoredByExplosion;

	protected virtual IntRange ExplodeDelayTicks => new IntRange(120, 180);

	protected virtual IntRange SpawnIntervalTicks => new IntRange(6, 60);

	protected Lord Lord => lord ?? LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), parent.Map);

	protected void Explode()
	{
		Map map = parent.Map;
		parent.Destroy(DestroyMode.KillFinalize);
		IntVec3 position = parent.Position;
		DamageDef vaporize = DamageDefOf.Vaporize;
		ThingWithComps instigator = parent;
		List<Thing> ignoredThings = thingsIgnoredByExplosion;
		GenExplosion.DoExplosion(position, map, 4.9f, vaporize, instigator, -1, -1f, null, null, null, null, null, 0f, 1, null, null, 255, applyDamageToExplosionCellsNeighbors: false, null, 0f, 1, 0f, damageFalloff: false, null, ignoredThings);
	}

	public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
	{
		base.Notify_KilledLeavingsLeft(leavings);
		if (thingsIgnoredByExplosion == null)
		{
			thingsIgnoredByExplosion = new List<Thing>();
		}
		else
		{
			thingsIgnoredByExplosion.Clear();
		}
		thingsIgnoredByExplosion.AddRange(leavings);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (activated && explodeTick > 0)
		{
			effecterExplodeWarmup?.EffectTick(parent, new TargetInfo(parent.Position, parent.Map));
			if (Find.TickManager.TicksGame > explodeTick)
			{
				effecterExplodeWarmup?.Cleanup();
				effecterExplodeWarmup = null;
				Explode();
			}
		}
	}

	protected void PrepareExplosion()
	{
		effecterExplodeWarmup = EffecterDefOf.ObeliskExplosionWarmup.Spawn();
		effecterExplodeWarmup.Trigger(parent, new TargetInfo(parent.Position, parent.Map));
		explodeTick = Find.TickManager.TicksGame + ExplodeDelayTicks.RandomInRange;
	}

	public override void OnActivityActivated()
	{
		base.OnActivityActivated();
		if (parent.Map != null)
		{
			pointsRemaining = (totalCombatPoints = StorytellerUtility.DefaultThreatPointsNow(parent.Map));
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref pointsRemaining, "pointsRemaining", 0f);
		Scribe_Values.Look(ref totalCombatPoints, "totalCombatPoints", 0f);
		Scribe_References.Look(ref lord, "lord");
		Scribe_Values.Look(ref nextSpawnTick, "nextSpawnTick", 0);
		Scribe_Values.Look(ref explodeTick, "explodeTick", 0);
	}
}
