using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_UndercaveInterest : GenStep
{
	private enum UnderCaveInterestKind
	{
		MushroomPatch,
		ChemfuelGenerator,
		InsectHive,
		CorpseGear,
		CorpsePile,
		SleepingFleshbeasts
	}

	private static readonly IntRange InterestPointCountRange = new IntRange(3, 5);

	private const int InterestPointSize = 20;

	private const float MinDistApart = 10f;

	private const float PatchDensity = 0.7f;

	private static readonly IntRange PatchSizeRange = new IntRange(50, 70);

	private static readonly IntRange ChemfuelCountRange = new IntRange(3, 5);

	private static readonly IntRange ChemfuelStackCountRange = new IntRange(10, 20);

	private const int ChemfuelSpawnRadius = 2;

	private const int ChemfuelPuddleSize = 20;

	private static readonly IntRange JellyCountRange = new IntRange(2, 3);

	private static readonly IntRange JellyStackCountRange = new IntRange(15, 40);

	private const int JellySpawnRadius = 3;

	private static readonly IntRange CorpseAgeRangeDays = new IntRange(15, 120);

	private const int GearSpawnRadius = 1;

	private static readonly IntRange GearStackCountRange = new IntRange(2, 5);

	private static readonly IntRange CorpseCountRange = new IntRange(3, 6);

	private const int CorpseSpawnRadius = 4;

	private static readonly IntRange NumFleshbeastsRange = new IntRange(2, 4);

	private const int SleepingFleshbeastSpawnRadius = 4;

	public override int SeedPart => 26098423;

	public override void Generate(Map map, GenStepParams parms)
	{
		Thing pitGateExit = map.listerThings.ThingsOfDef(ThingDefOf.CaveExit).FirstOrDefault();
		Pawn dreadmeld = map.mapPawns.AllPawnsSpawned.FirstOrDefault((Pawn p) => p.kindDef == PawnKindDefOf.Dreadmeld);
		int randomInRange = InterestPointCountRange.RandomInRange;
		List<IntVec3> interestPoints = new List<IntVec3>();
		for (int num = 0; num < randomInRange; num++)
		{
			if (CellFinder.TryFindRandomCell(map, delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					return false;
				}
				if (c.DistanceToEdge(map) <= 5)
				{
					return false;
				}
				if (pitGateExit != null && c.InHorDistOf(pitGateExit.Position, 10f))
				{
					return false;
				}
				if (dreadmeld != null && c.InHorDistOf(dreadmeld.Position, 10f))
				{
					return false;
				}
				return !interestPoints.Any((IntVec3 p) => c.InHorDistOf(p, 10f));
			}, out var result))
			{
				interestPoints.Add(result);
			}
		}
		foreach (IntVec3 item in interestPoints)
		{
			UnderCaveInterestKind underCaveInterestKind = Gen.RandomEnumValue<UnderCaveInterestKind>(disallowFirstValue: false);
			foreach (IntVec3 item2 in GridShapeMaker.IrregularLump(item, map, 20))
			{
				foreach (Thing item3 in item2.GetThingList(map).ToList())
				{
					if (item3.def.destroyable && (item2.GetEdifice(map)?.def?.building?.isNaturalRock == true || item2.GetEdifice(map)?.def == ThingDefOf.Fleshmass))
					{
						item3.Destroy();
					}
				}
			}
			switch (underCaveInterestKind)
			{
			case UnderCaveInterestKind.MushroomPatch:
				GenerateMushroomPatch(map, item);
				break;
			case UnderCaveInterestKind.ChemfuelGenerator:
				GenerateChemfuel(map, item);
				break;
			case UnderCaveInterestKind.InsectHive:
				GenerateHive(map, item);
				break;
			case UnderCaveInterestKind.CorpseGear:
				GenerateCorpseGear(map, item);
				break;
			case UnderCaveInterestKind.CorpsePile:
				GenerateCorpsePile(map, item);
				break;
			case UnderCaveInterestKind.SleepingFleshbeasts:
				GenerateSleepingFleshbeasts(map, item);
				break;
			}
		}
	}

	private void GenerateMushroomPatch(Map map, IntVec3 cell)
	{
		List<ThingDef> source = new List<ThingDef>
		{
			ThingDefOf.Plant_HealrootWild,
			ThingDefOf.Glowstool,
			ThingDefOf.Bryolux,
			ThingDefOf.Agarilux
		};
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(cell, map, PatchSizeRange.RandomInRange))
		{
			ThingDef thingDef = source.RandomElement();
			if (GenSpawn.CanSpawnAt(thingDef, item, map))
			{
				map.terrainGrid.SetTerrain(item, TerrainDefOf.SoilRich);
				Thing thing = ThingMaker.MakeThing(thingDef);
				if (Rand.Chance(0.7f) && GenPlace.TryPlaceThing(thing, item, map, ThingPlaceMode.Direct) && thing is Plant plant)
				{
					plant.Growth = Mathf.Clamp01(WildPlantSpawner.InitialGrowthRandomRange.RandomInRange);
				}
			}
		}
	}

	private void GenerateChemfuel(Map map, IntVec3 cell)
	{
		if (!GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.AncientGenerator ?? ThingDefOf.ChemfuelPoweredGenerator), cell, map, ThingPlaceMode.Direct))
		{
			return;
		}
		int randomInRange = ChemfuelCountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Chemfuel);
			thing.stackCount = ChemfuelStackCountRange.RandomInRange;
			GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Radius, null, null, null, 2);
		}
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(cell, map, 20))
		{
			if (item.GetEdifice(map) == null)
			{
				FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Fuel);
			}
		}
	}

	private void GenerateHive(Map map, IntVec3 cell)
	{
		HiveUtility.SpawnHive(cell, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: false);
		int randomInRange = JellyCountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.InsectJelly);
			thing.stackCount = JellyStackCountRange.RandomInRange;
			thing.SetForbidden(value: true);
			GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Radius, null, null, null, 3);
		}
	}

	private void GenerateCorpseGear(Map map, IntVec3 cell)
	{
		List<ThingDef> source = new List<ThingDef>
		{
			ThingDefOf.MedicineIndustrial,
			ThingDefOf.MealSurvivalPack
		};
		Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: true);
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Drifter, faction));
		pawn.health.SetDead();
		Corpse corpse = pawn.MakeCorpse(null, null);
		corpse.Age = Mathf.RoundToInt(CorpseAgeRangeDays.RandomInRange * 60000);
		corpse.GetComp<CompRottable>().RotProgress += corpse.Age;
		Find.WorldPawns.PassToWorld(pawn);
		GenSpawn.Spawn(corpse, cell, map);
		Thing thing = ThingMaker.MakeThing(source.RandomElement());
		thing.stackCount = GearStackCountRange.RandomInRange;
		GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Radius);
	}

	public static void GenerateCorpsePile(Map map, IntVec3 cell)
	{
		int randomInRange = CorpseCountRange.RandomInRange;
		Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: true);
		int age = Mathf.RoundToInt(CorpseAgeRangeDays.RandomInRange * 60000);
		for (int i = 0; i < randomInRange; i++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Drifter, faction));
			pawn.Kill(null, null);
			pawn.Corpse.Age = age;
			pawn.Corpse.GetComp<CompRottable>().RotProgress += pawn.Corpse.Age;
			GenPlace.TryPlaceThing(pawn.Corpse, cell, map, ThingPlaceMode.Radius, null, null, null, 4);
			pawn.Corpse.SetForbidden(value: true);
		}
	}

	private void GenerateSleepingFleshbeasts(Map map, IntVec3 cell)
	{
		int randomInRange = NumFleshbeastsRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Pawn thing = PawnGenerator.GeneratePawn(PawnKindDefOf.Fingerspike, Faction.OfEntities);
			GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Radius, null, null, null, 4);
			if (thing.TryGetComp<CompCanBeDormant>(out var comp))
			{
				comp.ToSleep();
			}
		}
	}
}
