using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_GameCondition : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<GameConditionDef> gameCondition;

	public SlateRef<bool> targetWorld;

	public SlateRef<int> duration;

	[NoTranslate]
	public SlateRef<string> storeGameConditionDescriptionFutureAs;

	private static Map GetMap(Slate slate)
	{
		if (!slate.TryGet<Map>("map", out var var))
		{
			return Find.RandomPlayerHomeMap;
		}
		return var;
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!targetWorld.GetValue(slate) && GetMap(slate) == null)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		float points = QuestGen.slate.Get("points", 0f);
		GameCondition gameCondition = GameConditionMaker.MakeCondition(this.gameCondition.GetValue(slate), duration.GetValue(slate));
		QuestPart_GameCondition questPart_GameCondition = new QuestPart_GameCondition();
		questPart_GameCondition.gameCondition = gameCondition;
		List<Rule> list = new List<Rule>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (targetWorld.GetValue(slate))
		{
			questPart_GameCondition.targetWorld = true;
			gameCondition.RandomizeSettings(points, null, list, dictionary);
		}
		else
		{
			Map map = GetMap(QuestGen.slate);
			questPart_GameCondition.mapParent = map.Parent;
			gameCondition.RandomizeSettings(points, map, list, dictionary);
		}
		questPart_GameCondition.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		QuestGen.quest.AddPart(questPart_GameCondition);
		if (!storeGameConditionDescriptionFutureAs.GetValue(slate).NullOrEmpty())
		{
			slate.Set(storeGameConditionDescriptionFutureAs.GetValue(slate), gameCondition.def.descriptionFuture);
		}
		QuestGen.AddQuestNameRules(list);
		QuestGen.AddQuestDescriptionRules(list);
		QuestGen.AddQuestDescriptionConstants(dictionary);
	}
}
