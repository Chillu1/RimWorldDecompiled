using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PawnObserver
{
	private Pawn pawn;

	private int intervalsUntilObserve;

	private List<Thought_MemoryObservationTerror> terrorThoughts = new List<Thought_MemoryObservationTerror>();

	private const int IntervalsBetweenObservations = 4;

	private const float ObservationRadius = 5f;

	public PawnObserver(Pawn pawn)
	{
		this.pawn = pawn;
		intervalsUntilObserve = 0;
	}

	public void ObserverInterval()
	{
		if (pawn.Spawned)
		{
			intervalsUntilObserve--;
			if (intervalsUntilObserve <= 0)
			{
				ObserveSurroundingThings();
				intervalsUntilObserve = 4 + Rand.RangeInclusive(-1, 1);
			}
		}
	}

	private void ObserveSurroundingThings()
	{
		TerrorUtility.RemoveAllTerrorThoughts(pawn);
		terrorThoughts.Clear();
		if (PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) || !pawn.Awake() || pawn.needs.mood == null)
		{
			return;
		}
		RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => pawn.Position.InHorDistOf(to.extentsClose.ClosestCellTo(pawn.Position), 5f), delegate(Region reg)
		{
			foreach (Thing item in reg.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse))
			{
				if (PossibleToObserve(item))
				{
					TryCreateObservedThought(item);
					TryCreateObservedHistoryEvent(item);
				}
			}
			foreach (Thing item2 in reg.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (PossibleToObserve(item2))
				{
					TryCreateObservedThought(item2);
					TryCreateObservedHistoryEvent(item2);
				}
			}
			foreach (Thing item3 in reg.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
			{
				if (PossibleToObserve(item3))
				{
					TryCreateObservedThought(item3);
					TryCreateObservedHistoryEvent(item3);
				}
			}
			return true;
		});
		foreach (Thought_MemoryObservationTerror item4 in TerrorUtility.TakeTopTerrorThoughts(terrorThoughts))
		{
			pawn.needs.mood.thoughts.memories.TryGainMemory(item4);
		}
		terrorThoughts.Clear();
		void TryCreateObservedHistoryEvent(Thing thing)
		{
			if (thing is IObservedThoughtGiver observedThoughtGiver)
			{
				HistoryEventDef historyEventDef = observedThoughtGiver.GiveObservedHistoryEvent(pawn);
				if (historyEventDef != null)
				{
					HistoryEvent historyEvent = new HistoryEvent(historyEventDef, pawn.Named(HistoryEventArgsNames.Doer), thing.Named(HistoryEventArgsNames.Subject));
					Find.HistoryEventsManager.RecordEvent(historyEvent);
				}
			}
		}
		void TryCreateObservedThought(Thing thing)
		{
			if (TerrorUtility.TryCreateTerrorThought(thing, out var thought))
			{
				terrorThoughts.Add(thought);
			}
			if (thing is IObservedThoughtGiver observedThoughtGiver)
			{
				Thought_Memory thought_Memory = observedThoughtGiver.GiveObservedThought(pawn);
				if (thought_Memory != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
				}
			}
		}
	}

	private bool PossibleToObserve(Thing thing)
	{
		if (thing.Position.InHorDistOf(pawn.Position, 5f))
		{
			return GenSight.LineOfSight(thing.Position, pawn.Position, pawn.Map, skipFirstCell: true);
		}
		return false;
	}
}
