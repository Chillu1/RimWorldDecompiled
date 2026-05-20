using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Zone_Growing : Zone, IPlantToGrowSettable
{
	private ThingDef plantDefToGrow;

	public bool allowSow = true;

	public bool allowCut = true;

	public override bool IsMultiselectable => true;

	protected override Color NextZoneColor => ZoneColorUtility.NextGrowingZoneColor();

	IEnumerable<IntVec3> IPlantToGrowSettable.Cells => base.Cells;

	public ThingDef PlantDefToGrow
	{
		get
		{
			if (plantDefToGrow != null)
			{
				return plantDefToGrow;
			}
			if (PollutionUtility.SettableEntirelyPolluted(this))
			{
				plantDefToGrow = ThingDefOf.Plant_Toxipotato;
			}
			else
			{
				plantDefToGrow = ThingDefOf.Plant_Potato;
			}
			return plantDefToGrow;
		}
	}

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
		Scribe_Values.Look(ref allowCut, "allowCut", defaultValue: true);
	}

	public void ContentsStatistics(out int totalPlants, out float averagePlantAgeTicks, out int oldestPlantAgeTicks, out float averagePlantGrowth, out float maxPlantGrowth)
	{
		averagePlantAgeTicks = 0f;
		totalPlants = 0;
		oldestPlantAgeTicks = 0;
		averagePlantGrowth = 0f;
		maxPlantGrowth = 0f;
		foreach (IntVec3 cell in base.Cells)
		{
			foreach (Thing thing in cell.GetThingList(base.Map))
			{
				if (thing.def == plantDefToGrow && thing is Plant plant)
				{
					totalPlants++;
					averagePlantAgeTicks += plant.Age;
					oldestPlantAgeTicks = Mathf.Max(oldestPlantAgeTicks, plant.Age);
					averagePlantGrowth += plant.Growth;
					maxPlantGrowth = Mathf.Max(maxPlantGrowth, plant.Growth);
				}
			}
		}
		averagePlantGrowth /= totalPlants;
		averagePlantAgeTicks /= totalPlants;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
		stringBuilder.AppendLine();
		if (!base.Cells.NullOrEmpty())
		{
			ContentsStatistics(out var totalPlants, out var averagePlantAgeTicks, out var oldestPlantAgeTicks, out var averagePlantGrowth, out var maxPlantGrowth);
			if (totalPlants > 0)
			{
				string arg = Mathf.RoundToInt(averagePlantAgeTicks).ToStringTicksToPeriodVerbose();
				string arg2 = oldestPlantAgeTicks.ToStringTicksToPeriodVerbose();
				stringBuilder.AppendLine(string.Format("{0}: {1} {2}", "Contains".Translate().CapitalizeFirst(), totalPlants, Find.ActiveLanguageWorker.Pluralize(plantDefToGrow.label, totalPlants)));
				stringBuilder.AppendLine(string.Format("{0}: {1} ({2})", "AveragePlantAge".Translate().CapitalizeFirst(), arg, "PercentGrowth".Translate(averagePlantGrowth.ToStringPercent())));
				stringBuilder.AppendLine(string.Format("{0}: {1} ({2})", "OldestPlantAge".Translate().CapitalizeFirst(), arg2, "PercentGrowth".Translate(maxPlantGrowth.ToStringPercent())));
			}
			IntVec3 c = base.Cells.First();
			if (c.UsesOutdoorTemperature(base.Map))
			{
				stringBuilder.AppendLine(string.Format("{0}: {1}", "OutdoorGrowingPeriod".Translate(), GrowingQuadrumsDescription(base.Map.Tile, plantDefToGrow)));
			}
			if (PlantUtility.GrowthSeasonNow(c, base.Map, plantDefToGrow))
			{
				stringBuilder.Append("GrowSeasonHereNow".Translate());
			}
			else
			{
				stringBuilder.Append("CannotGrowBadSeasonTemperature".Translate());
			}
		}
		return stringBuilder.ToString();
	}

	public static string GrowingQuadrumsDescription(PlanetTile tile, ThingDef plantDef = null)
	{
		float minTemp = plantDef?.plant.minOptimalGrowthTemperature ?? 6f;
		float maxTemp = plantDef?.plant.maxOptimalGrowthTemperature ?? 42f;
		List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, minTemp, maxTemp);
		if (list.NullOrEmpty())
		{
			return "NoGrowingPeriod".Translate();
		}
		if (list.Count == 12)
		{
			return "GrowYearRound".Translate();
		}
		return "PeriodDays".Translate($"{list.Count * 5}/{60}") + (" (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")");
	}

	public override void AddCell(IntVec3 c)
	{
		base.AddCell(c);
		foreach (Thing item in base.Map.thingGrid.ThingsListAt(c))
		{
			if (!item.TryGetComp(out CompPlantPreventCutting comp) || !comp.PreventCutting)
			{
				Designator_PlantsHarvestWood.PossiblyWarnPlayerImportantPlantDesignateCut(item);
			}
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		yield return new Command_Hide_ZoneGrow(this);
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
		yield return new Command_Toggle
		{
			defaultLabel = "CommandAllowSow".Translate(),
			defaultDesc = "CommandAllowSowDesc".Translate(),
			hotKey = KeyBindingDefOf.Command_ItemForbid,
			icon = TexCommand.ForbidOff,
			isActive = () => allowSow,
			toggleAction = delegate
			{
				allowSow = !allowSow;
			}
		};
		yield return new Command_Toggle
		{
			defaultLabel = "CommandAllowCut".Translate(),
			defaultDesc = "CommandAllowCutDesc".Translate(),
			icon = Designator_PlantsCut.IconTex,
			isActive = () => allowCut,
			toggleAction = delegate
			{
				allowCut = !allowCut;
			}
		};
	}

	public override IEnumerable<Gizmo> GetZoneAddGizmos()
	{
		yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Growing_Expand>();
	}

	public ThingDef GetPlantDefToGrow()
	{
		return PlantDefToGrow;
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
