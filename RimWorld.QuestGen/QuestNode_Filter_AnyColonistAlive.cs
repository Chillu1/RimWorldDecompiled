using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Filter_AnyColonistAlive : QuestNode_Filter
	{
		public SlateRef<Map> map;

		protected override QuestPart_Filter MakeFilterQuestPart()
		{
			Slate slate = QuestGen.slate;
			Map map = this.map.GetValue(slate) ?? slate.Get<Map>("map");
			return new QuestPart_Filter_AnyColonistAlive
			{
				mapParent = map.Parent
			};
		}
	}
}
