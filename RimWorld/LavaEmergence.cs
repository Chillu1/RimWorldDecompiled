using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class LavaEmergence : Thing
	{
		private static readonly IntRange EmergencePointsCountRange = new IntRange(2, 4);

		private static readonly IntRange LavaBeginCoolingDelay = new IntRange(300000, 360000);

		private const int LavaCoolInterval = 400;

		private const int ExpandIntervalTicks = 480;

		[Unsaved(false)]
		public int forcePoolSize = -1;

		[Unsaved(false)]
		public int forceCoolDelay = -1;

		protected HashSet<IntVec3> openCells = new HashSet<IntVec3>();

		protected HashSet<IntVec3> lavaCells = new HashSet<IntVec3>();

		protected int poolSize;

		private int coolDelay;

		protected virtual IntRange PoolSizeRange => new IntRange(450, 600);

		protected virtual bool FireLetter => true;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref openCells, "openCells", LookMode.Value);
			Scribe_Collections.Look(ref lavaCells, "lavaCells", LookMode.Value);
			Scribe_Values.Look(ref poolSize, "poolSize", 0);
			Scribe_Values.Look(ref coolDelay, "coolDelay", 0);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (respawningAfterLoad)
			{
				return;
			}
			openCells.AddRange(GetInitialCells(map));
			if (openCells.Count == 0)
			{
				Destroy();
				return;
			}
			poolSize = ((forcePoolSize > 0) ? forcePoolSize : PoolSizeRange.RandomInRange);
			coolDelay = ((forceCoolDelay > 0) ? forceCoolDelay : LavaBeginCoolingDelay.RandomInRange);
			if (FireLetter)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelLavaEmergence".Translate(), "LetterLavaEmergence".Translate(), LetterDefOf.ThreatSmall, new LookTargets(openCells.First(), base.Map));
			}
		}

		protected virtual IEnumerable<IntVec3> GetInitialCells(Map map)
		{
			if (!CellFinder.TryFindRandomCell(map, (IntVec3 c2) => c2.GetTerrain(map) == TerrainDefOf.LavaDeep && !c2.Fogged(map), out var result))
			{
				yield break;
			}
			HashSet<IntVec3> lavaDeepCells = new HashSet<IntVec3>();
			map.floodFiller.FloodFill(result, (IntVec3 c2) => c2.GetTerrain(map) == TerrainDefOf.LavaDeep, delegate(IntVec3 item)
			{
				lavaDeepCells.Add(item);
			});
			foreach (IntVec3 item in lavaDeepCells.ToList())
			{
				bool flag = false;
				IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
				foreach (IntVec3 intVec in cardinalDirections)
				{
					IntVec3 c = item + intVec;
					if (c.InBounds(map) && c.GetEdifice(map) == null && c.GetTerrain(map) != TerrainDefOf.LavaDeep)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					lavaDeepCells.Remove(item);
				}
			}
			if (lavaDeepCells.Count == 0)
			{
				yield break;
			}
			int count = EmergencePointsCountRange.RandomInRange;
			for (int i = 0; i < count; i++)
			{
				IntVec3 intVec2 = lavaDeepCells.RandomElement();
				lavaDeepCells.Remove(intVec2);
				IntVec3 intVec3 = intVec2;
				IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
				foreach (IntVec3 intVec4 in cardinalDirections)
				{
					IntVec3 intVec5 = intVec2 + intVec4;
					if (intVec5.InBounds(map) && intVec5.GetTerrain(map) != TerrainDefOf.LavaDeep)
					{
						intVec3 = intVec5;
						break;
					}
				}
				yield return intVec3;
			}
		}

		protected override void Tick()
		{
			if (this.IsHashIntervalTick(480))
			{
				SpreadLava();
				if (lavaCells.Count >= poolSize || openCells.Count == 0)
				{
					BeginCooling();
				}
			}
		}

		protected void SpreadLava()
		{
			if (openCells.Count == 0)
			{
				return;
			}
			if (!openCells.TryRandomElementByWeight((IntVec3 c) => Mathf.Max(AdjacentLavaCells(c), 1) * 4, out var result))
			{
				result = openCells.RandomElement();
			}
			openCells.Remove(result);
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				IntVec3 intVec2 = result + intVec;
				if (!openCells.Contains(intVec2) && !lavaCells.Contains(intVec2) && CanLavaSpreadInto(intVec2))
				{
					openCells.Add(intVec2);
				}
			}
			base.Map.terrainGrid.SetTempTerrain(result, TerrainDefOf.LavaShallow);
			lavaCells.Add(result);
		}

		protected void BeginCooling()
		{
			List<IntVec3> cells = lavaCells.ToList();
			int num = 0;
			while (!cells.Empty())
			{
				if (!cells.TryRandomElementByWeight(delegate(IntVec3 c)
				{
					int num2 = 0;
					IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
					foreach (IntVec3 intVec in cardinalDirections)
					{
						IntVec3 intVec2 = c + intVec;
						if (intVec2.InBounds(base.Map) && (cells.Contains(intVec2) || intVec2.GetTerrain(base.Map) == TerrainDefOf.LavaDeep))
						{
							num2++;
						}
					}
					return 16 - num2 * 4;
				}, out var result))
				{
					result = cells.RandomElement();
				}
				base.Map.tempTerrain.QueueRemoveTerrain(result, Find.TickManager.TicksGame + coolDelay + 400 * num);
				cells.Remove(result);
				num++;
			}
			Destroy();
		}

		private bool CanLavaSpreadInto(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && !edifice.IsClearableFreeBuilding)
			{
				return false;
			}
			TerrainDef terrain = c.GetTerrain(base.Map);
			if (!terrain.natural)
			{
				return false;
			}
			if (base.Map.terrainGrid.FoundationAt(c) != null)
			{
				return false;
			}
			if (terrain == TerrainDefOf.LavaDeep)
			{
				return false;
			}
			return true;
		}

		private int AdjacentLavaCells(IntVec3 c)
		{
			int num = 0;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				IntVec3 c2 = c + intVec;
				if (c2.InBounds(base.Map) && c2.GetTerrain(base.Map) == TerrainDefOf.LavaShallow)
				{
					num++;
				}
			}
			return num;
		}
	}
}
