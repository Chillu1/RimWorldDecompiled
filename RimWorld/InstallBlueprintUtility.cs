using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class InstallBlueprintUtility
	{
		public static void CancelBlueprintsFor(Thing th)
		{
			ExistingBlueprintFor(th)?.Destroy(DestroyMode.Cancel);
		}

		public static Blueprint_Install ExistingBlueprintFor(Thing th)
		{
			ThingDef installBlueprintDef = th.GetInnerIfMinified().def.installBlueprintDef;
			if (installBlueprintDef == null)
			{
				return null;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Thing> list = maps[i].listerThings.ThingsMatching(ThingRequest.ForDef(installBlueprintDef));
				for (int j = 0; j < list.Count; j++)
				{
					Blueprint_Install blueprint_Install = list[j] as Blueprint_Install;
					if (blueprint_Install != null && blueprint_Install.MiniToInstallOrBuildingToReinstall == th)
					{
						return blueprint_Install;
					}
				}
			}
			return null;
		}
	}
}
