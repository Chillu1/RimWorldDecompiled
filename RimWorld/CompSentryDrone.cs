using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompSentryDrone : ThingComp
{
	public enum SentryDroneMode
	{
		Patrol,
		Attack
	}

	private const int TicksUntilLeaveCombatState = 3600;

	private const float DoubleBackChance = 0.1f;

	private SentryDroneMode mode;

	private IntVec3 lastPatrolDest;

	private IntVec3 spawnPos;

	private long lastSensedHostileTick;

	private Pawn Drone => parent as Pawn;

	public SentryDroneMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			spawnPos = parent.Position;
		}
	}

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (Drone.Spawned && !Drone.Dead && Mode != SentryDroneMode.Attack)
		{
			Mode = SentryDroneMode.Attack;
			Drone.jobs.EndCurrentJob(JobCondition.InterruptForced);
			lastSensedHostileTick = Find.TickManager.TicksGame;
		}
	}

	public override void CompTickInterval(int delta)
	{
		if (!parent.Spawned || !parent.IsHashIntervalTick(60, delta))
		{
			return;
		}
		if (Mode == SentryDroneMode.Patrol)
		{
			Map map = parent.Map;
			IntVec3 position = parent.Position;
			{
				foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
				{
					if (!(item.Position.DistanceTo(position) > 20f) && item.HostileTo(parent) && GenSight.LineOfSight(position, item.Position, map))
					{
						Mode = SentryDroneMode.Attack;
						Drone.jobs.EndCurrentJob(JobCondition.InterruptForced);
						break;
					}
				}
				return;
			}
		}
		if (Drone.mindState.anyCloseHostilesRecently)
		{
			lastSensedHostileTick = Find.TickManager.TicksGame;
		}
		else if (Math.Abs(lastSensedHostileTick - Find.TickManager.TicksGame) >= 3600)
		{
			Mode = SentryDroneMode.Patrol;
			Drone.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	public IntVec3 GetNextPatrolDest()
	{
		if (!parent.Spawned)
		{
			return IntVec3.Invalid;
		}
		IntVec3 actualLastPatrolDest = lastPatrolDest;
		lastPatrolDest = parent.Position;
		Room room = parent.GetRoom();
		if (room == null || room.PsychologicallyOutdoors)
		{
			if (spawnPos.IsValid)
			{
				Room room2 = spawnPos.GetRoom(parent.Map);
				if (room2 != null && !room2.PsychologicallyOutdoors)
				{
					return room2.Cells.RandomElement();
				}
			}
			return IntVec3.Invalid;
		}
		List<Room> adjacentRooms = GetAdjacentRooms(room);
		if (adjacentRooms.Count > 0)
		{
			if (adjacentRooms.Count == 1)
			{
				return adjacentRooms[0].Cells.RandomElement();
			}
			if (Rand.Chance(0.1f))
			{
				Room room3 = adjacentRooms.FirstOrDefault((Room r) => r.ContainsCell(actualLastPatrolDest));
				if (room3 != null)
				{
					return room3.Cells.RandomElement();
				}
			}
			List<Room> list = adjacentRooms.Where((Room r) => !r.ContainsCell(actualLastPatrolDest)).ToList();
			if (list.Count == 0)
			{
				Log.Error("CompSentryDrone found multiple adjacent rooms but none of them were not the last room we were in?");
				return IntVec3.Invalid;
			}
			return list.RandomElement().Cells.RandomElement();
		}
		List<IntVec3> list2 = GenRadial.RadialCellsAround(parent.Position, 14f, useCenter: true).Where(delegate(IntVec3 c)
		{
			Room room4 = c.GetRoom(parent.Map);
			return room4 != null && !room4.PsychologicallyOutdoors && room4 != room;
		}).Take(30)
			.ToList();
		if (list2.Count > 0)
		{
			return list2.RandomElement();
		}
		return room.Cells.RandomElement();
	}

	private static List<Room> GetAdjacentRooms(Room room)
	{
		List<Room> list = new List<Room>();
		foreach (Region region in room.Regions)
		{
			foreach (RegionLink link in region.links)
			{
				Region otherRegion = link.GetOtherRegion(region);
				if (!otherRegion.IsDoorway)
				{
					continue;
				}
				foreach (RegionLink link2 in otherRegion.links)
				{
					Region otherRegion2 = link2.GetOtherRegion(otherRegion);
					if (!otherRegion2.Room.PsychologicallyOutdoors && !list.Contains(otherRegion2.Room) && otherRegion2.Room != room)
					{
						list.Add(otherRegion2.Room);
					}
				}
			}
		}
		return list;
	}

	public override bool WantHoldWeapon(Pawn pawn)
	{
		return mode == SentryDroneMode.Attack;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref mode, "mode", SentryDroneMode.Patrol);
		Scribe_Values.Look(ref lastPatrolDest, "lastPatrolDest");
		Scribe_Values.Look(ref spawnPos, "spawnPos");
		Scribe_Values.Look(ref lastSensedHostileTick, "lastSensedHostileTick", 0L);
	}
}
