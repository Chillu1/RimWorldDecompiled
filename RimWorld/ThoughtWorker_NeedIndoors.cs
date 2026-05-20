using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedIndoors : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.indoors == null)
			{
				return ThoughtState.Inactive;
			}
			if (p.HostFaction != null)
			{
				return ThoughtState.Inactive;
			}
			return p.needs.indoors.CurCategory switch
			{
				IndoorsCategory.ComfortablyIndoors => ThoughtState.Inactive, 
				IndoorsCategory.JustOutdoors => ThoughtState.ActiveAtStage(0), 
				IndoorsCategory.Outdoors => ThoughtState.ActiveAtStage(1), 
				IndoorsCategory.LongOutdoors => ThoughtState.ActiveAtStage(2), 
				IndoorsCategory.VeryLongOutdoors => ThoughtState.ActiveAtStage(3), 
				IndoorsCategory.BrutalOutdoors => ThoughtState.ActiveAtStage(4), 
				_ => throw new InvalidOperationException("Unknown IndoorsCategory"), 
			};
		}
	}
}
