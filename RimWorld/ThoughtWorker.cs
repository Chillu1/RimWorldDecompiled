using System;
using Verse;

namespace RimWorld;

public abstract class ThoughtWorker
{
	public ThoughtDef def;

	public virtual string PostProcessLabel(Pawn p, string label)
	{
		return label.Formatted(p.Named("PAWN"));
	}

	public virtual string PostProcessDescription(Pawn p, string description)
	{
		return description.Formatted(p.Named("PAWN"));
	}

	private ThoughtState? FailState(Pawn p)
	{
		if (!ThoughtUtility.CanGetThought(p, def))
		{
			return ThoughtState.Inactive;
		}
		return null;
	}

	public ThoughtState CurrentState(Pawn p)
	{
		return PostProcessedState(FailState(p) ?? CurrentStateInternal(p));
	}

	public ThoughtState CurrentSocialState(Pawn p, Pawn otherPawn)
	{
		return PostProcessedState(FailState(p) ?? CurrentSocialStateInternal(p, otherPawn));
	}

	private ThoughtState PostProcessedState(ThoughtState state)
	{
		if (def.invert)
		{
			state = ((!state.Active) ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive);
		}
		return state;
	}

	protected virtual ThoughtState CurrentStateInternal(Pawn p)
	{
		throw new NotImplementedException(def.defName + " (normal)");
	}

	protected virtual ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
	{
		throw new NotImplementedException(def.defName + " (social)");
	}

	public virtual float MoodMultiplier(Pawn p)
	{
		return 1f;
	}
}
