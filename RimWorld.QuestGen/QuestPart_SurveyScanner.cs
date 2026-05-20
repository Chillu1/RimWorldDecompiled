using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_SurveyScanner : QuestPartActivable
{
	private Site site;

	private Thing scanner;

	private string scannerTag;

	private int duration;

	public QuestPart_SurveyScanner()
	{
	}

	public QuestPart_SurveyScanner(Site site, string inSignalEnable, string scannerTag, int duration)
	{
		this.site = site;
		base.inSignalEnable = inSignalEnable;
		this.scannerTag = scannerTag;
		this.duration = duration;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref site, "site");
		Scribe_References.Look(ref scanner, "scanner");
		Scribe_Values.Look(ref scannerTag, "scannerTag");
		Scribe_Values.Look(ref duration, "duration", 0);
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignalEnable)
		{
			scanner = site.Map.listerThings.ThingsOfDef(ThingDefOf.SurveyScanner).FirstOrDefault();
			scanner.TryGetComp<CompCountdown>().endTick = GenTicks.TicksGame + duration;
			QuestUtility.AddQuestTag(ref scanner.questTags, scannerTag);
			scanner.SetFactionDirect(Faction.OfPlayer);
			Find.LetterStack.ReceiveLetter("LetterLabelSurveyScannerArrived".Translate(), "LetterSurveyScannerArrived".Translate(duration.ToStringTicksToPeriod().Named("DURATION")), LetterDefOf.NeutralEvent, scanner);
		}
	}
}
