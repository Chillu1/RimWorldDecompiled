using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EdgeDefense : SymbolResolver
	{
		private const int DefaultCellsPerTurret = 30;

		private const int DefaultCellsPerMortar = 75;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
			int num = rp.edgeDefenseGuardsCount ?? 0;
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
			int num2;
			int num3;
			bool flag;
			bool flag2;
			bool flag3;
			switch (width)
			{
			case 1:
				num2 = rp.edgeDefenseTurretsCount ?? 0;
				num3 = 0;
				flag = false;
				flag2 = true;
				flag3 = true;
				break;
			case 2:
				num2 = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
				num3 = 0;
				flag = false;
				flag2 = false;
				flag3 = true;
				break;
			case 3:
				num2 = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
				num3 = rp.edgeDefenseMortarsCount ?? (rp.rect.EdgeCellsCount / 75);
				flag = num3 == 0;
				flag2 = false;
				flag3 = true;
				break;
			default:
				num2 = rp.edgeDefenseTurretsCount ?? (rp.rect.EdgeCellsCount / 30);
				num3 = rp.edgeDefenseMortarsCount ?? (rp.rect.EdgeCellsCount / 75);
				flag = true;
				flag2 = false;
				flag3 = false;
				break;
			}
			if (faction != null && (int)faction.def.techLevel < 4)
			{
				num2 = 0;
				num3 = 0;
			}
			if (num > 0)
			{
				Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map);
				for (int i = 0; i < num; i++)
				{
					PawnGenerationRequest value = new PawnGenerationRequest(faction.RandomPawnKind(), faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true);
					ResolveParams resolveParams = rp;
					resolveParams.faction = faction;
					resolveParams.singlePawnLord = singlePawnLord;
					resolveParams.singlePawnGenerationRequest = value;
					resolveParams.singlePawnSpawnCellExtraPredicate = resolveParams.singlePawnSpawnCellExtraPredicate ?? ((Predicate<IntVec3>)delegate(IntVec3 x)
					{
						CellRect cellRect = rp.rect;
						for (int m = 0; m < width; m++)
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
			for (int j = 0; j < width; j++)
			{
				if (j % 2 == 0)
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
			CellRect rect2 = (flag3 ? rp.rect : rp.rect.ContractedBy(1));
			for (int k = 0; k < num3; k++)
			{
				ResolveParams resolveParams3 = rp;
				resolveParams3.faction = faction;
				resolveParams3.rect = rect2;
				BaseGen.symbolStack.Push("edgeMannedMortar", resolveParams3);
			}
			CellRect rect3 = (flag2 ? rp.rect : rp.rect.ContractedBy(1));
			for (int l = 0; l < num2; l++)
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
}
