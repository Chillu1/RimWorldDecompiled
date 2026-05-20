using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_RelayStabilizersRemainingAlert : QuestPart_Alert
{
	private Site site;

	private string inSignalStabilizerDealtWith;

	private int stabilizerCount;

	private int stabilizersRemaining;

	public List<Thing> things = new List<Thing>();

	public override string AlertLabel => string.Format("{0}: {1}/{2}", "RelayStabilizersRemainingAlertLabel".Translate(), stabilizersRemaining, stabilizerCount);

	public override string AlertExplanation => "RelayStabilizersRemainingAlertDescription".Translate();

	public override AlertReport AlertReport
	{
		get
		{
			if (stabilizersRemaining <= 0)
			{
				return AlertReport.Inactive;
			}
			return AlertReport.Active;
		}
	}

	public QuestPart_RelayStabilizersRemainingAlert()
	{
	}

	public QuestPart_RelayStabilizersRemainingAlert(Site site, string inSignalEnable, string inSignalDisable, string inSignalStabilizerDealtWith, int stabilizerCount)
	{
		this.site = site;
		base.inSignalEnable = inSignalEnable;
		base.inSignalDisable = inSignalDisable;
		this.inSignalStabilizerDealtWith = inSignalStabilizerDealtWith;
		this.stabilizerCount = stabilizerCount;
		stabilizersRemaining = stabilizerCount;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		stabilizersRemaining = stabilizerCount;
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].DestroyedOrNull() || things[i].IsHacked())
			{
				stabilizersRemaining--;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
		Scribe_Values.Look(ref inSignalStabilizerDealtWith, "inSignalStabilizerDealtWith");
		Scribe_Values.Look(ref stabilizerCount, "stabilizerCount", 0);
		Scribe_Values.Look(ref stabilizersRemaining, "stabilizersRemaining", 0);
		Scribe_Collections.Look(ref things, "things", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			things.RemoveAll((Thing x) => x == null);
		}
	}
}
