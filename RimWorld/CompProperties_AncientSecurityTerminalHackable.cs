using Verse;

namespace RimWorld;

public class CompProperties_AncientSecurityTerminalHackable : CompProperties_Hackable
{
	[MustTranslate]
	public string letterTurretDisabledLabel;

	[MustTranslate]
	public string letterTurretDisabledText;

	[MustTranslate]
	public string letterTurretHackedLabel;

	[MustTranslate]
	public string letterTurretHackedText;

	[MustTranslate]
	public string letterDoorOpenedLabel;

	[MustTranslate]
	public string letterDoorOpenedText;

	[MustTranslate]
	public string messageNoValidTarget;

	public CompProperties_AncientSecurityTerminalHackable()
	{
		compClass = typeof(CompAncientSecurityTerminal);
	}
}
