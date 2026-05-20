using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_BloodfeederColonist : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.IsBloodfeeder() || p.Faction == null)
		{
			return ThoughtState.Inactive;
		}
		_ = p.Ideo;
		bool flag = false;
		foreach (Pawn item in p.MapHeld.mapPawns.SpawnedPawnsInFaction(p.Faction))
		{
			if (item.IsBloodfeeder())
			{
				flag = true;
				Precept_Role precept_Role = item.Ideo?.GetRole(item);
				if (precept_Role != null && precept_Role.ideo == p.Ideo && precept_Role.def == PreceptDefOf.IdeoRole_Leader)
				{
					return ThoughtState.ActiveAtStage(2);
				}
			}
		}
		if (flag)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		return ThoughtState.ActiveAtStage(0);
	}
}
