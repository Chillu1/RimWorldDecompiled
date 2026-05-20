using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptWorker_Relic : PreceptWorker
	{
		public override IEnumerable<PreceptThingChance> ThingDefs
		{
			get
			{
				foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
				{
					if (item.relicChance != 0f)
					{
						yield return new PreceptThingChance
						{
							chance = item.relicChance,
							def = item
						};
					}
				}
			}
		}

		public override float GetThingOrder(PreceptThingChance thingChance)
		{
			return 0f - thingChance.chance;
		}
	}
}
