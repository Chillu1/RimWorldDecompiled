using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class PrisonBreakUtility
{
	private const float DistanceToJoinPrisonBreak = 20f;

	private const float ChanceForRoomToJoinPrisonBreak = 0.5f;

	private const float SapperChance = 0.5f;

	private const float BlissLobotomyMultiplier = 10f;

	private const float PrisonBreakMTBFactorSecurityDoorsOnly = 1.1f;

	private static readonly SimpleCurve PrisonBreakMTBFactorForDaysSincePrisonBreak = new SimpleCurve
	{
		new CurvePoint(0f, 20f),
		new CurvePoint(5f, 1.5f),
		new CurvePoint(10f, 1f)
	};

	private static HashSet<Region> tmpRegions = new HashSet<Region>();

	private static HashSet<Building_Door> tmpDoors = new HashSet<Building_Door>();

	private static HashSet<Room> participatingRooms = new HashSet<Room>();

	private static List<Pawn> allEscapingPrisoners = new List<Pawn>();

	private static List<Room> tmpToRemove = new List<Room>();

	private static List<Pawn> escapingPrisonersGroup = new List<Pawn>();

	public static float InitiatePrisonBreakMtbDays(Pawn pawn, StringBuilder sb = null, bool ignoreAsleep = false)
	{
		if (!ignoreAsleep && !pawn.Awake())
		{
			return -1f;
		}
		if (!CanParticipateInPrisonBreak(pawn))
		{
			return -1f;
		}
		Room room = pawn.GetRoom();
		if (room == null || !room.IsPrisonCell)
		{
			return -1f;
		}
		float basePrisonBreakMtbDays = pawn.kindDef.basePrisonBreakMtbDays;
		float num = Mathf.Clamp(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving), 0.01f, 1f);
		basePrisonBreakMtbDays /= num;
		if (sb != null && num != 1f)
		{
			sb.AppendLineIfNotEmpty();
			sb.Append("  - " + "FactorForMovement".Translate() + ": " + (1f / num).ToStringPercent());
		}
		if (pawn.genes != null)
		{
			float prisonBreakIntervalFactor = pawn.genes.PrisonBreakIntervalFactor;
			basePrisonBreakMtbDays *= prisonBreakIntervalFactor;
			if (sb != null && prisonBreakIntervalFactor != 1f)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append(string.Format("  - {0}: {1}", "FactorForGenes".Translate(), prisonBreakIntervalFactor.ToStringPercent()));
			}
		}
		tmpRegions.Clear();
		tmpDoors.Clear();
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
		float num2 = tmpDoors.Count;
		if (num2 > 0f)
		{
			basePrisonBreakMtbDays /= num2;
			if (sb != null && num2 > 1f)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("  - " + "FactorForDoorCount".Translate() + ": " + (1f / num2).ToStringPercent());
			}
			if (ModsConfig.AnomalyActive)
			{
				bool flag = false;
				bool flag2 = true;
				foreach (Building_Door tmpDoor in tmpDoors)
				{
					if (tmpDoor.def != ThingDefOf.SecurityDoor)
					{
						flag2 = false;
						break;
					}
					flag = true;
				}
				if (flag && flag2)
				{
					if (sb != null)
					{
						sb.AppendLineIfNotEmpty();
						sb.Append("  - " + "SecurityDoorsOnly".Translate() + ": " + 1.1f.ToStringPercent());
					}
					basePrisonBreakMtbDays *= 1.1f;
				}
			}
		}
		if (ModsConfig.AnomalyActive && pawn.health.hediffSet.HasHediff(HediffDefOf.BlissLobotomy))
		{
			basePrisonBreakMtbDays *= 10f;
			if (sb != null)
			{
				sb.AppendLineIfNotEmpty();
				sb.Append("  - " + "BlissLobotomy".Translate() + ": " + 10f.ToStringPercent());
			}
		}
		if (pawn.guest.everParticipatedInPrisonBreak)
		{
			float x = (float)(Find.TickManager.TicksGame - pawn.guest.lastPrisonBreakTicks) / 60000f;
			basePrisonBreakMtbDays *= PrisonBreakMTBFactorForDaysSincePrisonBreak.Evaluate(x);
		}
		return basePrisonBreakMtbDays;
	}

	public static bool CanParticipateInPrisonBreak(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return false;
		}
		if (!pawn.IsPrisoner)
		{
			return false;
		}
		if (IsPrisonBreaking(pawn))
		{
			return false;
		}
		if (GenePreventsPrisonBreaking(pawn, out var _))
		{
			return false;
		}
		if (pawn.Spawned && pawn.Map.IsPocketMap && pawn.Map.generatorDef.pocketMapProperties.preventPrisonerEscape)
		{
			return false;
		}
		if (pawn.Spawned && pawn.Map.Biome.inVacuum)
		{
			return false;
		}
		return true;
	}

	public static bool GenePreventsPrisonBreaking(Pawn pawn, out Gene gene)
	{
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			foreach (Gene item in pawn.genes.GenesListForReading)
			{
				if (item.def.prisonBreakMTBFactor < 0f)
				{
					gene = item;
					return true;
				}
			}
		}
		gene = null;
		return false;
	}

	public static bool IsPrisonBreaking(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			return lord.LordJob is LordJob_PrisonBreak;
		}
		return false;
	}

	public static void StartPrisonBreak(Pawn initiator)
	{
		StartPrisonBreak(initiator, out var letterText, out var letterLabel, out var letterDef, out var escapingPrisoners);
		if (!letterText.NullOrEmpty())
		{
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef, escapingPrisoners);
		}
	}

	public static void StartPrisonBreak(Pawn initiator, out string letterText, out string letterLabel, out LetterDef letterDef, out List<Pawn> escapingPrisoners)
	{
		participatingRooms.Clear();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(initiator.Position, 20f, useCenter: true))
		{
			if (item.InBounds(initiator.Map))
			{
				Room room = item.GetRoom(initiator.Map);
				if (room != null && IsOrCanBePrisonCell(room))
				{
					participatingRooms.Add(room);
				}
			}
		}
		RemoveRandomRooms(participatingRooms, initiator);
		int sapperThingID = -1;
		if (Rand.Value < 0.5f)
		{
			sapperThingID = initiator.thingIDNumber;
		}
		allEscapingPrisoners.Clear();
		foreach (Room participatingRoom in participatingRooms)
		{
			StartPrisonBreakIn(participatingRoom, allEscapingPrisoners, sapperThingID, participatingRooms);
		}
		participatingRooms.Clear();
		if (allEscapingPrisoners.Any())
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < allEscapingPrisoners.Count; i++)
			{
				stringBuilder.AppendLine("  - " + allEscapingPrisoners[i].NameShortColored.Resolve());
				allEscapingPrisoners[i].Notify_PrisonBreakout();
			}
			letterText = "LetterPrisonBreak".Translate(stringBuilder.ToString().TrimEndNewlines());
			letterLabel = "LetterLabelPrisonBreak".Translate();
			letterDef = LetterDefOf.ThreatBig;
			escapingPrisoners = allEscapingPrisoners.ToList();
			allEscapingPrisoners.Clear();
		}
		else
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			escapingPrisoners = null;
		}
		Find.TickManager.slower.SignalForceNormalSpeed();
	}

	private static bool IsOrCanBePrisonCell(Room room)
	{
		if (room.IsPrisonCell)
		{
			return true;
		}
		if (room.TouchesMapEdge)
		{
			return false;
		}
		bool result = false;
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (containedAndAdjacentThings[i] is Pawn { IsPrisoner: not false })
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private static void RemoveRandomRooms(HashSet<Room> participatingRooms, Pawn initiator)
	{
		Room room = initiator.GetRoom();
		tmpToRemove.Clear();
		foreach (Room participatingRoom in participatingRooms)
		{
			if (participatingRoom != room && !(Rand.Value < 0.5f))
			{
				tmpToRemove.Add(participatingRoom);
			}
		}
		for (int i = 0; i < tmpToRemove.Count; i++)
		{
			participatingRooms.Remove(tmpToRemove[i]);
		}
		tmpToRemove.Clear();
	}

	private static void StartPrisonBreakIn(Room room, List<Pawn> outAllEscapingPrisoners, int sapperThingID, HashSet<Room> participatingRooms)
	{
		escapingPrisonersGroup.Clear();
		AddPrisonersFrom(room, escapingPrisonersGroup);
		if (!escapingPrisonersGroup.Any())
		{
			return;
		}
		foreach (Room participatingRoom in participatingRooms)
		{
			if (participatingRoom != room && RoomsAreCloseToEachOther(room, participatingRoom))
			{
				AddPrisonersFrom(participatingRoom, escapingPrisonersGroup);
			}
		}
		if (!RCellFinder.TryFindRandomExitSpot(escapingPrisonersGroup[0], out var spot, TraverseMode.PassDoors) || !TryFindGroupUpLoc(escapingPrisonersGroup, spot, out var groupUpLoc))
		{
			return;
		}
		LordMaker.MakeNewLord(escapingPrisonersGroup[0].Faction, new LordJob_PrisonBreak(groupUpLoc, spot, sapperThingID), room.Map, escapingPrisonersGroup);
		for (int i = 0; i < escapingPrisonersGroup.Count; i++)
		{
			Pawn pawn = escapingPrisonersGroup[i];
			if (pawn.CurJob != null && pawn.GetPosture().Laying())
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			else
			{
				pawn.jobs.CheckForJobOverride();
			}
			pawn.guest.everParticipatedInPrisonBreak = true;
			pawn.guest.lastPrisonBreakTicks = Find.TickManager.TicksGame;
			outAllEscapingPrisoners.Add(pawn);
		}
		escapingPrisonersGroup.Clear();
	}

	private static void AddPrisonersFrom(Room room, List<Pawn> outEscapingPrisoners)
	{
		foreach (Thing containedAndAdjacentThing in room.ContainedAndAdjacentThings)
		{
			if (containedAndAdjacentThing is Pawn pawn && CanParticipateInPrisonBreak(pawn) && !outEscapingPrisoners.Contains(pawn))
			{
				outEscapingPrisoners.Add(pawn);
			}
		}
	}

	public static bool TryFindGroupUpLoc(List<Pawn> escapingPrisoners, IntVec3 exitPoint, out IntVec3 groupUpLoc)
	{
		groupUpLoc = IntVec3.Invalid;
		Map map = escapingPrisoners[0].Map;
		using (PawnPath pawnPath = map.pathFinder.FindPathNow(escapingPrisoners[0].Position, exitPoint, TraverseParms.For(escapingPrisoners[0], Danger.Deadly, TraverseMode.PassDoors)))
		{
			if (!pawnPath.Found)
			{
				Log.Warning("Prison break: could not find path for prisoner " + escapingPrisoners[0]?.ToString() + " to the exit point.");
				return false;
			}
			for (int i = 0; i < pawnPath.NodesLeftCount; i++)
			{
				IntVec3 intVec = pawnPath.Peek(pawnPath.NodesLeftCount - i - 1);
				Room room = intVec.GetRoom(map);
				if (room != null && !room.IsPrisonCell && (room.TouchesMapEdge || room.IsHuge || room.Cells.Count((IntVec3 x) => x.Standable(map)) >= 5))
				{
					groupUpLoc = CellFinder.RandomClosewalkCellNear(intVec, map, 3);
				}
			}
		}
		if (!groupUpLoc.IsValid)
		{
			groupUpLoc = escapingPrisoners[0].Position;
		}
		return true;
	}

	private static bool RoomsAreCloseToEachOther(Room a, Room b)
	{
		IntVec3 anyCell = a.FirstRegion.AnyCell;
		IntVec3 anyCell2 = b.FirstRegion.AnyCell;
		if (a.Map != b.Map)
		{
			return false;
		}
		if (!anyCell.WithinRegions(anyCell2, a.Map, 18, TraverseParms.For(TraverseMode.PassDoors)))
		{
			return false;
		}
		using PawnPath pawnPath = a.Map.pathFinder.FindPathNow(anyCell, anyCell2, TraverseParms.For(TraverseMode.PassDoors));
		if (!pawnPath.Found)
		{
			return false;
		}
		return pawnPath.NodesLeftCount < 24;
	}
}
