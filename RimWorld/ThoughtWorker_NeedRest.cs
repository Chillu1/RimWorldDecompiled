using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedRest : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.rest == null)
		{
			return ThoughtState.Inactive;
		}
		return p.needs.rest.CurCategory switch
		{
			RestCategory.Rested => ThoughtState.Inactive, 
			RestCategory.Tired => ThoughtState.ActiveAtStage(0), 
			RestCategory.VeryTired => ThoughtState.ActiveAtStage(1), 
			RestCategory.Exhausted => ThoughtState.ActiveAtStage(2), 
			_ => throw new NotImplementedException(), 
		};
	}
}
