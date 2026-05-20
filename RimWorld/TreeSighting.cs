using Verse;

namespace RimWorld
{
	public class TreeSighting : IExposable
	{
		private int tickSighted;

		public Thing tree;

		public Thing Tree => tree;

		public int TicksSinceSighting => Find.TickManager.TicksGame - tickSighted;

		public TreeSighting()
		{
		}

		public TreeSighting(Thing tree, int tickSighted)
		{
			this.tree = tree;
			this.tickSighted = tickSighted;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref tickSighted, "tickSighted", 0);
			Scribe_References.Look(ref tree, "tree");
		}
	}
}
