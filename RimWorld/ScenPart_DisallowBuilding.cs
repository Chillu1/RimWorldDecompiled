using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class ScenPart_DisallowBuilding : ScenPart_Rule
{
	private ThingDef building;

	private const string DisallowBuildingTag = "DisallowBuilding";

	protected override void ApplyRule()
	{
		Current.Game.Rules.SetAllowBuilding(building, allowed: false);
	}

	public override string Summary(Scenario scen)
	{
		return ScenSummaryList.SummaryWithList(scen, "DisallowBuilding", "ScenPart_DisallowBuilding".Translate());
	}

	public override IEnumerable<string> GetSummaryListEntries(string tag)
	{
		if (tag == "DisallowBuilding")
		{
			yield return building.LabelCap;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref building, "building");
	}

	public override void Randomize()
	{
		building = RandomizableBuildingDefs().RandomElement();
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), building.LabelCap))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (ThingDef item in from t in PossibleBuildingDefs()
			orderby t.label
			select t)
		{
			ThingDef localTd = item;
			list.Add(new FloatMenuOption(localTd.LabelCap, delegate
			{
				building = localTd;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public override bool TryMerge(ScenPart other)
	{
		if (other is ScenPart_DisallowBuilding scenPart_DisallowBuilding && scenPart_DisallowBuilding.building == building)
		{
			return true;
		}
		return false;
	}

	protected virtual IEnumerable<ThingDef> PossibleBuildingDefs()
	{
		return DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Building && d.BuildableByPlayer);
	}

	private IEnumerable<ThingDef> RandomizableBuildingDefs()
	{
		yield return ThingDefOf.Wall;
		yield return ThingDefOf.Turret_MiniTurret;
		yield return ThingDefOf.OrbitalTradeBeacon;
		yield return ThingDefOf.Battery;
		yield return ThingDefOf.TrapSpike;
		yield return ThingDefOf.Cooler;
		yield return ThingDefOf.Heater;
	}

	public override bool HasNullDefs()
	{
		if (!base.HasNullDefs())
		{
			return building == null;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ ((building != null) ? building.GetHashCode() : 0);
	}
}
