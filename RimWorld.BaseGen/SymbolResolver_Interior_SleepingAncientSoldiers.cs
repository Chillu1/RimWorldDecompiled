using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_SleepingAncientSoldiers : SymbolResolver
{
	private static List<CellRect> tmpCellRects = new List<CellRect>();

	public override bool CanResolve(ResolveParams rp)
	{
		if (base.CanResolve(rp) && rp.threatPoints.HasValue)
		{
			return rp.threatPoints.Value >= PawnKindDefOf.AncientSoldier.combatPower;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		int num = Mathf.RoundToInt(rp.threatPoints.Value / PawnKindDefOf.AncientSoldier.combatPower);
		tmpCellRects.Clear();
		ThingDef ancientCryptosleepCasket = ThingDefOf.AncientCryptosleepCasket;
		CellRect cellRect = new CellRect(0, 0, ancientCryptosleepCasket.size.x, ancientCryptosleepCasket.size.z).ExpandedBy(1);
		for (int i = 0; i < num; i++)
		{
			if (rp.rect.TryFindRandomInnerRect(new IntVec2(cellRect.Width, cellRect.Height), out var rect, (CellRect other) => !tmpCellRects.Any((CellRect r) => r.Overlaps(other))))
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = rect.ExpandedBy(-1);
				resolveParams.thingRot = ancientCryptosleepCasket.defaultPlacingRot;
				resolveParams.podContentsType = PodContentsType.AncientHostile;
				resolveParams.faction = Faction.OfAncientsHostile;
				BaseGen.symbolStack.Push("ancientCryptosleepCasket", resolveParams);
				tmpCellRects.Add(rect);
			}
		}
		tmpCellRects.Clear();
	}
}
