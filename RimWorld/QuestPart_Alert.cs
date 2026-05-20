using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_Alert : QuestPartActivable
{
	public string label;

	public string explanation;

	public LookTargets lookTargets;

	public bool critical;

	public bool getLookTargetsFromSignal;

	private string resolvedLabel;

	private string resolvedExplanation;

	private LookTargets resolvedLookTargets;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			GlobalTargetInfo globalTargetInfo = lookTargets.TryGetPrimaryTarget();
			if (globalTargetInfo.IsValid)
			{
				yield return globalTargetInfo;
			}
		}
	}

	public override string AlertLabel => resolvedLabel;

	public override string AlertExplanation => resolvedExplanation;

	public override bool AlertCritical => critical;

	public override AlertReport AlertReport
	{
		get
		{
			if (base.State != QuestPartState.Enabled)
			{
				return AlertReport.Inactive;
			}
			if (resolvedLookTargets.IsValid())
			{
				return AlertReport.CulpritsAre(resolvedLookTargets.targets);
			}
			return AlertReport.Active;
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		resolvedLabel = receivedArgs.GetFormattedText(label);
		resolvedExplanation = receivedArgs.GetFormattedText(explanation);
		resolvedLookTargets = lookTargets;
		if (getLookTargetsFromSignal && !resolvedLookTargets.IsValid())
		{
			SignalArgsUtility.TryGetLookTargets(receivedArgs, "SUBJECT", out resolvedLookTargets);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref label, "label");
		Scribe_Values.Look(ref explanation, "explanation");
		Scribe_Deep.Look(ref lookTargets, "lookTargets");
		Scribe_Values.Look(ref critical, "critical", defaultValue: false);
		Scribe_Values.Look(ref getLookTargetsFromSignal, "getLookTargetsFromSignal", defaultValue: false);
		Scribe_Values.Look(ref resolvedLabel, "resolvedLabel");
		Scribe_Values.Look(ref resolvedExplanation, "resolvedExplanation");
		Scribe_Deep.Look(ref resolvedLookTargets, "resolvedLookTargets");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		label = "DEV: Test";
		explanation = "Test text";
	}
}
