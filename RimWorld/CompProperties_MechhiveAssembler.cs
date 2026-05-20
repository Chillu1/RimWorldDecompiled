using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_MechhiveAssembler : CompProperties
{
	public List<PawnGenOption> options = new List<PawnGenOption>();

	public CompProperties_MechhiveAssembler()
	{
		compClass = typeof(CompMechhiveAssembler);
	}
}
