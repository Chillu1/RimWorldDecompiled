using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class FactionGenerator
{
	private const int MaxPreferredFactionNameLength = 20;

	public static IEnumerable<FactionDef> ConfigurableFactions
	{
		get
		{
			foreach (FactionDef item in from f in DefDatabase<FactionDef>.AllDefs
				where f.maxConfigurableAtWorldCreation > 0
				orderby f.configurationListOrderPriority
				select f)
			{
				yield return item;
			}
		}
	}

	public static void GenerateFactionsIntoWorldLayer(PlanetLayer layer, List<FactionDef> factions = null)
	{
		InitializeFactions(layer, factions);
		IEnumerable<Faction> source = Find.World.factionManager.AllFactionsListForReading.Where(Validator);
		if (source.Any())
		{
			float num = layer.Def.viewAngleSettlementsFactorCurve.Evaluate(Mathf.Clamp01(layer.ViewAngle / 180f));
			float randomInRange = layer.Def.settlementsPer100kTiles.RandomInRange;
			float scaleFactor = Find.World.info.overallPopulation.GetScaleFactor();
			int num2 = GenMath.RoundRandom((float)layer.TilesCount / 100000f * randomInRange * scaleFactor * num);
			num2 -= Find.WorldObjects.AllSettlementsOnLayer(layer).Count;
			for (int i = 0; i < num2; i++)
			{
				Faction faction = source.RandomElementByWeight((Faction x) => x.def.settlementGenerationWeight);
				WorldObject worldObject = WorldObjectMaker.MakeWorldObject(layer.Def.SettlementWorldObjectDef);
				worldObject.SetFaction(faction);
				worldObject.Tile = TileFinder.RandomSettlementTileFor(layer, faction);
				if (worldObject is INameableWorldObject nameableWorldObject)
				{
					nameableWorldObject.Name = SettlementNameGenerator.GenerateSettlementName(worldObject);
				}
				Find.WorldObjects.Add(worldObject);
			}
		}
		Find.IdeoManager.SortIdeos();
		bool Validator(Faction x)
		{
			if (!x.def.isPlayer && !x.Hidden && !x.temporary)
			{
				return CanExistOnLayer(layer, x.def);
			}
			return false;
		}
	}

	private static void InitializeFactions(PlanetLayer layer, List<FactionDef> factions)
	{
		if (factions != null)
		{
			foreach (FactionDef faction in factions)
			{
				if (CanExistOnLayer(layer, faction))
				{
					AddFactionToManager(layer, faction);
				}
			}
			return;
		}
		IOrderedEnumerable<FactionDef> orderedEnumerable = DefDatabase<FactionDef>.AllDefs.OrderBy((FactionDef x) => x.hidden);
		foreach (FactionDef facDef in orderedEnumerable)
		{
			if (!orderedEnumerable.Any((FactionDef x) => x.requiredCountAtGameStart > 0 && x.replacesFaction == facDef) && CanExistOnLayer(layer, facDef))
			{
				for (int num = 0; num < facDef.requiredCountAtGameStart; num++)
				{
					AddFactionToManager(layer, facDef);
				}
			}
		}
	}

	private static bool CanExistOnLayer(PlanetLayer layer, FactionDef f)
	{
		if (!f.layerBlacklist.NullOrEmpty() && f.layerBlacklist.Contains(layer.Def))
		{
			return false;
		}
		if (!f.layerWhitelist.NullOrEmpty() || !layer.IsRootSurface)
		{
			return f.layerWhitelist.Contains(layer.Def);
		}
		return true;
	}

	private static void AddFactionToManager(PlanetLayer layer, FactionDef facDef)
	{
		CreateFactionAndAddToManager(layer, facDef);
	}

	public static void CreateFactionAndAddToManager(FactionDef facDef)
	{
		CreateFactionAndAddToManager(Find.WorldGrid.Surface, facDef);
	}

	public static void CreateFactionAndAddToManager(PlanetLayer layer, FactionDef facDef)
	{
		if (facDef.fixedIdeo)
		{
			IdeoGenerationParms ideoGenerationParms = new IdeoGenerationParms(facDef, forceNoExpansionIdeo: false, null, null, name: facDef.ideoName, styles: facDef.styles, deities: facDef.deityPresets, hidden: facDef.hiddenIdeo, description: facDef.ideoDescription, forcedMemes: facDef.forcedMemes, classicExtra: false, forceNoWeaponPreference: false, forNewFluidIdeo: false, fixedIdeo: true, requiredPreceptsOnly: facDef.requiredPreceptsOnly);
			Find.FactionManager.Add(NewGeneratedFaction(layer, new FactionGeneratorParms(facDef, ideoGenerationParms)));
		}
		else
		{
			IdeoGenerationParms ideoGenerationParms2 = new IdeoGenerationParms(facDef, forceNoExpansionIdeo: false, null, null, name: facDef.ideoName, styles: facDef.styles, deities: facDef.deityPresets, hidden: facDef.hiddenIdeo, description: facDef.ideoDescription, forcedMemes: facDef.forcedMemes, classicExtra: false, forceNoWeaponPreference: false, forNewFluidIdeo: false, fixedIdeo: false, requiredPreceptsOnly: facDef.requiredPreceptsOnly);
			Find.FactionManager.Add(NewGeneratedFaction(layer, new FactionGeneratorParms(facDef, ideoGenerationParms2)));
		}
	}

	public static Faction NewGeneratedFaction(FactionGeneratorParms parms)
	{
		return NewGeneratedFaction(Find.WorldGrid.Surface, parms);
	}

	public static Faction NewGeneratedFaction(PlanetLayer layer, FactionGeneratorParms parms)
	{
		FactionDef factionDef = parms.factionDef;
		parms.ideoGenerationParms.forFaction = factionDef;
		Faction faction = new Faction();
		faction.def = factionDef;
		faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
		faction.colorFromSpectrum = NewRandomColorFromSpectrum(faction);
		faction.hidden = parms.hidden;
		if (factionDef.humanlikeFaction)
		{
			faction.ideos = new FactionIdeosTracker(faction);
			if (!faction.IsPlayer || !ModsConfig.IdeologyActive || !Find.GameInitData.startedFromEntry)
			{
				faction.ideos.ChooseOrGenerateIdeo(parms.ideoGenerationParms);
			}
		}
		if (!factionDef.isPlayer)
		{
			if (factionDef.fixedName != null)
			{
				faction.Name = factionDef.fixedName;
			}
			else
			{
				string text = "";
				for (int i = 0; i < 10; i++)
				{
					string text2 = NameGenerator.GenerateName(faction.def.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
					if (text2.Length <= 20)
					{
						text = text2;
					}
				}
				if (text.NullOrEmpty())
				{
					text = NameGenerator.GenerateName(faction.def.factionNameMaker, Find.FactionManager.AllFactionsVisible.Select((Faction fac) => fac.Name));
				}
				faction.Name = text;
			}
		}
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			faction.TryMakeInitialRelationsWith(item);
		}
		if (!faction.Hidden && !factionDef.isPlayer)
		{
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(layer.Def.SettlementWorldObjectDef);
			worldObject.SetFaction(faction);
			worldObject.Tile = TileFinder.RandomSettlementTileFor(layer, faction);
			if (worldObject is INameableWorldObject nameableWorldObject)
			{
				nameableWorldObject.Name = SettlementNameGenerator.GenerateSettlementName(worldObject);
			}
			Find.WorldObjects.Add(worldObject);
		}
		faction.TryGenerateNewLeader();
		return faction;
	}

	public static Faction NewGeneratedFactionWithRelations(FactionDef facDef, List<FactionRelation> relations, bool hidden = false)
	{
		SurfaceLayer surface = Find.WorldGrid.Surface;
		bool? hidden2 = hidden;
		return NewGeneratedFactionWithRelations(surface, new FactionGeneratorParms(facDef, default(IdeoGenerationParms), hidden2), relations);
	}

	public static Faction NewGeneratedFactionWithRelations(PlanetLayer layer, FactionDef facDef, List<FactionRelation> relations, bool hidden = false)
	{
		bool? hidden2 = hidden;
		return NewGeneratedFactionWithRelations(layer, new FactionGeneratorParms(facDef, default(IdeoGenerationParms), hidden2), relations);
	}

	public static Faction NewGeneratedFactionWithRelations(FactionGeneratorParms parms, List<FactionRelation> relations)
	{
		return NewGeneratedFactionWithRelations(Find.WorldGrid.Surface, parms, relations);
	}

	public static Faction NewGeneratedFactionWithRelations(PlanetLayer layer, FactionGeneratorParms parms, List<FactionRelation> relations)
	{
		Faction faction = NewGeneratedFaction(layer, parms);
		for (int i = 0; i < relations.Count; i++)
		{
			faction.SetRelation(relations[i]);
		}
		return faction;
	}

	public static float NewRandomColorFromSpectrum(Faction faction)
	{
		float num = -1f;
		float result = 0f;
		for (int i = 0; i < 20; i++)
		{
			float value = Rand.Value;
			float num2 = 1f;
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int j = 0; j < allFactionsListForReading.Count; j++)
			{
				Faction faction2 = allFactionsListForReading[j];
				if (faction2.def == faction.def)
				{
					float num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
					if (num3 < num2)
					{
						num2 = num3;
					}
				}
			}
			if (num2 > num)
			{
				num = num2;
				result = value;
			}
		}
		return result;
	}
}
