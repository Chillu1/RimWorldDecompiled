using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class RetainedCaravanData : IExposable
{
	private Map map;

	public PlanetTile nextTile = PlanetTile.Invalid;

	public PlanetTile destinationTile = PlanetTile.Invalid;

	private bool shouldPassStoryState;

	private float nextTileCostLeftPct;

	private bool paused;

	private CaravanArrivalAction arrivalAction;

	private int backCompatTile;

	public bool HasDestinationTile => destinationTile.Valid;

	public RetainedCaravanData(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref nextTile, "nextTile", PlanetTile.Invalid);
		Scribe_Values.Look(ref destinationTile, "destinationTile");
		Scribe_Values.Look(ref shouldPassStoryState, "shouldPassStoryState", defaultValue: false);
		Scribe_Values.Look(ref nextTileCostLeftPct, "nextTileCostLeftPct", -1f);
		Scribe_Values.Look(ref paused, "paused", defaultValue: false);
		Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
	}

	public void Notify_GeneratedTempIncidentMapFor(Caravan caravan)
	{
		if (map.Parent.def.isTempIncidentMapOwner)
		{
			Set(caravan);
		}
	}

	public void Notify_CaravanFormed(Caravan caravan)
	{
		if (shouldPassStoryState)
		{
			shouldPassStoryState = false;
			map.StoryState.CopyTo(caravan.StoryState);
		}
		if (nextTile.Valid && nextTile != caravan.Tile && caravan.CanReach(nextTile))
		{
			caravan.pather.StartPath(nextTile, null, repathImmediately: true);
			caravan.pather.nextTileCostLeft = caravan.pather.nextTileCostTotal * nextTileCostLeftPct;
			caravan.pather.Paused = paused;
			caravan.tweener.ResetTweenedPosToRoot();
		}
		if (HasDestinationTile && destinationTile != caravan.Tile)
		{
			caravan.pather.StartPath(destinationTile, arrivalAction, repathImmediately: true);
			destinationTile = PlanetTile.Invalid;
			arrivalAction = null;
		}
	}

	private void Set(Caravan caravan)
	{
		caravan.StoryState.CopyTo(map.StoryState);
		shouldPassStoryState = true;
		if (caravan.pather.Moving)
		{
			nextTile = caravan.pather.nextTile;
			nextTileCostLeftPct = caravan.pather.nextTileCostLeft / caravan.pather.nextTileCostTotal;
			paused = caravan.pather.Paused;
			destinationTile = caravan.pather.Destination;
			arrivalAction = caravan.pather.ArrivalAction;
		}
		else
		{
			nextTile = PlanetTile.Invalid;
			nextTileCostLeftPct = 0f;
			paused = false;
			destinationTile = PlanetTile.Invalid;
			arrivalAction = null;
		}
	}
}
