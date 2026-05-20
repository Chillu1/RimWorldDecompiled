using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_Carry : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.inventoryStock != null)
		{
			float num = rect.width - 4f;
			int num2 = Mathf.FloorToInt(num * (1f / 3f));
			float x = rect.x;
			InventoryStockGroupDef group = InventoryStockGroupDefOf.Medicine;
			Widgets.Dropdown(new Rect(x, rect.y + 2f, num2, rect.height - 4f), pawn, (Pawn p) => p.inventoryStock.GetDesiredThingForGroup(group), (Pawn p) => GenerateThingButtons(p, group), null, pawn.inventoryStock.GetDesiredThingForGroup(group).uiIcon, null, null, null, paintable: true);
			Widgets.Dropdown(new Rect(x + (float)num2 + 4f, width: Mathf.FloorToInt(num * (2f / 3f)), y: rect.y + 2f, height: rect.height - 4f), pawn, (Pawn p) => p.inventoryStock.GetDesiredCountForGroup(group), (Pawn p) => GenerateCountButtons(p, group), pawn.inventoryStock.GetDesiredCountForGroup(group).ToString(), null, null, null, null, paintable: true);
		}
	}

	private IEnumerable<Widgets.DropdownMenuElement<ThingDef>> GenerateThingButtons(Pawn pawn, InventoryStockGroupDef group)
	{
		foreach (ThingDef thing in group.thingDefs)
		{
			yield return new Widgets.DropdownMenuElement<ThingDef>
			{
				option = new FloatMenuOption(thing.LabelCap, delegate
				{
					pawn.inventoryStock.SetThingForGroup(group, thing);
				}, thing),
				payload = thing
			};
		}
	}

	private IEnumerable<Widgets.DropdownMenuElement<int>> GenerateCountButtons(Pawn pawn, InventoryStockGroupDef group)
	{
		for (int i = group.min; i <= group.max; i++)
		{
			int localI = i;
			yield return new Widgets.DropdownMenuElement<int>
			{
				option = new FloatMenuOption(i.ToString(), delegate
				{
					pawn.inventoryStock.SetCountForGroup(group, localI);
				}),
				payload = i
			};
		}
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(54f));
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(Mathf.CeilToInt(104f), GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 65);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private int GetValueToCompare(Pawn pawn)
	{
		if (pawn.inventoryStock != null)
		{
			return pawn.inventoryStock.GetDesiredCountForGroup(InventoryStockGroupDefOf.Medicine);
		}
		return int.MinValue;
	}
}
