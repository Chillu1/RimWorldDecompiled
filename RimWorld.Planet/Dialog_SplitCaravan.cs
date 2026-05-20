using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

public class Dialog_SplitCaravan : Window
{
	private enum Tab
	{
		Pawns,
		Items,
		FoodAndMedicine
	}

	private Caravan caravan;

	private List<TransferableOneWay> transferables;

	private TransferableOneWayWidget pawnsTransfer;

	private TransferableOneWayWidget itemsTransfer;

	private TransferableOneWayWidget foodAndMedicineTransfer;

	private Tab tab;

	private bool sourceMassUsageDirty = true;

	private float cachedSourceMassUsage;

	private bool sourceMassCapacityDirty = true;

	private float cachedSourceMassCapacity;

	private string cachedSourceMassCapacityExplanation;

	private bool sourceTilesPerDayDirty = true;

	private float cachedSourceTilesPerDay;

	private string cachedSourceTilesPerDayExplanation;

	private bool sourceDaysWorthOfFoodDirty = true;

	private (float days, float tillRot) cachedSourceDaysWorthOfFood;

	private bool sourceForagedFoodPerDayDirty = true;

	private (ThingDef food, float perDay) cachedSourceForagedFoodPerDay;

	private string cachedSourceForagedFoodPerDayExplanation;

	private bool sourceVisibilityDirty = true;

	private float cachedSourceVisibility;

	private string cachedSourceVisibilityExplanation;

	private bool destMassUsageDirty = true;

	private float cachedDestMassUsage;

	private bool destMassCapacityDirty = true;

	private float cachedDestMassCapacity;

	private string cachedDestMassCapacityExplanation;

	private bool destTilesPerDayDirty = true;

	private float cachedDestTilesPerDay;

	private string cachedDestTilesPerDayExplanation;

	private bool destDaysWorthOfFoodDirty = true;

	private (float days, float tillRot) cachedDestDaysWorthOfFood;

	private bool destForagedFoodPerDayDirty = true;

	private (ThingDef food, float perDay) cachedDestForagedFoodPerDay;

	private string cachedDestForagedFoodPerDayExplanation;

	private bool destVisibilityDirty = true;

	private float cachedDestVisibility;

	private string cachedDestVisibilityExplanation;

	private bool ticksToArriveDirty = true;

	private int cachedTicksToArrive;

	private const float TitleRectHeight = 35f;

	private const float BottomAreaHeight = 55f;

	private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

	private static List<TabRecord> tabsList = new List<TabRecord>();

	public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

	protected override float Margin => 0f;

	private BiomeDef Biome => caravan.Biome;

	private float SourceMassUsage
	{
		get
		{
			if (sourceMassUsageDirty)
			{
				sourceMassUsageDirty = false;
				cachedSourceMassUsage = CollectionsMassCalculator.MassUsageLeftAfterTransfer(transferables, IgnorePawnsInventoryMode.Ignore);
			}
			return cachedSourceMassUsage;
		}
	}

	private float SourceMassCapacity
	{
		get
		{
			if (sourceMassCapacityDirty)
			{
				sourceMassCapacityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedSourceMassCapacity = CollectionsMassCalculator.CapacityLeftAfterTransfer(transferables, stringBuilder);
				cachedSourceMassCapacityExplanation = stringBuilder.ToString();
			}
			return cachedSourceMassCapacity;
		}
	}

	private float SourceTilesPerDay
	{
		get
		{
			if (sourceTilesPerDayDirty)
			{
				sourceTilesPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedSourceTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDayLeftAfterTransfer(transferables, SourceMassUsage, SourceMassCapacity, caravan.Tile, caravan.pather.Moving ? caravan.pather.nextTile : PlanetTile.Invalid, caravan.Shuttle != null, stringBuilder);
				cachedSourceTilesPerDayExplanation = stringBuilder.ToString();
			}
			return cachedSourceTilesPerDay;
		}
	}

	private (float days, float tillRot) SourceDaysWorthOfFood
	{
		get
		{
			if (sourceDaysWorthOfFoodDirty)
			{
				sourceDaysWorthOfFoodDirty = false;
				float item;
				float item2;
				if (caravan.pather.Moving)
				{
					using (caravan.Tile.Layer.Pather.FindPath(caravan.Tile, caravan.pather.Destination, null))
					{
						item = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTransfer(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.Faction, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove);
						item2 = DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTransfer(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove);
					}
				}
				else
				{
					item = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFoodLeftAfterTransfer(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.Faction);
					item2 = DaysUntilRotCalculator.ApproxDaysUntilRotLeftAfterTransfer(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore);
				}
				cachedSourceDaysWorthOfFood = (days: item, tillRot: item2);
			}
			return cachedSourceDaysWorthOfFood;
		}
	}

	private (ThingDef food, float perDay) SourceForagedFoodPerDay
	{
		get
		{
			if (sourceForagedFoodPerDayDirty)
			{
				sourceForagedFoodPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedSourceForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDayLeftAfterTransfer(transferables, Biome, Faction.OfPlayer, stringBuilder);
				cachedSourceForagedFoodPerDayExplanation = stringBuilder.ToString();
			}
			return cachedSourceForagedFoodPerDay;
		}
	}

	private float SourceVisibility
	{
		get
		{
			if (sourceVisibilityDirty)
			{
				sourceVisibilityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedSourceVisibility = CaravanVisibilityCalculator.VisibilityLeftAfterTransfer(transferables, stringBuilder);
				cachedSourceVisibilityExplanation = stringBuilder.ToString();
			}
			return cachedSourceVisibility;
		}
	}

	private float DestMassUsage
	{
		get
		{
			if (destMassUsageDirty)
			{
				destMassUsageDirty = false;
				cachedDestMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.Ignore);
			}
			return cachedDestMassUsage;
		}
	}

	private float DestMassCapacity
	{
		get
		{
			if (destMassCapacityDirty)
			{
				destMassCapacityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedDestMassCapacity = CollectionsMassCalculator.CapacityTransferables(transferables, stringBuilder);
				cachedDestMassCapacityExplanation = stringBuilder.ToString();
			}
			return cachedDestMassCapacity;
		}
	}

	private float DestTilesPerDay
	{
		get
		{
			if (destTilesPerDayDirty)
			{
				destTilesPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedDestTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, DestMassUsage, DestMassCapacity, caravan.Tile, caravan.pather.Moving ? caravan.pather.nextTile : PlanetTile.Invalid, caravan.Shuttle != null, stringBuilder);
				cachedDestTilesPerDayExplanation = stringBuilder.ToString();
			}
			return cachedDestTilesPerDay;
		}
	}

	private (float days, float tillRot) DestDaysWorthOfFood
	{
		get
		{
			if (destDaysWorthOfFoodDirty)
			{
				destDaysWorthOfFoodDirty = false;
				float item;
				float item2;
				if (caravan.pather.Moving)
				{
					item = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.Faction, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove);
					item2 = DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove);
				}
				else
				{
					item = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore, caravan.Faction);
					item2 = DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, caravan.Tile, IgnorePawnsInventoryMode.Ignore);
				}
				cachedDestDaysWorthOfFood = (days: item, tillRot: item2);
			}
			return cachedDestDaysWorthOfFood;
		}
	}

	private (ThingDef food, float perDay) DestForagedFoodPerDay
	{
		get
		{
			if (destForagedFoodPerDayDirty)
			{
				destForagedFoodPerDayDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedDestForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables, Biome, Faction.OfPlayer, stringBuilder);
				cachedDestForagedFoodPerDayExplanation = stringBuilder.ToString();
			}
			return cachedDestForagedFoodPerDay;
		}
	}

	private float DestVisibility
	{
		get
		{
			if (destVisibilityDirty)
			{
				destVisibilityDirty = false;
				StringBuilder stringBuilder = new StringBuilder();
				cachedDestVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
				cachedDestVisibilityExplanation = stringBuilder.ToString();
			}
			return cachedDestVisibility;
		}
	}

	private int TicksToArrive
	{
		get
		{
			if (!caravan.pather.Moving)
			{
				return 0;
			}
			if (ticksToArriveDirty)
			{
				ticksToArriveDirty = false;
				cachedTicksToArrive = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(caravan, allowCaching: false);
			}
			return cachedTicksToArrive;
		}
	}

	public Dialog_SplitCaravan(Caravan caravan)
	{
		this.caravan = caravan;
		forcePause = true;
		absorbInputAroundWindow = true;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		CalculateAndRecacheTransferables();
	}

	public override void DoWindowContents(Rect inRect)
	{
		Rect rect = new Rect(0f, 0f, inRect.width, 35f);
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect, "SplitCaravan".Translate());
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.UpperLeft;
		CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(SourceMassUsage, SourceMassCapacity, cachedSourceMassCapacityExplanation, SourceTilesPerDay, cachedSourceTilesPerDayExplanation, SourceDaysWorthOfFood, SourceForagedFoodPerDay, cachedSourceForagedFoodPerDayExplanation, SourceVisibility, cachedSourceVisibilityExplanation), new CaravanUIUtility.CaravanInfo(DestMassUsage, DestMassCapacity, cachedDestMassCapacityExplanation, DestTilesPerDay, cachedDestTilesPerDayExplanation, DestDaysWorthOfFood, DestForagedFoodPerDay, cachedDestForagedFoodPerDayExplanation, DestVisibility, cachedDestVisibilityExplanation), caravan.Tile, caravan.pather.Moving ? new int?(TicksToArrive) : ((int?)null), -9999f, new Rect(12f, 35f, inRect.width - 24f, 40f));
		tabsList.Clear();
		tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
		{
			tab = Tab.Pawns;
		}, tab == Tab.Pawns));
		tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
		{
			tab = Tab.Items;
		}, tab == Tab.Items));
		tabsList.Add(new TabRecord("TravelSupplies".Translate(), delegate
		{
			tab = Tab.FoodAndMedicine;
		}, tab == Tab.FoodAndMedicine));
		inRect.yMin += 119f;
		Widgets.DrawMenuSection(inRect);
		TabDrawer.DrawTabs(inRect, tabsList);
		inRect = inRect.ContractedBy(17f);
		Widgets.BeginGroup(inRect);
		Rect rect2 = inRect.AtZero();
		DoBottomButtons(rect2);
		Rect inRect2 = rect2;
		inRect2.yMax -= 59f;
		bool anythingChanged = false;
		switch (tab)
		{
		case Tab.Pawns:
			pawnsTransfer.OnGUI(inRect2, out anythingChanged);
			break;
		case Tab.Items:
			itemsTransfer.OnGUI(inRect2, out anythingChanged);
			break;
		case Tab.FoodAndMedicine:
			foodAndMedicineTransfer.OnGUI(inRect2, out anythingChanged);
			break;
		}
		if (anythingChanged)
		{
			CountToTransferChanged();
		}
		Widgets.EndGroup();
	}

	public override bool CausesMessageBackground()
	{
		return true;
	}

	private void AddToTransferables(Thing t)
	{
		TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.Normal);
		if (transferableOneWay == null)
		{
			transferableOneWay = new TransferableOneWay();
			transferables.Add(transferableOneWay);
		}
		if (transferableOneWay.things.Contains(t))
		{
			Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
		}
		else
		{
			transferableOneWay.things.Add(t);
		}
	}

	private void DoBottomButtons(Rect rect)
	{
		Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f, BottomButtonSize.x, BottomButtonSize.y);
		if (Widgets.ButtonText(rect2, "AcceptButton".Translate()) && TrySplitCaravan())
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
			Close(doCloseSound: false);
		}
		if (Widgets.ButtonText(new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate()))
		{
			SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			CalculateAndRecacheTransferables();
		}
		if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate()))
		{
			Close();
		}
	}

	private void CalculateAndRecacheTransferables()
	{
		transferables = new List<TransferableOneWay>();
		AddPawnsToTransferables();
		AddItemsToTransferables();
		CaravanUIUtility.CreateCaravanTransferableWidgets(transferables, out pawnsTransfer, out itemsTransfer, out foodAndMedicineTransfer, "SplitCaravanThingCountTip".Translate(), IgnorePawnsInventoryMode.Ignore, () => DestMassCapacity - DestMassUsage, ignoreSpawnedCorpsesGearAndInventoryMass: false, caravan.Tile);
		CountToTransferChanged();
	}

	private bool TrySplitCaravan()
	{
		List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
		if (!CheckForErrors(pawnsFromTransferables))
		{
			return false;
		}
		for (int i = 0; i < pawnsFromTransferables.Count; i++)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawnsFromTransferables[i], caravan.PawnsListForReading, pawnsFromTransferables);
		}
		for (int j = 0; j < pawnsFromTransferables.Count; j++)
		{
			caravan.RemovePawn(pawnsFromTransferables[j]);
		}
		Caravan newCaravan = CaravanMaker.MakeCaravan(pawnsFromTransferables, caravan.Faction, caravan.Tile, addToWorldPawnsIfNotAlready: true);
		transferables.RemoveAll((TransferableOneWay x) => x.AnyThing is Pawn);
		for (int num = 0; num < transferables.Count; num++)
		{
			TransferableUtility.TransferNoSplit(transferables[num].things, transferables[num].CountToTransfer, delegate(Thing thing, int numToTake)
			{
				Pawn ownerOf = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
				if (ownerOf == null)
				{
					Log.Error("Error while splitting a caravan: Thing " + thing?.ToString() + " has no owner. Where did it come from then?");
				}
				else
				{
					CaravanInventoryUtility.MoveInventoryToSomeoneElse(ownerOf, thing, newCaravan.PawnsListForReading, null, numToTake);
				}
			});
		}
		return true;
	}

	private bool CheckForErrors(List<Pawn> pawns)
	{
		if (!pawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed))
		{
			if (ModsConfig.IdeologyActive)
			{
				Messages.Message("CaravanMustHaveAtLeastOneNonSlaveColonist".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), caravan, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		if (!AnyNonDownedColonistLeftInSourceCaravan(pawns))
		{
			if (ModsConfig.IdeologyActive)
			{
				Messages.Message("SourceCaravanMustHaveAtLeastOneNonSlaveColonist".Translate(), caravan, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Messages.Message("SourceCaravanMustHaveAtLeastOneColonist".Translate(), caravan, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	private void AddPawnsToTransferables()
	{
		List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
		for (int i = 0; i < pawnsListForReading.Count; i++)
		{
			AddToTransferables(pawnsListForReading[i]);
		}
	}

	private void AddItemsToTransferables()
	{
		List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
		for (int i = 0; i < list.Count; i++)
		{
			AddToTransferables(list[i]);
		}
	}

	private bool AnyNonDownedColonistLeftInSourceCaravan(List<Pawn> pawnsToTransfer)
	{
		return transferables.Any((TransferableOneWay x) => x.things.Any((Thing y) => y is Pawn pawn && CaravanUtility.IsOwner(pawn, Faction.OfPlayer) && !pawn.Downed && !pawnsToTransfer.Contains(pawn)));
	}

	private void CountToTransferChanged()
	{
		sourceMassUsageDirty = true;
		sourceMassCapacityDirty = true;
		sourceTilesPerDayDirty = true;
		sourceDaysWorthOfFoodDirty = true;
		sourceForagedFoodPerDayDirty = true;
		sourceVisibilityDirty = true;
		destMassUsageDirty = true;
		destMassCapacityDirty = true;
		destTilesPerDayDirty = true;
		destDaysWorthOfFoodDirty = true;
		destForagedFoodPerDayDirty = true;
		destVisibilityDirty = true;
		ticksToArriveDirty = true;
	}
}
