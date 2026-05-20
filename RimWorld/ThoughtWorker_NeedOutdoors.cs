using System;
using Verse;

namespace RimWorld;

public class ThoughtWorker_NeedOutdoors : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (p.needs.outdoors == null)
		{
			return ThoughtState.Inactive;
		}
		if (p.HostFaction != null)
		{
			return ThoughtState.Inactive;
		}
		return p.needs.outdoors.CurCategory switch
		{
			OutdoorsCategory.Entombed => ThoughtState.ActiveAtStage(0), 
			OutdoorsCategory.Trapped => ThoughtState.ActiveAtStage(1), 
			OutdoorsCategory.CabinFeverSevere => ThoughtState.ActiveAtStage(2), 
			OutdoorsCategory.CabinFeverLight => ThoughtState.ActiveAtStage(3), 
			OutdoorsCategory.NeedFreshAir => ThoughtState.ActiveAtStage(4), 
			OutdoorsCategory.Free => ThoughtState.Inactive, 
			_ => throw new InvalidOperationException("Unknown OutdoorsCategory"), 
		};
	}
}
