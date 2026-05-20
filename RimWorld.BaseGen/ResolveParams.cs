using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public struct ResolveParams
{
	public CellRect rect;

	public Faction faction;

	public float? threatPoints;

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

	public bool? spawnOutside;

	public List<Thing> singleThingInnerThings;

	public Pawn singlePawnToSpawn;

	public PawnKindDef singlePawnKindDef;

	public bool? disableSinglePawn;

	public Lord singlePawnLord;

	public Lord settlementLord;

	public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

	public PawnGenerationRequest? singlePawnGenerationRequest;

	public Action<Thing> postThingSpawn;

	public Action<Thing> postThingGenerate;

	public int? mechanoidsCount;

	public int? fleshbeastsCount;

	public int? hivesCount;

	public bool? disableHives;

	public Rot4? thingRot;

	public ThingDef wallStuff;

	public float? chanceToSkipWallBlock;

	public ThingDef wallThingDef;

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

	public string ancientCryptosleepCasketOpenSignalTag;

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

	public float? fixedCulativedPlantGrowth;

	public int? fillWithThingsPadding;

	public float? settlementPawnGroupPoints;

	public int? settlementPawnGroupSeed;

	public bool? settlementDontGeneratePawns;

	public bool? attackWhenPlayerBecameEnemy;

	public bool? streetHorizontal;

	public bool? edgeThingAvoidOtherEdgeThings;

	public bool? edgeThingMustReachMapEdge;

	public bool? allowPlacementOffEdge;

	public Rot4? thrustAxis;

	public FloatRange? hpPercentRange;

	public Thing conditionCauser;

	public bool? makeWarningLetter;

	public ThingFilter allowedMonumentThings;

	public bool? allowGeneratingThronerooms;

	public int? bedCount;

	public float workSitePoints;

	public IList<Thing> lootConcereteContents;

	public float? lootMarketValue;

	public Rot4? extraDoorEdge;

	public string sleepingInsectsWakeupSignalTag;

	public string sleepingMechanoidsWakeupSignalTag;

	public string rectTriggerSignalTag;

	public bool? destroyIfUnfogged;

	public string messageSignalTag;

	public string message;

	public MessageTypeDef messageType;

	public LookTargets lookTargets;

	public bool? spawnAnywhereIfNoGoodCell;

	public bool? ignoreRoofedRequirement;

	public IntVec3? overrideLoc;

	public bool? sendStandardLetter;

	public string infestationSignalTag;

	public float? insectsPoints;

	public string ambushSignalTag;

	public SignalActionAmbushType? ambushType;

	public IntVec3? spawnNear;

	public CellRect? spawnAround;

	public bool? spawnPawnsOnEdge;

	public float? ambushPoints;

	public int? cornerRadius;

	public SoundDef sound;

	public string soundOneShotActionSignalTag;

	public string unfoggedSignalTag;

	public Thing triggerContainerEmptiedThing;

	public string triggerContainerEmptiedSignalTag;

	public Building_Door openDoorActionDoor;

	public string openDoorActionSignalTag;

	public string triggerSecuritySignal;

	public Thing relicThing;

	public float? exteriorThreatPoints;

	public float? interiorThreatPoints;

	public SitePart sitePart;

	public int? minLengthAfterSplit;

	public bool? sendWokenUpMessage;

	public LayoutStructureSketch ancientLayoutStructureSketch;

	public bool? ignoreDoorways;

	public int? minorBuildingCount;

	public int? minorBuildingRadialDistance;

	public ThingDef centralBuilding;

	public PawnKindDef desiccatedCorpsePawnKind;

	public IntRange? desiccatedCorpseRandomAgeRange;

	public FloatRange? dessicatedCorpseDensityRange;

	public float edgeUnpolluteChance;

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
		string[] array = new string[142];
		array[0] = "rect=";
		CellRect cellRect = rect;
		array[1] = cellRect.ToString();
		array[2] = ", faction=";
		array[3] = ((faction != null) ? faction.ToString() : "null");
		array[4] = ", custom=";
		array[5] = ((custom != null) ? custom.Count.ToString() : "null");
		array[6] = ", combatPoints=";
		array[7] = (threatPoints.HasValue ? threatPoints.ToString() : "null");
		array[8] = ", pawnGroupMakerParams=";
		array[9] = ((pawnGroupMakerParams != null) ? pawnGroupMakerParams.ToString() : "null");
		array[10] = ", pawnGroupKindDef=";
		array[11] = ((pawnGroupKindDef != null) ? pawnGroupKindDef.ToString() : "null");
		array[12] = ", roofDef=";
		array[13] = ((roofDef != null) ? roofDef.ToString() : "null");
		array[14] = ", noRoof=";
		array[15] = (noRoof.HasValue ? noRoof.ToString() : "null");
		array[16] = ", addRoomCenterToRootsToUnfog=";
		array[17] = (addRoomCenterToRootsToUnfog.HasValue ? addRoomCenterToRootsToUnfog.ToString() : "null");
		array[18] = ", singleThingToSpawn=";
		array[19] = ((singleThingToSpawn != null) ? singleThingToSpawn.ToString() : "null");
		array[20] = ", singleThingDef=";
		array[21] = ((singleThingDef != null) ? singleThingDef.ToString() : "null");
		array[22] = ", singleThingStuff=";
		array[23] = ((singleThingStuff != null) ? singleThingStuff.ToString() : "null");
		array[24] = ", singleThingStackCount=";
		array[25] = (singleThingStackCount.HasValue ? singleThingStackCount.ToString() : "null");
		array[26] = ", skipSingleThingIfHasToWipeBuildingOrDoesntFit=";
		array[27] = (skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue ? skipSingleThingIfHasToWipeBuildingOrDoesntFit.ToString() : "null");
		array[28] = ", spawnBridgeIfTerrainCantSupportThing=";
		array[29] = (spawnBridgeIfTerrainCantSupportThing.HasValue ? spawnBridgeIfTerrainCantSupportThing.ToString() : "null");
		array[30] = ", singleThingInnerThings=";
		array[31] = ((singleThingInnerThings != null) ? singleThingInnerThings.ToString() : "null");
		array[32] = ", singlePawnToSpawn=";
		array[33] = ((singlePawnToSpawn != null) ? singlePawnToSpawn.ToString() : "null");
		array[34] = ", singlePawnKindDef=";
		array[35] = ((singlePawnKindDef != null) ? singlePawnKindDef.ToString() : "null");
		array[36] = ", disableSinglePawn=";
		array[37] = (disableSinglePawn.HasValue ? disableSinglePawn.ToString() : "null");
		array[38] = ", singlePawnLord=";
		array[39] = ((singlePawnLord != null) ? singlePawnLord.ToString() : "null");
		array[40] = ", settlementLord=";
		array[41] = ((settlementLord != null) ? settlementLord.ToString() : "null");
		array[42] = ", singlePawnSpawnCellExtraPredicate=";
		array[43] = ((singlePawnSpawnCellExtraPredicate != null) ? singlePawnSpawnCellExtraPredicate.ToString() : "null");
		array[44] = ", singlePawnGenerationRequest=";
		array[45] = (singlePawnGenerationRequest.HasValue ? singlePawnGenerationRequest.ToString() : "null");
		array[46] = ", postThingSpawn=";
		array[47] = ((postThingSpawn != null) ? postThingSpawn.ToString() : "null");
		array[48] = ", postThingGenerate=";
		array[49] = ((postThingGenerate != null) ? postThingGenerate.ToString() : "null");
		array[50] = ", mechanoidsCount=";
		array[51] = (mechanoidsCount.HasValue ? mechanoidsCount.ToString() : "null");
		array[52] = ", hivesCount=";
		array[53] = (hivesCount.HasValue ? hivesCount.ToString() : "null");
		array[54] = ", disableHives=";
		array[55] = (disableHives.HasValue ? disableHives.ToString() : "null");
		array[56] = ", thingRot=";
		array[57] = (thingRot.HasValue ? thingRot.ToString() : "null");
		array[58] = ", wallStuff=";
		array[59] = ((wallStuff != null) ? wallStuff.ToString() : "null");
		array[60] = ", chanceToSkipWallBlock=";
		array[61] = (chanceToSkipWallBlock.HasValue ? chanceToSkipWallBlock.ToString() : "null");
		array[62] = ", floorDef=";
		array[63] = ((floorDef != null) ? floorDef.ToString() : "null");
		array[64] = ", chanceToSkipFloor=";
		array[65] = (chanceToSkipFloor.HasValue ? chanceToSkipFloor.ToString() : "null");
		array[66] = ", filthDef=";
		array[67] = ((filthDef != null) ? filthDef.ToString() : "null");
		array[68] = ", filthDensity=";
		array[69] = (filthDensity.HasValue ? filthDensity.ToString() : "null");
		array[70] = ", floorOnlyIfTerrainSupports=";
		array[71] = (floorOnlyIfTerrainSupports.HasValue ? floorOnlyIfTerrainSupports.ToString() : "null");
		array[72] = ", allowBridgeOnAnyImpassableTerrain=";
		array[73] = (allowBridgeOnAnyImpassableTerrain.HasValue ? allowBridgeOnAnyImpassableTerrain.ToString() : "null");
		array[74] = ", clearEdificeOnly=";
		array[75] = (clearEdificeOnly.HasValue ? clearEdificeOnly.ToString() : "null");
		array[76] = ", clearFillageOnly=";
		array[77] = (clearFillageOnly.HasValue ? clearFillageOnly.ToString() : "null");
		array[78] = ", clearRoof=";
		array[79] = (clearRoof.HasValue ? clearRoof.ToString() : "null");
		array[80] = ", ancientCryptosleepCasketGroupID=";
		array[81] = (ancientCryptosleepCasketGroupID.HasValue ? ancientCryptosleepCasketGroupID.ToString() : "null");
		array[82] = ", podContentsType=";
		array[83] = (podContentsType.HasValue ? podContentsType.ToString() : "null");
		array[84] = ", ancientCryptosleepCasketOpenSignalTag=";
		array[85] = ((ancientCryptosleepCasketOpenSignalTag != null) ? ancientCryptosleepCasketOpenSignalTag.ToString() : "null");
		array[86] = ", thingSetMakerDef=";
		array[87] = ((thingSetMakerDef != null) ? thingSetMakerDef.ToString() : "null");
		array[88] = ", thingSetMakerParams=";
		array[89] = (thingSetMakerParams.HasValue ? thingSetMakerParams.ToString() : "null");
		array[90] = ", stockpileConcreteContents=";
		array[91] = ((stockpileConcreteContents != null) ? stockpileConcreteContents.Count.ToString() : "null");
		array[92] = ", stockpileMarketValue=";
		array[93] = (stockpileMarketValue.HasValue ? stockpileMarketValue.ToString() : "null");
		array[94] = ", innerStockpileSize=";
		array[95] = (innerStockpileSize.HasValue ? innerStockpileSize.ToString() : "null");
		array[96] = ", edgeDefenseWidth=";
		array[97] = (edgeDefenseWidth.HasValue ? edgeDefenseWidth.ToString() : "null");
		array[98] = ", edgeDefenseTurretsCount=";
		array[99] = (edgeDefenseTurretsCount.HasValue ? edgeDefenseTurretsCount.ToString() : "null");
		array[100] = ", edgeDefenseMortarsCount=";
		array[101] = (edgeDefenseMortarsCount.HasValue ? edgeDefenseMortarsCount.ToString() : "null");
		array[102] = ", edgeDefenseGuardsCount=";
		array[103] = (edgeDefenseGuardsCount.HasValue ? edgeDefenseGuardsCount.ToString() : "null");
		array[104] = ", mortarDef=";
		array[105] = ((mortarDef != null) ? mortarDef.ToString() : "null");
		array[106] = ", pathwayFloorDef=";
		array[107] = ((pathwayFloorDef != null) ? pathwayFloorDef.ToString() : "null");
		array[108] = ", cultivatedPlantDef=";
		array[109] = ((cultivatedPlantDef != null) ? cultivatedPlantDef.ToString() : "null");
		array[110] = ", fixedCulativedPlantGrowth=";
		array[111] = (fixedCulativedPlantGrowth.HasValue ? fixedCulativedPlantGrowth.ToString() : "null");
		array[112] = ", fillWithThingsPadding=";
		array[113] = (fillWithThingsPadding.HasValue ? fillWithThingsPadding.ToString() : "null");
		array[114] = ", settlementPawnGroupPoints=";
		array[115] = (settlementPawnGroupPoints.HasValue ? settlementPawnGroupPoints.ToString() : "null");
		array[116] = ", settlementPawnGroupSeed=";
		array[117] = (settlementPawnGroupSeed.HasValue ? settlementPawnGroupSeed.ToString() : "null");
		array[118] = ", settlementDontGeneratePawns=";
		array[119] = (settlementDontGeneratePawns.HasValue ? settlementDontGeneratePawns.ToString() : "null");
		array[120] = ", attackWhenPlayerBecameEnemy=";
		array[121] = (attackWhenPlayerBecameEnemy.HasValue ? attackWhenPlayerBecameEnemy.ToString() : "null");
		array[122] = ", streetHorizontal=";
		array[123] = (streetHorizontal.HasValue ? streetHorizontal.ToString() : "null");
		array[124] = ", edgeThingAvoidOtherEdgeThings=";
		array[125] = (edgeThingAvoidOtherEdgeThings.HasValue ? edgeThingAvoidOtherEdgeThings.ToString() : "null");
		array[126] = ", edgeThingMustReachMapEdge=";
		array[127] = (edgeThingMustReachMapEdge.HasValue ? edgeThingMustReachMapEdge.ToString() : "null");
		array[128] = ", allowPlacementOffEdge=";
		array[129] = (allowPlacementOffEdge.HasValue ? allowPlacementOffEdge.ToString() : "null");
		array[130] = ", thrustAxis=";
		array[131] = (thrustAxis.HasValue ? thrustAxis.ToString() : "null");
		array[132] = ", makeWarningLetter=";
		array[133] = (makeWarningLetter.HasValue ? makeWarningLetter.ToString() : "null");
		array[134] = ", allowedMonumentThings=";
		array[135] = ((allowedMonumentThings != null) ? allowedMonumentThings.ToString() : "null");
		array[136] = ", bedCount=";
		array[137] = (bedCount.HasValue ? bedCount.ToString() : ("null, workSitePoints=" + ((workSitePoints != 0f) ? workSitePoints.ToString() : "null") + ", lootConcereteContents=" + ((lootConcereteContents != null) ? lootConcereteContents.ToString() : "null") + ", lootMarketValue=" + (lootMarketValue.HasValue ? lootMarketValue.ToString() : "null")));
		array[138] = ", extraDoorEdge=";
		array[139] = (extraDoorEdge.HasValue ? extraDoorEdge.ToString() : "null");
		array[140] = ", minLengthAfterSplit=";
		array[141] = (minLengthAfterSplit.HasValue ? minLengthAfterSplit.ToString() : "null");
		return string.Concat(array);
	}
}
