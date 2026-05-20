using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddHediff : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<HediffDef> hediffDef;

	public SlateRef<IEnumerable<BodyPartDef>> partsToAffect;

	public SlateRef<bool> checkDiseaseContractChance;

	public SlateRef<bool> addToHyperlinks;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null && hediffDef.GetValue(slate) != null)
		{
			QuestPart_AddHediff questPart_AddHediff = new QuestPart_AddHediff();
			questPart_AddHediff.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_AddHediff.hediffDef = hediffDef.GetValue(slate);
			questPart_AddHediff.pawns.AddRange(pawns.GetValue(slate));
			questPart_AddHediff.checkDiseaseContractChance = checkDiseaseContractChance.GetValue(slate);
			if (partsToAffect.GetValue(slate) != null)
			{
				questPart_AddHediff.partsToAffect = new List<BodyPartDef>();
				questPart_AddHediff.partsToAffect.AddRange(partsToAffect.GetValue(slate));
			}
			questPart_AddHediff.addToHyperlinks = addToHyperlinks.GetValue(slate);
			QuestGen.quest.AddPart(questPart_AddHediff);
		}
	}
}
