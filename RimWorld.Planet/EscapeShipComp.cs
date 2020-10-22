using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class EscapeShipComp : WorldObjectComp
	{
		public override void PostMapGenerate()
		{
			Building building = ((MapParent)parent).Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.Ship_Reactor).FirstOrDefault();
			Building_ShipReactor building_ShipReactor;
			if (building != null && (building_ShipReactor = building as Building_ShipReactor) != null)
			{
				building_ShipReactor.charlonsReactor = true;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in CaravanArrivalAction_VisitEscapeShip.GetFloatMenuOptions(caravan, (MapParent)parent))
			{
				yield return floatMenuOption;
			}
		}
	}
}
