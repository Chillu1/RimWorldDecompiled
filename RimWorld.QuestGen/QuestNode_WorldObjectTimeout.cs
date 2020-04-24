using RimWorld.Planet;

namespace RimWorld.QuestGen
{
	public class QuestNode_WorldObjectTimeout : QuestNode_Delay
	{
		public SlateRef<WorldObject> worldObject;

		protected override QuestPart_Delay MakeDelayQuestPart()
		{
			return new QuestPart_WorldObjectTimeout
			{
				worldObject = worldObject.GetValue(QuestGen.slate)
			};
		}
	}
}
