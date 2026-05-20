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
			List<Map> maps = Find.Maps;
			Thing innerIfMinified = th.GetInnerIfMinified();
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].listerBuildings.TryGetReinstallBlueprint(innerIfMinified, out var bp))
				{
					return bp;
				}
			}
			return null;
		}
	}
}
