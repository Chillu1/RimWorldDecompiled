using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Infestation : QuestPart
{
	public string inSignal;

	public int hivesCount;

	public MapParent mapParent;

	public string tag;

	public IntVec3? overrideLoc;

	public string customLetterText;

	public string customLetterLabel;

	public LetterDef customLetterDef;

	public bool sendStandardLetter = true;

	private IntVec3 loc = IntVec3.Invalid;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
			if (mapParent != null && mapParent.HasMap && loc.IsValid)
			{
				yield return new TargetInfo(loc, mapParent.Map);
			}
		}
	}

	public override string QuestSelectTargetsLabel => "SelectHiveTargets".Translate();

	public override IEnumerable<GlobalTargetInfo> QuestSelectTargets
	{
		get
		{
			foreach (GlobalTargetInfo questSelectTarget in base.QuestSelectTargets)
			{
				yield return questSelectTarget;
			}
			if (mapParent == null || !mapParent.HasMap)
			{
				yield break;
			}
			List<Thing> hives = mapParent.Map.listerThings.ThingsOfDef(ThingDefOf.Hive);
			for (int i = 0; i < hives.Count; i++)
			{
				if (hives[i] is Hive hive && !hive.questTags.NullOrEmpty() && hive.questTags.Contains(tag))
				{
					yield return hive;
				}
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		loc = IntVec3.Invalid;
		if (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent))
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget() ?? Find.AnyPlayerHomeMap?.Parent;
		}
		if (mapParent == null || !mapParent.HasMap)
		{
			return;
		}
		Thing thing = InfestationUtility.SpawnTunnels(hivesCount, mapParent.Map, spawnAnywhereIfNoGoodCell: true, ignoreRoofedRequirement: true, tag, overrideLoc);
		if (thing != null)
		{
			loc = overrideLoc ?? thing.Position;
			if (sendStandardLetter)
			{
				TaggedString label = (customLetterLabel.NullOrEmpty() ? ((TaggedString)IncidentDefOf.Infestation.letterLabel) : customLetterLabel.Formatted(IncidentDefOf.Infestation.letterLabel.Named("BASELABEL")));
				TaggedString text = (customLetterText.NullOrEmpty() ? ((TaggedString)IncidentDefOf.Infestation.letterText) : customLetterText.Formatted(IncidentDefOf.Infestation.letterText.Named("BASETEXT")));
				Find.LetterStack.ReceiveLetter(label, text, customLetterDef ?? IncidentDefOf.Infestation.letterDef, new TargetInfo(loc, mapParent.Map), null, quest);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref hivesCount, "hivesCount", 0);
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
		Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
		Scribe_Values.Look(ref loc, "loc");
		Scribe_Values.Look(ref tag, "tag");
		Scribe_Values.Look(ref overrideLoc, "overrideLoc");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && !quest.Historical && (mapParent == null || !mapParent.HasMap))
		{
			Log.Warning($"Quest {quest} loaded with quest part {this} with invalid map and no fallback map. Ending quest.");
			quest.End(QuestEndOutcome.Unknown, sendLetter: false);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			mapParent = Find.RandomPlayerHomeMap.Parent;
			hivesCount = 5;
		}
	}
}
