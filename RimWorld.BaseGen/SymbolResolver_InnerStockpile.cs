using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_InnerStockpile : SymbolResolver
{
	private const int DefaultSize = 3;

	public override void Resolve(ResolveParams rp)
	{
		CellRect rect;
		if (rp.innerStockpileSize.HasValue)
		{
			if (!TryFindPerfectPlaceThenBest(rp.rect, rp.innerStockpileSize.Value, out rect))
			{
				return;
			}
		}
		else if (rp.stockpileConcreteContents != null)
		{
			int num = Mathf.CeilToInt(Mathf.Sqrt(rp.stockpileConcreteContents.Count));
			if (!TryFindRandomInnerRect(rp.rect, num, out rect, num * num, out var _))
			{
				rect = rp.rect;
			}
		}
		else if (!TryFindPerfectPlaceThenBest(rp.rect, 3, out rect))
		{
			return;
		}
		ResolveParams resolveParams = rp;
		resolveParams.rect = rect;
		BaseGen.symbolStack.Push("stockpile", resolveParams);
	}

	private bool TryFindPerfectPlaceThenBest(CellRect outerRect, int size, out CellRect rect)
	{
		if (!TryFindRandomInnerRect(outerRect, size, out rect, size * size, out var maxValidCellsFound))
		{
			if (maxValidCellsFound == 0)
			{
				return false;
			}
			if (!TryFindRandomInnerRect(outerRect, size, out rect, maxValidCellsFound, out var _))
			{
				return false;
			}
		}
		return true;
	}

	private bool TryFindRandomInnerRect(CellRect outerRect, int size, out CellRect rect, int minValidCells, out int maxValidCellsFound)
	{
		Map map = BaseGen.globalSettings.map;
		size = Mathf.Min(size, Mathf.Min(outerRect.Width, outerRect.Height));
		int maxValidCellsFoundLocal = 0;
		bool result = outerRect.TryFindRandomInnerRect(new IntVec2(size, size), out rect, delegate(CellRect x)
		{
			int num = 0;
			foreach (IntVec3 item in x)
			{
				if (item.Standable(map) && item.GetFirstItem(map) == null && item.GetFirstBuilding(map) == null)
				{
					num++;
				}
			}
			maxValidCellsFoundLocal = Mathf.Max(maxValidCellsFoundLocal, num);
			return num >= minValidCells;
		});
		maxValidCellsFound = maxValidCellsFoundLocal;
		return result;
	}
}
