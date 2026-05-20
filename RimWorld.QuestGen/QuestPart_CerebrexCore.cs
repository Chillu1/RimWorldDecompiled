using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_CerebrexCore : QuestPartActivable
{
	private Site site;

	private List<Thing> stabilizers;

	private Thing core;

	private string coreTag;

	private string stabilizerTag;

	private string StabilizerDestroyedSignal => stabilizerTag + ".Destroyed";

	public QuestPart_CerebrexCore()
	{
	}

	public QuestPart_CerebrexCore(Site site, string inSignalEnable, string coreTag, string stabilizerTag)
	{
		this.site = site;
		base.inSignalEnable = inSignalEnable;
		this.coreTag = coreTag;
		this.stabilizerTag = stabilizerTag;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
		Scribe_Collections.Look(ref stabilizers, "stabilizers", LookMode.Reference);
		Scribe_References.Look(ref core, "core");
		Scribe_Values.Look(ref coreTag, "coreTag");
		Scribe_Values.Look(ref stabilizerTag, "stabilizerTag");
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		signal.args.TryGetArg("SUBJECT", out Thing _);
		if ((!(signal.tag == inSignalEnable) || !SetupTags()) && !(signal.tag != StabilizerDestroyedSignal))
		{
			CompCerebrexCore compCerebrexCore = core.TryGetComp<CompCerebrexCore>();
			float num = ((compCerebrexCore.stabilizersRemaining == 1) ? 1.5f : 1f);
			MechhiveUtility.FireRaid(site.Map, StorytellerUtility.DefaultThreatPointsNow(site.Map) * num, -1, "MechhiveDefenseResponseLetterLabel".Translate(), "MechhiveDefenseResponseLetterText".Translate());
			compCerebrexCore.Notify_StabilizerDisabled();
		}
	}

	private bool SetupTags()
	{
		stabilizers = site.Map.listerThings.ThingsOfDef(ThingDefOf.CerebrexStabilizer).ToList();
		core = site.Map.listerThings.ThingsOfDef(ThingDefOf.CerebrexCore).FirstOrDefault();
		if (stabilizers.NullOrEmpty() || stabilizers.Count != 3 || core == null)
		{
			Log.Error("Mechhive Quest: Could not find cerebrex core or stabilizers.");
			return true;
		}
		foreach (Thing stabilizer in stabilizers)
		{
			QuestUtility.AddQuestTag(ref stabilizer.questTags, stabilizerTag);
		}
		QuestUtility.AddQuestTag(ref core.questTags, coreTag);
		return false;
	}
}
