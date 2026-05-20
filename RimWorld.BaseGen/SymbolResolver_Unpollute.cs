using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Unpollute : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			foreach (IntVec3 item in rp.rect)
			{
				if (item.InBounds(map) && item.CanUnpollute(map) && (!rp.rect.IsOnEdge(item) || Rand.Chance(rp.edgeUnpolluteChance)))
				{
					item.Unpollute(map);
				}
			}
		}
	}
}
