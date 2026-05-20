using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PreceptComp_SelfTookMemoryThought : PreceptComp_Thought
{
	public HistoryEventDef eventDef;

	public bool onlyForNonSlaves;

	public override IEnumerable<TraitRequirement> TraitsAffecting => ThoughtUtility.GetNullifyingTraits(thought);

	public override void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
	{
		if (ev.def != eventDef || !canApplySelfTookThoughts)
		{
			return;
		}
		Pawn arg = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
		if (arg.needs != null && arg.needs.mood != null && (!onlyForNonSlaves || !arg.IsSlave) && (thought.minExpectation == null || ExpectationsUtility.CurrentExpectationFor(arg).order >= thought.minExpectation.order))
		{
			Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thought, precept);
			if (thought_Memory is Thought_KilledInnocentAnimal thought_KilledInnocentAnimal && ev.args.TryGetArg(HistoryEventArgsNames.Victim, out Pawn arg2))
			{
				thought_KilledInnocentAnimal.SetAnimal(arg2);
			}
			if (thought_Memory is Thought_MemoryObservation thought_MemoryObservation && ev.args.TryGetArg(HistoryEventArgsNames.Subject, out Corpse arg3))
			{
				thought_MemoryObservation.Target = arg3;
			}
			arg.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
		}
	}
}
