using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_Settlement : SymbolResolver
{
	public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);

	public const float DefaultLootMarketValue = 1800f;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
		int num = 0;
		if (rp.edgeDefenseWidth.HasValue)
		{
			num = rp.edgeDefenseWidth.Value;
		}
		else if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && ((int)faction.def.techLevel >= 4 || Rand.Bool))
		{
			num = (Rand.Bool ? 2 : 4);
		}
		float num2 = (float)rp.rect.Area / 144f * 0.17f;
		BaseGen.globalSettings.minEmptyNodes = ((!(num2 < 1f)) ? GenMath.RoundRandom(num2) : 0);
		ResolveParams resolveParams = rp;
		ref ThingSetMakerDef thingSetMakerDef = ref resolveParams.thingSetMakerDef;
		if (thingSetMakerDef == null)
		{
			thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;
		}
		ref float? lootMarketValue = ref resolveParams.lootMarketValue;
		float valueOrDefault = lootMarketValue.GetValueOrDefault();
		if (!lootMarketValue.HasValue)
		{
			valueOrDefault = 1800f;
			lootMarketValue = valueOrDefault;
		}
		BaseGen.symbolStack.Push("lootScatter", resolveParams);
		if (rp.settlementDontGeneratePawns != true)
		{
			Lord singlePawnLord = (rp.settlementLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell, 25000, rp.attackWhenPlayerBecameEnemy == true), map));
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
			ResolveParams resolveParams2 = rp;
			resolveParams2.rect = rp.rect;
			resolveParams2.faction = faction;
			resolveParams2.singlePawnLord = singlePawnLord;
			resolveParams2.pawnGroupKindDef = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
			resolveParams2.singlePawnSpawnCellExtraPredicate = rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
			if (resolveParams2.pawnGroupMakerParams == null)
			{
				resolveParams2.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams2.pawnGroupMakerParams.tile = map.Tile;
				resolveParams2.pawnGroupMakerParams.faction = faction;
				resolveParams2.pawnGroupMakerParams.points = rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange;
				resolveParams2.pawnGroupMakerParams.inhabitants = true;
				resolveParams2.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			rp.bedCount = PawnGroupMakerUtility.GeneratePawnKindsExample(SymbolResolver_PawnGroup.GetGroupMakerParms(map, resolveParams2)).Count();
			BaseGen.symbolStack.Push("pawnGroup", resolveParams2);
		}
		if (SymbolResolver_TerrorBuildings.FactionShouldHaveTerrorBuildings(rp.faction))
		{
			BaseGen.symbolStack.Push("terrorBuildings", rp);
		}
		BaseGen.symbolStack.Push("outdoorLighting", rp);
		if ((int)faction.def.techLevel >= 4)
		{
			int num3 = (Rand.Chance(0.75f) ? GenMath.RoundRandom((float)rp.rect.Area / 400f) : 0);
			for (int num4 = 0; num4 < num3; num4++)
			{
				ResolveParams resolveParams3 = rp;
				resolveParams3.faction = faction;
				BaseGen.symbolStack.Push("firefoamPopper", resolveParams3);
			}
		}
		if (num > 0)
		{
			ResolveParams resolveParams4 = rp;
			resolveParams4.faction = faction;
			resolveParams4.edgeDefenseWidth = num;
			resolveParams4.edgeThingMustReachMapEdge = rp.edgeThingMustReachMapEdge ?? true;
			BaseGen.symbolStack.Push("edgeDefense", resolveParams4);
		}
		ResolveParams resolveParams5 = rp;
		resolveParams5.rect = rp.rect.ContractedBy(num);
		resolveParams5.faction = faction;
		BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams5);
		ResolveParams resolveParams6 = rp;
		resolveParams6.rect = rp.rect.ContractedBy(num);
		resolveParams6.faction = faction;
		resolveParams6.floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports ?? true;
		BaseGen.symbolStack.Push("basePart_outdoors", resolveParams6);
		ResolveParams resolveParams7 = rp;
		resolveParams7.floorDef = TerrainDefOf.Bridge;
		resolveParams7.floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports ?? true;
		resolveParams7.allowBridgeOnAnyImpassableTerrain = rp.allowBridgeOnAnyImpassableTerrain ?? true;
		BaseGen.symbolStack.Push("floor", resolveParams7);
		BaseGen.symbolStack.Push("removeDangerousTerrain", rp);
		if (ModsConfig.BiotechActive)
		{
			ResolveParams resolveParams8 = rp;
			resolveParams8.rect = rp.rect.ExpandedBy(Rand.Range(1, 4));
			resolveParams8.edgeUnpolluteChance = 0.5f;
			BaseGen.symbolStack.Push("unpollute", resolveParams8);
		}
	}
}
