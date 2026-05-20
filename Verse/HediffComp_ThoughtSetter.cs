using RimWorld;
using UnityEngine;

namespace Verse;

public class HediffComp_ThoughtSetter : HediffComp
{
	public HediffCompProperties_ThoughtSetter Props => (HediffCompProperties_ThoughtSetter)props;

	public void OverrideMoodOffset(int offset)
	{
		Thought_Memory thought_Memory = base.Pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(Props.thought);
		if (thought_Memory != null)
		{
			thought_Memory.moodOffset = offset;
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		TryAddMemory();
	}

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		base.Pawn.needs?.mood?.thoughts?.memories?.RemoveMemoriesOfDef(Props.thought);
	}

	public override void Notify_Spawned()
	{
		TryAddMemory();
	}

	private void TryAddMemory()
	{
		if (base.Pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(Props.thought) == null)
		{
			Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(Props.thought);
			thought_Memory.permanent = true;
			if (Props.moodOffsetRange == FloatRange.Zero)
			{
				thought_Memory.moodOffset = Props.moodOffset;
			}
			else
			{
				thought_Memory.moodOffset = Mathf.RoundToInt(Props.moodOffsetRange.RandomInRange);
			}
			base.Pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought_Memory);
		}
	}
}
