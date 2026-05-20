using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public static class FleshbeastUtility
{
	public enum MeatExplosionSize
	{
		Small,
		Normal,
		Large
	}

	private const float BloodLossSeverity = 0.2f;

	private const int BloodFilthCount = 3;

	private static List<IntVec3> tmpTakenCells = new List<IntVec3>();

	private const int FleshbeastBirthRange = 30;

	private const float FleshbeastBirthShakeMagnitude = 1f;

	private static readonly SimpleCurve FleshbeastResponsePointsCurve = new SimpleCurve
	{
		new CurvePoint(200f, 150f),
		new CurvePoint(500f, 250f),
		new CurvePoint(1000f, 400f),
		new CurvePoint(5000f, 650f)
	};

	public static IEnumerable<PawnKindDef> AllFleshbeasts
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				yield return PawnKindDefOf.Bulbfreak;
				yield return PawnKindDefOf.Fingerspike;
				yield return PawnKindDefOf.Trispike;
				yield return PawnKindDefOf.Toughspike;
			}
		}
	}

	public static MeatExplosionSize ExplosionSizeFor(Pawn pawn)
	{
		if ((double)pawn.BodySize >= 3.5)
		{
			return MeatExplosionSize.Large;
		}
		return MeatExplosionSize.Normal;
	}

	public static bool TryGiveMutation(Pawn pawn, HediffDef mutationDef)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (mutationDef.defaultInstallPart == null)
		{
			Log.ErrorOnce("Attempted to use mutation hediff which didn't specify a default install part (hediff: " + mutationDef.label, 194783821);
			return false;
		}
		List<BodyPartRecord> list = (from part in pawn.RaceProps.body.GetPartsWithDef(mutationDef.defaultInstallPart)
			where pawn.health.hediffSet.HasMissingPartFor(part)
			select part).ToList();
		List<BodyPartRecord> list2 = (from part in pawn.RaceProps.body.GetPartsWithDef(mutationDef.defaultInstallPart)
			where !pawn.health.hediffSet.HasDirectlyAddedPartFor(part)
			select part).ToList();
		BodyPartRecord bodyPartRecord = null;
		if (list.Any())
		{
			bodyPartRecord = list.RandomElement();
		}
		else if (list2.Any())
		{
			bodyPartRecord = list2.RandomElement();
		}
		if (bodyPartRecord == null)
		{
			return false;
		}
		MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, bodyPartRecord, pawn.PositionHeld, pawn.MapHeld);
		pawn.health.RestorePart(bodyPartRecord);
		pawn.health.AddHediff(mutationDef, bodyPartRecord);
		Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
		hediff.Severity = 0.2f;
		pawn.health.AddHediff(hediff);
		for (int num = 0; num < 3; num++)
		{
			pawn.health.DropBloodFilth();
		}
		MeatSplatter(3, pawn.PositionHeld, pawn.MapHeld);
		return true;
	}

	public static void MeatSplatter(int filthCount, IntVec3 pos, Map map, MeatExplosionSize size = MeatExplosionSize.Normal)
	{
		switch (size)
		{
		case MeatExplosionSize.Small:
			EffecterDefOf.MeatExplosionSmall.Spawn(pos, map).Cleanup();
			break;
		case MeatExplosionSize.Normal:
			EffecterDefOf.MeatExplosion.Spawn(pos, map).Cleanup();
			break;
		case MeatExplosionSize.Large:
			EffecterDefOf.MeatExplosionLarge.Spawn(pos, map).Cleanup();
			break;
		}
		CellRect cellRect = new CellRect(pos.x, pos.z, 3, 3).ClipInsideMap(map);
		for (int i = 0; i < filthCount; i++)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			ThingDef filthDef = (Rand.Bool ? ThingDefOf.Filth_Blood : ThingDefOf.Filth_TwistedFlesh);
			if (randomCell.InBounds(map) && GenSight.LineOfSight(randomCell, pos, map))
			{
				FilthMaker.TryMakeFilth(randomCell, map, filthDef);
			}
		}
	}

	public static Pawn SpawnFleshbeastFromPawn(Pawn pawn, bool randomKind = false, bool dropGear = false, params PawnKindDef[] ignoreKinds)
	{
		if (!ModsConfig.AnomalyActive || pawn == null)
		{
			return null;
		}
		IntVec3 positionHeld = pawn.PositionHeld;
		Map mapHeld = pawn.MapHeld;
		EffecterDefOf.MeatExplosion.Spawn(pawn.PositionHeld, pawn.MapHeld).Cleanup();
		if (dropGear)
		{
			pawn.Strip(notifyFaction: false);
		}
		if (pawn.Corpse != null)
		{
			pawn.Corpse.Destroy();
		}
		else
		{
			pawn.Destroy();
		}
		PawnKindDef kind = ((!randomKind) ? FleshbeastForAnimal(pawn) : AllFleshbeasts.Where((PawnKindDef x) => !ignoreKinds.Contains(x)).RandomElement());
		Faction ofEntities = Faction.OfEntities;
		float? fixedBiologicalAge = 0f;
		float? fixedChronologicalAge = 0f;
		Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, ofEntities, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge));
		CompInspectStringEmergence compInspectStringEmergence = pawn2.TryGetComp<CompInspectStringEmergence>();
		if (compInspectStringEmergence != null)
		{
			compInspectStringEmergence.sourcePawn = pawn;
		}
		return (Pawn)GenSpawn.Spawn(pawn2, positionHeld, mapHeld);
	}

	public static Pawn SpawnFleshbeastFromGround(IntVec3 cell, Map map, IntRange delayTicks, float maxPoints = -1f)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return null;
		}
		if (maxPoints <= 0f || !AllFleshbeasts.Where((PawnKindDef x) => x.combatPower <= maxPoints).TryRandomElement(out var result))
		{
			result = AllFleshbeasts.RandomElement();
		}
		Pawn pawn = PawnGenerator.GeneratePawn(result, Faction.OfEntities);
		if (pawn != null)
		{
			FleshbeastGroundSpawner obj = (FleshbeastGroundSpawner)ThingMaker.MakeThing(ThingDefOf.FleshbeastGroundSpawner);
			obj.Init(pawn, delayTicks);
			GenSpawn.Spawn(obj, cell, map);
		}
		return pawn;
	}

	public static Thing SpawnFleshbeastsFromPitBurrowEmergence(IntVec3 cell, Map map, float points, IntRange emergenceDelay, IntRange spawnDelay, bool assaultColony = true)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return null;
		}
		List<Pawn> fleshbeastsForPoints = GetFleshbeastsForPoints(points, map);
		BuildingGroundSpawner obj = (BuildingGroundSpawner)ThingMaker.MakeThing(ThingDefOf.PitBurrowSpawner);
		obj.emergeDelay = emergenceDelay;
		PitBurrow obj2 = (PitBurrow)obj.ThingToSpawn;
		obj2.emergingFleshbeasts = fleshbeastsForPoints;
		obj2.emergeDelay = spawnDelay.RandomInRange;
		obj2.assaultColony = assaultColony;
		GenSpawn.Spawn(obj, cell, map);
		return obj;
	}

	public static Thing SpawnPawnAsFlyer(Pawn pawn, Map map, IntVec3 rootCell, int jumpDist = 5, bool requiresLOS = true)
	{
		if (!pawn.Spawned)
		{
			GenSpawn.Spawn(pawn, rootCell, map);
		}
		tmpTakenCells.Clear();
		if (RCellFinder.TryFindRandomCellNearWith(rootCell, (IntVec3 c) => !c.Fogged(map) && c.Standable(map) && !tmpTakenCells.Contains(c) && c.GetFirstPawn(map) == null && (!requiresLOS || GenSight.LineOfSight(rootCell, c, map, skipFirstCell: true)), map, out var result, 5, jumpDist))
		{
			pawn.rotationTracker.FaceCell(result);
			tmpTakenCells.Add(result);
			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, pawn, result, null, null);
			if (pawnFlyer != null)
			{
				GenSpawn.Spawn(pawnFlyer, result, map);
			}
			return pawnFlyer;
		}
		return null;
	}

	private static bool IsValidProstheticShoulder(Pawn pawn, BodyPartRecord part)
	{
		if (pawn.health.hediffSet.HasDirectlyAddedPartFor(part))
		{
			return pawn.health.hediffSet.GetDirectlyAddedPartFor(part).def != HediffDefOf.Tentacle;
		}
		return false;
	}

	private static PawnKindDef FleshbeastForAnimal(Pawn animal)
	{
		if (animal.BodySize < 0.75f)
		{
			return PawnKindDefOf.Fingerspike;
		}
		if (animal.BodySize < 3.5f)
		{
			if (!Rand.Bool)
			{
				return PawnKindDefOf.Trispike;
			}
			return PawnKindDefOf.Toughspike;
		}
		return PawnKindDefOf.Bulbfreak;
	}

	public static float ExistingFleshBeastThreat(Map map)
	{
		float num = 0f;
		foreach (Pawn item in map.mapPawns.PawnsInFaction(Faction.OfEntities))
		{
			if (item.kindDef.IsFleshBeast())
			{
				num += item.kindDef.combatPower;
			}
		}
		return num;
	}

	public static bool IsFleshBeast(this PawnKindDef pawnKind)
	{
		return pawnKind.RaceProps.FleshType == FleshTypeDefOf.Fleshbeast;
	}

	public static List<Pawn> GetFleshbeastsForPoints(float points, Map map, bool allowDreadmeld = false)
	{
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
		pawnGroupMakerParms.groupKind = (allowDreadmeld ? PawnGroupKindDefOf.FleshbeastsWithDreadmeld : PawnGroupKindDefOf.Fleshbeasts);
		pawnGroupMakerParms.tile = map.Tile;
		pawnGroupMakerParms.faction = Faction.OfEntities;
		pawnGroupMakerParms.points = ((points > 0f) ? points : StorytellerUtility.DefaultThreatPointsNow(map));
		pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, pawnGroupMakerParms.faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind) * 1.05f);
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
	}

	public static IEnumerable<Pawn> SplitFleshbeast(Pawn fleshbeast)
	{
		if (!(fleshbeast.RaceProps.deathAction is DeathActionProperties_Divide divide))
		{
			yield return fleshbeast;
			yield break;
		}
		for (int i = 0; i < divide.dividePawnCount; i++)
		{
			IEnumerable<Pawn> enumerable = SplitFleshbeast(PawnGenerator.GeneratePawn(new PawnGenerationRequest(divide.dividePawnKindOptions.RandomElement(), fleshbeast.Faction)));
			foreach (Pawn item in enumerable)
			{
				yield return item;
			}
		}
	}

	public static void DoFleshbeastResponse(CompGrowsFleshmassTendrils source, IntVec3 position)
	{
		Map map = source.parent.Map;
		List<Thing> list = new List<Thing>();
		foreach (Thing item in map.listerThings.ThingsOfDef(ThingDefOf.Fleshmass_Active))
		{
			if (!item.Position.InHorDistOf(position, 30f))
			{
				continue;
			}
			IntVec3[] adjacentCells = GenAdj.AdjacentCells;
			foreach (IntVec3 intVec in adjacentCells)
			{
				if ((item.Position + intVec).Standable(map))
				{
					list.Add(item);
					break;
				}
			}
		}
		if (list.Empty())
		{
			return;
		}
		SoundDefOf.FleshmassBirth.PlayOneShot(new TargetInfo(position, map));
		Find.CameraDriver.shaker.DoShake(1f);
		List<Pawn> fleshbeastsForPoints = GetFleshbeastsForPoints(FleshbeastResponsePointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(map)), map);
		List<PawnFlyer> list2 = new List<PawnFlyer>();
		List<IntVec3> list3 = new List<IntVec3>();
		foreach (Pawn item2 in fleshbeastsForPoints)
		{
			Thing thing = list.RandomElement();
			GenSpawn.Spawn(item2, thing.Position, map);
			CellFinder.TryFindRandomCellNear(thing.Position, map, 2, (IntVec3 cell) => !cell.Fogged(map) && cell.Walkable(map) && !cell.Impassable(map), out var result);
			item2.rotationTracker.FaceCell(result);
			list2.Add(PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, item2, result, null, null));
			list3.Add(thing.Position);
		}
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), map);
		SpawnRequest spawnRequest = new SpawnRequest(list2.Cast<Thing>().ToList(), list3, 1, 2f / (float)fleshbeastsForPoints.Count, lord);
		spawnRequest.spawnEffect = EffecterDefOf.MeatExplosionSmall;
		map.deferredSpawner.AddRequest(spawnRequest);
		Find.LetterStack.ReceiveLetter("FleshmassResponseLabel".Translate(), "FleshmassResponseText".Translate(), LetterDefOf.ThreatBig, fleshbeastsForPoints);
	}
}
