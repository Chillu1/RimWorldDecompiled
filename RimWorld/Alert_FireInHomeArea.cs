using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_FireInHomeArea : Alert_Critical
	{
		private Fire FireInHomeArea
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.Fire);
					for (int j = 0; j < list.Count; j++)
					{
						Thing thing = list[j];
						if (maps[i].areaManager.Home[thing.Position] && !thing.Position.Fogged(thing.Map))
						{
							return (Fire)thing;
						}
					}
				}
				return null;
			}
		}

		public Alert_FireInHomeArea()
		{
			defaultLabel = "FireInHomeArea".Translate();
			defaultExplanation = "FireInHomeAreaDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return FireInHomeArea;
		}
	}
}
