using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesUnnatural : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		return t is UnnaturalCorpse;
	}
}
