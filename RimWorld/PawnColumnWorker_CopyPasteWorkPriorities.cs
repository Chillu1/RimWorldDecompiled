using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnColumnWorker_CopyPasteWorkPriorities : PawnColumnWorker_CopyPaste
{
	private static DefMap<WorkTypeDef, int> clipboard;

	protected override bool AnythingInClipboard => clipboard != null;

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (!pawn.Dead && pawn.workSettings != null && pawn.workSettings.EverWork)
		{
			base.DoCell(rect, pawn, table);
		}
	}

	protected override void CopyFrom(Pawn p)
	{
		if (clipboard == null)
		{
			clipboard = new DefMap<WorkTypeDef, int>();
		}
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef w = allDefsListForReading[i];
			clipboard[w] = ((!p.WorkTypeIsDisabled(w)) ? p.workSettings.GetPriority(w) : 0);
		}
	}

	protected override void PasteTo(Pawn p)
	{
		List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			WorkTypeDef w = allDefsListForReading[i];
			if (!p.WorkTypeIsDisabled(w))
			{
				p.workSettings.SetPriority(w, clipboard[w]);
			}
		}
	}
}
