using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_MannedMortar : SymbolResolver
{
	private const float MaxShellDefMarketValue = 250f;

	public override bool CanResolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		if (!base.CanResolve(rp))
		{
			return false;
		}
		int num = 0;
		foreach (IntVec3 item in rp.rect)
		{
			if (item.Standable(map))
			{
				num++;
			}
		}
		if (num < 2)
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: true, TechLevel.Industrial) ?? Find.FactionManager.RandomEnemyFaction();
		Rot4 rot = rp.thingRot ?? Rot4.Random;
		ThingDef thingDef = rp.mortarDef ?? DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Building && x.building.IsMortar && x.building.buildingTags.Contains("Artillery_MannedMortar")).RandomElement();
		if (TryFindMortarSpawnCell(rp.rect, rot, thingDef, out var cell))
		{
			if (thingDef.HasComp(typeof(CompMannable)))
			{
				IntVec3 c = ThingUtility.InteractionCellWhenAt(thingDef, cell, rot, map);
				Lord singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_ManTurrets(), map);
				PawnGenerationRequest value = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: true);
				ResolveParams resolveParams = rp;
				resolveParams.faction = faction;
				resolveParams.singlePawnGenerationRequest = value;
				resolveParams.rect = CellRect.SingleCell(c);
				resolveParams.singlePawnLord = singlePawnLord;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
			ThingDef thingDef2 = TurretGunUtility.TryFindRandomShellDef(thingDef, allowEMP: false, allowToxGas: false, mustHarmHealth: true, faction.def.techLevel, allowAntigrainWarhead: false, 250f, faction);
			if (thingDef2 != null)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.faction = faction;
				resolveParams2.singleThingDef = thingDef2;
				resolveParams2.singleThingStackCount = Rand.RangeInclusive(5, Mathf.Min(8, thingDef2.stackLimit));
				BaseGen.symbolStack.Push("thing", resolveParams2);
			}
			ResolveParams resolveParams3 = rp;
			resolveParams3.faction = faction;
			resolveParams3.singleThingDef = thingDef;
			resolveParams3.rect = CellRect.SingleCell(cell);
			resolveParams3.thingRot = rot;
			BaseGen.symbolStack.Push("thing", resolveParams3);
		}
	}

	private bool TryFindMortarSpawnCell(CellRect rect, Rot4 rot, ThingDef mortarDef, out IntVec3 cell)
	{
		Map map = BaseGen.globalSettings.map;
		Predicate<CellRect> edgeTouchCheck;
		if (rot == Rot4.North)
		{
			edgeTouchCheck = (CellRect x) => x.Cells.Any((IntVec3 y) => y.z == rect.maxZ);
		}
		else if (rot == Rot4.South)
		{
			edgeTouchCheck = (CellRect x) => x.Cells.Any((IntVec3 y) => y.z == rect.minZ);
		}
		else if (rot == Rot4.West)
		{
			edgeTouchCheck = (CellRect x) => x.Cells.Any((IntVec3 y) => y.x == rect.minX);
		}
		else
		{
			edgeTouchCheck = (CellRect x) => x.Cells.Any((IntVec3 y) => y.x == rect.maxX);
		}
		return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 x)
		{
			CellRect obj = GenAdj.OccupiedRect(x, rot, mortarDef.size);
			if (!ThingUtility.InteractionCellWhenAt(mortarDef, x, rot, map).Standable(map))
			{
				return false;
			}
			return obj.FullyContainedWithin(rect) && edgeTouchCheck(obj);
		}, out cell);
	}
}
