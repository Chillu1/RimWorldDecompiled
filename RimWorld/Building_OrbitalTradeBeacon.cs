using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_OrbitalTradeBeacon : Building
	{
		private const float TradeRadius = 7.9f;

		private static List<IntVec3> tradeableCells = new List<IntVec3>();

		public IEnumerable<IntVec3> TradeableCells => TradeableCellsAround(base.Position, base.Map);

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = MakeMatchingStockpile;
				command_Action.hotKey = KeyBindingDefOf.Misc1;
				command_Action.defaultDesc = "CommandMakeBeaconStockpileDesc".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile");
				command_Action.defaultLabel = "CommandMakeBeaconStockpileLabel".Translate();
				yield return command_Action;
			}
		}

		private void MakeMatchingStockpile()
		{
			Designator des = DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>();
			des.DesignateMultiCell(TradeableCells.Where((IntVec3 c) => des.CanDesignateCell(c).Accepted));
		}

		public static List<IntVec3> TradeableCellsAround(IntVec3 pos, Map map)
		{
			tradeableCells.Clear();
			if (!pos.InBounds(map))
			{
				return tradeableCells;
			}
			Region region = pos.GetRegion(map);
			if (region == null)
			{
				return tradeableCells;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null, delegate(Region r)
			{
				foreach (IntVec3 cell in r.Cells)
				{
					if (cell.InHorDistOf(pos, 7.9f))
					{
						tradeableCells.Add(cell);
					}
				}
				return false;
			}, 16);
			return tradeableCells;
		}

		public static IEnumerable<Building_OrbitalTradeBeacon> AllPowered(Map map)
		{
			foreach (Building_OrbitalTradeBeacon item in map.listerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>())
			{
				CompPowerTrader comp = item.GetComp<CompPowerTrader>();
				if (comp == null || comp.PowerOn)
				{
					yield return item;
				}
			}
		}
	}
}
