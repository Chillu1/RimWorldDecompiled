using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class StatWorker_ContainmentStrength : StatWorker
{
	private struct ContainmentStrengthValues
	{
		public float lighting;

		public float wallHp;

		public float doorHp;

		public float floor;

		public int otherHoldingPlatformsCount;

		public float otherHoldingPlatformsOffset;

		public float globalOffset;

		public float fullyRoofed;

		public float holderFactor;

		public bool doorBreach;

		public float Sum => (lighting + wallHp + doorHp + otherHoldingPlatformsOffset + globalOffset + fullyRoofed + floor) * holderFactor;
	}

	private static HashSet<Region> tmpRegions = new HashSet<Region>();

	private static HashSet<Building_Door> tmpDoors = new HashSet<Building_Door>();

	private const float LightingMultiplier = 10f;

	private const float DoorHpDivisor = 5f;

	private const float OtherHoldingPlatformsFactor = 0.9f;

	private const float NotFullyRoofed = -30f;

	private static readonly SimpleCurve WallContainmentStrengthFromHPCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1000f, 100f),
		new CurvePoint(10000f, 150f)
	};

	private static StringBuilder explanationSB = new StringBuilder();

	public override bool ShouldShowFor(StatRequest req)
	{
		if (req.Def is ThingDef thingDef && thingDef.HasComp<CompEntityHolder>())
		{
			return true;
		}
		if (req.StatBases.GetStatValueFromList(StatDefOf.ContainmentStrength, 0f) != 0f)
		{
			return true;
		}
		return false;
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		return base.GetValueUnfinalized(req, applyPostProcess) + CalculateValues(req).Sum;
	}

	public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
	{
		explanationSB.Clear();
		if (req.HasThing)
		{
			ContainmentStrengthValues containmentStrengthValues = CalculateValues(req);
			if (containmentStrengthValues.lighting > 0f)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_AverageLighting".Translate(), containmentStrengthValues.lighting));
			}
			if (containmentStrengthValues.fullyRoofed < 0f)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_NotFullyRoofed".Translate(), containmentStrengthValues.fullyRoofed));
			}
			if (containmentStrengthValues.wallHp > 0f)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_AverageWallHp".Translate(), containmentStrengthValues.wallHp));
			}
			if (containmentStrengthValues.doorHp > 0f || containmentStrengthValues.doorBreach)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_AverageDoorHp".Translate(), containmentStrengthValues.doorHp) + (containmentStrengthValues.doorBreach ? string.Format(" ({0})", "Stat_ContainmentStrength_DoorForcedOpen".Translate()) : string.Empty));
			}
			if (containmentStrengthValues.floor > 0f)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_Flooring".Translate(), containmentStrengthValues.floor));
			}
			if (containmentStrengthValues.otherHoldingPlatformsCount > 0)
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_OtherHoldingPlatforms".Translate(containmentStrengthValues.otherHoldingPlatformsCount + 1), containmentStrengthValues.otherHoldingPlatformsOffset));
			}
			if (!Mathf.Approximately(containmentStrengthValues.globalOffset, 0f))
			{
				explanationSB.AppendLine(string.Format("{0}: {1:F2}", "Stat_ContainmentStrength_MapwideOffset".Translate(), containmentStrengthValues.globalOffset));
			}
			if (!Mathf.Approximately(containmentStrengthValues.holderFactor, 1f))
			{
				explanationSB.AppendLine(string.Format("{0}: x{1:F2}", "Stat_ContainmentStrength_Platform".Translate(), containmentStrengthValues.holderFactor));
			}
		}
		explanationSB.Append(base.GetExplanationUnfinalized(req, numberSense));
		return explanationSB.ToString();
	}

	private ContainmentStrengthValues CalculateValues(StatRequest req)
	{
		if (!req.HasThing)
		{
			return default(ContainmentStrengthValues);
		}
		Map mapHeld = req.Thing.MapHeld;
		Room room = req.Thing.GetRoom();
		if (room == null || mapHeld == null || room.TouchesMapEdge)
		{
			return default(ContainmentStrengthValues);
		}
		bool psychologicallyOutdoors = room.PsychologicallyOutdoors;
		float num = 0f;
		foreach (IntVec3 cell in room.Cells)
		{
			if (!psychologicallyOutdoors)
			{
				num += mapHeld.glowGrid.GroundGlowAt(cell);
			}
		}
		num = num / (float)room.CellCount * 10f;
		float num2 = 0f;
		float avgDoorHp = 0f;
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		float globalOffset = 0f;
		bool doorBreach = false;
		float num6 = 0f;
		int num7 = 0;
		float holderFactor = 1f;
		if (!psychologicallyOutdoors)
		{
			int doorCount;
			try
			{
				CalculateDoorStats(room, out doorCount, out avgDoorHp, out doorBreach);
			}
			finally
			{
				tmpRegions.Clear();
				tmpDoors.Clear();
			}
			foreach (Thing containedAndAdjacentThing in room.ContainedAndAdjacentThings)
			{
				if (ThingRequestGroup.EntityHolder.Includes(containedAndAdjacentThing.def) && containedAndAdjacentThing != req.Thing)
				{
					num5++;
				}
			}
			if (req.Thing.TryGetComp(out CompEntityHolder comp))
			{
				holderFactor = comp.Props.containmentFactor;
			}
			foreach (IntVec3 cell2 in room.Cells)
			{
				if (cell2.InBounds(mapHeld) && !cell2.Filled(mapHeld))
				{
					num6 += cell2.GetTerrain(mapHeld).statBases.GetStatValueFromList(StatDefOf.ContainmentStrength, 0f);
					num7++;
				}
			}
			foreach (IntVec3 item in room.BorderCellsCached.Distinct())
			{
				if (item.InBounds(mapHeld))
				{
					Building edifice = item.GetEdifice(mapHeld);
					if (edifice != null && !(edifice is Building_Door))
					{
						num2 += (float)edifice.HitPoints;
						num4++;
					}
				}
			}
			num3 = ((num7 == 0) ? 0f : (num6 / (float)num7));
			num2 = ((num4 == 0) ? 0f : WallContainmentStrengthFromHPCurve.Evaluate(num2 / (float)num4));
			avgDoorHp = ((doorCount == 0) ? 0f : (avgDoorHp / (float)doorCount / 5f));
		}
		float num8 = num + num2 + avgDoorHp + num3;
		float num9 = num8;
		for (int i = 0; i < num5; i++)
		{
			num8 *= 0.9f;
		}
		num9 = num8 - num9;
		return new ContainmentStrengthValues
		{
			lighting = num,
			wallHp = num2,
			doorHp = avgDoorHp,
			otherHoldingPlatformsCount = num5,
			otherHoldingPlatformsOffset = num9,
			globalOffset = globalOffset,
			floor = num3,
			fullyRoofed = ((!psychologicallyOutdoors && room.OpenRoofCount > 0) ? (-30f) : 0f),
			holderFactor = holderFactor,
			doorBreach = doorBreach
		};
	}

	private void CalculateDoorStats(Room room, out int doorCount, out float avgDoorHp, out bool doorBreach)
	{
		foreach (Region region in room.Regions)
		{
			foreach (RegionLink link in region.links)
			{
				Region otherRegion = link.GetOtherRegion(region);
				if (otherRegion.type != RegionType.Portal || !tmpRegions.Add(otherRegion))
				{
					continue;
				}
				Building_Door door = otherRegion.door;
				for (int i = 0; i < otherRegion.links.Count; i++)
				{
					Region regionA = otherRegion.links[i].RegionA;
					Region regionB = otherRegion.links[i].RegionB;
					if ((regionA.Room != room && regionA != otherRegion && regionA.door != door) || (regionB.Room != room && regionB != otherRegion && regionB.door != door))
					{
						tmpDoors.Add(door);
						break;
					}
				}
			}
		}
		doorCount = tmpDoors.Count;
		avgDoorHp = 0f;
		doorBreach = false;
		foreach (Building_Door tmpDoor in tmpDoors)
		{
			if (tmpDoor.ContainmentBreached)
			{
				doorBreach = true;
				avgDoorHp = 0f;
			}
			else if (!doorBreach)
			{
				avgDoorHp += tmpDoor.HitPoints;
			}
		}
	}

	public static bool AnyDoorForcedOpen(Room room)
	{
		if (room.PsychologicallyOutdoors)
		{
			return false;
		}
		foreach (Thing containedAndAdjacentThing in room.ContainedAndAdjacentThings)
		{
			if (containedAndAdjacentThing is Building_Door { ContainmentBreached: not false })
			{
				return true;
			}
		}
		return false;
	}
}
