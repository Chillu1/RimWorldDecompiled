using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Settlement : SymbolResolver
	{
		public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);

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
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect;
			resolveParams.faction = faction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms))));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.tile = map.Tile;
				resolveParams.pawnGroupMakerParams.faction = faction;
				resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? DefaultPawnsPoints.RandomInRange);
				resolveParams.pawnGroupMakerParams.inhabitants = true;
				resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			BaseGen.symbolStack.Push("pawnGroup", resolveParams);
			BaseGen.symbolStack.Push("outdoorLighting", rp);
			if ((int)faction.def.techLevel >= 4)
			{
				int num3 = Rand.Chance(0.75f) ? GenMath.RoundRandom((float)rp.rect.Area / 400f) : 0;
				for (int i = 0; i < num3; i++)
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.faction = faction;
					BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
				}
			}
			if (num > 0)
			{
				ResolveParams resolveParams3 = rp;
				resolveParams3.faction = faction;
				resolveParams3.edgeDefenseWidth = num;
				resolveParams3.edgeThingMustReachMapEdge = (rp.edgeThingMustReachMapEdge ?? true);
				BaseGen.symbolStack.Push("edgeDefense", resolveParams3);
			}
			ResolveParams resolveParams4 = rp;
			resolveParams4.rect = rp.rect.ContractedBy(num);
			resolveParams4.faction = faction;
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);
			ResolveParams resolveParams5 = rp;
			resolveParams5.rect = rp.rect.ContractedBy(num);
			resolveParams5.faction = faction;
			resolveParams5.floorOnlyIfTerrainSupports = (rp.floorOnlyIfTerrainSupports ?? true);
			BaseGen.symbolStack.Push("basePart_outdoors", resolveParams5);
			ResolveParams resolveParams6 = rp;
			resolveParams6.floorDef = TerrainDefOf.Bridge;
			resolveParams6.floorOnlyIfTerrainSupports = (rp.floorOnlyIfTerrainSupports ?? true);
			resolveParams6.allowBridgeOnAnyImpassableTerrain = (rp.allowBridgeOnAnyImpassableTerrain ?? true);
			BaseGen.symbolStack.Push("floor", resolveParams6);
		}
	}
}
