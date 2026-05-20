using UnityEngine;
using Verse;

namespace RimWorld;

public class Thought_MemorySocial : Thought_Memory, ISocialThought
{
	public float opinionOffset;

	public override bool ShouldDiscard
	{
		get
		{
			if (otherPawn != null && opinionOffset != 0f)
			{
				return base.ShouldDiscard;
			}
			return true;
		}
	}

	public override bool VisibleInNeedsTab
	{
		get
		{
			if (base.VisibleInNeedsTab)
			{
				return MoodOffset() != 0f;
			}
			return false;
		}
	}

	private float AgePct => (float)age / (float)DurationTicks;

	private float AgeFactor => Mathf.InverseLerp(1f, def.lerpOpinionToZeroAfterDurationPct, AgePct);

	public virtual float OpinionOffset()
	{
		if (ThoughtUtility.ThoughtNullified(pawn, def))
		{
			return 0f;
		}
		if (ShouldDiscard)
		{
			return 0f;
		}
		return opinionOffset * AgeFactor;
	}

	public Pawn OtherPawn()
	{
		return otherPawn;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref opinionOffset, "opinionOffset", 0f);
	}

	public override void Init()
	{
		base.Init();
		opinionOffset = base.CurStage.baseOpinionOffset;
	}

	public override bool TryMergeWithExistingMemory(out bool showBubble)
	{
		showBubble = false;
		return false;
	}

	public override bool GroupsWith(Thought other)
	{
		if (!(other is Thought_MemorySocial thought_MemorySocial))
		{
			return false;
		}
		if (base.GroupsWith(other))
		{
			return otherPawn == thought_MemorySocial.otherPawn;
		}
		return false;
	}
}
