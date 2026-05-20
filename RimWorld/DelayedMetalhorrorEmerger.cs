using Verse;

namespace RimWorld;

public class DelayedMetalhorrorEmerger : Thing
{
	public int emergeDelayTicks;

	private int emergeTick;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			emergeTick = GenTicks.TicksGame + emergeDelayTicks;
		}
	}

	protected override void Tick()
	{
		if (base.Spawned && GenTicks.TicksGame >= emergeTick)
		{
			Pawn[] array = base.Map.mapPawns.AllHumanlike.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				MetalhorrorUtility.TryEmerge(array[i]);
			}
			Destroy();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref emergeDelayTicks, "emergeDelayTicks", 0);
		Scribe_Values.Look(ref emergeTick, "emergeTick", 0);
	}

	public static DelayedMetalhorrorEmerger Spawn(Map map, int delayTicks)
	{
		DelayedMetalhorrorEmerger delayedMetalhorrorEmerger = (DelayedMetalhorrorEmerger)ThingMaker.MakeThing(ThingDefOf.DelayedMetalhorrorEmerger);
		delayedMetalhorrorEmerger.emergeDelayTicks = delayTicks;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, 0f, allowFogged: true))
		{
			return null;
		}
		GenSpawn.Spawn(delayedMetalhorrorEmerger, result, map);
		return delayedMetalhorrorEmerger;
	}
}
