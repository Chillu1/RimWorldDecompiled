using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PowerNetGrid
{
	private Map map;

	private PowerNet[] netGrid;

	private Dictionary<PowerNet, List<IntVec3>> powerNetCells = new Dictionary<PowerNet, List<IntVec3>>();

	public PowerNetGrid(Map map)
	{
		this.map = map;
		netGrid = new PowerNet[map.cellIndices.NumGridCells];
	}

	public PowerNet TransmittedPowerNetAt(IntVec3 c)
	{
		return netGrid[map.cellIndices.CellToIndex(c)];
	}

	public void Notify_PowerNetCreated(PowerNet newNet)
	{
		if (powerNetCells.ContainsKey(newNet))
		{
			Log.Warning("Net " + newNet?.ToString() + " is already registered in PowerNetGrid.");
			powerNetCells.Remove(newNet);
		}
		List<IntVec3> list = new List<IntVec3>();
		powerNetCells.Add(newNet, list);
		for (int i = 0; i < newNet.transmitters.Count; i++)
		{
			CellRect cellRect = newNet.transmitters[i].parent.OccupiedRect();
			for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
			{
				for (int k = cellRect.minX; k <= cellRect.maxX; k++)
				{
					int num = map.cellIndices.CellToIndex(k, j);
					if (netGrid[num] != null)
					{
						Log.Warning("Two power nets on the same cell (" + k + ", " + j + "). First transmitters: " + newNet.transmitters[0].parent.LabelCap + " and " + (netGrid[num].transmitters.NullOrEmpty() ? "[none]" : netGrid[num].transmitters[0].parent.LabelCap) + ".");
					}
					netGrid[num] = newNet;
					list.Add(new IntVec3(k, 0, j));
				}
			}
		}
	}

	public void Notify_PowerNetDeleted(PowerNet deadNet)
	{
		if (!powerNetCells.TryGetValue(deadNet, out var value))
		{
			Log.Warning("Net " + deadNet?.ToString() + " does not exist in PowerNetGrid's dictionary.");
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			int num = map.cellIndices.CellToIndex(value[i]);
			if (netGrid[num] == deadNet)
			{
				netGrid[num] = null;
			}
			else
			{
				Log.Warning("Multiple nets on the same cell " + value[i].ToString() + ". This is probably a result of an earlier error.");
			}
		}
		powerNetCells.Remove(deadNet);
	}

	public void DrawDebugPowerNetGrid()
	{
		if (!DebugViewSettings.drawPowerNetGrid || Current.ProgramState != ProgramState.Playing || map != Find.CurrentMap)
		{
			return;
		}
		Rand.PushState();
		foreach (IntVec3 item in Find.CameraDriver.CurrentViewRect.ClipInsideMap(map))
		{
			PowerNet powerNet = netGrid[map.cellIndices.CellToIndex(item)];
			if (powerNet != null)
			{
				Rand.Seed = powerNet.GetHashCode();
				CellRenderer.RenderCell(item, Rand.Value);
			}
		}
		Rand.PopState();
	}
}
