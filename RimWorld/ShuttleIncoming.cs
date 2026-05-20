using System.Linq;
using Verse;

namespace RimWorld;

public class ShuttleIncoming : Skyfaller, IActiveTransporter, IThingHolder
{
	public ActiveTransporterInfo Contents
	{
		get
		{
			return ((ActiveTransporter)innerContainer[0]).Contents;
		}
		set
		{
			((ActiveTransporter)innerContainer[0]).Contents = value;
		}
	}

	protected override void SpawnThings()
	{
		for (int num = innerContainer.Count - 1; num >= 0; num--)
		{
			Thing thing = innerContainer[num];
			foreach (IntVec3 item in GenAdj.CellsOccupiedBy(base.Position, thing.Rotation, thing.def.Size))
			{
				foreach (Thing item2 in base.Map.thingGrid.ThingsAt(item).ToList())
				{
					if (item2.def.IsBlueprint)
					{
						item2.Destroy(DestroyMode.Cancel);
					}
				}
			}
			foreach (Thing item3 in base.Map.thingGrid.ThingsAt(ThingUtility.InteractionCellWhenAt(thing.def, base.Position, thing.Rotation, base.Map)).ToList())
			{
				if (item3.def.IsBlueprint)
				{
					item3.Destroy(DestroyMode.Cancel);
				}
			}
		}
		base.SpawnThings();
	}
}
