namespace RimWorld.QuestGen
{
	public static class QuestGen_Requirements
	{
		public static QuestPart_RequirementsToAcceptFactionRelation RequirementsToAcceptFactionRelation(this Quest quest, Faction faction, FactionRelationKind relationKind, bool acceptIfDefeated = false)
		{
			QuestPart_RequirementsToAcceptFactionRelation questPart_RequirementsToAcceptFactionRelation = new QuestPart_RequirementsToAcceptFactionRelation();
			questPart_RequirementsToAcceptFactionRelation.otherFaction = faction;
			questPart_RequirementsToAcceptFactionRelation.relationKind = relationKind;
			questPart_RequirementsToAcceptFactionRelation.acceptIfDefeated = acceptIfDefeated;
			quest.AddPart(questPart_RequirementsToAcceptFactionRelation);
			return questPart_RequirementsToAcceptFactionRelation;
		}
	}
}
