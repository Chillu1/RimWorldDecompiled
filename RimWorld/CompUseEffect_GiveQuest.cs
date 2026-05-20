using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class CompUseEffect_GiveQuest : CompUseEffect
{
	public CompProperties_UseEffectGiveQuest Props => (CompProperties_UseEffectGiveQuest)props;

	public override void DoEffect(Pawn user)
	{
		if (!Props.skipIfMechlinkAlreadySentMechs || !(parent is Mechlink { sentMechsToPlayer: not false }))
		{
			Slate slate = new Slate();
			slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(user.Map));
			slate.Set("asker", user);
			slate.Set("map", user.Map);
			if (Props.discovered.HasValue)
			{
				slate.Set("discovered", Props.discovered.Value);
			}
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(Props.quest, slate);
			if (!quest.hidden && quest.root.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest);
			}
		}
	}
}
