using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRelationsInfo : QuestNode
{
	public SlateRef<Pawn> pawn;

	public SlateRef<IEnumerable<Pawn>> otherPawns;

	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<string> nonRelatedLabel;

	public SlateRef<string> nonRelatedLabelPlural;

	private static List<string> tmpRelations = new List<string>();

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		return true;
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		if (pawn.GetValue(slate) == null || otherPawns.GetValue(slate) == null)
		{
			return;
		}
		tmpRelations.Clear();
		int num = 0;
		foreach (Pawn item in otherPawns.GetValue(slate))
		{
			PawnRelationDef mostImportantRelation = pawn.GetValue(slate).GetMostImportantRelation(item);
			if (mostImportantRelation != null)
			{
				tmpRelations.Add(mostImportantRelation.GetGenderSpecificLabel(item));
			}
			else
			{
				num++;
			}
		}
		if (num == 1)
		{
			tmpRelations.Add(nonRelatedLabel.GetValue(slate));
		}
		else if (num >= 2)
		{
			tmpRelations.Add(num + " " + nonRelatedLabelPlural.GetValue(slate));
		}
		if (tmpRelations.Any())
		{
			slate.Set(storeAs.GetValue(slate), tmpRelations.ToCommaList(useAnd: true));
			tmpRelations.Clear();
		}
	}
}
