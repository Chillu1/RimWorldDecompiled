using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Thought_MemoryObservation : Thought_Memory
{
	protected int targetThingID;

	public virtual Thing Target
	{
		set
		{
			targetThingID = value.thingIDNumber;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref targetThingID, "targetThingID", 0);
	}

	public override bool TryMergeWithExistingMemory(out bool showBubble)
	{
		ThoughtHandler thoughts = pawn.needs.mood.thoughts;
		Thought_MemoryObservation thought_MemoryObservation = null;
		List<Thought_Memory> memories = thoughts.memories.Memories;
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i] is Thought_MemoryObservation thought_MemoryObservation2 && thought_MemoryObservation2.def == def && thought_MemoryObservation2.targetThingID == targetThingID && (thought_MemoryObservation == null || thought_MemoryObservation2.age > thought_MemoryObservation.age))
			{
				thought_MemoryObservation = thought_MemoryObservation2;
			}
		}
		if (thought_MemoryObservation != null)
		{
			showBubble = thought_MemoryObservation.age > thought_MemoryObservation.DurationTicks / 2;
			thought_MemoryObservation.Renew();
			return true;
		}
		showBubble = true;
		return false;
	}
}
