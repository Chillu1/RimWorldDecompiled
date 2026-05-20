using System.Linq;
using LudeonTK;
using RimWorld;

namespace Verse;

public static class DebugOutputsTerrain
{
	[DebugOutput]
	public static void Terrains()
	{
		DebugTables.MakeTablesDialog(DefDatabase<TerrainDef>.AllDefs, new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName), new TableDataGetter<TerrainDef>("work", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild).ToString()), new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty).ToString()), new TableDataGetter<TerrainDef>("beauty outdoors", (TerrainDef d) => (!d.StatBaseDefined(StatDefOf.BeautyOutdoors)) ? "" : d.GetStatValueAbstract(StatDefOf.BeautyOutdoors).ToString()), new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness).ToString()), new TableDataGetter<TerrainDef>("flammability", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Flammability).ToString()), new TableDataGetter<TerrainDef>("path\ncost", (TerrainDef d) => d.pathCost.ToString()), new TableDataGetter<TerrainDef>("fertility", (TerrainDef d) => d.fertility.ToStringPercentEmptyZero()), new TableDataGetter<TerrainDef>("acceptance\nmask", (TerrainDef d) => string.Join(",", (from e in d.filthAcceptanceMask.GetAllSelectedItems<FilthSourceFlags>()
			select e.ToString()).ToArray())), new TableDataGetter<TerrainDef>("generated\nfilth", (TerrainDef d) => (d.generatedFilth == null) ? "" : d.generatedFilth.defName), new TableDataGetter<TerrainDef>("hold\nsnow", (TerrainDef d) => d.holdSnowOrSand.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("take\nfootprints", (TerrainDef d) => d.takeFootprints.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("avoid\nwander", (TerrainDef d) => d.avoidWander.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("buildable", (TerrainDef d) => d.BuildableByPlayer.ToStringCheckBlank()), new TableDataGetter<TerrainDef>("cost\nlist", (TerrainDef d) => DebugOutputsEconomy.CostListString(d, divideByVolume: false, starIfOnlyBuyable: false)), new TableDataGetter<TerrainDef>("research", (TerrainDef d) => (d.researchPrerequisites == null) ? "" : d.researchPrerequisites.Select((ResearchProjectDef pr) => pr.defName).ToCommaList()), new TableDataGetter<TerrainDef>("affordances", (TerrainDef d) => d.affordances.Select((TerrainAffordanceDef af) => af.defName).ToCommaList()));
	}

	[DebugOutput]
	public static void TerrainAffordances()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Building && !d.IsFrame).Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>()), new TableDataGetter<BuildableDef>("type", (BuildableDef d) => (!(d is TerrainDef)) ? "building" : "terrain"), new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("terrain\naffordance\nneeded", (BuildableDef d) => (d.terrainAffordanceNeeded == null) ? "" : d.terrainAffordanceNeeded.defName));
	}
}
