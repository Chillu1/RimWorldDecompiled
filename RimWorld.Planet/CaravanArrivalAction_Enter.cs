using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class CaravanArrivalAction_Enter : CaravanArrivalAction
{
	private MapParent mapParent;

	public override string Label => "EnterMap".Translate(mapParent.Label);

	public override string ReportString => "CaravanEntering".Translate(mapParent.Label);

	public CaravanArrivalAction_Enter()
	{
	}

	public CaravanArrivalAction_Enter(MapParent mapParent)
	{
		this.mapParent = mapParent;
	}

	public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (mapParent != null && mapParent.Tile != destinationTile)
		{
			return false;
		}
		return CanEnter(caravan, mapParent);
	}

	public override void Arrived(Caravan caravan)
	{
		Map map = mapParent.Map;
		if (map != null)
		{
			CaravanDropInventoryMode dropInventoryMode = (map.IsPlayerHome ? CaravanDropInventoryMode.UnloadIndividually : CaravanDropInventoryMode.DoNotDrop);
			bool draftColonists = mapParent.Faction != null && mapParent.Faction.HostileTo(Faction.OfPlayer);
			if (caravan.IsPlayerControlled || mapParent.Faction == Faction.OfPlayer)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(mapParent), "LetterCaravanEnteredMap".Translate(caravan.Label, mapParent).CapitalizeFirst(), LetterDefOf.NeutralEvent, caravan.PawnsListForReading);
			}
			CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, dropInventoryMode, draftColonists);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
	}

	public static FloatMenuAcceptanceReport CanEnter(Caravan caravan, MapParent mapParent)
	{
		if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
		{
			return false;
		}
		if (mapParent.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MapParent mapParent)
	{
		return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanEnter(caravan, mapParent), () => new CaravanArrivalAction_Enter(mapParent), "EnterMap".Translate(mapParent.Label), caravan, mapParent.Tile, mapParent);
	}
}
