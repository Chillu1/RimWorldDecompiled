using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class History : IExposable
{
	public Archive archive = new Archive();

	private List<HistoryAutoRecorderGroup> autoRecorderGroups;

	public SimpleCurveDrawerStyle curveDrawerStyle;

	public int lastPsylinkAvailable = -999999;

	public int lastTickPlayerRaidedSomeone = -9999999;

	public bool mechanitorEverDied;

	public bool mechlinkEverAvailable;

	public bool mechanoidDatacoreOpportunityAvailable;

	public bool mechanoidDatacoreReadOrLost;

	public bool everThirdTrimesterPregnancy;

	public bool everCapturedUnrecruitablePawn;

	public bool duplicateSicknessDiscovered;

	public HistoryEventsManager historyEventsManager = new HistoryEventsManager();

	public History()
	{
		autoRecorderGroups = new List<HistoryAutoRecorderGroup>();
		AddOrRemoveHistoryRecorderGroups();
		curveDrawerStyle = new SimpleCurveDrawerStyle();
		curveDrawerStyle.DrawMeasures = true;
		curveDrawerStyle.DrawPoints = false;
		curveDrawerStyle.DrawBackground = true;
		curveDrawerStyle.DrawBackgroundLines = false;
		curveDrawerStyle.DrawLegend = true;
		curveDrawerStyle.DrawCurveMousePoint = true;
		curveDrawerStyle.OnlyPositiveValues = true;
		curveDrawerStyle.UseFixedSection = true;
		curveDrawerStyle.UseAntiAliasedLines = true;
		curveDrawerStyle.PointsRemoveOptimization = true;
		curveDrawerStyle.MeasureLabelsXCount = 10;
		curveDrawerStyle.MeasureLabelsYCount = 5;
		curveDrawerStyle.XIntegersOnly = true;
		curveDrawerStyle.LabelX = "Day".Translate();
	}

	public void HistoryTick()
	{
		for (int i = 0; i < autoRecorderGroups.Count; i++)
		{
			autoRecorderGroups[i].Tick();
		}
		historyEventsManager.HistoryEventsManagerTick();
	}

	public List<HistoryAutoRecorderGroup> Groups()
	{
		return autoRecorderGroups;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref archive, "archive");
		Scribe_Collections.Look(ref autoRecorderGroups, "autoRecorderGroups", LookMode.Deep);
		Scribe_Values.Look(ref lastPsylinkAvailable, "lastPsylinkAvailable", -999999);
		Scribe_Values.Look(ref lastTickPlayerRaidedSomeone, "lastTickPlayerRaidedSomeone", -9999999);
		Scribe_Deep.Look(ref historyEventsManager, "historyEventsManager");
		Scribe_Values.Look(ref mechanitorEverDied, "mechanitorEverDied", defaultValue: false);
		Scribe_Values.Look(ref mechlinkEverAvailable, "mechlinkEverAvailable", defaultValue: false);
		Scribe_Values.Look(ref mechanoidDatacoreReadOrLost, "mechanoidDatacoreReadOrLost", defaultValue: false);
		Scribe_Values.Look(ref mechanoidDatacoreOpportunityAvailable, "mechanoidDatacoreOpportunityAvailable", defaultValue: false);
		Scribe_Values.Look(ref everThirdTrimesterPregnancy, "everThirdTrimesterPregnancy", defaultValue: false);
		Scribe_Values.Look(ref everCapturedUnrecruitablePawn, "everCapturedUnrecruitablePawn", defaultValue: false);
		Scribe_Values.Look(ref duplicateSicknessDiscovered, "duplicateSicknessDiscovered", defaultValue: false);
		BackCompatibility.PostExposeData(this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			AddOrRemoveHistoryRecorderGroups();
			if (lastPsylinkAvailable == -999999)
			{
				lastPsylinkAvailable = Find.TickManager.TicksGame;
			}
		}
	}

	public void Notify_PsylinkAvailable()
	{
		lastPsylinkAvailable = Find.TickManager.TicksGame;
	}

	public void Notify_PlayerRaidedSomeone()
	{
		lastTickPlayerRaidedSomeone = Find.TickManager.TicksGame;
	}

	public void Notify_MechanitorDied()
	{
		mechanitorEverDied = true;
	}

	public void Notify_MechlinkAvailable()
	{
		mechlinkEverAvailable = true;
	}

	public void Notify_MechanoidDatacoreOppurtunityAvailable()
	{
		mechanoidDatacoreOpportunityAvailable = true;
	}

	public void Notify_MechanoidDatacoreReadOrLost()
	{
		mechanoidDatacoreReadOrLost = true;
	}

	public void Notify_DuplicateSicknessDiscovered()
	{
		duplicateSicknessDiscovered = true;
	}

	public void FinalizeInit()
	{
		lastPsylinkAvailable = Find.TickManager.TicksGame;
	}

	private void AddOrRemoveHistoryRecorderGroups()
	{
		if (autoRecorderGroups.RemoveAll((HistoryAutoRecorderGroup x) => x == null) != 0)
		{
			Log.Warning("Some history auto recorder groups were null.");
		}
		foreach (HistoryAutoRecorderGroupDef def in DefDatabase<HistoryAutoRecorderGroupDef>.AllDefs)
		{
			if (!autoRecorderGroups.Any((HistoryAutoRecorderGroup x) => x.def == def))
			{
				HistoryAutoRecorderGroup historyAutoRecorderGroup = new HistoryAutoRecorderGroup();
				historyAutoRecorderGroup.def = def;
				historyAutoRecorderGroup.AddOrRemoveHistoryRecorders();
				autoRecorderGroups.Add(historyAutoRecorderGroup);
			}
		}
		autoRecorderGroups.RemoveAll((HistoryAutoRecorderGroup x) => x.def == null);
	}
}
