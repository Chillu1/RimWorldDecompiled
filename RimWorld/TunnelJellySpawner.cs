using Verse;

namespace RimWorld
{
	public class TunnelJellySpawner : TunnelHiveSpawner
	{
		public int jellyCount;

		protected override void Spawn(Map map, IntVec3 loc)
		{
			if (jellyCount <= 0)
			{
				return;
			}
			int num = Rand.Range(2, 3);
			int num2 = jellyCount;
			for (int i = 0; i < num; i++)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.InsectJelly);
				if (i < num - 1)
				{
					thing.stackCount = jellyCount / num;
					num2 -= thing.stackCount;
				}
				else
				{
					thing.stackCount = num2;
				}
				GenSpawn.Spawn(thing, CellFinder.RandomClosewalkCellNear(loc, map, 2), map);
			}
			jellyCount = 0;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref jellyCount, "jellyCount", 0);
		}
	}
}
