using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompAnimalPenMarker : CompAutoCut, IRenameable
{
	private const int FoodCalculatorCacheTicks = 20;

	public string label = "";

	private ThingFilter animalFilter = new ThingFilter();

	private List<ThingDef> forceDisplayedAnimalDefs = new List<ThingDef>();

	private PenFoodCalculator cachedFoodCalculator;

	private int? cachedFoodCalculator_cachedAtTick;

	public CompProperties_AnimalPenMarker Props => props as CompProperties_AnimalPenMarker;

	public PenMarkerState PenState => parent?.Map?.animalPenManager.GetPenMarkerState(this);

	public ThingFilter AnimalFilter => animalFilter;

	public List<ThingDef> ForceDisplayedAnimalDefs => forceDisplayedAnimalDefs;

	public override bool CanDesignatePlants
	{
		get
		{
			PenMarkerState penState = PenState;
			if (penState != null)
			{
				return !penState.Unenclosed;
			}
			return false;
		}
	}

	public string RenamableLabel
	{
		get
		{
			return label ?? BaseLabel;
		}
		set
		{
			label = value;
		}
	}

	public string BaseLabel => parent.def.label.CapitalizeFirst();

	public string InspectLabel => RenamableLabel;

	public PenFoodCalculator PenFoodCalculator
	{
		get
		{
			if (cachedFoodCalculator == null)
			{
				cachedFoodCalculator = new PenFoodCalculator();
				cachedFoodCalculator_cachedAtTick = null;
			}
			if (!cachedFoodCalculator_cachedAtTick.HasValue || Find.TickManager.TicksGame > cachedFoodCalculator_cachedAtTick + 20)
			{
				cachedFoodCalculator.ResetAndProcessPen(this);
				cachedFoodCalculator_cachedAtTick = Find.TickManager.TicksGame;
			}
			return cachedFoodCalculator;
		}
	}

	public void AddForceDisplayedAnimal(ThingDef animalDef)
	{
		if (!forceDisplayedAnimalDefs.Contains(animalDef))
		{
			forceDisplayedAnimalDefs.Add(animalDef);
		}
	}

	public void RemoveForceDisplayedAnimal(ThingDef animalDef)
	{
		forceDisplayedAnimalDefs.Remove(animalDef);
	}

	public bool AcceptsToPen(Pawn animal)
	{
		if (animalFilter.Allows(animal))
		{
			return AnimalPenUtility.GetFixedAnimalFilter().Allows(animal);
		}
		return false;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!respawningAfterLoad && !parent.BeingTransportedOnGravship && label.NullOrEmpty())
		{
			label = parent.Map.animalPenManager.MakeNewAnimalPenName() ?? ((string)"AnimalPenMarkerDefaultLabel".Translate(1));
			forceDisplayedAnimalDefs.Add(ThingDefOf.Cow);
			forceDisplayedAnimalDefs.Add(ThingDefOf.Goat);
			forceDisplayedAnimalDefs.Add(ThingDefOf.Chicken);
			base.AutoCutFilter.CopyAllowancesFrom(parent.Map.animalPenManager.GetDefaultAutoCutFilter());
			animalFilter.CopyAllowancesFrom(AnimalPenUtility.GetDefaultAnimalFilter());
		}
	}

	public override IEnumerable<IntVec3> GetAutoCutCells()
	{
		foreach (Region directlyConnectedRegion in PenState.DirectlyConnectedRegions)
		{
			foreach (IntVec3 cell in directlyConnectedRegion.Cells)
			{
				yield return cell;
			}
		}
	}

	public override ThingFilter GetDefaultAutoCutFilter()
	{
		return parent.Map.animalPenManager.GetDefaultAutoCutFilter();
	}

	public override ThingFilter GetFixedAutoCutFilter()
	{
		return parent.Map.animalPenManager.GetDefaultAutoCutFilter();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref label, "label");
		Scribe_Deep.Look(ref animalFilter, "animalFilter");
		Scribe_Collections.Look(ref forceDisplayedAnimalDefs, "forceDisplayedAnimalDefs", LookMode.Undefined);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && forceDisplayedAnimalDefs == null)
		{
			forceDisplayedAnimalDefs = new List<ThingDef>();
		}
	}

	public override string CompInspectStringExtra()
	{
		if (!parent.Spawned)
		{
			return string.Empty;
		}
		PenFoodCalculator penFoodCalculator = PenFoodCalculator;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(label).Append(": ").AppendLine(penFoodCalculator.PenSizeDescription());
		PenMarkerState penState = PenState;
		if (penState.Unenclosed)
		{
			stringBuilder.Append("PenUnenclosedLabel".Translate());
			if (penState.PassableDoors)
			{
				stringBuilder.Append(" (" + "PenOpenDoorLabel".Translate() + ")");
			}
			else
			{
				stringBuilder.Append(".");
			}
			stringBuilder.AppendLine();
		}
		else if (!penState.HasOutsideAccess)
		{
			stringBuilder.Append("NoExteriorAccess".Translate());
			stringBuilder.AppendLine();
		}
		bool num = penFoodCalculator.SumNutritionConsumptionPerDay > penFoodCalculator.NutritionPerDayToday;
		string value = PenFoodCalculator.NutritionPerDayToString(penFoodCalculator.NutritionPerDayToday, withUnits: false);
		string text = PenFoodCalculator.NutritionPerDayToString(penFoodCalculator.SumNutritionConsumptionPerDay, withUnits: false);
		if (num)
		{
			text = text.Colorize(ColorLibrary.RedReadable);
		}
		stringBuilder.Append("PenFoodTab_NaturalNutritionGrowthRate".Translate()).Append(": ").AppendLine(value);
		stringBuilder.Append("PenFoodTab_TotalNutritionConsumptionRate".Translate()).Append(": ").AppendLine(text);
		if (penFoodCalculator.sumStockpiledNutritionAvailableNow > 0f)
		{
			stringBuilder.Append("PenFoodTab_StockpileTotal".Translate()).Append(": ").AppendLine(PenFoodCalculator.NutritionToString(penFoodCalculator.sumStockpiledNutritionAvailableNow, withUnits: false));
		}
		return stringBuilder.ToString().TrimEnd();
	}
}
