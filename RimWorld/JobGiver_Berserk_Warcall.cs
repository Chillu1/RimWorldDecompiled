using Verse;

namespace RimWorld;

public class JobGiver_Berserk_Warcall : JobGiver_Berserk
{
	protected override bool IsGoodTarget(Thing thing)
	{
		if (!base.IsGoodTarget(thing))
		{
			if (thing.Spawned)
			{
				return !(thing is Pawn);
			}
			return false;
		}
		return true;
	}
}
