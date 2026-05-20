using Verse;

namespace RimWorld;

public class Pawn_StyleObserverTracker : IExposable
{
	public Pawn pawn;

	private int styleDominanceThoughtIndex = -1;

	private const int StyleObservationCenterCellRadius = 5;

	private const int StyleObservationInterval = 900;

	private const float BaseDominancePointsThreshold = 10f;

	public int StyleDominanceThoughtIndex => styleDominanceThoughtIndex;

	public Pawn_StyleObserverTracker()
	{
	}

	public Pawn_StyleObserverTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	private bool CellValid(IntVec3 cell)
	{
		if (cell.InBounds(pawn.Map) && !cell.Fogged(pawn.Map))
		{
			return GenSight.LineOfSight(pawn.Position, cell, pawn.Map);
		}
		return false;
	}

	public void StyleObserverTickInterval(int delta)
	{
		if (!pawn.IsHashIntervalTick(900, delta) || !ModsConfig.IdeologyActive || pawn.Ideo == null || !pawn.Spawned)
		{
			return;
		}
		int lastIndex = styleDominanceThoughtIndex;
		styleDominanceThoughtIndex = -1;
		if (pawn.Awake() && !PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) && pawn.needs?.mood != null)
		{
			Room room = pawn.GetRoom();
			if (room != null && CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 5, CellValid, out var result))
			{
				float styleDominanceFromCellsCenteredOn = IdeoUtility.GetStyleDominanceFromCellsCenteredOn(result, pawn.Position, pawn.Map, pawn.Ideo);
				float pointsThreshold = ((!room.IsDoorway && (float)room.CellCount < 10f) ? ((float)room.CellCount) : 10f);
				UpdateStyleDominanceThoughtIndex(styleDominanceFromCellsCenteredOn, pointsThreshold, lastIndex);
			}
		}
	}

	private void UpdateStyleDominanceThoughtIndex(float styleDominance, float pointsThreshold, int lastIndex)
	{
		if (styleDominance >= pointsThreshold)
		{
			styleDominanceThoughtIndex = 0;
		}
		else if (styleDominance <= 0f - pointsThreshold)
		{
			styleDominanceThoughtIndex = 1;
		}
		else
		{
			styleDominanceThoughtIndex = -1;
		}
		if (lastIndex != styleDominanceThoughtIndex)
		{
			pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref styleDominanceThoughtIndex, "styleDominanceThoughtIndex", -1);
	}
}
