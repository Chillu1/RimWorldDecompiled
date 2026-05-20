using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public interface ILordJobAssignmentsManager<RoleType> where RoleType : ILordJobRole
{
	bool SpectatorsAllowed { get; }

	List<Pawn> SpectatorsForReading { get; }

	IEnumerable<IGrouping<string, RoleType>> RoleGroups();

	IEnumerable<Pawn> AssignedPawns(RoleType role);

	RoleType ForcedRole(Pawn pawn);

	RoleType RoleForPawn(Pawn pawn, bool includeForced = true);

	Pawn FirstAssignedPawn(RoleType role);

	bool Required(Pawn pawn);

	bool PawnParticipating(Pawn pawn);

	bool PawnSpectating(Pawn pawn);

	bool CanParticipate(Pawn pawn, out TaggedString reason);

	bool TryAssignSpectate(Pawn pawn, Pawn insertBefore = null);

	bool TryAssign(Pawn pawn, RoleType role, out PsychicRitualRoleDef.Reason reason, PsychicRitualRoleDef.Context context = PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, Pawn insertBefore = null);

	bool TryUnassignAnyRole(Pawn pawn);

	void RemoveParticipant(Pawn pawn);

	string PawnNotAssignableReason(Pawn p, RoleType role);
}
