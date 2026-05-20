using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedComfort : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.comfort == null)
		{
			return ThoughtState.Inactive;
		}
		return p.needs.comfort.CurCategory switch
		{
			ComfortCategory.Uncomfortable => ThoughtState.ActiveAtStage(0), 
			ComfortCategory.Normal => ThoughtState.Inactive, 
			ComfortCategory.Comfortable => ThoughtState.ActiveAtStage(1), 
			ComfortCategory.VeryComfortable => ThoughtState.ActiveAtStage(2), 
			ComfortCategory.ExtremelyComfortable => ThoughtState.ActiveAtStage(3), 
			ComfortCategory.LuxuriantlyComfortable => ThoughtState.ActiveAtStage(4), 
			_ => throw new NotImplementedException(), 
		};
	}
}
