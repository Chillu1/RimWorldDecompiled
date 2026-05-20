using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_HateChantGeneratePositions : LordToil
{
	private static readonly Queue<IntVec3> Frontier = new Queue<IntVec3>();

	private static readonly HashSet<IntVec3> Visited = new HashSet<IntVec3>();

	private const int PositionsPerBatch = 5;

	private readonly int GenerationInterval = 0.2f.SecondsToTicks();

	private readonly string ReadySignal;

	protected LordToilData_HateChantGeneratePositions Data => (LordToilData_HateChantGeneratePositions)data;

	public List<PsychicRitualParticipant> Participants => Data.chanters;

	public LordToil_HateChantGeneratePositions(IEnumerable<PsychicRitualParticipant> chanterPositionPairs, string readySignal)
	{
		PsychicRitualParticipant[] collection = (chanterPositionPairs as PsychicRitualParticipant[]) ?? chanterPositionPairs.ToArray();
		data = new LordToilData_HateChantGeneratePositions();
		Data.chanters = new List<PsychicRitualParticipant>(collection);
		Data.foundPositions = new HashSet<IntVec3>();
		ReadySignal = readySignal;
	}

	public override void UpdateAllDuties()
	{
		foreach (var (pawn2, intVec2) in Data.chanters)
		{
			if (pawn2?.mindState != null)
			{
				pawn2.mindState.duty = new PawnDuty(DutyDefOf.Idle, intVec2);
			}
		}
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		int count = Data.foundPositions.Count;
		if (count == Data.chanters.Count)
		{
			if (DebugViewSettings.drawHateChanterPositions)
			{
				Material mat = DebugSolidColorMats.MaterialOf(Color.magenta * new Color(1f, 1f, 1f, 0.4f));
				foreach (PsychicRitualParticipant participant in Participants)
				{
					base.Map.debugDrawer.FlashCell(participant.location, mat, null, 100000);
				}
			}
			Find.SignalManager.SendSignal(new Signal(ReadySignal));
			return;
		}
		if (count > Data.chanters.Count)
		{
			Log.Error("Generated more positions than chanters.");
		}
		if (base.Map.IsHashIntervalTick(GenerationInterval))
		{
			using (new ProfilerBlock("HateChantersBatch"))
			{
				ProcessBatch(count);
			}
			if (Data.foundPositions.Count == count)
			{
				Log.Error("No positions found in this batch, something is wrong.");
			}
		}
	}

	private void ProcessBatch(int foundPositionsCount)
	{
		int num = Mathf.Min(5, Data.chanters.Count - foundPositionsCount);
		for (int i = 0; i < num; i++)
		{
			PsychicRitualParticipant psychicRitualParticipant = Data.chanters[foundPositionsCount + i];
			IntVec3 position = psychicRitualParticipant.pawn.Position;
			IntVec3 intVec = FindChantingPosition(position, 4, 5, 2000);
			if (intVec != IntVec3.Invalid)
			{
				psychicRitualParticipant.location = intVec;
				if (!Data.foundPositions.Add(intVec))
				{
					Log.Error($"Tried adding pos to found pos twice: {intVec}");
				}
				continue;
			}
			IntVec3 intVec2 = CellFinder.RandomClosewalkCellNear(position, base.Map, 5);
			if (Data.foundPositions.Add(intVec2))
			{
				psychicRitualParticipant.location = intVec2;
				continue;
			}
			psychicRitualParticipant.location = position;
			Data.foundPositions.Add(position);
		}
	}

	private IntVec3 FindChantingPosition(IntVec3 start, int edificeCheckRadius, int minEdificeCount, int maxIterations)
	{
		Frontier.Clear();
		Visited.Clear();
		int num = 0;
		Frontier.Enqueue(start);
		Visited.Add(start);
		while (Frontier.Count > 0)
		{
			IntVec3 intVec = Frontier.Dequeue();
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec2 = intVec + GenAdj.CardinalDirections[i];
				if (ValidateChantingPosition(intVec2, edificeCheckRadius, minEdificeCount, Visited))
				{
					return intVec2;
				}
				Frontier.Enqueue(intVec2);
				Visited.Add(intVec2);
			}
			num++;
			if (num > maxIterations)
			{
				return IntVec3.Invalid;
			}
		}
		return IntVec3.Invalid;
	}

	private bool ValidateChantingPosition(IntVec3 validatedPos, int edificeCheckRadius, int minEdificeCount, HashSet<IntVec3> visited)
	{
		if (!validatedPos.InBounds(base.Map) || visited.Contains(validatedPos) || Data.foundPositions.Contains(validatedPos))
		{
			return false;
		}
		if ((!base.Map.TileInfo.AllowRoofedEdgeWalkIn && validatedPos.Roofed(base.Map)) || !validatedPos.Standable(base.Map) || base.Map.avoidGrid[validatedPos] != 0 || validatedPos.GetFirstPawn(base.Map) != null)
		{
			return false;
		}
		District district = validatedPos.GetDistrict(base.Map);
		if (!district.Room.TouchesMapEdge || !district.Room.PsychologicallyOutdoors)
		{
			return false;
		}
		int num = 0;
		foreach (IntVec3 item in CellRect.CenteredOn(validatedPos, edificeCheckRadius).ClipInsideMap(base.Map))
		{
			if (item.GetEdifice(base.Map) != null)
			{
				num++;
			}
			if (num == minEdificeCount)
			{
				return true;
			}
		}
		return false;
	}
}
