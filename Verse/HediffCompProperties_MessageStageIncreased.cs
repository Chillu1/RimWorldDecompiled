namespace Verse;

public class HediffCompProperties_MessageStageIncreased : HediffCompProperties
{
	[MustTranslate]
	public string message;

	public MessageTypeDef messageType;

	public HediffCompProperties_MessageStageIncreased()
	{
		compClass = typeof(HediffComp_MessageStageIncreased);
	}
}
