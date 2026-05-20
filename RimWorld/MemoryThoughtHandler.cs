using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class MemoryThoughtHandler : IExposable
{
	public Pawn pawn;

	private List<Thought_Memory> memories = new List<Thought_Memory>();

	private List<Thought_Memory> tmpMemories = new List<Thought_Memory>();

	public List<Thought_Memory> Memories => memories;

	public MemoryThoughtHandler(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			tmpMemories.Clear();
			for (int num = memories.Count - 1; num >= 0; num--)
			{
				if (!memories[num].Save)
				{
					tmpMemories.Add(memories[num]);
					memories.Remove(memories[num]);
				}
			}
		}
		Scribe_Collections.Look(ref memories, "memories", LookMode.Deep);
		foreach (Thought_Memory tmpMemory in tmpMemories)
		{
			memories.Add(tmpMemory);
		}
		tmpMemories.Clear();
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num2 = memories.Count - 1; num2 >= 0; num2--)
		{
			if (memories[num2].def == null)
			{
				memories.RemoveAt(num2);
			}
			else
			{
				memories[num2].pawn = pawn;
				if (memories[num2].permanent)
				{
					bool flag = false;
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						HediffComp_ThoughtSetter hediffComp_ThoughtSetter = hediff.TryGetComp<HediffComp_ThoughtSetter>();
						if (hediffComp_ThoughtSetter != null && hediffComp_ThoughtSetter.Props.thought == memories[num2].def)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						memories[num2].permanent = false;
					}
				}
			}
		}
	}

	public void MemoryThoughtInterval()
	{
		for (int i = 0; i < memories.Count; i++)
		{
			memories[i].ThoughtInterval();
		}
		RemoveExpiredMemories();
	}

	private void RemoveExpiredMemories()
	{
		for (int num = memories.Count - 1; num >= 0; num--)
		{
			Thought_Memory thought_Memory = memories[num];
			if (thought_Memory.ShouldDiscard)
			{
				RemoveMemory(thought_Memory);
				if (thought_Memory.def.nextThought != null)
				{
					TryGainMemory(thought_Memory.def.nextThought);
				}
			}
		}
	}

	public void TryGainMemoryFast(ThoughtDef mem, Precept sourcePrecept = null)
	{
		Thought_Memory firstMemoryOfDef = GetFirstMemoryOfDef(mem);
		if (firstMemoryOfDef != null)
		{
			firstMemoryOfDef.Renew();
		}
		else
		{
			TryGainMemory(mem, null, sourcePrecept);
		}
	}

	public void TryGainMemoryFast(ThoughtDef mem, int stage, Precept sourcePrecept = null)
	{
		Thought_Memory firstMemoryOfDef = GetFirstMemoryOfDef(mem);
		if (firstMemoryOfDef != null)
		{
			firstMemoryOfDef.Renew();
			firstMemoryOfDef.SetForcedStage(stage);
		}
		else
		{
			TryGainMemory(mem, null, sourcePrecept);
			GetFirstMemoryOfDef(mem)?.SetForcedStage(stage);
		}
	}

	public void TryGainMemory(ThoughtDef def, Pawn otherPawn = null, Precept sourcePrecept = null)
	{
		if (!def.IsMemory)
		{
			Log.Error(def?.ToString() + " is not a memory thought.");
		}
		else
		{
			TryGainMemory(ThoughtMaker.MakeThought(def, sourcePrecept), otherPawn);
		}
	}

	public void TryGainMemory(Thought_Memory newThought, Pawn otherPawn = null)
	{
		if (!ThoughtUtility.CanGetThought(pawn, newThought.def))
		{
			return;
		}
		if (newThought is Thought_MemorySocial)
		{
			if (newThought.otherPawn == null && otherPawn == null)
			{
				Log.Error("Can't gain social thought " + newThought.def?.ToString() + " because its otherPawn is null and otherPawn passed to this method is also null. Social thoughts must have otherPawn.");
				return;
			}
			otherPawn = otherPawn ?? newThought.otherPawn;
			if (!newThought.def.socialTargetDevelopmentalStageFilter.Has(otherPawn.DevelopmentalStage))
			{
				return;
			}
		}
		newThought.pawn = pawn;
		newThought.otherPawn = otherPawn;
		if (!newThought.TryMergeWithExistingMemory(out var showBubble))
		{
			memories.Add(newThought);
		}
		if (newThought.def.stackLimitForSameOtherPawn >= 0)
		{
			while (NumMemoriesInGroup(newThought) > newThought.def.stackLimitForSameOtherPawn)
			{
				RemoveMemory(OldestMemoryInGroup(newThought));
			}
		}
		if (newThought.def.stackLimit >= 0)
		{
			while (NumMemoriesOfDef(newThought.def) > newThought.def.stackLimit)
			{
				RemoveMemory(OldestMemoryOfDef(newThought.def));
			}
		}
		if (newThought.def.thoughtToMake != null)
		{
			TryGainMemory(newThought.def.thoughtToMake, newThought.otherPawn);
		}
		List<Thought_Memory> list = ((newThought.def.replaceThoughts == null) ? memories : new List<Thought_Memory>(memories));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != newThought && list[i].GroupsWith(newThought))
			{
				list[i].Notify_NewThoughtInGroupAdded(newThought);
			}
			if (newThought.def.replaceThoughts != null && newThought.def.replaceThoughts.Contains(list[i].def))
			{
				RemoveMemory(list[i]);
			}
		}
		if (showBubble && newThought.def.showBubble && pawn.Spawned && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			MoteMaker.MakeMoodThoughtBubble(pawn, newThought);
		}
	}

	public Thought_Memory OldestMemoryInGroup(Thought_Memory group)
	{
		Thought_Memory result = null;
		int num = -9999;
		for (int i = 0; i < memories.Count; i++)
		{
			Thought_Memory thought_Memory = memories[i];
			if (thought_Memory.GroupsWith(group) && thought_Memory.age > num)
			{
				result = thought_Memory;
				num = thought_Memory.age;
			}
		}
		return result;
	}

	public Thought_Memory OldestMemoryOfDef(ThoughtDef def)
	{
		Thought_Memory result = null;
		int num = -9999;
		for (int i = 0; i < memories.Count; i++)
		{
			Thought_Memory thought_Memory = memories[i];
			if (thought_Memory.def == def && thought_Memory.age > num)
			{
				result = thought_Memory;
				num = thought_Memory.age;
			}
		}
		return result;
	}

	public void RemoveMemory(Thought_Memory th)
	{
		if (!memories.Remove(th))
		{
			Log.Warning("Tried to remove memory thought of def " + th.def.defName + " but it's not here.");
		}
	}

	public int NumMemoriesInGroup(Thought_Memory group)
	{
		int num = 0;
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i].GroupsWith(group))
			{
				num++;
			}
		}
		return num;
	}

	public int NumMemoriesOfDef(ThoughtDef def)
	{
		int num = 0;
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i].def == def)
			{
				num++;
			}
		}
		return num;
	}

	public Thought_Memory GetFirstMemoryOfDef(ThoughtDef def)
	{
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i].def == def)
			{
				return memories[i];
			}
		}
		return null;
	}

	public void RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDef def, Pawn otherPawn)
	{
		for (int num = memories.Count - 1; num >= 0; num--)
		{
			Thought_Memory thought_Memory = memories[num];
			if (thought_Memory.def == def && thought_Memory.otherPawn == otherPawn)
			{
				RemoveMemory(thought_Memory);
			}
		}
	}

	public void RemoveMemoriesWhereOtherPawnIs(Pawn otherPawn)
	{
		while (true)
		{
			Thought_Memory thought_Memory = memories.Find((Thought_Memory x) => x.otherPawn == otherPawn);
			if (thought_Memory != null)
			{
				RemoveMemory(thought_Memory);
				continue;
			}
			break;
		}
	}

	public void RemoveMemoriesOfDef(ThoughtDef def)
	{
		if (!def.IsMemory)
		{
			Log.Warning(def?.ToString() + " is not a memory thought.");
			return;
		}
		while (true)
		{
			Thought_Memory thought_Memory = memories.Find((Thought_Memory x) => x.def == def);
			if (thought_Memory != null)
			{
				RemoveMemory(thought_Memory);
				continue;
			}
			break;
		}
	}

	public void RemoveMemoriesOfDefIf(ThoughtDef def, Func<Thought_Memory, bool> predicate)
	{
		if (!def.IsMemory)
		{
			Log.Warning(def?.ToString() + " is not a memory thought.");
			return;
		}
		while (true)
		{
			Thought_Memory thought_Memory = memories.Find((Thought_Memory x) => x.def == def && predicate(x));
			if (thought_Memory != null)
			{
				RemoveMemory(thought_Memory);
				continue;
			}
			break;
		}
	}

	public bool AnyMemoryConcerns(Pawn otherPawn)
	{
		for (int i = 0; i < memories.Count; i++)
		{
			if (memories[i].otherPawn == otherPawn)
			{
				return true;
			}
		}
		return false;
	}

	public void Notify_PawnDiscarded(Pawn discarded)
	{
		RemoveMemoriesWhereOtherPawnIs(discarded);
	}
}
