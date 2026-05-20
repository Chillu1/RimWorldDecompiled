using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedJoy : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.joy == null)
		{
			return ThoughtState.Inactive;
		}
		return p.needs.joy.CurCategory switch
		{
			JoyCategory.Empty => ThoughtState.ActiveAtStage(0), 
			JoyCategory.VeryLow => ThoughtState.ActiveAtStage(1), 
			JoyCategory.Low => ThoughtState.ActiveAtStage(2), 
			JoyCategory.Satisfied => ThoughtState.Inactive, 
			JoyCategory.High => ThoughtState.ActiveAtStage(3), 
			JoyCategory.Extreme => ThoughtState.ActiveAtStage(4), 
			_ => throw new NotImplementedException(), 
		};
	}
}
