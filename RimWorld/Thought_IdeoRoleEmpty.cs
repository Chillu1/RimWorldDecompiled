using Verse;

namespace RimWorld;

public class Thought_IdeoRoleEmpty : Thought_Situational
{
	public Precept_Role Role => (Precept_Role)sourcePrecept;

	public override string LabelCap => base.CurStage.LabelCap.Formatted(Role.Named("ROLE"));

	public override string Description => base.CurStage.description.Formatted(Role.ideo.memberName, Role.Named("ROLE"));

	protected override ThoughtState CurrentStateInternal()
	{
		if (pawn.IsSlave)
		{
			return false;
		}
		if (GenDate.DaysPassed < 10)
		{
			return false;
		}
		if (Role.def.leaderRole && !Faction.OfPlayer.ideos.IsPrimary(Role.ideo))
		{
			return false;
		}
		return Role.Active && pawn.Ideo == Role.ideo && Role.ChosenPawnSingle() == null;
	}

	public override bool GroupsWith(Thought other)
	{
		if (other is Thought_IdeoRoleEmpty thought_IdeoRoleEmpty)
		{
			return Role == thought_IdeoRoleEmpty.Role;
		}
		return false;
	}
}
