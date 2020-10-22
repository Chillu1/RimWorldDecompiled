using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Zone_Growing : Zone, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow = ThingDefOf.Plant_Potato;

		public bool allowSow = true;

		public override bool IsMultiselectable => true;

		protected override Color NextZoneColor => ZoneColorUtility.NextGrowingZoneColor();

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells => base.Cells;

		public Zone_Growing()
		{
		}

		public Zone_Growing(ZoneManager zoneManager)
			: base("GrowingZone".Translate(), zoneManager)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
			Scribe_Values.Look(ref allowSow, "allowSow", defaultValue: true);
		}

		public override string GetInspectString()
		{
			string text = "";
			if (!base.Cells.NullOrEmpty())
			{
				IntVec3 c = base.Cells.First();
				if (c.UsesOutdoorTemperature(base.Map))
				{
					text += "OutdoorGrowingPeriod".Translate() + ": " + GrowingQuadrumsDescription(base.Map.Tile) + "\n";
				}
				text = ((!PlantUtility.GrowthSeasonNow(c, base.Map, forSowing: true)) ? ((string)(text + "CannotGrowBadSeasonTemperature".Translate())) : ((string)(text + "GrowSeasonHereNow".Translate())));
			}
			return text;
		}

		public static string GrowingQuadrumsDescription(int tile)
		{
			List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, 10f, 42f);
			if (list.NullOrEmpty())
			{
				return "NoGrowingPeriod".Translate();
			}
			if (list.Count == 12)
			{
				return "GrowYearRound".Translate();
			}
			return "PeriodDays".Translate(list.Count * 5 + "/" + 60) + " (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")";
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.defaultLabel = "CommandAllowSow".Translate();
			command_Toggle.defaultDesc = "CommandAllowSowDesc".Translate();
			command_Toggle.hotKey = KeyBindingDefOf.Command_ItemForbid;
			command_Toggle.icon = TexCommand.ForbidOff;
			command_Toggle.isActive = () => allowSow;
			command_Toggle.toggleAction = delegate
			{
				allowSow = !allowSow;
			};
			yield return command_Toggle;
		}

		public override IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Growing_Expand>();
		}

		public ThingDef GetPlantDefToGrow()
		{
			return plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			return true;
		}
	}
}
