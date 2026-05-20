using Verse;

namespace RimWorld;

public class CompProperties_HoldingPlatformTarget : CompProperties
{
	public PawnKindDef heldPawnKind;

	[MustTranslate]
	public string capturedLetterLabel;

	[MustTranslate]
	public string capturedLetterText;

	public float baseEscapeIntervalMtbDays = 60f;

	public bool lookForTargetOnEscape = true;

	public bool canBeExecuted = true;

	public bool getsColdContainmentBonus;

	public bool hasAnimation = true;

	public CompProperties_HoldingPlatformTarget()
	{
		compClass = typeof(CompHoldingPlatformTarget);
	}
}
