using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EdgeWalls : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			ThingDef wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
			foreach (IntVec3 edgeCell in rp.rect.EdgeCells)
			{
				TrySpawnWall(edgeCell, rp, wallStuff);
			}
		}

		private Thing TrySpawnWall(IntVec3 c, ResolveParams rp, ThingDef wallStuff)
		{
			Map map = BaseGen.globalSettings.map;
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (!thingList[i].def.destroyable)
				{
					return null;
				}
				if (thingList[i] is Building_Door)
				{
					return null;
				}
			}
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				thingList[num].Destroy();
			}
			if (rp.chanceToSkipWallBlock.HasValue && Rand.Chance(rp.chanceToSkipWallBlock.Value))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(ThingDefOf.Wall, wallStuff);
			thing.SetFaction(rp.faction);
			return GenSpawn.Spawn(thing, c, map);
		}
	}
}
