using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker_RoomRequirementsNotMet : ThoughtWorker
	{
		protected abstract IEnumerable<string> UnmetRequirements(Pawn p);

		protected bool Active(Pawn p)
		{
			if (p.royalty == null || p.MapHeld == null || !p.MapHeld.IsPlayerHome)
			{
				return false;
			}
			return UnmetRequirements(p).Any();
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!Active(p))
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(0);
		}
	}
}
