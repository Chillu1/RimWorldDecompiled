namespace Verse;

public class HediffCompProperties_Disappears : HediffCompProperties
{
	public IntRange disappearsAfterTicks;

	public bool showRemainingTime;

	public bool canUseDecimalsShortForm;

	public MentalStateDef requiredMentalState;

	[MustTranslate]
	public string messageOnDisappear;

	[MustTranslate]
	public string letterTextOnDisappear;

	[MustTranslate]
	public string letterLabelOnDisappear;

	public bool sendLetterOnDisappearIfDead = true;

	public bool leaveFreshWounds = true;

	public HediffCompProperties_Disappears()
	{
		compClass = typeof(HediffComp_Disappears);
	}
}
