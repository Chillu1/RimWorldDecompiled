using System.Collections.Generic;

namespace Verse;

public class HediffCompProperties_CauseMentalState : HediffCompProperties
{
	public MentalStateDef animalMentalState;

	public MentalStateDef animalMentalStateAlias;

	public MentalStateDef humanMentalState;

	[MustTranslate]
	public string overrideLetterLabel;

	[MustTranslate]
	public string overrideLetterDesc;

	public LetterDef letterDef;

	public float mtbDaysToCauseMentalState;

	public float minSeverity;

	public bool endMentalStateOnCure = true;

	public bool removeOnTriggered;

	public bool forced;

	public HediffCompProperties_CauseMentalState()
	{
		compClass = typeof(HediffComp_CauseMentalState);
	}

	public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (endMentalStateOnCure && removeOnTriggered)
		{
			yield return "hediff comp cause mental state has both endMentalStateOnCure and removeOnTriggered enabled which are mutually exclusive. Did you forget to disable endMentalStateOnCure?";
		}
	}
}
