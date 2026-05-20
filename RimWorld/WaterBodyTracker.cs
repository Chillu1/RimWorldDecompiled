using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WaterBodyTracker : IExposable
	{
		private readonly Map map;

		private List<WaterBody> bodies;

		public int lastRareCatchTick;

		public int lastNegativeCatchTick;

		[Unsaved(false)]
		private bool anyBodyContainsFish;

		[Unsaved(false)]
		private Dictionary<IntVec3, WaterBody> bodiesByCell = new Dictionary<IntVec3, WaterBody>();

		private const int TickInterval = 2500;

		public List<WaterBody> Bodies => bodies;

		public bool AnyBodyContainsFish => anyBodyContainsFish;

		public WaterBodyTracker(Map map)
		{
			this.map = map;
		}

		public void ConstructBodies()
		{
			if (!ModsConfig.OdysseyActive)
			{
				return;
			}
			bodiesByCell.Clear();
			WaterBody body;
			if (bodies == null)
			{
				bodies = new List<WaterBody>();
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					IntVec3 c = map.cellIndices.IndexToCell(i);
					WaterBodyType waterBodyType = c.GetWaterBodyType(map);
					if (waterBodyType != WaterBodyType.None && !TryGetWaterBodyAt(c, out body))
					{
						TryCreateBodyOrAddToExisting(c, waterBodyType);
					}
				}
				for (int j = 0; j < bodies.Count; j++)
				{
					bodies[j].Initialize();
					if (bodies[j].HasFish)
					{
						anyBodyContainsFish = true;
					}
				}
				return;
			}
			for (int k = 0; k < bodies.Count; k++)
			{
				WaterBody waterBody = bodies[k];
				waterBody.CellCount = 0;
				waterBody.cells.Clear();
				FloodFillBody(waterBody.rootCell, bodies[k]);
				waterBody.RecacheState();
				if (waterBody.HasFish)
				{
					anyBodyContainsFish = true;
				}
			}
			for (int l = 0; l < map.cellIndices.NumGridCells; l++)
			{
				IntVec3 c2 = map.cellIndices.IndexToCell(l);
				WaterBodyType waterBodyType2 = c2.GetWaterBodyType(map);
				if (waterBodyType2 != WaterBodyType.None && !TryGetWaterBodyAt(c2, out body))
				{
					TryCreateBodyOrAddToExisting(c2, waterBodyType2);
				}
			}
		}

		public void Tick()
		{
			if (map.TileInfo.MaxFishPopulation <= 0f || !map.IsHashIntervalTick(2500))
			{
				return;
			}
			GameCondition culprit;
			float num = map.gameConditionManager.FishPopulationOffsetFactorPerDay(map, out culprit);
			for (int i = 0; i < bodies.Count; i++)
			{
				if (bodies[i] != null)
				{
					bodies[i].Population += bodies[i].MaxPopulation * num / 24f;
				}
			}
		}

		public void Notify_Fished(IntVec3 c, float amount)
		{
			if (TryGetWaterBodyAt(c, out var body))
			{
				body.Population -= amount;
			}
		}

		public bool AnyFishPopulationAt(IntVec3 c)
		{
			if (TryGetWaterBodyAt(c, out var body))
			{
				return body.HasFish;
			}
			return false;
		}

		public float FishPopulationAt(IntVec3 c)
		{
			if (!TryGetWaterBodyAt(c, out var body))
			{
				return 0f;
			}
			return body.Population;
		}

		public float MaxPopulationAt(IntVec3 c)
		{
			if (!TryGetWaterBodyAt(c, out var body))
			{
				return 0f;
			}
			return body.MaxPopulation;
		}

		public float PopulationPercentAt(IntVec3 c)
		{
			float num = MaxPopulationAt(c);
			if (num <= 0f)
			{
				return 0f;
			}
			return FishPopulationAt(c) / num;
		}

		public WaterBody WaterBodyAt(IntVec3 c)
		{
			if (!TryGetWaterBodyAt(c, out var body))
			{
				return null;
			}
			return body;
		}

		public bool TryGetWaterBodyAt(IntVec3 c, out WaterBody body)
		{
			if (!c.InBounds(map))
			{
				body = null;
				return false;
			}
			if (bodiesByCell.TryGetValue(c, out body))
			{
				return body != null;
			}
			return false;
		}

		public void Notify_TerrainChanged(IntVec3 cell, TerrainDef oldTerr, TerrainDef newTerr)
		{
			if (Current.ProgramState == ProgramState.Playing && newTerr?.waterBodyType != oldTerr?.waterBodyType)
			{
				WaterBodyType waterBodyType = cell.GetWaterBodyType(map);
				if (waterBodyType != WaterBodyType.None)
				{
					TryCreateBodyOrAddToExisting(cell, waterBodyType);
				}
				else
				{
					RemoveCellFromExistingBody(cell);
				}
				cell.GetWaterBody(map)?.Notify_TerrainChanged(cell, oldTerr, newTerr);
			}
		}

		public void Notify_PollutionChanged(IntVec3 cell, bool isPolluted)
		{
			if (WaterBodyAt(cell) != null)
			{
				cell.GetWaterBody(map)?.Notify_PollutionChanged(cell, isPolluted);
			}
		}

		private void TryCreateBodyOrAddToExisting(IntVec3 c, WaterBodyType waterBodyType)
		{
			if (TryGetWaterBodyAt(c, out var body) && body.waterBodyType == waterBodyType)
			{
				return;
			}
			WaterBody waterBody = null;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c2 = c + GenAdj.CardinalDirections[i];
				if (c2.InBounds(map) && c2.GetWaterBodyType(map) == waterBodyType && TryGetWaterBodyAt(c2, out var body2) && (waterBody == null || body2.HasFish))
				{
					waterBody = body2;
				}
			}
			if (waterBody != null)
			{
				FloodFillBody(c, waterBody);
				return;
			}
			WaterBody waterBody2 = new WaterBody(map, c);
			bodies.Add(waterBody2);
			waterBody2.Initialize();
			bodiesByCell.SetOrAdd(c, waterBody2);
			waterBody2.cells.Add(c);
			if (waterBody2.MaxPopulation > 0f)
			{
				anyBodyContainsFish = true;
			}
		}

		private void RemoveCellFromExistingBody(IntVec3 c)
		{
			if (c.GetZone(map) is Zone_Fishing zone_Fishing)
			{
				zone_Fishing.RemoveCell(c);
			}
			if (c.GetWaterBodyType(map) != WaterBodyType.None || !TryGetWaterBodyAt(c, out var body))
			{
				return;
			}
			bodiesByCell.Remove(c);
			body.cells.Remove(c);
			body.CellCount--;
			if (body.CellCount == 0)
			{
				bodies.Remove(body);
			}
			else if (body.rootCell == c)
			{
				body.rootCell = bodiesByCell.Keys.First((IntVec3 x) => bodiesByCell[x] == body);
			}
		}

		private void FloodFillBody(IntVec3 c, WaterBody body)
		{
			map.floodFiller.FloodFill(c, delegate(IntVec3 x)
			{
				if (TryGetWaterBodyAt(x, out var body2) && body2 == body)
				{
					return false;
				}
				return (x.GetWaterBodyType(map) == body.waterBodyType) ? true : false;
			}, delegate(IntVec3 x)
			{
				if (TryGetWaterBodyAt(x, out var body2) && body2 != body)
				{
					bodies.Remove(body2);
				}
				body.CellCount++;
				bodiesByCell.SetOrAdd(x, body);
				body.cells.Add(x);
			});
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref bodies, "waterBodies", LookMode.Deep);
			Scribe_Values.Look(ref lastRareCatchTick, "lastRareCatchTick", 0);
			Scribe_Values.Look(ref lastNegativeCatchTick, "lastNegativeCatchTick", 0);
			if (Scribe.mode != LoadSaveMode.LoadingVars || bodies == null)
			{
				return;
			}
			foreach (WaterBody body in bodies)
			{
				body.map = map;
			}
		}

		public void DebugDraw()
		{
			if (!DebugViewSettings.drawWaterBodies)
			{
				return;
			}
			foreach (WaterBody body in bodies)
			{
				Rand.PushState(body.GetHashCode());
				Color col = new Color(Rand.Range(0f, 1f), Rand.Range(0f, 1f), Rand.Range(0f, 1f), 0.5f);
				foreach (IntVec3 cell in body.cells)
				{
					CellRenderer.RenderCell(cell, SolidColorMaterials.SimpleSolidColorMaterial(col));
				}
				Rand.PopState();
			}
		}
	}
}
