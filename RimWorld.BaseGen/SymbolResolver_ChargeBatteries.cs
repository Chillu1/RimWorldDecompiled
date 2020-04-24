using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_ChargeBatteries : SymbolResolver
	{
		private static List<CompPowerBattery> batteries = new List<CompPowerBattery>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			batteries.Clear();
			foreach (IntVec3 item in rp.rect)
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					CompPowerBattery compPowerBattery = thingList[i].TryGetComp<CompPowerBattery>();
					if (compPowerBattery != null && !batteries.Contains(compPowerBattery))
					{
						batteries.Add(compPowerBattery);
					}
				}
			}
			for (int j = 0; j < batteries.Count; j++)
			{
				float num = Rand.Range(0.1f, 0.3f);
				batteries[j].SetStoredEnergyPct(Mathf.Min(batteries[j].StoredEnergyPct + num, 1f));
			}
			batteries.Clear();
		}
	}
}
