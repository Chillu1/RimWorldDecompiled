using RimWorld;
using System.Linq;

namespace Verse
{
	public static class DebugToolsMisc
	{
		[DebugAction("General", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AttachFire()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TryAttachFire(1f);
			}
		}
	}
}
