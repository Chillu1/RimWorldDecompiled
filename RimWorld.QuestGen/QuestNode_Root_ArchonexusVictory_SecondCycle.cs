namespace RimWorld.QuestGen
{
	public class QuestNode_Root_ArchonexusVictory_SecondCycle : QuestNode_Root_ArchonexusVictory_Cycle
	{
		private const int MaxAllowedRelicsToTake = 2;

		protected override int ArchonexusCycle => 2;

		protected override void RunInt()
		{
			base.RunInt();
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			TryAddStudyRequirement(quest, slate, ThingDefOf.MajorArchotechStructureStudiable);
			quest.DialogWithCloseBehavior("[resolvedQuestDescription]", null, quest.AddedSignal, null, null, QuestPart.SignalListenMode.NotYetAcceptedOnly, QuestPartDialogCloseAction.CloseActionKey.ArchonexusVictorySound2nd);
			Faction faction = slate.Get<Faction>("roughOutlander");
			if (faction != null)
			{
				quest.RequirementsToAcceptFactionRelation(faction, FactionRelationKind.Ally, acceptIfDefeated: true);
			}
			PickNewColony(faction, WorldObjectDefOf.Settlement_ThirdArchonexusCycle, SoundDefOf.GameStartSting_SecondArchonexusCycle, 2);
			slate.Set("factionless", faction == null);
		}
	}
}
