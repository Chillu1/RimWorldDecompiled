using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_ArchonexusVictory : QuestNode
{
	public Faction CivilOutlander => Find.FactionManager.AllFactionsVisible.FirstOrDefault((Faction f) => f.def == FactionDefOf.OutlanderCivil);

	public Faction RoughOutlander => Find.FactionManager.AllFactionsVisible.FirstOrDefault((Faction f) => f.def == FactionDefOf.OutlanderRough);

	public Faction RoughTribe => Find.FactionManager.AllFactionsVisible.FirstOrDefault((Faction f) => f.def == FactionDefOf.TribeRough);

	protected override void RunInt()
	{
		if (ModLister.CheckIdeology("Archonexus victory"))
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			quest.AddPart(new QuestPart_SubquestGenerator_ArchonexusVictory
			{
				inSignalEnable = slate.Get<string>("inSignal"),
				interval = new IntRange(0, 0),
				maxSuccessfulSubquests = 3,
				maxActiveSubquests = 1,
				civilOutlander = CivilOutlander,
				roughOutlander = RoughOutlander,
				roughTribe = RoughTribe,
				subquestDefs = 
				{
					QuestScriptDefOf.EndGame_ArchonexusVictory_FirstCycle,
					QuestScriptDefOf.EndGame_ArchonexusVictory_SecondCycle,
					QuestScriptDefOf.EndGame_ArchonexusVictory_ThirdCycle
				}
			});
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
