using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class LavaFXComponent : MapComponent
{
	private CellFXSpawner deepLavaSpawner;

	private CellFXSpawner shallowLavaSpawner;

	private const float DeepLavaFXPerTilePerSecond = 0.015f;

	private const float ShallowLavaFXPerTilePerSecond = 0.015f;

	public LavaFXComponent(Map map)
		: base(map)
	{
		map.events.TerrainChanged += Notify_OnTerrainChanged;
		deepLavaSpawner = new CellFXSpawner(0.015f, EmitDeepLavaFX);
		shallowLavaSpawner = new CellFXSpawner(0.015f, EmitShallowLavaFX);
	}

	private void Notify_OnTerrainChanged(IntVec3 cell)
	{
		if (!TryAddLavaCell(cell))
		{
			deepLavaSpawner.Cells.Remove(cell);
			shallowLavaSpawner.Cells.Remove(cell);
		}
	}

	public override void FinalizeInit()
	{
		if (ModsConfig.OdysseyActive)
		{
			RefreshLavaCells();
		}
	}

	public override void MapComponentTick()
	{
		if (ModsConfig.OdysseyActive)
		{
			deepLavaSpawner.Tick();
			shallowLavaSpawner.Tick();
		}
	}

	public void EmitDeepLavaFX(Vector3 pos)
	{
		FleckMaker.ThrowMicroSparks(pos, map);
		ThrowLavaSmoke(pos, map, 1.5f);
	}

	public void EmitShallowLavaFX(Vector3 pos)
	{
		FleckMaker.ThrowMicroSparks(pos, map);
		FleckMaker.ThrowSmoke(pos, map, 1.5f);
	}

	private void RefreshLavaCells()
	{
		deepLavaSpawner.Cells.Clear();
		shallowLavaSpawner.Cells.Clear();
		for (int i = 0; i < map.Size.x; i++)
		{
			for (int j = 0; j < map.Size.z; j++)
			{
				TryAddLavaCell(new IntVec3(i, 0, j));
			}
		}
	}

	private bool TryAddLavaCell(IntVec3 cell)
	{
		if (map.terrainGrid.TerrainAt(cell) == TerrainDefOf.LavaDeep)
		{
			deepLavaSpawner.Cells.Add(cell);
			return true;
		}
		if (map.terrainGrid.TempTerrainAt(cell) == TerrainDefOf.LavaShallow)
		{
			shallowLavaSpawner.Cells.Add(cell);
			return true;
		}
		return false;
	}

	private void ThrowLavaSmoke(Vector3 loc, Map map, float size)
	{
		if (loc.ShouldSpawnMotesAt(map))
		{
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.LavaSmoke, Rand.Range(1.5f, 2.5f) * size);
			dataStatic.rotationRate = Rand.Range(-30f, 30f);
			dataStatic.velocityAngle = Rand.Range(30, 40);
			dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
			map.flecks.CreateFleck(dataStatic);
		}
	}
}
