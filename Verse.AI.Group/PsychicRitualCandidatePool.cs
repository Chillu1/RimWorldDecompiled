using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualCandidatePool : ILordJobCandidatePool
{
	private List<Pawn> nonAssignablePawns;

	private List<Pawn> allCandidatePawns;

	public List<Pawn> NonAssignablePawns => nonAssignablePawns;

	public List<Pawn> AllCandidatePawns => allCandidatePawns;

	public PsychicRitualCandidatePool(List<Pawn> allCandidatePawns, List<Pawn> nonAssignablePawns)
	{
		this.allCandidatePawns = allCandidatePawns;
		this.nonAssignablePawns = nonAssignablePawns;
	}
}
