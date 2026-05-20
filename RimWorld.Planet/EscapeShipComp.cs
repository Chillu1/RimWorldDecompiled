using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class EscapeShipComp : WorldObjectComp
{
	public override void PostMapGenerate()
	{
		Building_ShipReactor building_ShipReactor = ((MapParent)parent).Map.listerBuildings.AllBuildingsColonistOfClass<Building_ShipReactor>().FirstOrDefault();
		if (building_ShipReactor != null)
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
