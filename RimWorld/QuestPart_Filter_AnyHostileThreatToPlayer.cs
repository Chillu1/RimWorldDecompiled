using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_AnyHostileThreatToPlayer : QuestPart_Filter
	{
		public MapParent mapParent;

		public bool countDormantPawnsAsHostile;

		protected override bool Pass(SignalArgs args)
		{
			if (mapParent != null && mapParent.Map != null)
			{
				return GenHostility.AnyHostileActiveThreatToPlayer(mapParent.Map, countDormantPawnsAsHostile);
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref mapParent, "parent");
			Scribe_Values.Look(ref countDormantPawnsAsHostile, "countDormantPawnsAsHostile", defaultValue: false);
		}
	}
}
