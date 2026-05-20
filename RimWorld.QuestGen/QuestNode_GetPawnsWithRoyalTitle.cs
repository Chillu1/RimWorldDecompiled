using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetPawnsWithRoyalTitle : QuestNode
{
	public SlateRef<List<Pawn>> pawns;

	[NoTranslate]
	public SlateRef<string> storeAs;

	[NoTranslate]
	public SlateRef<string> storeCountAs;

	[NoTranslate]
	public SlateRef<string> storePawnsLabelAs;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) == null)
		{
			return;
		}
		IEnumerable<Pawn> filteredPawns = GetFilteredPawns(pawns.GetValue(slate));
		slate.Set(storeAs.GetValue(slate), filteredPawns);
		if (storeCountAs.GetValue(slate) != null)
		{
			slate.Set(storeCountAs.GetValue(slate), filteredPawns.Count());
		}
		if (storePawnsLabelAs.GetValue(slate) != null)
		{
			slate.Set(storePawnsLabelAs.GetValue(slate), filteredPawns.Select((Pawn p) => p.LabelNoCountColored.Resolve()).ToCommaList(useAnd: true));
		}
	}

	private IEnumerable<Pawn> GetFilteredPawns(List<Pawn> pawns)
	{
		_ = QuestGen.slate;
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].royalty != null && pawns[i].royalty.AllTitlesInEffectForReading.Any())
			{
				yield return pawns[i];
			}
		}
	}
}
