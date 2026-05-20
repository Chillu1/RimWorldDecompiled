using Verse;

namespace RimWorld;

public class Pawn_ReadingTracker : IExposable
{
	public Pawn pawn;

	private ReadingPolicy curPolicy;

	public ReadingPolicy CurrentPolicy
	{
		get
		{
			if (pawn.IsMutant && pawn.mutant.Def.disablePolicies)
			{
				return null;
			}
			if (curPolicy == null)
			{
				curPolicy = Current.Game.readingPolicyDatabase.DefaultReadingPolicy();
			}
			return curPolicy;
		}
		set
		{
			curPolicy = value;
		}
	}

	public Pawn_ReadingTracker()
	{
	}

	public Pawn_ReadingTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref curPolicy, "curAssignment");
	}
}
