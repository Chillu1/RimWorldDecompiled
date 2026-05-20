using Verse;

namespace RimWorld;

public class Pawn_OutfitTracker : IExposable
{
	public Pawn pawn;

	private ApparelPolicy curApparelPolicy;

	public OutfitForcedHandler forcedHandler = new OutfitForcedHandler();

	public ApparelPolicy CurrentApparelPolicy
	{
		get
		{
			if (pawn.IsMutant && (pawn.mutant.Def.disableApparel || pawn.mutant.Def.disablePolicies))
			{
				return null;
			}
			if (curApparelPolicy == null)
			{
				curApparelPolicy = Current.Game.outfitDatabase.DefaultOutfit();
			}
			return curApparelPolicy;
		}
		set
		{
			if (curApparelPolicy != value)
			{
				curApparelPolicy = value;
				if (pawn.mindState != null)
				{
					pawn.mindState.Notify_OutfitChanged();
				}
			}
		}
	}

	public Pawn_OutfitTracker()
	{
	}

	public Pawn_OutfitTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref curApparelPolicy, "curOutfit");
		Scribe_Deep.Look(ref forcedHandler, "overrideHandler");
	}
}
