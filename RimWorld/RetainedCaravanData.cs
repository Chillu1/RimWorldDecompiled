using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class RetainedCaravanData : IExposable
	{
		private Map map;

		private bool shouldPassStoryState;

		private int nextTile = -1;

		private float nextTileCostLeftPct;

		private bool paused;

		private int destinationTile = -1;

		private CaravanArrivalAction arrivalAction;

		public bool HasDestinationTile => destinationTile != -1;

		public RetainedCaravanData(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref shouldPassStoryState, "shouldPassStoryState", defaultValue: false);
			Scribe_Values.Look(ref nextTile, "nextTile", -1);
			Scribe_Values.Look(ref nextTileCostLeftPct, "nextTileCostLeftPct", -1f);
			Scribe_Values.Look(ref paused, "paused", defaultValue: false);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
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
			if (nextTile != -1 && nextTile != caravan.Tile && caravan.CanReach(nextTile))
			{
				caravan.pather.StartPath(nextTile, null, repathImmediately: true);
				caravan.pather.nextTileCostLeft = caravan.pather.nextTileCostTotal * nextTileCostLeftPct;
				caravan.pather.Paused = paused;
				caravan.tweener.ResetTweenedPosToRoot();
			}
			if (HasDestinationTile && destinationTile != caravan.Tile)
			{
				caravan.pather.StartPath(destinationTile, arrivalAction, repathImmediately: true);
				destinationTile = -1;
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
				nextTile = -1;
				nextTileCostLeftPct = 0f;
				paused = false;
				destinationTile = -1;
				arrivalAction = null;
			}
		}
	}
}
