using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyColonistAlive : QuestPart_Filter
	{
		public MapParent mapParent;

		protected override bool Pass(SignalArgs args)
		{
			if (mapParent != null && mapParent.HasMap)
			{
				return mapParent.Map.mapPawns.ColonistCount != 0;
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
