using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompTerraformer : ThingComp
{
	private int convertTick;

	public CompProperties_Terraformer Props => (CompProperties_Terraformer)props;

	public int TicksTillConvert => Mathf.Max(convertTick - GenTicks.TicksGame, 0);

	private bool Active
	{
		get
		{
			if (!parent.Spawned)
			{
				return false;
			}
			if (!TryGetCellToCovert(out var _))
			{
				return false;
			}
			return true;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
		{
			convertTick = GenTicks.TicksGame + Props.secondsPerConvert.SecondsToTicks();
		}
	}

	private bool TryGetCellToCovert(out IntVec3 cell)
	{
		int num = GenRadial.NumCellsInRadius(Props.radius);
		Map map = parent.Map;
		for (int i = 0; i < num; i++)
		{
			cell = parent.Position + GenRadial.RadialPattern[i];
			if (CanEverConvertCell(cell, map, Props.convertTerrainDef))
			{
				return true;
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}

	public static bool CanEverConvertCell(IntVec3 cell, Map map, TerrainDef skip = null)
	{
		if (!cell.InBounds(map))
		{
			return false;
		}
		if (cell.GetEdifice(map) != null)
		{
			return false;
		}
		TerrainDef terrain = cell.GetTerrain(map);
		if (!terrain.canEverTerraform)
		{
			return false;
		}
		if (skip != null && terrain == skip)
		{
			return false;
		}
		if (terrain.passability == Traversability.Impassable)
		{
			return false;
		}
		if (!terrain.affordances.Contains(TerrainAffordanceDefOf.Light))
		{
			return false;
		}
		if (terrain.isFoundation)
		{
			return false;
		}
		return true;
	}

	private void Convert()
	{
		if (TryGetCellToCovert(out var cell))
		{
			parent.Map.terrainGrid.SetTerrain(cell, Props.convertTerrainDef);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Convert",
				action = Convert,
				Disabled = !Active
			};
		}
	}

	public override void CompTickRare()
	{
		if (GenTicks.TicksGame >= convertTick && Active)
		{
			convertTick = GenTicks.TicksGame + Props.secondsPerConvert.SecondsToTicks();
			Convert();
		}
	}

	public override void PostDrawExtraSelectionOverlays()
	{
		GenDraw.DrawRadiusRing(parent.Position, Props.radius);
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref convertTick, "convertTick", 0);
	}
}
