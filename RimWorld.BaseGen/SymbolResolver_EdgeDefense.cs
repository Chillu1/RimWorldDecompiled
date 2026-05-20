using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_EdgeDefense : SymbolResolver
{
	private const int DefaultCellsPerTurret = 30;

	private const int DefaultCellsPerMortar = 75;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
		int valueOrDefault = rp.edgeDefenseGuardsCount.GetValueOrDefault();
		int width;
		if (rp.edgeDefenseWidth.HasValue)
		{
			width = rp.edgeDefenseWidth.Value;
		}
		else if (rp.edgeDefenseMortarsCount.HasValue && rp.edgeDefenseMortarsCount.Value > 0)
		{
			width = 4;
		}
		else
		{
			width = (Rand.Bool ? 2 : 4);
		}
		width = Mathf.Clamp(width, 1, Mathf.Min(rp.rect.Width, rp.rect.Height) / 2);
		int num;
		int num2;
		bool flag;
		bool flag2;
		bool flag3;
		switch (width)
		{
		case 1:
			num = rp.edgeDefenseTurretsCount.GetValueOrDefault();
			num2 = 0;
			flag = false;
			flag2 = true;
			flag3 = true;
			break;
		case 2:
			num = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
			num2 = 0;
			flag = false;
			flag2 = false;
			flag3 = true;
			break;
		case 3:
			num = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
			num2 = rp.edgeDefenseMortarsCount ?? (rp.rect.EdgeCellsCount / 75);
			flag = num2 == 0;
			flag2 = false;
			flag3 = true;
			break;
		default:
			num = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
			num2 = rp.edgeDefenseMortarsCount ?? (rp.rect.EdgeCellsCount / 75);
			flag = true;
			flag2 = false;
			flag3 = false;
			break;
		}
		if (faction != null && (int)faction.def.techLevel < 4)
		{
			num = 0;
			num2 = 0;
		}
		if (valueOrDefault > 0 && rp.settlementDontGeneratePawns != true)
		{
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell, 180000), map);
			for (int i = 0; i < valueOrDefault; i++)
			{
				PawnGenerationRequest value = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true);
				ResolveParams resolveParams = rp;
				resolveParams.faction = faction;
				resolveParams.singlePawnLord = singlePawnLord;
				resolveParams.singlePawnGenerationRequest = value;
				resolveParams.singlePawnSpawnCellExtraPredicate = resolveParams.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)delegate(IntVec3 x)
				{
					CellRect cellRect = rp.rect;
					for (int j = 0; j < width; j++)
					{
						if (cellRect.IsOnEdge(x))
						{
							return true;
						}
						cellRect = cellRect.ContractedBy(1);
					}
					return true;
				});
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
		CellRect rect = rp.rect;
		for (int num3 = 0; num3 < width; num3++)
		{
			if (num3 % 2 == 0)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.faction = faction;
				resolveParams2.rect = rect;
				BaseGen.symbolStack.Push("edgeSandbags", resolveParams2);
				if (!flag)
				{
					break;
				}
			}
			rect = rect.ContractedBy(1);
		}
		if (rp.settlementDontGeneratePawns != true)
		{
			CellRect rect2 = (flag3 ? rp.rect : rp.rect.ContractedBy(1));
			for (int num4 = 0; num4 < num2; num4++)
			{
				ResolveParams resolveParams3 = rp;
				resolveParams3.faction = faction;
				resolveParams3.rect = rect2;
				BaseGen.symbolStack.Push("edgeMannedMortar", resolveParams3);
			}
		}
		CellRect rect3 = (flag2 ? rp.rect : rp.rect.ContractedBy(1));
		for (int num5 = 0; num5 < num; num5++)
		{
			ResolveParams resolveParams4 = rp;
			resolveParams4.faction = faction;
			resolveParams4.singleThingDef = ThingDefOf.Turret_MiniTurret;
			resolveParams4.rect = rect3;
			resolveParams4.edgeThingAvoidOtherEdgeThings = rp.edgeThingAvoidOtherEdgeThings ?? true;
			BaseGen.symbolStack.Push("edgeThing", resolveParams4);
		}
	}
}
