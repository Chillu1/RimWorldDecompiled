using Verse;

namespace RimWorld
{
	public class RitualRoleColonistConnectable : RitualRoleColonist
	{
		public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
		{
			if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason: false))
			{
				return false;
			}
			if (precept?.ideo != null && precept.ritualOnlyForIdeoMembers && p.Ideo != precept.ideo)
			{
				if (!skipReason)
				{
					reason = "CantStartRitualSelectedPawnMustBeMember".Translate(p.Named("PAWN"), precept.ideo.Named("IDEO"));
				}
				return false;
			}
			if (StatDefOf.PsychicSensitivity.Worker.IsDisabledFor(p) || p.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
			{
				if (!skipReason)
				{
					reason = "CantStartRitualSelectedPawnPsychicallyDeaf".Translate(p.Named("PAWN"));
				}
				return false;
			}
			if (p.needs?.mood?.thoughts?.memories != null && p.needs.mood.thoughts.memories.GetFirstMemoryOfDef(ThoughtDefOf.ConnectedTreeDied) != null)
			{
				if (!skipReason)
				{
					reason = "MessageRitualRolePawnRecentlyTornConnection".Translate(p);
				}
				return false;
			}
			return true;
		}
	}
}
