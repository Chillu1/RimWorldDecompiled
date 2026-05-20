using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class QuestPart : IExposable, ILoadReferenceable
{
	public enum SignalListenMode
	{
		OngoingOnly,
		NotYetAcceptedOnly,
		OngoingOrNotYetAccepted,
		HistoricalOnly,
		Always
	}

	public Quest quest;

	public SignalListenMode signalListenMode;

	public string debugLabel;

	public virtual string DescriptionPart { get; }

	public int Index => quest.PartsListForReading.IndexOf(this);

	public virtual IEnumerable<GlobalTargetInfo> QuestLookTargets => Enumerable.Empty<GlobalTargetInfo>();

	public virtual string QuestSelectTargetsLabel => null;

	public virtual IEnumerable<GlobalTargetInfo> QuestSelectTargets => Enumerable.Empty<GlobalTargetInfo>();

	public virtual IEnumerable<Faction> InvolvedFactions => Enumerable.Empty<Faction>();

	public virtual IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks => Enumerable.Empty<Dialog_InfoCard.Hyperlink>();

	public virtual bool IncreasesPopulation => false;

	public virtual bool RequiresAccepter => false;

	public virtual bool PreventsAutoAccept => RequiresAccepter;

	public virtual bool QuestPartReserves(Pawn p)
	{
		return false;
	}

	public virtual bool QuestPartReserves(Faction f)
	{
		return false;
	}

	public virtual bool QuestPartReserves(TransportShip ship)
	{
		return false;
	}

	public virtual void Cleanup()
	{
	}

	public virtual void AssignDebugData()
	{
	}

	public virtual void PreQuestAccept()
	{
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref signalListenMode, "signalListenMode", SignalListenMode.OngoingOnly);
		Scribe_Values.Look(ref debugLabel, "debugLabel");
	}

	public virtual void Notify_QuestSignalReceived(Signal signal)
	{
	}

	public virtual void Notify_ThingsProduced(Pawn worker, List<Thing> things)
	{
	}

	public virtual void Notify_PlantHarvested(Pawn worker, Thing harvested)
	{
	}

	public virtual void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
	{
	}

	public virtual void Notify_PawnBorn(Thing baby, Thing birther, Pawn mother, Pawn father)
	{
	}

	public virtual void Notify_FactionRemoved(Faction faction)
	{
	}

	public virtual void Notify_PawnDiscarded(Pawn pawn)
	{
	}

	public virtual void Notify_PreCleanup()
	{
	}

	public virtual void PostQuestAdded()
	{
	}

	public virtual void ReplacePawnReferences(Pawn replace, Pawn with)
	{
	}

	public virtual void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
	}

	public override string ToString()
	{
		string text = $"{GetType().Name} (quest.id={quest?.id ?? (-1)}, index={Index}";
		if (!debugLabel.NullOrEmpty())
		{
			text = text + ", debugLabel=" + debugLabel;
		}
		return text + ")";
	}

	public string GetUniqueLoadID()
	{
		return "QuestPart_" + quest.id + "_" + Index;
	}
}
