using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Nociosphere : CompProperties_Interactable
{
	public class AttackDetails
	{
		public AbilityDef ability;

		public bool requiresPawn = true;

		public bool aiPreferArtificial;

		public float maxCooldownTicks = 500f;
	}

	public List<AttackDetails> attacks = new List<AttackDetails>();

	public FloatRange sentOnslaughtDurationSeconds;

	public int minOnslaughtTicks;

	public float activityOnRoofCollapsed = 0.3f;

	[MustTranslate]
	public string onslaughtInspectText;

	[MustTranslate]
	public string becomingUnstableInspectText;

	[MustTranslate]
	public string unstableInspectText;

	[MustTranslate]
	public string unstableWarning;

	[MustTranslate]
	public string leftLetterLabel;

	[MustTranslate]
	public string leftLetterText;

	[MustTranslate]
	public string onslaughtEndedMessage;

	[MustTranslate]
	public string departingMessage;

	public CompProperties_Nociosphere()
	{
		compClass = typeof(CompNociosphere);
	}
}
