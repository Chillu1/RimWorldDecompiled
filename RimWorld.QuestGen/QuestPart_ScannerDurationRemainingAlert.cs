using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_ScannerDurationRemainingAlert : QuestPart_Alert
{
	private Site site;

	private Thing scanner;

	private int duration;

	private int endTick;

	private int fireRaidTick;

	private int fireRaidRemainingTicks;

	private string completedSignal;

	private string raidSignal;

	public override string AlertLabel => string.Format("{0}: {1}", "SurveyScannerAlertLabel".Translate(), Mathf.Max(endTick - GenTicks.TicksGame, 0).ToStringTicksToPeriod());

	public override string AlertExplanation => "SurveyScannerAlertDescription".Translate();

	public override AlertReport AlertReport => base.State == QuestPartState.Enabled;

	public override GlobalTargetInfo? AlertCulpritTarget => scanner;

	public QuestPart_ScannerDurationRemainingAlert()
	{
	}

	public QuestPart_ScannerDurationRemainingAlert(Site site, string inSignalEnable, string inSignalDisable, string completedSignal, string raidSignal, int duration, int fireRaidRemainingTicks)
	{
		this.site = site;
		base.inSignalEnable = inSignalEnable;
		base.inSignalDisable = inSignalDisable;
		this.completedSignal = completedSignal;
		this.raidSignal = raidSignal;
		this.duration = duration;
		this.fireRaidRemainingTicks = fireRaidRemainingTicks;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalEnable)
		{
			scanner = site.Map.listerThings.ThingsOfDef(ThingDefOf.SurveyScanner).FirstOrDefault();
			endTick = GenTicks.TicksGame + duration;
			if (fireRaidRemainingTicks > 0)
			{
				fireRaidTick = Mathf.Max(GenTicks.TicksGame + 2500, endTick - fireRaidRemainingTicks);
			}
		}
	}

	public override void QuestPartTick()
	{
		if (GenTicks.TicksGame >= endTick)
		{
			Find.SignalManager.SendSignal(new Signal(completedSignal));
			Complete();
		}
		else if (GenTicks.TicksGame >= fireRaidTick && fireRaidTick > 0)
		{
			fireRaidTick = -1;
			Find.SignalManager.SendSignal(new Signal(raidSignal));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
		Scribe_References.Look(ref scanner, "scanner");
		Scribe_Values.Look(ref completedSignal, "completedSignal");
		Scribe_Values.Look(ref raidSignal, "raidSignal");
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref endTick, "endTick", 0);
		Scribe_Values.Look(ref fireRaidTick, "fireRaidTick", 0);
		Scribe_Values.Look(ref fireRaidRemainingTicks, "fireRaidRemainingTicks", 0);
	}
}
