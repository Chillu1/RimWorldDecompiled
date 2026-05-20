namespace RimWorld;

public class CompProperties_UseEffectGiveQuest : CompProperties_UseEffect
{
	public QuestScriptDef quest;

	public bool sendLetterQuestAvailable = true;

	public bool skipIfMechlinkAlreadySentMechs;

	public bool? discovered;

	public CompProperties_UseEffectGiveQuest()
	{
		compClass = typeof(CompUseEffect_GiveQuest);
	}
}
