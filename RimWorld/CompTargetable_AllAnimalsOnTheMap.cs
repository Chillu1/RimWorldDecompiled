using Verse;

namespace RimWorld
{
	public class CompTargetable_AllAnimalsOnTheMap : CompTargetable_AllPawnsOnTheMap
	{
		protected override TargetingParameters GetTargetingParameters()
		{
			TargetingParameters targetingParameters = base.GetTargetingParameters();
			targetingParameters.validator = ((TargetInfo targ) => BaseTargetValidator(targ.Thing) && ((targ.Thing as Pawn)?.RaceProps.Animal ?? false));
			return targetingParameters;
		}
	}
}
