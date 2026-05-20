using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class MainTabWindow_Animals : MainTabWindow_PawnTable
{
	protected override PawnTableDef PawnTableDef => PawnTableDefOf.Animals;

	protected override IEnumerable<Pawn> Pawns => Find.CurrentMap.mapPawns.ColonyAnimals;

	public override void DoWindowContents(Rect rect)
	{
		base.DoWindowContents(rect);
		if (Widgets.ButtonText(new Rect(rect.x, rect.y, Mathf.Min(rect.width, 260f), 32f), "ManageAutoSlaughter".Translate()))
		{
			Find.WindowStack.Add(new Dialog_AutoSlaughter(Find.CurrentMap));
		}
	}
}
