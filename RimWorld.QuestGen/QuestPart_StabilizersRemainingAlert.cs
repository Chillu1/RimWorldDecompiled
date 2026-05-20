using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_StabilizersRemainingAlert : QuestPart_Alert
{
	private Site site;

	private Thing core;

	private CompCerebrexCore coreComp;

	private CompCerebrexCore CoreComp
	{
		get
		{
			if (coreComp == null)
			{
				coreComp = core.TryGetComp<CompCerebrexCore>();
			}
			return coreComp;
		}
	}

	public override string AlertLabel => string.Format("{0}: {1}/{2}", "StabilizersRemainingAlertLabel".Translate(), CoreComp.stabilizersRemaining, 3);

	public override string AlertExplanation => "StabilizersRemainingAlertDescription".Translate();

	public override AlertReport AlertReport
	{
		get
		{
			CompCerebrexCore compCerebrexCore = CoreComp;
			if (compCerebrexCore == null || compCerebrexCore.stabilizersRemaining <= 0)
			{
				return AlertReport.Inactive;
			}
			return AlertReport.Active;
		}
	}

	public QuestPart_StabilizersRemainingAlert()
	{
	}

	public QuestPart_StabilizersRemainingAlert(Site site, string inSignalEnable, string inSignalDisable)
	{
		this.site = site;
		base.inSignalEnable = inSignalEnable;
		base.inSignalDisable = inSignalDisable;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalEnable)
		{
			core = site.Map.listerThings.ThingsOfDef(ThingDefOf.CerebrexCore).FirstOrDefault();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
		Scribe_References.Look(ref core, "core");
	}
}
