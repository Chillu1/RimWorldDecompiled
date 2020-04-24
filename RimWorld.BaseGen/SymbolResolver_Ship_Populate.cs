using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Populate : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			if (!rp.thrustAxis.HasValue)
			{
				Log.ErrorOnce("No thrust axis when generating ship parts", 50627817);
			}
			foreach (KeyValuePair<ThingDef, int> item in ShipUtility.RequiredParts())
			{
				for (int i = 0; i < item.Value; i++)
				{
					Rot4 rotation = Rot4.Random;
					if (item.Key == ThingDefOf.Ship_Engine && rp.thrustAxis.HasValue)
					{
						rotation = rp.thrustAxis.Value;
					}
					AttemptToPlace(item.Key, rp.rect, rotation, rp.faction);
				}
			}
		}

		public void AttemptToPlace(ThingDef thingDef, CellRect rect, Rot4 rotation, Faction faction)
		{
			Map map = BaseGen.globalSettings.map;
			IntVec3 loc = (from cell in rect.Cells.InRandomOrder()
				where GenConstruct.CanPlaceBlueprintAt(thingDef, cell, rotation, map).Accepted && GenAdj.OccupiedRect(cell, rotation, thingDef.Size).AdjacentCellsCardinal.Any((IntVec3 edgeCell) => edgeCell.InBounds(map) && edgeCell.GetThingList(map).Any((Thing thing) => thing.def == ThingDefOf.Ship_Beam))
				select cell).FirstOrFallback(IntVec3.Invalid);
			if (loc.IsValid)
			{
				Thing thing2 = ThingMaker.MakeThing(thingDef);
				thing2.SetFaction(faction);
				CompHibernatable compHibernatable = thing2.TryGetComp<CompHibernatable>();
				if (compHibernatable != null)
				{
					compHibernatable.State = HibernatableStateDefOf.Hibernating;
				}
				GenSpawn.Spawn(thing2, loc, BaseGen.globalSettings.map, rotation);
			}
		}
	}
}
