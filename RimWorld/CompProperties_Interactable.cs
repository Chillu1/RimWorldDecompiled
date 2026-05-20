using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Interactable : CompProperties
{
	public int cooldownTicks;

	public int activeTicks;

	public int ticksToActivate;

	public int cooldownFleckSpawnIntervalTicks;

	public float cooldownFleckScale = 1f;

	public bool cooldownPreventsRefuel;

	public bool maintainProgress;

	public bool showMustBeActivatedByColonist = true;

	public bool forceNormalSpeedOnInteracting;

	public bool remainingSecondsInInspectString;

	public bool ignoreForbidden;

	public bool requiresPower;

	public TargetingParameters targetingParameters;

	public StatDef activateStat;

	public FleckDef fleckOnUsed;

	public float fleckOnUsedScale;

	public FleckDef cooldownFleck;

	public SoundDef soundActivate;

	public EffecterDef interactionEffecter;

	[MustTranslate]
	public string jobString;

	[MustTranslate]
	public string activatingStringPending;

	[MustTranslate]
	public string activatingString;

	[MustTranslate]
	public string activateLabelString;

	[MustTranslate]
	public string activateDescString;

	[MustTranslate]
	public string guiLabelString;

	[MustTranslate]
	public string onCooldownString;

	[MustTranslate]
	public string inspectString;

	[MustTranslate]
	public string messageCompletedString;

	[MustTranslate]
	public string messageCooldownEnded;

	[NoTranslate]
	public string activateTexPath;

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (compClass != null && !typeof(CompInteractable).IsAssignableFrom(compClass))
		{
			yield return parentDef.defName + " has compClass but is not subclass of CompInteractable.";
		}
		if (activateTexPath.NullOrEmpty())
		{
			yield return parentDef.defName + " has no activate texture.";
		}
	}
}
