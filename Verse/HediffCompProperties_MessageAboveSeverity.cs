namespace Verse;

public class HediffCompProperties_MessageAboveSeverity : HediffCompProperties
{
	public float severity;

	public MessageTypeDef messageType;

	[MustTranslate]
	public string message;

	public HediffCompProperties_MessageAboveSeverity()
	{
		compClass = typeof(HediffComp_MessageAboveSeverity);
	}
}
