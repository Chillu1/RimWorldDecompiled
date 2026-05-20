using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedPlay : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.play == null)
			{
				return ThoughtState.Inactive;
			}
			return p.needs.play.CurCategory switch
			{
				PlayCategory.Empty => ThoughtState.ActiveAtStage(0), 
				PlayCategory.VeryLow => ThoughtState.ActiveAtStage(1), 
				PlayCategory.Low => ThoughtState.ActiveAtStage(2), 
				PlayCategory.Satisfied => ThoughtState.Inactive, 
				PlayCategory.High => ThoughtState.ActiveAtStage(3), 
				PlayCategory.Extreme => ThoughtState.ActiveAtStage(4), 
				_ => throw new NotImplementedException(), 
			};
		}
	}
}
