using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class Caravan : WorldObject, IThingHolder, IIncidentTarget, ILoadReferenceable, ITrader
{
	private int uniqueId = -1;

	private string nameInt;

	public ThingOwner<Pawn> pawns;

	public bool autoJoinable;

	public Caravan_PathFollower pather;

	public Caravan_GotoMoteRenderer gotoMote;

	public Caravan_Tweener tweener;

	public Caravan_TraderTracker trader;

	public Caravan_ForageTracker forage;

	public Caravan_NeedsTracker needs;

	public Caravan_CarryTracker carryTracker;

	public Caravan_BedsTracker beds;

	public Caravan_BabyTracker babies;

	public StoryState storyState;

	private Material cachedMat;

	private bool cachedImmobilized;

	private int cachedImmobilizedForTicks = -99999;

	private (float days, float tillRot) cachedDaysWorthOfFood;

	private int cachedDaysWorthOfFoodForTicks = -99999;

	public bool notifiedOutOfFood;

	private const int ImmobilizedCacheDuration = 60;

	private const int DaysWorthOfFoodCacheDuration = 3000;

	private static readonly Texture2D SplitCommand = ContentFinder<Texture2D>.Get("UI/Commands/SplitCaravan");

	private static readonly Color PlayerCaravanColor = new Color(1f, 0.863f, 0.33f);

	public bool hasShuttleDirty = true;

	private Building_PassengerShuttle cachedShuttle;

	public List<Pawn> PawnsListForReading => pawns.InnerListForReading;

	public override float DrawAltitude => 0.035f;

	public override Material Material
	{
		get
		{
			if (cachedMat == null)
			{
				cachedMat = MaterialPool.MatFrom(color: (base.Faction == null) ? Color.white : ((!base.Faction.IsPlayer) ? base.Faction.Color : PlayerCaravanColor), texPath: def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, renderQueue: 3600);
			}
			return cachedMat;
		}
	}

	public string Name
	{
		get
		{
			return nameInt;
		}
		set
		{
			nameInt = value;
		}
	}

	public override Vector3 DrawPos => tweener.TweenedPos;

	public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

	public bool ImmobilizedByMass
	{
		get
		{
			if (Find.TickManager.TicksGame - cachedImmobilizedForTicks < 60)
			{
				return cachedImmobilized;
			}
			cachedImmobilized = MassUsage > MassCapacity;
			cachedImmobilizedForTicks = Find.TickManager.TicksGame;
			return cachedImmobilized;
		}
	}

	public (float days, float tillRot) DaysWorthOfFood
	{
		get
		{
			if (Find.TickManager.TicksGame - cachedDaysWorthOfFoodForTicks < 3000)
			{
				return cachedDaysWorthOfFood;
			}
			cachedDaysWorthOfFood = (days: DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this), tillRot: DaysUntilRotCalculator.ApproxDaysUntilRot(this));
			cachedDaysWorthOfFoodForTicks = Find.TickManager.TicksGame;
			return cachedDaysWorthOfFood;
		}
	}

	public bool CantMove
	{
		get
		{
			if (!NightResting && !pather.Paused && !AllOwnersHaveMentalBreak && !AllOwnersDowned && !ImmobilizedByMass)
			{
				return Shuttle != null;
			}
			return true;
		}
	}

	public float MassCapacity => CollectionsMassCalculator.Capacity(PawnsListForReading);

	public string MassCapacityExplanation
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			CollectionsMassCalculator.Capacity(PawnsListForReading, stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public float MassUsage => CollectionsMassCalculator.MassUsage(PawnsListForReading, IgnorePawnsInventoryMode.DontIgnore);

	public bool AllOwnersDowned
	{
		get
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (IsOwner(pawns[i]) && !pawns[i].Downed)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool AllOwnersHaveMentalBreak
	{
		get
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (IsOwner(pawns[i]) && !pawns[i].InMentalState)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool NightResting
	{
		get
		{
			if (!base.Spawned)
			{
				return false;
			}
			if (!needs.AnyPawnsNeedRest)
			{
				return false;
			}
			if (pather.Moving && pather.nextTile == pather.Destination && Caravan_PathFollower.IsValidFinalPushDestination(pather.Destination) && Mathf.CeilToInt(pather.nextTileCostLeft / 1f) <= 10000)
			{
				return false;
			}
			return CaravanNightRestUtility.RestingNowAt(base.Tile);
		}
	}

	public int LeftRestTicks
	{
		get
		{
			if (!NightResting)
			{
				return 0;
			}
			return CaravanNightRestUtility.LeftRestTicksAt(base.Tile);
		}
	}

	public int LeftNonRestTicks
	{
		get
		{
			if (NightResting)
			{
				return 0;
			}
			return CaravanNightRestUtility.LeftNonRestTicksAt(base.Tile);
		}
	}

	public override string Label
	{
		get
		{
			if (nameInt != null)
			{
				return nameInt;
			}
			return base.Label;
		}
	}

	public override bool HasName => !nameInt.NullOrEmpty();

	public int TicksPerMove => CaravanTicksPerMoveUtility.GetTicksPerMove(this);

	public override bool AppendFactionToInspectString => false;

	public float Visibility => CaravanVisibilityCalculator.Visibility(this);

	public string VisibilityExplanation
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			CaravanVisibilityCalculator.Visibility(this, stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public string TicksPerMoveExplanation
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			CaravanTicksPerMoveUtility.GetTicksPerMove(this, stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public IEnumerable<Thing> AllThings => CaravanInventoryUtility.AllInventoryItems(this).Concat(pawns);

	public Building_PassengerShuttle Shuttle
	{
		get
		{
			if (hasShuttleDirty)
			{
				hasShuttleDirty = false;
				cachedShuttle = CaravanInventoryUtility.FindShuttle(this);
			}
			return cachedShuttle;
		}
	}

	public int ConstantRandSeed => uniqueId ^ 0x2B6813E1;

	public StoryState StoryState => storyState;

	public GameConditionManager GameConditionManager
	{
		get
		{
			Log.ErrorOnce("Attempted to retrieve condition manager directly from caravan", 13291050);
			return null;
		}
	}

	public float PlayerWealthForStoryteller
	{
		get
		{
			if (!IsPlayerControlled)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < pawns.Count; i++)
			{
				num += WealthWatcher.GetEquipmentApparelAndInventoryWealth(pawns[i]);
				if (pawns[i].Faction == Faction.OfPlayer)
				{
					float num2 = pawns[i].MarketValue;
					if (pawns[i].IsSlave)
					{
						num2 *= 0.75f;
					}
					num += num2;
				}
			}
			return num * 0.7f;
		}
	}

	public IEnumerable<Pawn> PlayerPawnsForStoryteller
	{
		get
		{
			if (!IsPlayerControlled)
			{
				return Enumerable.Empty<Pawn>();
			}
			return PawnsListForReading.Where((Pawn x) => x.Faction == Faction.OfPlayer);
		}
	}

	public FloatRange IncidentPointsRandomFactorRange => StorytellerUtility.CaravanPointsRandomFactorRange;

	public TraderKindDef TraderKind => trader.TraderKind;

	public IEnumerable<Thing> Goods => trader.Goods;

	public int RandomPriceFactorSeed => trader.RandomPriceFactorSeed;

	public string TraderName => trader.TraderName;

	public bool CanTradeNow => trader.CanTradeNow;

	public float TradePriceImprovementOffsetForPlayer => 0f;

	public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

	public void SetUniqueId(int newId)
	{
		if (uniqueId != -1 || newId < 0)
		{
			Log.Error("Tried to set caravan with uniqueId " + uniqueId + " to have uniqueId " + newId);
		}
		uniqueId = newId;
	}

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		return trader.ColonyThingsWillingToBuy(playerNegotiator);
	}

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
	}

	public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
	}

	public Caravan()
	{
		pawns = new ThingOwner<Pawn>(this, oneStackOnly: false, LookMode.Reference);
		pather = new Caravan_PathFollower(this);
		gotoMote = new Caravan_GotoMoteRenderer();
		tweener = new Caravan_Tweener(this);
		trader = new Caravan_TraderTracker(this);
		forage = new Caravan_ForageTracker(this);
		needs = new Caravan_NeedsTracker(this);
		carryTracker = new Caravan_CarryTracker(this);
		beds = new Caravan_BedsTracker(this);
		if (ModsConfig.BiotechActive)
		{
			babies = new Caravan_BabyTracker(this);
		}
		storyState = new StoryState(this);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			pawns.RemoveAll((Pawn x) => x.Destroyed);
		}
		Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
		Scribe_Values.Look(ref nameInt, "name");
		Scribe_Deep.Look(ref pawns, "pawns", this);
		Scribe_Values.Look(ref autoJoinable, "autoJoinable", defaultValue: false);
		Scribe_Deep.Look(ref pather, "pather", this);
		Scribe_Deep.Look(ref trader, "trader", this);
		Scribe_Deep.Look(ref forage, "forage", this);
		Scribe_Deep.Look(ref needs, "needs", this);
		Scribe_Deep.Look(ref carryTracker, "carryTracker", this);
		Scribe_Deep.Look(ref beds, "beds", this);
		Scribe_Deep.Look(ref storyState, "storyState", this);
		BackCompatibility.PostExposeData(this);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			babies?.Recache();
		}
	}

	public override void PostAdd()
	{
		base.PostAdd();
		carryTracker.Notify_CaravanSpawned();
		beds.Notify_CaravanSpawned();
		babies?.Recache();
		Find.ColonistBar.MarkColonistsDirty();
		hasShuttleDirty = true;
	}

	public override void PostRemove()
	{
		base.PostRemove();
		pather.StopDead();
		Find.ColonistBar.MarkColonistsDirty();
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		CheckAnyNonWorldPawns();
		pather.PatherTickInterval(delta);
		tweener.TweenerTickInterval(delta);
		forage.ForageTrackerTickInterval(delta);
		carryTracker.CarryTrackerTickInterval(delta);
		beds.BedsTrackerTickInterval(delta);
		needs.NeedsTrackerTickInterval(delta);
		babies?.TickInterval(delta);
		CaravanDrugPolicyUtility.CheckTakeScheduledDrugs(this, delta);
		CaravanTendUtility.CheckTend(this, delta);
		if (ModsConfig.BiotechActive)
		{
			CaravanPollutionUtility.CheckDamageFromPollution(this, delta);
		}
	}

	public override void SpawnSetup()
	{
		base.SpawnSetup();
		tweener.ResetTweenedPosToRoot();
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (IsPlayerControlled && pather.curPath != null)
		{
			pather.curPath.DrawPath(this);
		}
		gotoMote.RenderMote();
	}

	public void AddPawn(Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
	{
		if (p == null)
		{
			Log.Warning("Tried to add a null pawn to " + this);
			return;
		}
		if (p.Dead)
		{
			Log.Warning("Tried to add " + p?.ToString() + " to " + this?.ToString() + ", but this pawn is dead.");
			return;
		}
		Pawn pawn = p.carryTracker.CarriedThing as Pawn;
		if (pawn != null)
		{
			p.carryTracker.innerContainer.Remove(pawn);
		}
		p.DeSpawnOrDeselect();
		if (pawns.TryAdd(p))
		{
			if (ShouldAutoCapture(p))
			{
				p.guest.CapturedBy(base.Faction);
			}
			if (pawn != null)
			{
				if (ShouldAutoCapture(pawn))
				{
					pawn.guest.CapturedBy(base.Faction, p);
				}
				AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
				if (addCarriedPawnToWorldPawnsIfAny)
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
		}
		else
		{
			Log.Error("Couldn't add pawn " + p?.ToString() + " to caravan.");
		}
	}

	public void AddPawnOrItem(Thing thing, bool addCarriedPawnToWorldPawnsIfAny)
	{
		if (thing == null)
		{
			Log.Warning("Tried to add a null thing to " + this);
		}
		else if (thing is Pawn p)
		{
			AddPawn(p, addCarriedPawnToWorldPawnsIfAny);
		}
		else
		{
			CaravanInventoryUtility.GiveThing(this, thing);
		}
	}

	public bool ContainsPawn(Pawn p)
	{
		return pawns.Contains(p);
	}

	public void RemovePawn(Pawn p)
	{
		pawns.Remove(p);
	}

	public void RemoveAllPawns()
	{
		pawns.Clear();
	}

	public bool IsOwner(Pawn p)
	{
		if (pawns.Contains(p))
		{
			return CaravanUtility.IsOwner(p, base.Faction);
		}
		return false;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (stringBuilder.Length != 0)
		{
			stringBuilder.AppendLine();
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].IsColonist)
			{
				num++;
			}
			else if (pawns[i].RaceProps.Animal)
			{
				num2++;
			}
			else if (pawns[i].IsPrisoner)
			{
				num3++;
			}
			else if (pawns[i].RaceProps.IsMechanoid)
			{
				num6++;
			}
			if (pawns[i].Downed && !pawns[i].ageTracker.CurLifeStage.alwaysDowned)
			{
				num4++;
			}
			if (pawns[i].InMentalState)
			{
				num5++;
			}
		}
		stringBuilder.Append("CaravanColonistsCount".Translate(num, (num == 1) ? Faction.OfPlayer.def.pawnSingular : Faction.OfPlayer.def.pawnsPlural));
		if (num2 == 1)
		{
			stringBuilder.Append(", " + "CaravanAnimal".Translate());
		}
		else if (num2 > 1)
		{
			stringBuilder.Append(", " + "CaravanAnimalsCount".Translate(num2));
		}
		if (num3 == 1)
		{
			stringBuilder.Append(", " + "CaravanPrisoner".Translate());
		}
		else if (num3 > 1)
		{
			stringBuilder.Append(", " + "CaravanPrisonersCount".Translate(num3));
		}
		if (ModsConfig.BiotechActive)
		{
			if (num6 == 1)
			{
				stringBuilder.Append(", " + "CaravanMech".Translate());
			}
			else if (num6 > 1)
			{
				stringBuilder.Append(", " + "CaravanMechsCount".Translate(num6));
			}
		}
		stringBuilder.AppendLine();
		if (num5 > 0)
		{
			stringBuilder.Append("CaravanPawnsInMentalState".Translate(num5));
		}
		if (num4 > 0)
		{
			if (num5 > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append("CaravanPawnsDowned".Translate(num4));
		}
		if (num5 > 0 || num4 > 0)
		{
			stringBuilder.AppendLine();
		}
		if (pather.Moving)
		{
			if (pather.ArrivalAction != null)
			{
				stringBuilder.Append(pather.ArrivalAction.ReportString);
			}
			else
			{
				stringBuilder.Append("CaravanTraveling".Translate());
			}
		}
		else
		{
			Settlement settlement = CaravanVisitUtility.SettlementVisitedNow(this);
			if (settlement != null)
			{
				stringBuilder.Append("CaravanVisiting".Translate(settlement.Label));
			}
			else
			{
				stringBuilder.Append("CaravanWaiting".Translate());
			}
		}
		if (pather.Moving)
		{
			float num7 = (float)CaravanArrivalTimeEstimator.EstimatedTicksToArrive(this, allowCaching: true) / 60000f;
			stringBuilder.AppendLine();
			stringBuilder.Append("CaravanEstimatedTimeToDestination".Translate(num7.ToString("0.#")));
		}
		if (AllOwnersDowned)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("AllCaravanMembersDowned".Translate());
		}
		else if (AllOwnersHaveMentalBreak)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("AllCaravanMembersMentalBreak".Translate());
		}
		else if (ImmobilizedByMass)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("CaravanImmobilizedByMass".Translate());
		}
		if (needs.AnyPawnOutOfFood(out var malnutritionHediff))
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("CaravanOutOfFood".Translate());
			if (!malnutritionHediff.NullOrEmpty())
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(malnutritionHediff);
				stringBuilder.Append(".");
			}
		}
		if (!pather.MovingNow)
		{
			int usedBedCount = beds.GetUsedBedCount();
			stringBuilder.AppendLine();
			stringBuilder.Append(CaravanBedUtility.AppendUsingBedsLabel("CaravanResting".Translate(), usedBedCount));
		}
		else
		{
			string inspectStringLine = carryTracker.GetInspectStringLine();
			if (!inspectStringLine.NullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(inspectStringLine);
			}
			string inBedForMedicalReasonsInspectStringLine = beds.GetInBedForMedicalReasonsInspectStringLine();
			if (!inBedForMedicalReasonsInspectStringLine.NullOrEmpty())
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(inBedForMedicalReasonsInspectStringLine);
			}
		}
		if (ModsConfig.OdysseyActive && Shuttle != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(string.Format("{0}: {1:F0} / {2:F0}", "CaravanShuttleFuel".Translate(), Shuttle.FuelLevel, Shuttle.MaxFuelLevel));
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (Find.WorldSelector.SingleSelectedObject == this)
		{
			yield return new Gizmo_CaravanInfo(this);
		}
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (IsPlayerControlled)
		{
			if (Find.WorldSelector.SingleSelectedObject == this)
			{
				yield return SettleInEmptyTileUtility.SettleCommand(this);
				yield return SettleInEmptyTileUtility.SetupCamp(this);
				foreach (Pawn p in pawns)
				{
					if (p.royalty == null)
					{
						continue;
					}
					foreach (FactionPermit allFactionPermit in p.royalty.AllFactionPermits)
					{
						IEnumerable<Gizmo> caravanGizmos = allFactionPermit.Permit.Worker.GetCaravanGizmos(p, allFactionPermit.Faction);
						if (caravanGizmos == null)
						{
							continue;
						}
						foreach (Gizmo item in caravanGizmos)
						{
							yield return item;
						}
					}
				}
				if (PawnsListForReading.Count((Pawn x) => x.IsColonist) >= 2)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "CommandSplitCaravan".Translate();
					command_Action.defaultDesc = "CommandSplitCaravanDesc".Translate();
					command_Action.icon = SplitCommand;
					command_Action.hotKey = KeyBindingDefOf.Misc5;
					command_Action.action = delegate
					{
						Find.WindowStack.Add(new Dialog_SplitCaravan(this));
					};
					yield return command_Action;
				}
			}
			if (pather.Moving)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.hotKey = KeyBindingDefOf.Misc1;
				command_Toggle.isActive = () => pather.Paused;
				command_Toggle.toggleAction = delegate
				{
					if (pather.Moving)
					{
						pather.Paused = !pather.Paused;
					}
				};
				command_Toggle.defaultDesc = "CommandToggleCaravanPauseDesc".Translate(2f.ToString("0.#"), 0.3f.ToStringPercent());
				command_Toggle.icon = TexCommand.PauseCaravan;
				command_Toggle.defaultLabel = "CommandPauseCaravan".Translate();
				yield return command_Toggle;
			}
			if (CaravanMergeUtility.ShouldShowMergeCommand)
			{
				yield return CaravanMergeUtility.MergeCommand(this);
			}
			foreach (Gizmo gizmo2 in forage.GetGizmos())
			{
				yield return gizmo2;
			}
			foreach (WorldObject item2 in Find.WorldObjects.ObjectsAt(base.Tile))
			{
				foreach (Gizmo caravanGizmo in item2.GetCaravanGizmos(this))
				{
					yield return caravanGizmo;
				}
			}
			if (Find.WorldSelector.SingleSelectedObject == this)
			{
				foreach (Pawn p in pawns)
				{
					if (p.abilities != null && !p.Downed && !p.IsPrisoner)
					{
						foreach (Ability ability in p.abilities.abilities)
						{
							if (!ability.def.showGizmoOnWorldView)
							{
								continue;
							}
							foreach (Command gizmo3 in ability.GetGizmos())
							{
								yield return gizmo3;
							}
						}
					}
					if (p.genes == null)
					{
						continue;
					}
					foreach (Gene item3 in p.genes.GenesListForReading)
					{
						if (!item3.def.showGizmoOnWorldView)
						{
							continue;
						}
						foreach (Gizmo gizmo4 in item3.GetGizmos())
						{
							yield return gizmo4;
						}
					}
				}
			}
			if (ModsConfig.OdysseyActive && Shuttle != null && Find.WorldSelector.NumSelectedObjects == 1)
			{
				yield return GetLaunchGizmo();
				yield return GetRefuelGizmo();
				if (DebugSettings.ShowDevGizmos)
				{
					Command_Action command_Action2 = new Command_Action();
					command_Action2.defaultLabel = "DEV: End cooldown";
					command_Action2.action = delegate
					{
						Shuttle.LaunchableComp.lastLaunchTick = Find.TickManager.TicksGame - Shuttle.LaunchableComp.Props.cooldownTicks;
					};
					yield return command_Action2;
				}
			}
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		Command_Action command_Action3 = new Command_Action();
		command_Action3.defaultLabel = "DEV: Mental break";
		command_Action3.action = delegate
		{
			if (PawnsListForReading.Where((Pawn x) => x.RaceProps.Humanlike && !x.InMentalState).TryRandomElement(out var result))
			{
				result.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad);
			}
		};
		yield return command_Action3;
		Command_Action command_Action4 = new Command_Action();
		command_Action4.defaultLabel = "DEV: Make random pawn hungry";
		command_Action4.action = delegate
		{
			if (PawnsListForReading.Where((Pawn x) => x.needs.food != null).TryRandomElement(out var result))
			{
				result.needs.food.CurLevelPercentage = 0f;
			}
		};
		yield return command_Action4;
		Command_Action command_Action5 = new Command_Action();
		command_Action5.defaultLabel = "DEV: Kill random pawn";
		command_Action5.action = delegate
		{
			if (PawnsListForReading.TryRandomElement(out var result))
			{
				result.Kill(null, null);
				Messages.Message("DEV: Killed " + result.LabelShort, this, MessageTypeDefOf.TaskCompletion, historical: false);
			}
		};
		yield return command_Action5;
		if (ModsConfig.IdeologyActive)
		{
			Command_Action command_Action6 = new Command_Action();
			command_Action6.defaultLabel = "DEV: Kill all non-slave pawns";
			command_Action6.action = delegate
			{
				for (int num = PawnsListForReading.Count - 1; num >= 0; num--)
				{
					Pawn pawn = PawnsListForReading[num];
					if (!pawn.IsSlave)
					{
						pawn.Kill(null, null);
						Messages.Message("DEV: Killed " + pawn.LabelShort, this, MessageTypeDefOf.TaskCompletion, historical: false);
					}
				}
			};
			yield return command_Action6;
		}
		Command_Action command_Action7 = new Command_Action();
		command_Action7.defaultLabel = "DEV: Harm random pawn";
		command_Action7.action = delegate
		{
			if (PawnsListForReading.TryRandomElement(out var result))
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.Scratch, 10f, 999f);
				result.TakeDamage(dinfo);
			}
		};
		yield return command_Action7;
		Command_Action command_Action8 = new Command_Action();
		command_Action8.defaultLabel = "DEV: Down random pawn";
		command_Action8.action = delegate
		{
			if (PawnsListForReading.Where((Pawn x) => !x.Downed).TryRandomElement(out var result))
			{
				HealthUtility.DamageUntilDowned(result);
				Messages.Message("DEV: Downed " + result.LabelShort, this, MessageTypeDefOf.TaskCompletion, historical: false);
			}
		};
		yield return command_Action8;
		Command_Action command_Action9 = new Command_Action();
		command_Action9.defaultLabel = "DEV: Plague on random pawn";
		command_Action9.action = delegate
		{
			if (PawnsListForReading.Where((Pawn x) => !x.Downed && x.RaceProps.IsFlesh).TryRandomElement(out var result))
			{
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Plague, result);
				hediff.Severity = HediffDefOf.Plague.stages[1].minSeverity - 0.001f;
				result.health.AddHediff(hediff);
				Messages.Message("DEV: Gave advanced plague to " + result.LabelShort, this, MessageTypeDefOf.TaskCompletion, historical: false);
			}
		};
		yield return command_Action9;
		Command_Action command_Action10 = new Command_Action();
		command_Action10.defaultLabel = "DEV: Teleport to destination";
		command_Action10.action = delegate
		{
			base.Tile = pather.Destination;
			pather.StopDead();
		};
		yield return command_Action10;
		if (ModsConfig.RoyaltyActive)
		{
			Command_Action command_Action11 = new Command_Action();
			command_Action11.defaultLabel = "DEV: +20% psyfocus";
			command_Action11.action = delegate
			{
				for (int i = 0; i < PawnsListForReading.Count; i++)
				{
					PawnsListForReading[i].psychicEntropy?.OffsetPsyfocusDirectly(0.2f);
				}
			};
			yield return command_Action11;
		}
		if (!ModsConfig.BiotechActive)
		{
			yield break;
		}
		IEnumerable<Thing> allDissolvingThings = CaravanInventoryUtility.GetAllDissolvingThings(this);
		if (allDissolvingThings.Any())
		{
			Command_Action command_Action12 = new Command_Action();
			command_Action12.defaultLabel = "DEV: Trigger random dissolution event";
			command_Action12.action = delegate
			{
				allDissolvingThings.RandomElement().TryGetComp<CompDissolution>().TriggerDissolutionEvent();
			};
			yield return command_Action12;
		}
	}

	private Gizmo GetLaunchGizmo()
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandLaunchGroup".Translate();
		command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
		command_Action.icon = CompLaunchable.LaunchCommandTex;
		command_Action.action = delegate
		{
			Shuttle.LaunchableComp.StartChoosingDestination(delegate(PlanetTile tile, TransportersArrivalAction action)
			{
				CaravanShuttleUtility.LaunchShuttle(this, tile, action);
			});
		};
		AcceptanceReport acceptanceReport = CaravanShuttleUtility.CanLaunchCaravanShuttle(this);
		if (!acceptanceReport.Accepted)
		{
			command_Action.Disable(acceptanceReport.Reason);
		}
		return command_Action;
	}

	private Gizmo GetRefuelGizmo()
	{
		float fuelInCaravan = CaravanShuttleUtility.FuelInCaravan(this);
		string text = null;
		if (fuelInCaravan <= 0f)
		{
			text = "NoFuelInCaravan".Translate();
		}
		if (Mathf.Approximately(Shuttle.FuelLevel, Shuttle.MaxFuelLevel))
		{
			text = "ShuttleFullyFueled".Translate();
		}
		return new Command_Action
		{
			defaultLabel = "CommandRefuelShuttleFromCargo".Translate(),
			defaultDesc = "CommandRefuelShuttleFromCargoDesc".Translate(),
			icon = Building_PassengerShuttle.RefuelFromCargoIcon.Texture,
			action = delegate
			{
				int to = Mathf.FloorToInt(Mathf.Min(fuelInCaravan, Shuttle.MaxFuelLevel - Shuttle.FuelLevel));
				Dialog_Slider window = new Dialog_Slider((int val) => "RefuelShuttleCount".Translate(val), 1, to, delegate(int count)
				{
					CaravanShuttleUtility.ConsumeFuelFromCaravanInventory(this, count);
					Shuttle.RefuelableComp.Refuel(count);
				});
				Find.WindowStack.Add(window);
			},
			Disabled = !text.NullOrEmpty(),
			disabledReason = text
		};
	}

	public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
	{
		foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
		{
			yield return transportersFloatMenuOption;
		}
		foreach (FloatMenuOption floatMenuOption in TransportersArrivalAction_GiveToCaravan.GetFloatMenuOptions(launchAction, pods, this))
		{
			yield return floatMenuOption;
		}
	}

	public void RecacheInventory()
	{
		cachedImmobilizedForTicks = -99999;
		cachedDaysWorthOfFoodForTicks = -99999;
		hasShuttleDirty = true;
	}

	public virtual void Notify_MemberDied(Pawn member)
	{
		if (!base.Spawned)
		{
			Log.Error("Caravan member died in an unspawned caravan. Unspawned caravans shouldn't be kept for more than a single frame.");
		}
		if (!PawnsListForReading.Any((Pawn x) => x != member && IsOwner(x)))
		{
			List<Thing> list = CaravanInventoryUtility.AllInventoryItems(this);
			for (int num = 0; num < list.Count; num++)
			{
				list[num].Notify_AbandonedAtTile(base.Tile);
			}
			RemovePawn(member);
			if (base.Faction == Faction.OfPlayer)
			{
				if (ModsConfig.IdeologyActive && PawnsListForReading.Any((Pawn x) => x != member && x.IsSlave))
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterOnlySlaveCaravanColonistsLeft".Translate(Name).CapitalizeFirst(), LetterDefOf.NegativeEvent, new GlobalTargetInfo(base.Tile));
				}
				else
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAllCaravanColonistsDied".Translate(), "LetterAllCaravanColonistsDied".Translate(Name).CapitalizeFirst(), LetterDefOf.NegativeEvent, new GlobalTargetInfo(base.Tile));
				}
			}
			pawns.Clear();
			Destroy();
		}
		else
		{
			member.Strip();
			RemovePawn(member);
		}
	}

	public virtual void Notify_Merged(List<Caravan> group)
	{
		notifiedOutOfFood = false;
	}

	public virtual void Notify_StartedTrading()
	{
		notifiedOutOfFood = false;
	}

	private void CheckAnyNonWorldPawns()
	{
		for (int num = pawns.Count - 1; num >= 0; num--)
		{
			if (!pawns[num].IsWorldPawn())
			{
				Log.Error($"Caravan member {pawns[num]} is not a world pawn. Removing...");
				pawns.Remove(pawns[num]);
			}
		}
	}

	private bool ShouldAutoCapture(Pawn p)
	{
		return CaravanUtility.ShouldAutoCapture(p, base.Faction);
	}

	public void Notify_PawnRemoved(Pawn p)
	{
		Find.ColonistBar.MarkColonistsDirty();
		RecacheInventory();
		carryTracker.Notify_PawnRemoved();
		beds.Notify_PawnRemoved();
		babies?.Recache();
		p.mindState?.ClearBreastfeedCaravan();
	}

	public void Notify_PawnAdded(Pawn p)
	{
		Find.ColonistBar.MarkColonistsDirty();
		RecacheInventory();
		babies?.Recache();
	}

	public void Notify_DestinationOrPauseStatusChanged()
	{
		cachedDaysWorthOfFoodForTicks = -99999;
	}

	public void Notify_Teleported()
	{
		tweener.ResetTweenedPosToRoot();
		pather.Notify_Teleported_Int();
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return pawns;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}
}
