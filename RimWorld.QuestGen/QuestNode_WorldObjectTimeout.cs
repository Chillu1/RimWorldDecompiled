using RimWorld.Planet;

namespace RimWorld.QuestGen
{
	public class QuestNode_WorldObjectTimeout : QuestNode_Delay
	{
		public SlateRef<bool> destroyOnCleanup;

		public SlateRef<WorldObject> worldObject;

		protected override QuestPart_Delay MakeDelayQuestPart()
		{
			QuestPart_WorldObjectTimeout questPart_WorldObjectTimeout = new QuestPart_WorldObjectTimeout();
			questPart_WorldObjectTimeout.worldObject = worldObject.GetValue(QuestGen.slate);
			destroyOnCleanup.TryGetValue(QuestGen.slate, out questPart_WorldObjectTimeout.destroyOnCleanup);
			return questPart_WorldObjectTimeout;
		}
	}
}
