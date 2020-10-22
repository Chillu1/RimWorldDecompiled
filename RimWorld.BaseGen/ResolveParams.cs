using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public struct ResolveParams
	{
		public CellRect rect;

		public Faction faction;

		private Dictionary<string, object> custom;

		public PawnGroupMakerParms pawnGroupMakerParams;

		public PawnGroupKindDef pawnGroupKindDef;

		public RoofDef roofDef;

		public bool? noRoof;

		public bool? addRoomCenterToRootsToUnfog;

		public Thing singleThingToSpawn;

		public ThingDef singleThingDef;

		public ThingDef singleThingStuff;

		public int? singleThingStackCount;

		public bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit;

		public bool? spawnBridgeIfTerrainCantSupportThing;

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

		public PawnGenerationRequest? singlePawnGenerationRequest;

		public Action<Thing> postThingSpawn;

		public Action<Thing> postThingGenerate;

		public int? mechanoidsCount;

		public int? hivesCount;

		public bool? disableHives;

		public Rot4? thingRot;

		public ThingDef wallStuff;

		public float? chanceToSkipWallBlock;

		public TerrainDef floorDef;

		public float? chanceToSkipFloor;

		public ThingDef filthDef;

		public FloatRange? filthDensity;

		public bool? floorOnlyIfTerrainSupports;

		public bool? allowBridgeOnAnyImpassableTerrain;

		public bool? clearEdificeOnly;

		public bool? clearFillageOnly;

		public bool? clearRoof;

		public int? ancientCryptosleepCasketGroupID;

		public PodContentsType? podContentsType;

		public ThingSetMakerDef thingSetMakerDef;

		public ThingSetMakerParams? thingSetMakerParams;

		public IList<Thing> stockpileConcreteContents;

		public float? stockpileMarketValue;

		public int? innerStockpileSize;

		public int? edgeDefenseWidth;

		public int? edgeDefenseTurretsCount;

		public int? edgeDefenseMortarsCount;

		public int? edgeDefenseGuardsCount;

		public ThingDef mortarDef;

		public TerrainDef pathwayFloorDef;

		public ThingDef cultivatedPlantDef;

		public int? fillWithThingsPadding;

		public float? settlementPawnGroupPoints;

		public int? settlementPawnGroupSeed;

		public bool? streetHorizontal;

		public bool? edgeThingAvoidOtherEdgeThings;

		public bool? edgeThingMustReachMapEdge;

		public bool? allowPlacementOffEdge;

		public Rot4? thrustAxis;

		public FloatRange? hpPercentRange;

		public Thing conditionCauser;

		public bool? makeWarningLetter;

		public ThingFilter allowedMonumentThings;

		public void SetCustom<T>(string name, T obj, bool inherit = false)
		{
			ResolveParamsUtility.SetCustom(ref custom, name, obj, inherit);
		}

		public void RemoveCustom(string name)
		{
			ResolveParamsUtility.RemoveCustom(ref custom, name);
		}

		public bool TryGetCustom<T>(string name, out T obj)
		{
			return ResolveParamsUtility.TryGetCustom<T>(custom, name, out obj);
		}

		public T GetCustom<T>(string name)
		{
			return ResolveParamsUtility.GetCustom<T>(custom, name);
		}

		public override string ToString()
		{
			return string.Concat("rect=", rect, ", faction=", (faction != null) ? faction.ToString() : "null", ", custom=", (custom != null) ? custom.Count.ToString() : "null", ", pawnGroupMakerParams=", (pawnGroupMakerParams != null) ? pawnGroupMakerParams.ToString() : "null", ", pawnGroupKindDef=", (pawnGroupKindDef != null) ? pawnGroupKindDef.ToString() : "null", ", roofDef=", (roofDef != null) ? roofDef.ToString() : "null", ", noRoof=", noRoof.HasValue ? noRoof.ToString() : "null", ", addRoomCenterToRootsToUnfog=", addRoomCenterToRootsToUnfog.HasValue ? addRoomCenterToRootsToUnfog.ToString() : "null", ", singleThingToSpawn=", (singleThingToSpawn != null) ? singleThingToSpawn.ToString() : "null", ", singleThingDef=", (singleThingDef != null) ? singleThingDef.ToString() : "null", ", singleThingStuff=", (singleThingStuff != null) ? singleThingStuff.ToString() : "null", ", singleThingStackCount=", singleThingStackCount.HasValue ? singleThingStackCount.ToString() : "null", ", skipSingleThingIfHasToWipeBuildingOrDoesntFit=", skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue ? skipSingleThingIfHasToWipeBuildingOrDoesntFit.ToString() : "null", ", spawnBridgeIfTerrainCantSupportThing=", spawnBridgeIfTerrainCantSupportThing.HasValue ? spawnBridgeIfTerrainCantSupportThing.ToString() : "null", ", singlePawnToSpawn=", (singlePawnToSpawn != null) ? singlePawnToSpawn.ToString() : "null", ", singlePawnKindDef=", (singlePawnKindDef != null) ? singlePawnKindDef.ToString() : "null", ", disableSinglePawn=", disableSinglePawn.HasValue ? disableSinglePawn.ToString() : "null", ", singlePawnLord=", (singlePawnLord != null) ? singlePawnLord.ToString() : "null", ", singlePawnSpawnCellExtraPredicate=", (singlePawnSpawnCellExtraPredicate != null) ? singlePawnSpawnCellExtraPredicate.ToString() : "null", ", singlePawnGenerationRequest=", singlePawnGenerationRequest.HasValue ? singlePawnGenerationRequest.ToString() : "null", ", postThingSpawn=", (postThingSpawn != null) ? postThingSpawn.ToString() : "null", ", postThingGenerate=", (postThingGenerate != null) ? postThingGenerate.ToString() : "null", ", mechanoidsCount=", mechanoidsCount.HasValue ? mechanoidsCount.ToString() : "null", ", hivesCount=", hivesCount.HasValue ? hivesCount.ToString() : "null", ", disableHives=", disableHives.HasValue ? disableHives.ToString() : "null", ", thingRot=", thingRot.HasValue ? thingRot.ToString() : "null", ", wallStuff=", (wallStuff != null) ? wallStuff.ToString() : "null", ", chanceToSkipWallBlock=", chanceToSkipWallBlock.HasValue ? chanceToSkipWallBlock.ToString() : "null", ", floorDef=", (floorDef != null) ? floorDef.ToString() : "null", ", chanceToSkipFloor=", chanceToSkipFloor.HasValue ? chanceToSkipFloor.ToString() : "null", ", filthDef=", (filthDef != null) ? filthDef.ToString() : "null", ", filthDensity=", filthDensity.HasValue ? filthDensity.ToString() : "null", ", floorOnlyIfTerrainSupports=", floorOnlyIfTerrainSupports.HasValue ? floorOnlyIfTerrainSupports.ToString() : "null", ", allowBridgeOnAnyImpassableTerrain=", allowBridgeOnAnyImpassableTerrain.HasValue ? allowBridgeOnAnyImpassableTerrain.ToString() : "null", ", clearEdificeOnly=", clearEdificeOnly.HasValue ? clearEdificeOnly.ToString() : "null", ", clearFillageOnly=", clearFillageOnly.HasValue ? clearFillageOnly.ToString() : "null", ", clearRoof=", clearRoof.HasValue ? clearRoof.ToString() : "null", ", ancientCryptosleepCasketGroupID=", ancientCryptosleepCasketGroupID.HasValue ? ancientCryptosleepCasketGroupID.ToString() : "null", ", podContentsType=", podContentsType.HasValue ? podContentsType.ToString() : "null", ", thingSetMakerDef=", (thingSetMakerDef != null) ? thingSetMakerDef.ToString() : "null", ", thingSetMakerParams=", thingSetMakerParams.HasValue ? thingSetMakerParams.ToString() : "null", ", stockpileConcreteContents=", (stockpileConcreteContents != null) ? stockpileConcreteContents.Count.ToString() : "null", ", stockpileMarketValue=", stockpileMarketValue.HasValue ? stockpileMarketValue.ToString() : "null", ", innerStockpileSize=", innerStockpileSize.HasValue ? innerStockpileSize.ToString() : "null", ", edgeDefenseWidth=", edgeDefenseWidth.HasValue ? edgeDefenseWidth.ToString() : "null", ", edgeDefenseTurretsCount=", edgeDefenseTurretsCount.HasValue ? edgeDefenseTurretsCount.ToString() : "null", ", edgeDefenseMortarsCount=", edgeDefenseMortarsCount.HasValue ? edgeDefenseMortarsCount.ToString() : "null", ", edgeDefenseGuardsCount=", edgeDefenseGuardsCount.HasValue ? edgeDefenseGuardsCount.ToString() : "null", ", mortarDef=", (mortarDef != null) ? mortarDef.ToString() : "null", ", pathwayFloorDef=", (pathwayFloorDef != null) ? pathwayFloorDef.ToString() : "null", ", cultivatedPlantDef=", (cultivatedPlantDef != null) ? cultivatedPlantDef.ToString() : "null", ", fillWithThingsPadding=", fillWithThingsPadding.HasValue ? fillWithThingsPadding.ToString() : "null", ", settlementPawnGroupPoints=", settlementPawnGroupPoints.HasValue ? settlementPawnGroupPoints.ToString() : "null", ", settlementPawnGroupSeed=", settlementPawnGroupSeed.HasValue ? settlementPawnGroupSeed.ToString() : "null", ", streetHorizontal=", streetHorizontal.HasValue ? streetHorizontal.ToString() : "null", ", edgeThingAvoidOtherEdgeThings=", edgeThingAvoidOtherEdgeThings.HasValue ? edgeThingAvoidOtherEdgeThings.ToString() : "null", ", edgeThingMustReachMapEdge=", edgeThingMustReachMapEdge.HasValue ? edgeThingMustReachMapEdge.ToString() : "null", ", allowPlacementOffEdge=", allowPlacementOffEdge.HasValue ? allowPlacementOffEdge.ToString() : "null", ", thrustAxis=", thrustAxis.HasValue ? thrustAxis.ToString() : "null", ", makeWarningLetter=", makeWarningLetter.HasValue ? makeWarningLetter.ToString() : "null", ", allowedMonumentThings=", (allowedMonumentThings != null) ? allowedMonumentThings.ToString() : "null");
		}
	}
}
