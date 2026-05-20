using Verse;
using Verse.Sound;

namespace RimWorld;

public class DelayedEffecterSpawner : Thing
{
	public Pawn pawnToSpawn;

	public int emergeDelayTicks;

	public EffecterDef emergeEffecter;

	public SoundDef emergeSound;

	public EffecterDef preEmergeEffecter;

	private int spawnTick;

	private int spawnPreEffectTick;

	private bool spawnedMistEffect;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			spawnTick = GenTicks.TicksGame + emergeDelayTicks;
			if (preEmergeEffecter != null)
			{
				spawnPreEffectTick = spawnTick - preEmergeEffecter.maintainTicks;
			}
		}
	}

	protected override void Tick()
	{
		if (base.Spawned)
		{
			if (GenTicks.TicksGame >= spawnPreEffectTick && !spawnedMistEffect)
			{
				spawnedMistEffect = true;
				preEmergeEffecter?.Spawn(base.Position, base.Map).Cleanup();
			}
			if (GenTicks.TicksGame >= spawnTick)
			{
				Map map = base.Map;
				IntVec3 position = base.Position;
				Destroy();
				GenSpawn.Spawn(pawnToSpawn, position, map);
				emergeEffecter?.Spawn(position, map).Cleanup();
				emergeSound?.PlayOneShot(pawnToSpawn);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref emergeDelayTicks, "emergeDelayTicks", 0);
		Scribe_Values.Look(ref spawnPreEffectTick, "spawnPreEffectTick", 0);
		Scribe_Values.Look(ref spawnedMistEffect, "spawnedMistEffect", defaultValue: false);
		Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
		Scribe_Defs.Look(ref emergeEffecter, "emergeEffecter");
		Scribe_Defs.Look(ref emergeSound, "emergeSound");
		Scribe_Defs.Look(ref preEmergeEffecter, "preEmergeEffecter");
		Scribe_Deep.Look(ref pawnToSpawn, "pawnToSpawn");
	}

	public static DelayedEffecterSpawner Spawn(Pawn pawn, IntVec3 pos, Map map, int delayTicks, EffecterDef emergeEffect = null, EffecterDef preEmergeEffect = null, SoundDef emergeSound = null)
	{
		DelayedEffecterSpawner obj = (DelayedEffecterSpawner)ThingMaker.MakeThing(ThingDefOf.DelayedEffecterSpawner);
		obj.pawnToSpawn = pawn;
		obj.emergeDelayTicks = delayTicks;
		obj.preEmergeEffecter = preEmergeEffect;
		obj.emergeEffecter = emergeEffect;
		obj.emergeSound = emergeSound;
		GenSpawn.Spawn(obj, pos, map);
		return obj;
	}
}
