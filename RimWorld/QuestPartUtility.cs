using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public static class QuestPartUtility
	{
		public static T MakeAndAddEndCondition<T>(Quest quest, string inSignalActivate, QuestEndOutcome outcome, Letter letter = null) where T : QuestPartActivable, new()
		{
			T val = new T();
			val.inSignalEnable = inSignalActivate;
			quest.AddPart(val);
			if (letter != null)
			{
				QuestPart_Letter questPart_Letter = new QuestPart_Letter();
				questPart_Letter.letter = letter;
				questPart_Letter.inSignal = val.OutSignalCompleted;
				quest.AddPart(questPart_Letter);
			}
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = val.OutSignalCompleted;
			questPart_QuestEnd.outcome = outcome;
			quest.AddPart(questPart_QuestEnd);
			return val;
		}

		public static QuestPart_QuestEnd MakeAndAddEndNodeWithLetter(Quest quest, string inSignalActivate, QuestEndOutcome outcome, Letter letter)
		{
			QuestPart_Letter questPart_Letter = new QuestPart_Letter();
			questPart_Letter.letter = letter;
			questPart_Letter.inSignal = inSignalActivate;
			quest.AddPart(questPart_Letter);
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = inSignalActivate;
			questPart_QuestEnd.outcome = outcome;
			quest.AddPart(questPart_QuestEnd);
			return questPart_QuestEnd;
		}

		public static QuestPart_Delay MakeAndAddQuestTimeoutDelay(Quest quest, int delayTicks, WorldObject worldObject)
		{
			QuestPart_WorldObjectTimeout questPart_WorldObjectTimeout = new QuestPart_WorldObjectTimeout();
			questPart_WorldObjectTimeout.delayTicks = delayTicks;
			questPart_WorldObjectTimeout.expiryInfoPart = "QuestExpiresIn".Translate();
			questPart_WorldObjectTimeout.expiryInfoPartTip = "QuestExpiresOn".Translate();
			questPart_WorldObjectTimeout.isBad = true;
			questPart_WorldObjectTimeout.outcomeCompletedSignalArg = QuestEndOutcome.Fail;
			questPart_WorldObjectTimeout.inSignalEnable = quest.InitiateSignal;
			quest.AddPart(questPart_WorldObjectTimeout);
			string text = "Quest" + quest.id + ".DelayingWorldObject";
			QuestUtility.AddQuestTag(ref worldObject.questTags, text);
			questPart_WorldObjectTimeout.inSignalDisable = text + ".MapGenerated";
			QuestPart_QuestEnd questPart_QuestEnd = new QuestPart_QuestEnd();
			questPart_QuestEnd.inSignal = questPart_WorldObjectTimeout.OutSignalCompleted;
			quest.AddPart(questPart_QuestEnd);
			return questPart_WorldObjectTimeout;
		}
	}
}
