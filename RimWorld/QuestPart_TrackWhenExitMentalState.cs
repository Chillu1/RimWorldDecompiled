using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_TrackWhenExitMentalState : QuestPart
{
	public string tag;

	public List<string> inSignals;

	public string outSignal;

	public MapParent mapParent;

	public MentalStateDef mentalStateDef;

	private bool signalSent;

	[Unsaved(false)]
	private List<Pawn> cachedPawns;

	private List<Pawn> TrackedPawns
	{
		get
		{
			if (cachedPawns == null)
			{
				cachedPawns = mapParent.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.InMentalState && p.MentalStateDef == mentalStateDef && !p.questTags.NullOrEmpty() && p.questTags.Contains(tag)).ToList();
			}
			return cachedPawns;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent))
		{
			mapParent = quest.TryFindNewSuitableMapParentForRetarget();
		}
		if (!signalSent && inSignals.Contains(signal.tag))
		{
			Pawn pawn = TrackedPawns.Find((Pawn p) => p == signal.args.GetArg<Pawn>("SUBJECT"));
			if (pawn != null)
			{
				cachedPawns.Remove(pawn);
			}
			if (!cachedPawns.Any())
			{
				Find.SignalManager.SendSignal(new Signal(outSignal));
				signalSent = true;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref tag, "tag");
		Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
		Scribe_Values.Look(ref outSignal, "outSignal");
		Scribe_Defs.Look(ref mentalStateDef, "mentalStateDef");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref signalSent, "signalSent", defaultValue: false);
	}
}
