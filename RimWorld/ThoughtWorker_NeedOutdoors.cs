using System;
using Verse;

namespace RimWorld
{
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
			switch (p.needs.outdoors.CurCategory)
			{
			case OutdoorsCategory.Entombed:
				return ThoughtState.ActiveAtStage(0);
			case OutdoorsCategory.Trapped:
				return ThoughtState.ActiveAtStage(1);
			case OutdoorsCategory.CabinFeverSevere:
				return ThoughtState.ActiveAtStage(2);
			case OutdoorsCategory.CabinFeverLight:
				return ThoughtState.ActiveAtStage(3);
			case OutdoorsCategory.NeedFreshAir:
				return ThoughtState.ActiveAtStage(4);
			case OutdoorsCategory.Free:
				return ThoughtState.Inactive;
			default:
				throw new InvalidOperationException("Unknown OutdoorsCategory");
			}
		}
	}
}
