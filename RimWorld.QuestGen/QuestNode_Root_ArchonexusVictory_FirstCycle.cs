namespace RimWorld.QuestGen
{
	public class QuestNode_Root_ArchonexusVictory_FirstCycle : QuestNode_Root_ArchonexusVictory_Cycle
	{
		protected override int ArchonexusCycle => 1;

		protected override void RunInt()
		{
			base.RunInt();
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Faction faction = slate.Get<Faction>("civilOutlander");
			if (faction != null)
			{
				quest.RequirementsToAcceptFactionRelation(faction, FactionRelationKind.Ally, acceptIfDefeated: true);
			}
			PickNewColony(faction, WorldObjectDefOf.Settlement_SecondArchonexusCycle, SoundDefOf.GameStartSting_FirstArchonexusCycle);
			slate.Set("factionless", faction == null);
		}
	}
}
