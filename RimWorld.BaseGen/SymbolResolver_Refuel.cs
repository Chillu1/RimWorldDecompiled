using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Refuel : SymbolResolver
	{
		private static List<CompRefuelable> refuelables = new List<CompRefuelable>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			refuelables.Clear();
			foreach (IntVec3 item in rp.rect)
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					CompRefuelable compRefuelable = thingList[i].TryGetComp<CompRefuelable>();
					if (compRefuelable != null && !refuelables.Contains(compRefuelable))
					{
						refuelables.Add(compRefuelable);
					}
				}
			}
			for (int j = 0; j < refuelables.Count; j++)
			{
				float fuelCapacity = refuelables[j].Props.fuelCapacity;
				float amount = Rand.Range(fuelCapacity / 2f, fuelCapacity);
				refuelables[j].Refuel(amount);
			}
			refuelables.Clear();
		}
	}
}
