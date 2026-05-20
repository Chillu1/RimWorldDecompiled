using System.Collections.Generic;
using Verse;

namespace RimWorld;

public interface ILordJobCandidatePool
{
	List<Pawn> AllCandidatePawns { get; }

	List<Pawn> NonAssignablePawns { get; }
}
