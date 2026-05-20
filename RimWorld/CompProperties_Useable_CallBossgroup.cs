using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class CompProperties_Useable_CallBossgroup : CompProperties_UseEffect
{
	public BossgroupDef bossgroupDef;

	public EffecterDef effecterDef;

	public EffecterDef prepareEffecterDef;

	[NoTranslate]
	public string spawnLetterTextKey;

	[NoTranslate]
	public string spawnLetterLabelKey;

	[NoTranslate]
	public string unlockedLetterTextKey;

	[NoTranslate]
	public string unlockedLetterLabelKey;

	public int delayTicks = -1;

	private List<ThingDef> tmpMechsUsingRewards = new List<ThingDef>();

	public CompProperties_Useable_CallBossgroup()
	{
		compClass = typeof(CompUseEffect_CallBossgroup);
	}

	public override void Notify_PostUnlockedByResearch(ThingDef parent)
	{
		if (Find.TickManager.TicksGame > 0 && !unlockedLetterLabelKey.NullOrEmpty() && !unlockedLetterTextKey.NullOrEmpty())
		{
			SendBossgroupDetailsLetter(unlockedLetterLabelKey, unlockedLetterTextKey, parent);
		}
	}

	public void SendBossgroupDetailsLetter(string labelKey, string textKey, ThingDef parent)
	{
		List<ThingDef> list = new List<ThingDef> { parent };
		list.AddRange(bossgroupDef.boss.kindDef.race.killedLeavingsPlayerHostile.Select((ThingDefCountClass t) => t.thingDef));
		Find.LetterStack.ReceiveLetter(FormatLetterLabel(labelKey), FormatLetterText(textKey, parent), LetterDefOf.NeutralEvent, null, null, null, list);
	}

	public string FormatLetterLabel(string label)
	{
		return label.Translate(NamedArgumentUtility.Named(bossgroupDef.boss.kindDef, "LEADER"));
	}

	public string FormatLetterText(string text, ThingDef parent)
	{
		string arg = bossgroupDef.boss.kindDef.race.killedLeavingsPlayerHostile.Select((ThingDefCountClass r) => r.Label + " x" + r.count).ToLineList("- ");
		return text.Translate(NamedArgumentUtility.Named(parent, "PARENT"), NamedArgumentUtility.Named(bossgroupDef.boss.kindDef, "LEADER"), arg.Named("REWARDSLIST"));
	}
}
