using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_FormCaravan : Window
	{
		private enum Tab
		{
			Pawns,
			Items
		}

		private Map map;

		private bool reform;

		private Action onClosed;

		private bool canChooseRoute;

		private bool mapAboutToBeRemoved;

		public bool choosingRoute;

		private bool thisWindowInstanceEverOpened;

		public List<TransferableOneWay> transferables;

		private TransferableOneWayWidget pawnsTransfer;

		private TransferableOneWayWidget itemsTransfer;

		private Tab tab;

		private float lastMassFlashTime = -9999f;

		private int startingTile = -1;

		private int destinationTile = -1;

		private bool massUsageDirty = true;

		private float cachedMassUsage;

		private bool massCapacityDirty = true;

		private float cachedMassCapacity;

		private string cachedMassCapacityExplanation;

		private bool tilesPerDayDirty = true;

		private float cachedTilesPerDay;

		private string cachedTilesPerDayExplanation;

		private bool daysWorthOfFoodDirty = true;

		private Pair<float, float> cachedDaysWorthOfFood;

		private bool foragedFoodPerDayDirty = true;

		private Pair<ThingDef, float> cachedForagedFoodPerDay;

		private string cachedForagedFoodPerDayExplanation;

		private bool visibilityDirty = true;

		private float cachedVisibility;

		private string cachedVisibilityExplanation;

		private bool ticksToArriveDirty = true;

		private int cachedTicksToArrive;

		private const float TitleRectHeight = 35f;

		private const float BottomAreaHeight = 55f;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		private const float MaxDaysWorthOfFoodToShowWarningDialog = 5f;

		private static List<TabRecord> tabsList = new List<TabRecord>();

		private static List<Thing> tmpPackingSpots = new List<Thing>();

		public int CurrentTile => map.Tile;

		public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

		protected override float Margin => 0f;

		private bool AutoStripSpawnedCorpses => reform;

		private bool ListPlayerPawnsInventorySeparately => reform;

		private BiomeDef Biome => map.Biome;

		private bool MustChooseRoute
		{
			get
			{
				if (canChooseRoute)
				{
					if (reform)
					{
						return map.Parent is Settlement;
					}
					return true;
				}
				return false;
			}
		}

		private bool ShowCancelButton
		{
			get
			{
				if (!mapAboutToBeRemoved)
				{
					return true;
				}
				bool flag = false;
				for (int i = 0; i < transferables.Count; i++)
				{
					Pawn pawn = transferables[i].AnyThing as Pawn;
					if (pawn != null && pawn.IsColonist && !pawn.Downed)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
				return false;
			}
		}

		private IgnorePawnsInventoryMode IgnoreInventoryMode
		{
			get
			{
				if (!ListPlayerPawnsInventorySeparately)
				{
					return IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload;
				}
				return IgnorePawnsInventoryMode.IgnoreIfAssignedToUnloadOrPlayerPawn;
			}
		}

		public float MassUsage
		{
			get
			{
				if (massUsageDirty)
				{
					massUsageDirty = false;
					cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnoreInventoryMode, includePawnsMass: false, AutoStripSpawnedCorpses);
				}
				return cachedMassUsage;
			}
		}

		public float MassCapacity
		{
			get
			{
				if (massCapacityDirty)
				{
					massCapacityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedMassCapacity = CollectionsMassCalculator.CapacityTransferables(transferables, stringBuilder);
					cachedMassCapacityExplanation = stringBuilder.ToString();
				}
				return cachedMassCapacity;
			}
		}

		private float TilesPerDay
		{
			get
			{
				if (tilesPerDayDirty)
				{
					tilesPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, MassUsage, MassCapacity, CurrentTile, startingTile, stringBuilder);
					cachedTilesPerDayExplanation = stringBuilder.ToString();
				}
				return cachedTilesPerDay;
			}
		}

		private Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (daysWorthOfFoodDirty)
				{
					daysWorthOfFoodDirty = false;
					float first;
					float second;
					if (destinationTile != -1)
					{
						using (WorldPath path = Find.WorldPathFinder.FindPath(CurrentTile, destinationTile, null))
						{
							int ticksPerMove = CaravanTicksPerMoveUtility.GetTicksPerMove(new CaravanTicksPerMoveUtility.CaravanInfo(this));
							first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, CurrentTile, IgnoreInventoryMode, Faction.OfPlayer, path, 0f, ticksPerMove);
							second = DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, CurrentTile, IgnoreInventoryMode, path, 0f, ticksPerMove);
						}
					}
					else
					{
						first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, CurrentTile, IgnoreInventoryMode, Faction.OfPlayer);
						second = DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, CurrentTile, IgnoreInventoryMode);
					}
					cachedDaysWorthOfFood = new Pair<float, float>(first, second);
				}
				return cachedDaysWorthOfFood;
			}
		}

		private Pair<ThingDef, float> ForagedFoodPerDay
		{
			get
			{
				if (foragedFoodPerDayDirty)
				{
					foragedFoodPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables, Biome, Faction.OfPlayer, stringBuilder);
					cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
				}
				return cachedForagedFoodPerDay;
			}
		}

		private float Visibility
		{
			get
			{
				if (visibilityDirty)
				{
					visibilityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
					cachedVisibilityExplanation = stringBuilder.ToString();
				}
				return cachedVisibility;
			}
		}

		private int TicksToArrive
		{
			get
			{
				if (destinationTile == -1)
				{
					return 0;
				}
				if (ticksToArriveDirty)
				{
					ticksToArriveDirty = false;
					using (WorldPath path = Find.WorldPathFinder.FindPath(CurrentTile, destinationTile, null))
					{
						cachedTicksToArrive = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(CurrentTile, destinationTile, path, 0f, CaravanTicksPerMoveUtility.GetTicksPerMove(new CaravanTicksPerMoveUtility.CaravanInfo(this)), Find.TickManager.TicksAbs);
					}
				}
				return cachedTicksToArrive;
			}
		}

		private bool MostFoodWillRotSoon
		{
			get
			{
				float num = 0f;
				float num2 = 0f;
				for (int i = 0; i < transferables.Count; i++)
				{
					TransferableOneWay transferableOneWay = transferables[i];
					if (transferableOneWay.HasAnyThing && transferableOneWay.CountToTransfer > 0 && transferableOneWay.ThingDef.IsNutritionGivingIngestible && !(transferableOneWay.AnyThing is Corpse))
					{
						float num3 = 600f;
						CompRottable compRottable = transferableOneWay.AnyThing.TryGetComp<CompRottable>();
						if (compRottable != null)
						{
							num3 = (float)DaysUntilRotCalculator.ApproxTicksUntilRot_AssumeTimePassesBy(compRottable, CurrentTile) / 60000f;
						}
						float num4 = transferableOneWay.ThingDef.GetStatValueAbstract(StatDefOf.Nutrition) * (float)transferableOneWay.CountToTransfer;
						if (num3 < 5f)
						{
							num += num4;
						}
						else
						{
							num2 += num4;
						}
					}
				}
				if (num == 0f && num2 == 0f)
				{
					return false;
				}
				return num / (num + num2) >= 0.75f;
			}
		}

		public Dialog_FormCaravan(Map map, bool reform = false, Action onClosed = null, bool mapAboutToBeRemoved = false)
		{
			this.map = map;
			this.reform = reform;
			this.onClosed = onClosed;
			this.mapAboutToBeRemoved = mapAboutToBeRemoved;
			canChooseRoute = (!reform || !map.retainedCaravanData.HasDestinationTile);
			closeOnAccept = !reform;
			closeOnCancel = !reform;
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			choosingRoute = false;
			if (!thisWindowInstanceEverOpened)
			{
				thisWindowInstanceEverOpened = true;
				CalculateAndRecacheTransferables();
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.FormCaravan, KnowledgeAmount.Total);
			}
		}

		public override void PostClose()
		{
			base.PostClose();
			if (onClosed != null && !choosingRoute)
			{
				onClosed();
			}
		}

		public void Notify_NoLongerChoosingRoute()
		{
			choosingRoute = false;
			if (!Find.WindowStack.IsOpen(this) && onClosed != null)
			{
				onClosed();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 35f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, (reform ? "ReformCaravan" : "FormCaravan").Translate());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, cachedMassCapacityExplanation, TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation), null, CurrentTile, (destinationTile == -1) ? null : new int?(TicksToArrive), lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), lerpMassColor: true, (destinationTile == -1) ? ((TaggedString)null) : ("\n" + "DaysWorthOfFoodTooltip_OnlyFirstWaypoint".Translate()));
			tabsList.Clear();
			tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
			{
				tab = Tab.Pawns;
			}, tab == Tab.Pawns));
			tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
			{
				tab = Tab.Items;
			}, tab == Tab.Items));
			inRect.yMin += 119f;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, tabsList);
			tabsList.Clear();
			inRect = inRect.ContractedBy(17f);
			inRect.height += 17f;
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			DoBottomButtons(rect2);
			Rect inRect2 = rect2;
			inRect2.yMax -= 76f;
			bool anythingChanged = false;
			switch (tab)
			{
			case Tab.Pawns:
				pawnsTransfer.OnGUI(inRect2, out anythingChanged);
				break;
			case Tab.Items:
				itemsTransfer.OnGUI(inRect2, out anythingChanged);
				break;
			}
			if (anythingChanged)
			{
				CountToTransferChanged();
			}
			GUI.EndGroup();
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		public void Notify_ChoseRoute(int destinationTile)
		{
			this.destinationTile = destinationTile;
			startingTile = CaravanExitMapUtility.BestExitTileToGoTo(destinationTile, map);
			ticksToArriveDirty = true;
			daysWorthOfFoodDirty = true;
			Messages.Message("MessageChoseRoute".Translate(), MessageTypeDefOf.CautionInput, historical: false);
			soundAppear.PlayOneShotOnCamera();
		}

		private void AddToTransferables(Thing t, bool setToTransferMax = false)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				transferables.Add(transferableOneWay);
			}
			if (transferableOneWay.things.Contains(t))
			{
				Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
				return;
			}
			transferableOneWay.things.Add(t);
			if (setToTransferMax)
			{
				transferableOneWay.AdjustTo(transferableOneWay.CountToTransfer + t.stackCount);
			}
		}

		private void DoBottomButtons(Rect rect)
		{
			Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f - 17f, BottomButtonSize.x, BottomButtonSize.y);
			if (Widgets.ButtonText(rect2, "AcceptButton".Translate()))
			{
				if (reform)
				{
					if (TryReformCaravan())
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						Close(doCloseSound: false);
					}
				}
				else
				{
					List<string> list = new List<string>();
					Pair<float, float> daysWorthOfFood = DaysWorthOfFood;
					if (daysWorthOfFood.First < 5f)
					{
						list.Add((daysWorthOfFood.First < 0.1f) ? "DaysWorthOfFoodWarningDialog_NoFood".Translate() : "DaysWorthOfFoodWarningDialog".Translate(daysWorthOfFood.First.ToString("0.#")));
					}
					else if (MostFoodWillRotSoon)
					{
						list.Add("CaravanFoodWillRotSoonWarningDialog".Translate());
					}
					if (!TransferableUtility.GetPawnsFromTransferables(transferables).Any((Pawn pawn) => CaravanUtility.IsOwner(pawn, Faction.OfPlayer) && !pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled))
					{
						list.Add("CaravanIncapableOfSocial".Translate());
					}
					if (list.Count > 0)
					{
						if (CheckForErrors(TransferableUtility.GetPawnsFromTransferables(transferables)))
						{
							string str2 = string.Concat(list.Select((string str) => str + "\n\n").ToArray()) + "CaravanAreYouSure".Translate();
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(str2, delegate
							{
								if (TryFormAndSendCaravan())
								{
									Close(doCloseSound: false);
								}
							}));
						}
					}
					else if (TryFormAndSendCaravan())
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						Close(doCloseSound: false);
					}
				}
			}
			if (Widgets.ButtonText(new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate()))
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				CalculateAndRecacheTransferables();
			}
			if (ShowCancelButton && Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate()))
			{
				Close();
			}
			if (canChooseRoute)
			{
				Rect rect3 = new Rect(rect.width - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y);
				if (Widgets.ButtonText(rect3, "ChooseRouteButton".Translate()))
				{
					List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
					soundClose.PlayOneShotOnCamera();
					if (!pawnsFromTransferables.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed))
					{
						Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						Find.WorldRoutePlanner.Start(this);
					}
				}
				if (destinationTile != -1)
				{
					Rect rect4 = rect3;
					rect4.y += rect3.height + 4f;
					rect4.height = 200f;
					rect4.xMin -= 200f;
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect4, "CaravanEstimatedDaysToDestination".Translate(((float)TicksToArrive / 60000f).ToString("0.#")));
					Text.Anchor = TextAnchor.UpperLeft;
				}
			}
			if (Prefs.DevMode)
			{
				float width = 200f;
				float num = BottomButtonSize.y / 2f;
				if (Widgets.ButtonText(new Rect(0f, rect.height - 55f - 17f, width, num), "Dev: Send instantly") && DebugTryFormCaravanInstantly())
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Close(doCloseSound: false);
				}
				if (Widgets.ButtonText(new Rect(0f, rect.height - 55f - 17f + num, width, num), "Dev: Select everything"))
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SetToSendEverything();
				}
			}
		}

		private void CalculateAndRecacheTransferables()
		{
			transferables = new List<TransferableOneWay>();
			AddPawnsToTransferables();
			AddItemsToTransferables();
			CaravanUIUtility.CreateCaravanTransferableWidgets(transferables, out pawnsTransfer, out itemsTransfer, "FormCaravanColonyThingCountTip".Translate(), IgnoreInventoryMode, () => MassCapacity - MassUsage, AutoStripSpawnedCorpses, CurrentTile, mapAboutToBeRemoved);
			CountToTransferChanged();
		}

		private bool DebugTryFormCaravanInstantly()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			if (!pawnsFromTransferables.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			AddItemsFromTransferablesToRandomInventories(pawnsFromTransferables);
			int num = startingTile;
			if (num < 0)
			{
				num = CaravanExitMapUtility.RandomBestExitTileFrom(map);
			}
			if (num < 0)
			{
				num = CurrentTile;
			}
			CaravanFormingUtility.FormAndCreateCaravan(pawnsFromTransferables, Faction.OfPlayer, CurrentTile, num, destinationTile);
			return true;
		}

		private bool TryFormAndSendCaravan()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			if (!CheckForErrors(pawnsFromTransferables))
			{
				return false;
			}
			Direction8Way direction8WayFromTo = Find.WorldGrid.GetDirection8WayFromTo(CurrentTile, startingTile);
			if (!TryFindExitSpot(pawnsFromTransferables, reachableForEveryColonist: true, out IntVec3 spot))
			{
				if (!TryFindExitSpot(pawnsFromTransferables, reachableForEveryColonist: false, out spot))
				{
					Messages.Message("CaravanCouldNotFindExitSpot".Translate(direction8WayFromTo.LabelShort()), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				Messages.Message("CaravanCouldNotFindReachableExitSpot".Translate(direction8WayFromTo.LabelShort()), new GlobalTargetInfo(spot, map), MessageTypeDefOf.CautionInput, historical: false);
			}
			if (!TryFindRandomPackingSpot(spot, out IntVec3 packingSpot))
			{
				Messages.Message("CaravanCouldNotFindPackingSpot".Translate(direction8WayFromTo.LabelShort()), new GlobalTargetInfo(spot, map), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			CaravanFormingUtility.StartFormingCaravan(pawnsFromTransferables.Where((Pawn x) => !x.Downed).ToList(), pawnsFromTransferables.Where((Pawn x) => x.Downed).ToList(), Faction.OfPlayer, transferables, packingSpot, spot, startingTile, destinationTile);
			Messages.Message("CaravanFormationProcessStarted".Translate(), pawnsFromTransferables[0], MessageTypeDefOf.PositiveEvent, historical: false);
			return true;
		}

		private bool TryReformCaravan()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			if (!CheckForErrors(pawnsFromTransferables))
			{
				return false;
			}
			AddItemsFromTransferablesToRandomInventories(pawnsFromTransferables);
			Caravan caravan = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawnsFromTransferables, Faction.OfPlayer, CurrentTile, CurrentTile, destinationTile, sendMessage: false);
			map.Parent.CheckRemoveMapNow();
			TaggedString taggedString = "MessageReformedCaravan".Translate();
			if (caravan.pather.Moving && caravan.pather.ArrivalAction != null)
			{
				taggedString += " " + "MessageFormedCaravan_Orders".Translate() + ": " + caravan.pather.ArrivalAction.Label + ".";
			}
			Messages.Message(taggedString, caravan, MessageTypeDefOf.TaskCompletion, historical: false);
			return true;
		}

		private void AddItemsFromTransferablesToRandomInventories(List<Pawn> pawns)
		{
			transferables.RemoveAll((TransferableOneWay x) => x.AnyThing is Pawn);
			if (ListPlayerPawnsInventorySeparately)
			{
				for (int i = 0; i < pawns.Count; i++)
				{
					if (CanListInventorySeparately(pawns[i]))
					{
						ThingOwner<Thing> innerContainer = pawns[i].inventory.innerContainer;
						for (int num = innerContainer.Count - 1; num >= 0; num--)
						{
							RemoveCarriedItemFromTransferablesOrDrop(innerContainer[num], pawns[i], transferables);
						}
					}
				}
				for (int j = 0; j < transferables.Count; j++)
				{
					if (transferables[j].things.Any((Thing x) => !x.Spawned))
					{
						transferables[j].things.SortBy((Thing x) => x.Spawned);
					}
				}
			}
			for (int k = 0; k < transferables.Count; k++)
			{
				if (!(transferables[k].AnyThing is Corpse))
				{
					TransferableUtility.Transfer(transferables[k].things, transferables[k].CountToTransfer, delegate(Thing splitPiece, IThingHolder originalHolder)
					{
						Thing item2 = splitPiece.TryMakeMinified();
						CaravanInventoryUtility.FindPawnToMoveInventoryTo(item2, pawns, null).inventory.innerContainer.TryAdd(item2);
					});
				}
			}
			for (int l = 0; l < transferables.Count; l++)
			{
				if (transferables[l].AnyThing is Corpse)
				{
					TransferableUtility.TransferNoSplit(transferables[l].things, transferables[l].CountToTransfer, delegate(Thing originalThing, int numToTake)
					{
						if (AutoStripSpawnedCorpses)
						{
							Corpse corpse = originalThing as Corpse;
							if (corpse != null && corpse.Spawned)
							{
								corpse.Strip();
							}
						}
						Thing item = originalThing.SplitOff(numToTake);
						CaravanInventoryUtility.FindPawnToMoveInventoryTo(item, pawns, null).inventory.innerContainer.TryAdd(item);
					});
				}
			}
		}

		private bool CheckForErrors(List<Pawn> pawns)
		{
			if (MustChooseRoute && destinationTile < 0)
			{
				Messages.Message("MessageMustChooseRouteFirst".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (!reform && startingTile < 0)
			{
				Messages.Message("MessageNoValidExitTile".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (!pawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer) && !x.Downed))
			{
				Messages.Message("CaravanMustHaveAtLeastOneColonist".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (!reform && MassUsage > MassCapacity)
			{
				FlashMass();
				Messages.Message("TooBigCaravanMassUsage".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			Pawn pawn = pawns.Find((Pawn x) => !x.IsColonist && !pawns.Any((Pawn y) => y.IsColonist && y.CanReach(x, PathEndMode.Touch, Danger.Deadly)));
			if (pawn != null)
			{
				Messages.Message("CaravanPawnIsUnreachable".Translate(pawn.LabelShort, pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].ThingDef.category != ThingCategory.Item)
				{
					continue;
				}
				int countToTransfer = transferables[i].CountToTransfer;
				int num = 0;
				if (countToTransfer <= 0)
				{
					continue;
				}
				for (int j = 0; j < transferables[i].things.Count; j++)
				{
					Thing t = transferables[i].things[j];
					if (!t.Spawned || pawns.Any((Pawn x) => x.IsColonist && x.CanReach(t, PathEndMode.Touch, Danger.Deadly)))
					{
						num += t.stackCount;
						if (num >= countToTransfer)
						{
							break;
						}
					}
				}
				if (num < countToTransfer)
				{
					if (countToTransfer == 1)
					{
						Messages.Message("CaravanItemIsUnreachableSingle".Translate(transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						Messages.Message("CaravanItemIsUnreachableMulti".Translate(countToTransfer, transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
					}
					return false;
				}
			}
			return true;
		}

		private bool TryFindExitSpot(List<Pawn> pawns, bool reachableForEveryColonist, out IntVec3 spot)
		{
			CaravanExitMapUtility.GetExitMapEdges(Find.WorldGrid, CurrentTile, startingTile, out Rot4 primary, out Rot4 secondary);
			if ((!(primary != Rot4.Invalid) || !TryFindExitSpot(pawns, reachableForEveryColonist, primary, out spot)) && (!(secondary != Rot4.Invalid) || !TryFindExitSpot(pawns, reachableForEveryColonist, secondary, out spot)) && !TryFindExitSpot(pawns, reachableForEveryColonist, primary.Rotated(RotationDirection.Clockwise), out spot))
			{
				return TryFindExitSpot(pawns, reachableForEveryColonist, primary.Rotated(RotationDirection.Counterclockwise), out spot);
			}
			return true;
		}

		private bool TryFindExitSpot(List<Pawn> pawns, bool reachableForEveryColonist, Rot4 exitDirection, out IntVec3 spot)
		{
			if (startingTile < 0)
			{
				Log.Error("Can't find exit spot because startingTile is not set.");
				spot = IntVec3.Invalid;
				return false;
			}
			Predicate<IntVec3> validator = (IntVec3 x) => !x.Fogged(map) && x.Standable(map);
			if (reachableForEveryColonist)
			{
				return CellFinder.TryFindRandomEdgeCellWith(delegate(IntVec3 x)
				{
					if (!validator(x))
					{
						return false;
					}
					for (int j = 0; j < pawns.Count; j++)
					{
						if (pawns[j].IsColonist && !pawns[j].Downed && !pawns[j].CanReach(x, PathEndMode.Touch, Danger.Deadly))
						{
							return false;
						}
					}
					return true;
				}, map, exitDirection, CellFinder.EdgeRoadChance_Always, out spot);
			}
			IntVec3 intVec = IntVec3.Invalid;
			int num = -1;
			foreach (IntVec3 item in CellRect.WholeMap(map).GetEdgeCells(exitDirection).InRandomOrder())
			{
				if (validator(item))
				{
					int num2 = 0;
					for (int i = 0; i < pawns.Count; i++)
					{
						if (pawns[i].IsColonist && !pawns[i].Downed && pawns[i].CanReach(item, PathEndMode.Touch, Danger.Deadly))
						{
							num2++;
						}
					}
					if (num2 > num)
					{
						num = num2;
						intVec = item;
					}
				}
			}
			spot = intVec;
			return intVec.IsValid;
		}

		private bool TryFindRandomPackingSpot(IntVec3 exitSpot, out IntVec3 packingSpot)
		{
			tmpPackingSpots.Clear();
			List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.CaravanPackingSpot);
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors);
			for (int i = 0; i < list.Count; i++)
			{
				if (map.reachability.CanReach(exitSpot, list[i], PathEndMode.OnCell, traverseParams))
				{
					tmpPackingSpots.Add(list[i]);
				}
			}
			if (tmpPackingSpots.Any())
			{
				Thing thing = tmpPackingSpots.RandomElement();
				tmpPackingSpots.Clear();
				packingSpot = thing.Position;
				return true;
			}
			return RCellFinder.TryFindRandomSpotJustOutsideColony(exitSpot, map, out packingSpot);
		}

		private void AddPawnsToTransferables()
		{
			List<Pawn> list = AllSendablePawns(map, reform);
			for (int i = 0; i < list.Count; i++)
			{
				bool setToTransferMax = (reform || mapAboutToBeRemoved) && !CaravanUtility.ShouldAutoCapture(list[i], Faction.OfPlayer);
				AddToTransferables(list[i], setToTransferMax);
			}
		}

		private void AddItemsToTransferables()
		{
			List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(map, reform, reform, reform);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransferables(list[i]);
			}
			if (AutoStripSpawnedCorpses)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].Spawned)
					{
						TryAddCorpseInventoryAndGearToTransferables(list[j]);
					}
				}
			}
			if (!ListPlayerPawnsInventorySeparately)
			{
				return;
			}
			List<Pawn> list2 = AllSendablePawns(map, reform);
			for (int k = 0; k < list2.Count; k++)
			{
				if (!CanListInventorySeparately(list2[k]))
				{
					continue;
				}
				ThingOwner<Thing> innerContainer = list2[k].inventory.innerContainer;
				for (int l = 0; l < innerContainer.Count; l++)
				{
					AddToTransferables(innerContainer[l], setToTransferMax: true);
					if (AutoStripSpawnedCorpses && innerContainer[l].Spawned)
					{
						TryAddCorpseInventoryAndGearToTransferables(innerContainer[l]);
					}
				}
			}
		}

		private void TryAddCorpseInventoryAndGearToTransferables(Thing potentiallyCorpse)
		{
			Corpse corpse = potentiallyCorpse as Corpse;
			if (corpse != null)
			{
				AddCorpseInventoryAndGearToTransferables(corpse);
			}
		}

		private void AddCorpseInventoryAndGearToTransferables(Corpse corpse)
		{
			Pawn_InventoryTracker inventory = corpse.InnerPawn.inventory;
			Pawn_ApparelTracker apparel = corpse.InnerPawn.apparel;
			Pawn_EquipmentTracker equipment = corpse.InnerPawn.equipment;
			for (int i = 0; i < inventory.innerContainer.Count; i++)
			{
				AddToTransferables(inventory.innerContainer[i]);
			}
			if (apparel != null)
			{
				List<Apparel> wornApparel = apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					AddToTransferables(wornApparel[j]);
				}
			}
			if (equipment != null)
			{
				List<ThingWithComps> allEquipmentListForReading = equipment.AllEquipmentListForReading;
				for (int k = 0; k < allEquipmentListForReading.Count; k++)
				{
					AddToTransferables(allEquipmentListForReading[k]);
				}
			}
		}

		private void RemoveCarriedItemFromTransferablesOrDrop(Thing carried, Pawn carrier, List<TransferableOneWay> transferables)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(carried, transferables, TransferAsOneMode.PodsOrCaravanPacking);
			int num;
			if (transferableOneWay == null)
			{
				num = carried.stackCount;
			}
			else if (transferableOneWay.CountToTransfer >= carried.stackCount)
			{
				transferableOneWay.AdjustBy(-carried.stackCount);
				transferableOneWay.things.Remove(carried);
				num = 0;
			}
			else
			{
				num = carried.stackCount - transferableOneWay.CountToTransfer;
				transferableOneWay.AdjustTo(0);
			}
			if (num > 0)
			{
				Thing thing = carried.SplitOff(num);
				if (carrier.SpawnedOrAnyParentSpawned)
				{
					GenPlace.TryPlaceThing(thing, carrier.PositionHeld, carrier.MapHeld, ThingPlaceMode.Near);
				}
				else
				{
					thing.Destroy();
				}
			}
		}

		private void FlashMass()
		{
			lastMassFlashTime = Time.time;
		}

		public static bool CanListInventorySeparately(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer)
			{
				return p.HostFaction == Faction.OfPlayer;
			}
			return true;
		}

		private void SetToSendEverything()
		{
			for (int i = 0; i < transferables.Count; i++)
			{
				transferables[i].AdjustTo(transferables[i].GetMaximumToTransfer());
			}
			CountToTransferChanged();
		}

		private void CountToTransferChanged()
		{
			massUsageDirty = true;
			massCapacityDirty = true;
			tilesPerDayDirty = true;
			daysWorthOfFoodDirty = true;
			foragedFoodPerDayDirty = true;
			visibilityDirty = true;
			ticksToArriveDirty = true;
		}

		public static List<Pawn> AllSendablePawns(Map map, bool reform)
		{
			return CaravanFormingUtility.AllSendablePawns(map, allowEvenIfDowned: true, reform, reform, reform);
		}
	}
}
