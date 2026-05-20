using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_GravShip : QuestNode
{
	private const int MechhiveWarningDelayTicks = 2700000;

	private List<QuestScriptDef> subquestDefs = new List<QuestScriptDef>();

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		string inSignalEnable = QuestGen.slate.Get<string>("inSignal");
		QuestPart_SubquestGenerator_Gravcores questPart_SubquestGenerator_Gravcores = new QuestPart_SubquestGenerator_Gravcores
		{
			inSignalEnable = inSignalEnable,
			maxActiveSubquests = 3
		};
		questPart_SubquestGenerator_Gravcores.subquestDefs.AddRange(subquestDefs);
		quest.AddPart(questPart_SubquestGenerator_Gravcores);
		if (Faction.OfMechanoids != null)
		{
			quest.Delay(2700000, delegate
			{
				quest.Letter(LetterDefOf.NeutralEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMechhiveWarning".Translate(), text: "LetterTextMechhiveWarning".Translate());
			});
		}
	}
}
