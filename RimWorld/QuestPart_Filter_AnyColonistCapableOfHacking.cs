using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyColonistCapableOfHacking : QuestPart_Filter
	{
		public MapParent mapParent;

		protected override bool Pass(SignalArgs args)
		{
			if (mapParent == null || !mapParent.HasMap)
			{
				return false;
			}
			foreach (Pawn item in mapParent.Map.mapPawns.FreeColonistsSpawned)
			{
				if (HackUtility.IsCapableOfHacking(item))
				{
					return true;
				}
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "mapParent");
		}
	}
}
