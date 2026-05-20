using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class CompOrbitalScanner : ThingComp
{
	private int locateSignalTick = -1;

	private CompPowerTrader compPowerTrader;

	private OrbitalScannerWorldComponent orbitalScannerWorldComponent;

	private List<QuestScriptDef> scannerQuests;

	private static readonly IntRange TrackSignalDurationRangeTicks = new IntRange(480000, 600000);

	private CompPowerTrader PowerTrader => compPowerTrader ?? (compPowerTrader = parent.TryGetComp<CompPowerTrader>());

	private OrbitalScannerWorldComponent OrbitalScannerWorldComponent => orbitalScannerWorldComponent ?? (orbitalScannerWorldComponent = Find.World.GetComponent<OrbitalScannerWorldComponent>());

	private List<QuestScriptDef> ScannerQuests => scannerQuests ?? (scannerQuests = QuestUtility.GetGiverQuests(QuestGiverTag.OrbitalScanner).ToList());

	private bool IsLocating => locateSignalTick > 0;

	private int TicksUntilLocated => locateSignalTick - Find.TickManager.TicksGame;

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref locateSignalTick, "locateSignalTick", 0);
	}

	public override void CompTick()
	{
		if (!PowerTrader.PowerOn)
		{
			if (locateSignalTick > 0)
			{
				locateSignalTick++;
			}
		}
		else if (!IsLocating)
		{
			OrbitalScannerWorldComponent.Notify_ScannerWorking(this);
		}
		else if (Find.TickManager.TicksGame >= locateSignalTick)
		{
			LocateSignal();
		}
	}

	public void ReceiveSignal()
	{
		locateSignalTick = Find.TickManager.TicksGame + TrackSignalDurationRangeTicks.RandomInRange;
		Messages.Message(string.Format("{0} ({1}).", "MessageOrbitalSignalDetected".Translate(), "OrbitalScannerCompletesIn".Translate(TicksUntilLocated.ToStringTicksToPeriod())), parent, MessageTypeDefOf.PositiveEvent);
	}

	private void LocateSignal()
	{
		float points = StorytellerUtility.DefaultThreatPointsNow(parent.Map);
		Slate slate = new Slate();
		slate.Set("points", points);
		slate.Set("discoveryMethod", "QuestDiscoveredFromOrbitalScanner".Translate());
		QuestScriptDef questScriptDef = ScannerQuests.RandomElementByWeight((QuestScriptDef q) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(q, points, Find.World.StoryState));
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questScriptDef, slate);
		if (!quest.hidden && questScriptDef.sendAvailableLetter)
		{
			QuestUtility.SendLetterQuestAvailable(quest, slate.Get<string>("discoveryMethod"));
		}
		locateSignalTick = -1;
	}

	public override string CompInspectStringExtra()
	{
		if (!PowerTrader.PowerOn)
		{
			return null;
		}
		if (IsLocating)
		{
			return "OrbitalScannerLocating".Translate() + ": " + TicksUntilLocated.ToStringTicksToPeriod();
		}
		if (PowerTrader.PowerOn)
		{
			return "OrbitalScannerScanning".Translate();
		}
		return null;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (DebugSettings.ShowDevGizmos)
		{
			if (!IsLocating)
			{
				yield return new Command_Action
				{
					defaultLabel = "Find Signal",
					action = ReceiveSignal
				};
			}
			else
			{
				yield return new Command_Action
				{
					defaultLabel = "Locate Signal",
					action = LocateSignal
				};
			}
		}
	}
}
