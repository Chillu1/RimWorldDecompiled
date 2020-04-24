using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TutorialState : IExposable
	{
		public List<Thing> startingItems = new List<Thing>();

		public CellRect roomRect;

		public CellRect sandbagsRect;

		public int endTick = -1;

		public bool introDone;

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && startingItems != null)
			{
				startingItems.RemoveAll((Thing it) => it == null || it.Destroyed || (it.Map == null && it.MapHeld == null));
			}
			Scribe_Collections.Look(ref startingItems, "startingItems", LookMode.Reference);
			Scribe_Values.Look(ref roomRect, "roomRect");
			Scribe_Values.Look(ref sandbagsRect, "sandbagsRect");
			Scribe_Values.Look(ref endTick, "endTick", -1);
			Scribe_Values.Look(ref introDone, "introDone", defaultValue: false);
			if (startingItems != null)
			{
				startingItems.RemoveAll((Thing it) => it == null);
			}
		}

		public void Notify_TutorialEnding()
		{
			startingItems.Clear();
			roomRect = default(CellRect);
			sandbagsRect = default(CellRect);
			endTick = Find.TickManager.TicksGame;
		}

		public void AddStartingItem(Thing t)
		{
			if (!startingItems.Contains(t))
			{
				startingItems.Add(t);
			}
		}
	}
}
