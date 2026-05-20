using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_Hackable : CompProperties
{
	public float defence;

	public EffecterDef effectHacking;

	public QuestScriptDef completedQuest;

	public bool glowIfHacked;

	public SoundDef hackingCompletedSound;

	public IntRange lockoutDurationHoursRange = IntRange.Zero;

	public bool showProgressAfterHackCompletion = true;

	public bool lockoutPermanently;

	public bool onlyRemotelyHackable;

	public int intellectualSkillPrerequisite;

	public List<ThingDefCountClass> dropOnHacked = new List<ThingDefCountClass>();

	public List<ThingDefCountClass> dropOnDestroyed = new List<ThingDefCountClass>();

	[MustTranslate]
	public string notHackedInspectString;

	[MustTranslate]
	public string hackedInspectString;

	[MustTranslate]
	public string hackedMessage;

	[MustTranslate]
	public string autohackWarningString;

	[MustTranslate]
	public string hackedLetterLabel;

	[MustTranslate]
	public string hackedLetterText;

	public LetterDef hackedLetterDef;

	[MustTranslate]
	public string destroyedLetterLabel;

	[MustTranslate]
	public string destroyedLetterText;

	public LetterDef destroyedLetterDef;

	public GraphicData unhackedGraphicData;

	public GraphicData hackedGraphicData;

	public Vector3 unhackedGraphicOffset;

	public Vector3 hackedGraphicOffset;

	public DrawerType graphicDrawerType = DrawerType.MapMeshOnly;

	public CompProperties_Hackable()
	{
		compClass = typeof(CompHackable);
	}
}
