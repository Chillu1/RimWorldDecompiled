using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class BreakdownManager : MapComponent
	{
		private List<CompBreakdownable> comps = new List<CompBreakdownable>();

		public HashSet<Thing> brokenDownThings = new HashSet<Thing>();

		public const int CheckIntervalTicks = 1041;

		public BreakdownManager(Map map)
			: base(map)
		{
		}

		public void Register(CompBreakdownable c)
		{
			comps.Add(c);
			if (c.BrokenDown)
			{
				brokenDownThings.Add(c.parent);
			}
		}

		public void Deregister(CompBreakdownable c)
		{
			comps.Remove(c);
			brokenDownThings.Remove(c.parent);
		}

		public override void MapComponentTick()
		{
			if (Find.TickManager.TicksGame % 1041 == 0)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CheckForBreakdown();
				}
			}
		}

		public void Notify_BrokenDown(Thing thing)
		{
			brokenDownThings.Add(thing);
		}

		public void Notify_Repaired(Thing thing)
		{
			brokenDownThings.Remove(thing);
		}
	}
}
