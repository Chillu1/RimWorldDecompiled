using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse;

public class Thing : Entity, ISelectable, ILoadReferenceable, ISignalReceiver, IExposable, IEquatable<Thing>
{
	public ThingDef def;

	public int thingIDNumber = -1;

	private sbyte mapIndexOrState = -1;

	private IntVec3 positionInt = IntVec3.Invalid;

	private Rot4 rotationInt = Rot4.North;

	public int stackCount = 1;

	protected Faction factionInt;

	private ThingDef stuffInt;

	private Graphic graphicInt;

	protected Graphic styleGraphicInt;

	private int hitPointsInt = -1;

	public ThingOwner holdingOwner;

	public List<string> questTags;

	public int spawnedTick = -1;

	public int despawnedTick = -1;

	public int? overrideGraphicIndex;

	public bool debugRotLocked;

	private bool beingTransportedOnGravship;

	private int tickDelta;

	private bool beenRevealed;

	public bool shouldHighlightCached;

	public int shouldHighlightCachedTick;

	public Color highlightColorCached;

	public int highlightColorCachedTick;

	protected const sbyte UnspawnedState = -1;

	private const sbyte MemoryState = -2;

	private const sbyte DiscardedState = -3;

	private List<IThingHolder> tmpHolders;

	private bool cached;

	private bool cachedIsHolder;

	private IThingHolder cachedHolder;

	private IThingHolderTickable cachedTickable;

	public static bool allowDestroyNonDestroyable = false;

	private static Dictionary<Thing, string> facIDsCached = new Dictionary<Thing, string>();

	private static List<string> tmpDeteriorationReasons = new List<string>();

	public static HashSet<RitualPatternDef> showingGizmosForRitualsTmp = new HashSet<RitualPatternDef>();

	private static List<string> tmpIdeoNames = new List<string>();

	public const float SmeltCostRecoverFraction = 0.25f;

	public virtual int HitPoints
	{
		get
		{
			return hitPointsInt;
		}
		set
		{
			hitPointsInt = value;
		}
	}

	public int MaxHitPoints => Mathf.RoundToInt(this.GetStatValue(StatDefOf.MaxHitPoints, applyPostProcess: true, 10));

	public virtual float MarketValue => this.GetStatValue(StatDefOf.MarketValue);

	public virtual float RoyalFavorValue => this.GetStatValue(StatDefOf.RoyalFavorValue);

	public virtual int? OverrideGraphicIndex => overrideGraphicIndex;

	public virtual Texture UIIconOverride => null;

	public bool EverSeenByPlayer
	{
		get
		{
			return this.GetEverSeenByPlayer();
		}
		set
		{
			this.SetEverSeenByPlayer(value);
		}
	}

	public virtual ThingStyleDef StyleDef
	{
		get
		{
			return this.GetStyleDef();
		}
		set
		{
			styleGraphicInt = null;
			this.SetStyleDef(value);
		}
	}

	public Precept_ThingStyle StyleSourcePrecept
	{
		get
		{
			return this.GetStyleSourcePrecept();
		}
		set
		{
			this.SetStyleSourcePrecept(value);
		}
	}

	public bool FlammableNow
	{
		get
		{
			if (this.GetStatValue(StatDefOf.Flammability) < 0.01f)
			{
				return false;
			}
			if (Spawned && !FireBulwark)
			{
				List<Thing> thingList = Position.GetThingList(Map);
				if (thingList != null)
				{
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i].FireBulwark)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}

	public virtual bool FireBulwark => def.Fillage == FillCategory.Full;

	public bool Destroyed
	{
		get
		{
			if (mapIndexOrState != -2)
			{
				return mapIndexOrState == -3;
			}
			return true;
		}
	}

	public bool Discarded => mapIndexOrState == -3;

	public bool Spawned
	{
		get
		{
			if (mapIndexOrState < 0 || Find.Maps == null)
			{
				return false;
			}
			if (mapIndexOrState < Find.Maps.Count)
			{
				return true;
			}
			Log.ErrorOnce($"Thing {ThingID} is associated with invalid map index {mapIndexOrState}", 64664487);
			return false;
		}
	}

	public bool SpawnedOrAnyParentSpawned => SpawnedParentOrMe != null;

	public Thing SpawnedParentOrMe
	{
		get
		{
			if (Spawned)
			{
				return this;
			}
			if (ParentHolder != null)
			{
				return ThingOwnerUtility.SpawnedParentOrMe(ParentHolder);
			}
			return null;
		}
	}

	public int TickSpawned => spawnedTick;

	public int TickDeSpawned => despawnedTick;

	public Map Map
	{
		get
		{
			if (mapIndexOrState >= 0)
			{
				return Find.Maps?[mapIndexOrState];
			}
			return null;
		}
	}

	public Map MapHeld
	{
		get
		{
			if (Spawned)
			{
				return Map;
			}
			if (ParentHolder == null)
			{
				return null;
			}
			return ThingOwnerUtility.GetRootMap(ParentHolder);
		}
	}

	public IntVec3 Position
	{
		get
		{
			return positionInt;
		}
		set
		{
			if (value == positionInt)
			{
				return;
			}
			if (Spawned)
			{
				if (def.AffectsRegions)
				{
					Log.Warning("Changed position of a spawned thing which affects regions. This is not supported.");
				}
				DirtyMapMesh(Map);
				RegionListersUpdater.DeregisterInRegions(this, Map);
				Map.thingGrid.Deregister(this);
				Map.coverGrid.DeRegister(this);
			}
			positionInt = value;
			if (Spawned)
			{
				Map.thingGrid.Register(this);
				Map.coverGrid.Register(this);
				Map.gasGrid.Notify_ThingSpawned(this);
				RegionListersUpdater.RegisterInRegions(this, Map);
				DirtyMapMesh(Map);
				if (def.AffectsReachability)
				{
					Map.reachability.ClearCache();
				}
			}
		}
	}

	public IntVec3 PositionHeld
	{
		get
		{
			if (Spawned)
			{
				return Position;
			}
			IntVec3 rootPosition = ThingOwnerUtility.GetRootPosition(ParentHolder);
			if (rootPosition.IsValid)
			{
				return rootPosition;
			}
			return Position;
		}
	}

	public Rot4 Rotation
	{
		get
		{
			return rotationInt;
		}
		set
		{
			if (value == rotationInt || debugRotLocked)
			{
				return;
			}
			if (Spawned && (def.size.x != 1 || def.size.z != 1))
			{
				if (def.AffectsRegions)
				{
					Log.Warning("Changed rotation of a spawned non-single-cell thing which affects regions. This is not supported.");
				}
				RegionListersUpdater.DeregisterInRegions(this, Map);
				Map.thingGrid.Deregister(this);
			}
			rotationInt = value;
			if (Spawned && (def.size.x != 1 || def.size.z != 1))
			{
				Map.thingGrid.Register(this);
				RegionListersUpdater.RegisterInRegions(this, Map);
				Map.gasGrid.Notify_ThingSpawned(this);
				if (def.AffectsReachability)
				{
					Map.reachability.ClearCache();
				}
			}
		}
	}

	public bool Smeltable
	{
		get
		{
			if (this.IsRelic())
			{
				return false;
			}
			if (def.smeltable)
			{
				if (def.MadeFromStuff)
				{
					return Stuff.smeltable;
				}
				return true;
			}
			return false;
		}
	}

	public bool BurnableByRecipe
	{
		get
		{
			if (def.burnableByRecipe)
			{
				if (def.MadeFromStuff)
				{
					return Stuff.burnableByRecipe;
				}
				return true;
			}
			return false;
		}
	}

	public IThingHolder ParentHolder => holdingOwner?.Owner;

	public Faction Faction => factionInt;

	public string ThingID
	{
		get
		{
			if (def.HasThingIDNumber)
			{
				return def.defName + thingIDNumber;
			}
			return def.defName;
		}
		set
		{
			thingIDNumber = IDNumberFromThingID(value);
		}
	}

	public IntVec2 RotatedSize
	{
		get
		{
			if (!rotationInt.IsHorizontal)
			{
				return def.size;
			}
			return new IntVec2(def.size.z, def.size.x);
		}
	}

	public virtual CellRect? CustomRectForSelector => null;

	public override string Label
	{
		get
		{
			if (stackCount > 1)
			{
				return LabelNoCount + " x" + stackCount.ToStringCached();
			}
			return LabelNoCount;
		}
	}

	public virtual string LabelNoCount => GenLabel.ThingLabel(this, 1);

	public override string LabelCap => Label.CapitalizeFirst(def);

	public virtual string LabelCapNoCount => LabelNoCount.CapitalizeFirst(def);

	public override string LabelShort => LabelNoCount;

	public virtual string LabelNoParenthesis => GenLabel.ThingLabel(this, 1, includeHp: false, includeQuality: false);

	public string LabelNoParenthesisCap => LabelNoParenthesis.CapitalizeFirst();

	public virtual ModContentPack ContentSource => def.modContentPack;

	public virtual bool IngestibleNow
	{
		get
		{
			if (this.IsBurning())
			{
				return false;
			}
			return def.IsIngestible;
		}
	}

	public ThingDef Stuff => stuffInt;

	public Graphic DefaultGraphic
	{
		get
		{
			if (graphicInt == null)
			{
				if (def.graphicData == null)
				{
					return BaseContent.BadGraphic;
				}
				graphicInt = def.graphicData.GraphicColoredFor(this);
			}
			return graphicInt;
		}
	}

	public virtual Graphic Graphic
	{
		get
		{
			ThingStyleDef styleDef = StyleDef;
			if (styleDef?.Graphic != null)
			{
				if (styleGraphicInt == null)
				{
					if (styleDef.graphicData != null)
					{
						styleGraphicInt = styleDef.graphicData.GraphicColoredFor(this);
					}
					else
					{
						styleGraphicInt = styleDef.Graphic;
					}
				}
				return styleGraphicInt;
			}
			return DefaultGraphic;
		}
	}

	public virtual List<IntVec3> InteractionCells => ThingUtility.InteractionCellsWhenAt(def, Position, Rotation, Map, allowFallbackCell: true);

	public virtual IntVec3 InteractionCell => ThingUtility.InteractionCellWhenAt(def, Position, Rotation, Map);

	public float AmbientTemperature
	{
		get
		{
			if (Spawned)
			{
				return GenTemperature.GetTemperatureForCell(Position, Map);
			}
			if (ParentHolder != null)
			{
				for (IThingHolder parentHolder = ParentHolder; parentHolder != null; parentHolder = parentHolder.ParentHolder)
				{
					if (ThingOwnerUtility.TryGetFixedTemperature(parentHolder, this, out var temperature))
					{
						return temperature;
					}
				}
			}
			if (SpawnedOrAnyParentSpawned)
			{
				return GenTemperature.GetTemperatureForCell(PositionHeld, MapHeld);
			}
			if (Tile.Valid)
			{
				return GenTemperature.GetTemperatureAtTile(Tile);
			}
			return 21f;
		}
	}

	public PlanetTile Tile
	{
		get
		{
			if (Spawned)
			{
				return Map.Tile;
			}
			if (ParentHolder != null)
			{
				return ThingOwnerUtility.GetRootTile(ParentHolder);
			}
			return PlanetTile.Invalid;
		}
	}

	public virtual bool Suspended
	{
		get
		{
			if (Spawned)
			{
				return false;
			}
			if (ParentHolder != null)
			{
				return ThingOwnerUtility.ContentsSuspended(ParentHolder);
			}
			return false;
		}
	}

	public bool InCryptosleep
	{
		get
		{
			if (Spawned)
			{
				return false;
			}
			if (ParentHolder != null)
			{
				return ThingOwnerUtility.ContentsInCryptosleep(ParentHolder);
			}
			return false;
		}
	}

	public virtual string DescriptionDetailed => def.DescriptionDetailed;

	public virtual string DescriptionFlavor => def.description;

	public bool IsOnHoldingPlatform
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				return ParentHolder is Building_HoldingPlatform;
			}
			return false;
		}
	}

	public TerrainAffordanceDef TerrainAffordanceNeeded => def.GetTerrainAffordanceNeed(stuffInt);

	public bool BeingTransportedOnGravship => beingTransportedOnGravship;

	protected virtual int MinTickIntervalRate => 1;

	protected virtual int MaxTickIntervalRate => 15;

	protected virtual int UpdateRateTickOffset => this.HashOffset();

	public virtual int UpdateRateTicks => GenTicks.GetCameraUpdateRate(this);

	public Vector3? DrawPosHeld
	{
		get
		{
			if (Spawned)
			{
				return DrawPos;
			}
			return ThingOwnerUtility.SpawnedParentOrMe(ParentHolder)?.DrawPos;
		}
	}

	public virtual Vector3 DrawPos => this.TrueCenter();

	public virtual Vector2 DrawSize
	{
		get
		{
			if (def.graphicData != null)
			{
				return def.graphicData.drawSize;
			}
			return Vector2.one;
		}
	}

	public virtual Color DrawColor
	{
		get
		{
			if (Stuff != null)
			{
				return def.GetColorForStuff(Stuff);
			}
			if (def.graphicData != null)
			{
				return def.graphicData.color;
			}
			return Color.white;
		}
		set
		{
			Log.Error($"Cannot set instance color on non-ThingWithComps {LabelCap} at {Position}.");
		}
	}

	public virtual Color DrawColorTwo
	{
		get
		{
			if (def.graphicData != null)
			{
				return def.graphicData.colorTwo;
			}
			return Color.white;
		}
	}

	public virtual IEnumerable<DefHyperlink> DescriptionHyperlinks
	{
		get
		{
			if (def.descriptionHyperlinks != null)
			{
				for (int i = 0; i < def.descriptionHyperlinks.Count; i++)
				{
					yield return def.descriptionHyperlinks[i];
				}
			}
		}
	}

	public static int IDNumberFromThingID(string thingID)
	{
		string value = Regex.Match(thingID, "\\d+$").Value;
		int result = 0;
		try
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			result = Convert.ToInt32(value, invariantCulture);
		}
		catch (Exception ex)
		{
			Log.Error("Could not convert id number from thingID=" + thingID + ", numString=" + value + " Exception=" + ex);
		}
		return result;
	}

	public void DoTick()
	{
		if (Destroyed)
		{
			return;
		}
		if (!cached)
		{
			cached = true;
			cachedHolder = this as IThingHolder;
			cachedTickable = this as IThingHolderTickable;
			cachedIsHolder = cachedHolder != null;
		}
		bool flag = holdingOwner != null && !Spawned;
		if (def.tickerType == TickerType.Normal)
		{
			using (ProfilerBlock.Scope("DoTick()"))
			{
				using (ProfilerBlock.Scope("Tick()"))
				{
					Tick();
				}
				if (Destroyed)
				{
					return;
				}
				tickDelta++;
				int num = Mathf.Min(Mathf.Max(UpdateRateTicks, MinTickIntervalRate), MaxTickIntervalRate);
				if (tickDelta >= num || GenTicks.IsTickInterval(UpdateRateTickOffset, num))
				{
					using (ProfilerBlock.Scope("TickInterval()"))
					{
						TickInterval(tickDelta);
					}
					tickDelta = 0;
				}
			}
		}
		else if (def.tickerType == TickerType.Rare && ((!cachedIsHolder && !flag) || this.IsHashIntervalTick(250)))
		{
			using (ProfilerBlock.Scope("TickRare()"))
			{
				TickRare();
			}
		}
		else if (def.tickerType == TickerType.Long && ((!cachedIsHolder && !flag) || this.IsHashIntervalTick(2000)))
		{
			using (ProfilerBlock.Scope("TickLong()"))
			{
				TickLong();
			}
		}
		if (Destroyed || !cachedIsHolder || (cachedTickable != null && !cachedTickable.ShouldTickContents))
		{
			return;
		}
		if (tmpHolders == null)
		{
			tmpHolders = new List<IThingHolder>(8);
		}
		tmpHolders.Add(cachedHolder);
		cachedHolder.GetChildHolders(tmpHolders);
		for (int i = 0; i < tmpHolders.Count; i++)
		{
			ThingOwner directlyHeldThings = tmpHolders[i].GetDirectlyHeldThings();
			if (directlyHeldThings != null)
			{
				directlyHeldThings.DoTick();
				if (Destroyed)
				{
					break;
				}
			}
		}
		tmpHolders.Clear();
	}

	public virtual void PostMake()
	{
		ThingIDMaker.GiveIDTo(this);
		if (def.useHitPoints)
		{
			HitPoints = Mathf.RoundToInt((float)MaxHitPoints * Mathf.Clamp01(def.startingHpRange.RandomInRange));
		}
	}

	public virtual void PostPostMake()
	{
		if (!def.randomStyle.NullOrEmpty() && Rand.Chance(def.randomStyleChance))
		{
			StyleDef = def.randomStyle.RandomElementByWeight((ThingStyleChance x) => x.Chance).StyleDef;
		}
	}

	public virtual void PostQualitySet()
	{
	}

	public string GetUniqueLoadID()
	{
		return "Thing_" + ThingID;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (Destroyed)
		{
			Log.Error("Spawning destroyed thing " + this?.ToString() + " at " + Position.ToString() + ". Correcting.");
			mapIndexOrState = -1;
			if (HitPoints <= 0 && def.useHitPoints)
			{
				HitPoints = 1;
			}
		}
		if (Spawned)
		{
			Log.Error("Tried to spawn already-spawned thing " + this?.ToString() + " at " + Position.ToString());
			return;
		}
		int num = Find.Maps.IndexOf(map);
		if (num < 0)
		{
			Log.Error("Tried to spawn thing " + this?.ToString() + ", but the map provided does not exist.");
			return;
		}
		if (stackCount > def.stackLimit)
		{
			Log.Error("Spawned " + this?.ToString() + " with stackCount " + stackCount + " but stackLimit is " + def.stackLimit + ". Truncating.");
			stackCount = def.stackLimit;
		}
		mapIndexOrState = (sbyte)num;
		RegionListersUpdater.RegisterInRegions(this, map);
		if (!map.spawnedThings.TryAdd(this, canMergeWithExistingStacks: false))
		{
			Log.Error("Couldn't add thing " + this?.ToString() + " to spawned things.");
		}
		map.listerThings.Add(this);
		map.thingGrid.Register(this);
		map.gasGrid.Notify_ThingSpawned(this);
		map.mapTemperature.Notify_ThingSpawned(this);
		if (map.IsPlayerHome)
		{
			EverSeenByPlayer = true;
		}
		if (Find.TickManager != null)
		{
			Find.TickManager.RegisterAllTickabilityFor(this);
		}
		DirtyMapMesh(map);
		if (def.drawerType != DrawerType.MapMeshOnly)
		{
			map.dynamicDrawManager.RegisterDrawable(this);
		}
		map.tooltipGiverList.Notify_ThingSpawned(this);
		if (def.CanAffectLinker)
		{
			map.linkGrid.Notify_LinkerCreatedOrDestroyed(this);
			map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things, regenAdjacentCells: true, regenAdjacentSections: false);
		}
		if (!def.CanOverlapZones)
		{
			map.zoneManager.Notify_NoZoneOverlapThingSpawned(this);
		}
		if (def.AffectsRegions)
		{
			map.regionDirtyer.Notify_ThingAffectingRegionsSpawned(this);
		}
		if (def.pathCost != 0 || def.passability == Traversability.Impassable)
		{
			map.pathing.RecalculatePerceivedPathCostUnderThing(this);
		}
		if (def.AffectsReachability)
		{
			map.reachability.ClearCache();
		}
		map.coverGrid.Register(this);
		if (def.category == ThingCategory.Item)
		{
			map.listerHaulables.Notify_Spawned(this);
			map.listerMergeables.Notify_Spawned(this);
		}
		map.attackTargetsCache.Notify_ThingSpawned(this);
		map.regionGrid.GetValidRegionAt_NoRebuild(Position)?.Room?.Notify_ContainedThingSpawnedOrDespawned(this);
		StealAIDebugDrawer.Notify_ThingChanged(this);
		if (this is IHaulDestination haulDestination)
		{
			map.haulDestinationManager.AddHaulDestination(haulDestination);
		}
		if (this is IHaulSource source)
		{
			map.haulDestinationManager.AddHaulSource(source);
		}
		if (this is IThingHolder && Find.ColonistBar != null)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
		if (def.category == ThingCategory.Item)
		{
			ISlotGroupParent slotGroupParent = Position.GetSlotGroup(map)?.parent;
			if (slotGroupParent != null)
			{
				slotGroupParent.Notify_ReceivedThing(this);
				GenThing.TryDirtyAdjacentGroupContainers(slotGroupParent, map);
			}
		}
		if (def.receivesSignals)
		{
			Find.SignalManager.RegisterReceiver(this);
		}
		if (!BeingTransportedOnGravship)
		{
			def.soundSpawned?.PlayOneShot(this);
			if (!respawningAfterLoad)
			{
				QuestUtility.SendQuestTargetSignals(questTags, "Spawned", this.Named("SUBJECT"));
				spawnedTick = Find.TickManager.TicksGame;
				despawnedTick = -1;
				if (AnomalyUtility.ShouldNotifyCodex(this, EntityDiscoveryType.Spawn, out var entries))
				{
					Find.EntityCodex.SetDiscovered(entries, def, this);
				}
				else
				{
					Find.HiddenItemsManager.SetDiscovered(def);
				}
			}
		}
		map.events.Notify_ThingSpawned(this);
	}

	public bool DeSpawnOrDeselect(DestroyMode mode = DestroyMode.Vanish)
	{
		bool flag = Current.ProgramState == ProgramState.Playing && Find.Selector.IsSelected(this);
		if (Spawned)
		{
			DeSpawn(mode);
		}
		else if (flag)
		{
			Find.Selector.Deselect(this);
			Find.MainButtonsRoot.tabs.Notify_SelectedObjectDespawned();
		}
		return flag;
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (Destroyed)
		{
			Log.Error("Tried to despawn " + this.ToStringSafe() + " which is already destroyed.");
			return;
		}
		if (!Spawned)
		{
			Log.Error("Tried to despawn " + this.ToStringSafe() + " which is not spawned.");
			return;
		}
		Map map = Map;
		map.overlayDrawer.DisposeHandle(this);
		RegionListersUpdater.DeregisterInRegions(this, map);
		map.spawnedThings.Remove(this);
		map.listerThings.Remove(this);
		map.thingGrid.Deregister(this);
		map.coverGrid.DeRegister(this);
		if (def.receivesSignals)
		{
			Find.SignalManager.DeregisterReceiver(this);
		}
		map.tooltipGiverList.Notify_ThingDespawned(this);
		if (def.CanAffectLinker)
		{
			map.linkGrid.Notify_LinkerCreatedOrDestroyed(this);
			map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things, regenAdjacentCells: true, regenAdjacentSections: false);
		}
		if (Find.Selector.IsSelected(this))
		{
			Find.Selector.Deselect(this);
			Find.MainButtonsRoot.tabs.Notify_SelectedObjectDespawned();
		}
		DirtyMapMesh(map);
		if (def.drawerType != DrawerType.MapMeshOnly)
		{
			map.dynamicDrawManager.DeRegisterDrawable(this);
		}
		map.regionGrid.GetValidRegionAt_NoRebuild(Position)?.Room?.Notify_ContainedThingSpawnedOrDespawned(this);
		if (def.AffectsRegions)
		{
			map.regionDirtyer.Notify_ThingAffectingRegionsDespawned(this);
		}
		if (def.pathCost != 0 || def.passability == Traversability.Impassable)
		{
			map.pathing.RecalculatePerceivedPathCostUnderThing(this);
		}
		if (def.AffectsReachability)
		{
			map.reachability.ClearCache();
		}
		Find.TickManager.DeRegisterAllTickabilityFor(this);
		mapIndexOrState = -1;
		if (def.category == ThingCategory.Item)
		{
			map.listerHaulables.Notify_DeSpawned(this);
			map.listerMergeables.Notify_DeSpawned(this);
		}
		map.attackTargetsCache.Notify_ThingDespawned(this);
		map.physicalInteractionReservationManager.ReleaseAllForTarget(this);
		if (this is IHaulEnroute thing)
		{
			map.enrouteManager.Notify_ContainerDespawned(thing);
		}
		StealAIDebugDrawer.Notify_ThingChanged(this);
		if (this is IHaulDestination haulDestination)
		{
			map.haulDestinationManager.RemoveHaulDestination(haulDestination);
		}
		if (this is IHaulSource source)
		{
			map.haulDestinationManager.RemoveHaulSource(source);
		}
		if (this is IThingHolder && Find.ColonistBar != null)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
		if (def.category == ThingCategory.Item)
		{
			ISlotGroupParent slotGroupParent = Position.GetSlotGroup(map)?.parent;
			if (slotGroupParent != null)
			{
				slotGroupParent.Notify_LostThing(this);
				GenThing.TryDirtyAdjacentGroupContainers(slotGroupParent, map);
			}
		}
		QuestUtility.SendQuestTargetSignals(questTags, "Despawned", this.Named("SUBJECT"));
		spawnedTick = -1;
		despawnedTick = Find.TickManager.TicksGame;
		map.events.Notify_ThingDespawned(this);
	}

	public virtual void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
	{
		Destroy(DestroyMode.KillFinalize);
	}

	public virtual void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (!allowDestroyNonDestroyable && !def.destroyable)
		{
			Log.Error("Tried to destroy non-destroyable thing " + this);
			return;
		}
		if (Destroyed)
		{
			Log.Error("Tried to destroy already-destroyed thing " + this);
			return;
		}
		bool spawned = Spawned;
		Map map = Map;
		if (StyleSourcePrecept != null)
		{
			StyleSourcePrecept.Notify_ThingLost(this, spawned);
		}
		if (Spawned)
		{
			DeSpawn(mode);
		}
		else if (Current.ProgramState == ProgramState.Playing && Find.Selector.IsSelected(this))
		{
			Find.Selector.Deselect(this);
			Find.MainButtonsRoot.tabs.Notify_SelectedObjectDespawned();
		}
		mapIndexOrState = -2;
		if (def.DiscardOnDestroyed)
		{
			Discard();
		}
		CompExplosive compExplosive = this.TryGetComp<CompExplosive>();
		if (spawned)
		{
			List<Thing> list = new List<Thing>();
			GenLeaving.DoLeavingsFor(this, map, mode, list);
			compExplosive?.AddThingsIgnoredByExplosion(list);
			Notify_KilledLeavingsLeft(list);
		}
		if (holdingOwner != null)
		{
			holdingOwner.Notify_ContainedItemDestroyed(this);
		}
		RemoveAllReservationsAndDesignationsOnThis();
		if (!(this is Pawn))
		{
			stackCount = 0;
		}
		if (mode != DestroyMode.QuestLogic)
		{
			QuestUtility.SendQuestTargetSignals(questTags, "Destroyed", this.Named("SUBJECT"));
		}
		if (mode == DestroyMode.KillFinalize)
		{
			QuestUtility.SendQuestTargetSignals(questTags, "Killed", this.Named("SUBJECT"), map.Named("MAP"));
		}
	}

	public virtual void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
	{
	}

	public virtual void PostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
	{
		if (def.colorGeneratorInTraderStock != null)
		{
			this.SetColor(def.colorGeneratorInTraderStock.NewRandomizedColor());
		}
	}

	public virtual float GetBeauty(bool outside)
	{
		if (!outside || !def.StatBaseDefined(StatDefOf.BeautyOutdoors))
		{
			return this.GetStatValue(StatDefOf.Beauty);
		}
		return this.GetStatValue(StatDefOf.BeautyOutdoors);
	}

	public virtual void Notify_MyMapRemoved()
	{
		if (def.receivesSignals)
		{
			Find.SignalManager.DeregisterReceiver(this);
		}
		if (StyleSourcePrecept != null)
		{
			StyleSourcePrecept.Notify_ThingLost(this);
		}
		if (!ThingOwnerUtility.AnyParentIs<Pawn>(this))
		{
			mapIndexOrState = -3;
		}
		ThingOwner thingOwner = holdingOwner;
		if (thingOwner != null && thingOwner.Owner is Map)
		{
			holdingOwner = null;
		}
		RemoveAllReservationsAndDesignationsOnThis();
	}

	public virtual void Notify_LordDestroyed()
	{
	}

	public virtual void Notify_AbandonedAtTile(PlanetTile tile)
	{
	}

	public virtual void Notify_KilledLeavingsLeft(List<Thing> leavings)
	{
	}

	public virtual void Notify_Studied(Pawn studier, float amount, KnowledgeCategoryDef category = null)
	{
	}

	public virtual void Notify_Unfogged()
	{
		if (!beenRevealed)
		{
			beenRevealed = true;
			if (ModsConfig.AnomalyActive && AnomalyUtility.ShouldNotifyCodex(this, EntityDiscoveryType.Unfog, out var entries))
			{
				Find.EntityCodex.SetDiscovered(entries, def, this);
			}
			else
			{
				Find.HiddenItemsManager.SetDiscovered(def);
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Unfogged", this);
			CompLetterOnRevealed compLetterOnRevealed = this.TryGetComp<CompLetterOnRevealed>();
			if (compLetterOnRevealed != null)
			{
				Find.LetterStack.ReceiveLetter(compLetterOnRevealed.Props.label, compLetterOnRevealed.Props.text, compLetterOnRevealed.Props.letterDef, this);
			}
		}
	}

	public void ForceSetStateToUnspawned()
	{
		mapIndexOrState = -1;
	}

	public void DecrementMapIndex()
	{
		if (mapIndexOrState <= 0)
		{
			Log.Warning("Tried to decrement map index for " + this?.ToString() + ", but mapIndexOrState=" + mapIndexOrState);
		}
		else
		{
			mapIndexOrState--;
		}
	}

	private void RemoveAllReservationsAndDesignationsOnThis()
	{
		if (def.category == ThingCategory.Mote)
		{
			return;
		}
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].reservationManager.ReleaseAllForTarget(this);
			maps[i].physicalInteractionReservationManager.ReleaseAllForTarget(this);
			if (this is IAttackTarget target)
			{
				maps[i].attackTargetReservationManager.ReleaseAllForTarget(target);
			}
			maps[i].designationManager.RemoveAllDesignationsOn(this);
		}
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref tickDelta, "tickDelta", 0);
		if (def.HasThingIDNumber)
		{
			string value = ThingID;
			Scribe_Values.Look(ref value, "id");
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				ThingID = value;
			}
		}
		Scribe_Values.Look<sbyte>(ref mapIndexOrState, "map", -1);
		if (Scribe.mode == LoadSaveMode.LoadingVars && mapIndexOrState >= 0)
		{
			mapIndexOrState = -1;
		}
		Scribe_Values.Look(ref positionInt, "pos", IntVec3.Invalid);
		Scribe_Values.Look(ref rotationInt, "rot", Rot4.North);
		Scribe_Values.Look(ref debugRotLocked, "debugRotLocked", defaultValue: false);
		if (def.useHitPoints)
		{
			Scribe_Values.Look(ref hitPointsInt, "health", -1);
		}
		bool flag = def.tradeability != Tradeability.None && def.category == ThingCategory.Item;
		if (def.stackLimit > 1 || flag)
		{
			Scribe_Values.Look(ref stackCount, "stackCount", 0, forceSave: true);
		}
		Scribe_Defs.Look(ref stuffInt, "stuff");
		string facID = ((factionInt != null) ? factionInt.GetUniqueLoadID() : "null");
		Scribe_Values.Look(ref facID, "faction", "null");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (facID == "null")
			{
				factionInt = null;
			}
			else if (Find.World != null && Find.FactionManager != null)
			{
				factionInt = Find.FactionManager.AllFactions.FirstOrDefault((Faction fa) => fa.GetUniqueLoadID() == facID);
			}
			else
			{
				facIDsCached.SetOrAdd(this, facID);
			}
		}
		if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
		{
			if (facID == "null" && facIDsCached.TryGetValue(this, out facID))
			{
				facIDsCached.Remove(this);
			}
			if (facID != "null")
			{
				factionInt = Find.FactionManager.AllFactions.FirstOrDefault((Faction fa) => fa.GetUniqueLoadID() == facID);
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			facIDsCached.Clear();
			if (def.MadeFromStuff && Stuff == null)
			{
				Log.Error($"{this} is made from stuff but has no stuff set. Setting default stuff.");
				SetStuffDirect(GenStuff.DefaultStuffFor(def));
				if (Stuff == null)
				{
					Log.Error($"Failed to find stuff for {this} after loading.");
				}
			}
		}
		Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
		Scribe_Values.Look(ref overrideGraphicIndex, "overrideGraphicIndex");
		Scribe_Values.Look(ref spawnedTick, "spawnedTick", -1);
		Scribe_Values.Look(ref despawnedTick, "despawnedTick", 0);
		Scribe_Values.Look(ref beenRevealed, "beenRevealed", defaultValue: false);
		BackCompatibility.PostExposeData(this);
	}

	public virtual void PostMapInit()
	{
	}

	public void DrawNowAt(Vector3 drawLoc, bool flip = false)
	{
		DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc, flip);
	}

	public void DynamicDrawPhase(DrawPhase phase)
	{
		if (def.drawerType != DrawerType.MapMeshOnly)
		{
			DynamicDrawPhaseAt(phase, DrawPos);
		}
	}

	public virtual void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		if (phase == DrawPhase.Draw)
		{
			DrawAt(drawLoc, flip);
		}
	}

	protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (def.drawerType == DrawerType.RealtimeOnly || !Spawned)
		{
			Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
		}
		SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);
	}

	public virtual void Print(SectionLayer layer)
	{
		if (!def.dontPrint)
		{
			Graphic.Print(layer, this, 0f);
		}
	}

	public void DirtyMapMesh(Map map)
	{
		if (def.drawerType == DrawerType.RealtimeOnly)
		{
			return;
		}
		foreach (IntVec3 item in this.OccupiedRect())
		{
			map.mapDrawer.MapMeshDirty(item, MapMeshFlagDefOf.Things);
		}
	}

	public virtual void DrawGUIOverlay()
	{
		if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
		{
			QualityCategory qc;
			if (def.stackLimit > 1)
			{
				GenMapUI.DrawThingLabel(this, stackCount.ToStringCached());
			}
			else if (def.drawGUIOverlayQuality && this.TryGetQuality(out qc))
			{
				GenMapUI.DrawThingLabel(this, qc.GetLabelShort());
			}
		}
	}

	public virtual void DrawExtraSelectionOverlays()
	{
		if (def.specialDisplayRadius > 0.1f)
		{
			GenDraw.DrawRadiusRing(Position, def.specialDisplayRadius);
		}
		if (def.drawPlaceWorkersWhileSelected && def.PlaceWorkers != null)
		{
			for (int i = 0; i < def.PlaceWorkers.Count; i++)
			{
				def.PlaceWorkers[i].DrawGhost(def, Position, Rotation, Color.white, this);
			}
		}
		GenDraw.DrawInteractionCells(def, Position, rotationInt);
	}

	public virtual string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, this);
		return stringBuilder.ToString();
	}

	public virtual string GetInspectStringLowPriority()
	{
		string result = null;
		tmpDeteriorationReasons.Clear();
		float f = SteadyEnvironmentEffects.FinalDeteriorationRate(this, tmpDeteriorationReasons);
		if (tmpDeteriorationReasons.Count != 0)
		{
			result = string.Format("{0}: {1} ({2})", "DeterioratingBecauseOf".Translate(), tmpDeteriorationReasons.ToCommaList().CapitalizeFirst(), "PerDay".Translate(f.ToStringByStyle(ToStringStyle.FloatMaxTwo)));
		}
		return result;
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		Gizmo gizmo = ContainingSelectionUtility.SelectContainingThingGizmo(this);
		if (gizmo != null)
		{
			yield return gizmo;
		}
		showingGizmosForRitualsTmp.Clear();
		foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
		{
			for (int i = 0; i < ideo.PreceptsListForReading.Count; i++)
			{
				Precept precept = ideo.PreceptsListForReading[i];
				if (!(precept is Precept_Ritual ritual) || (precept.def.mergeRitualGizmosFromAllIdeos && showingGizmosForRitualsTmp.Contains(ritual.sourcePattern)) || !ritual.ShouldShowGizmo(this))
				{
					continue;
				}
				foreach (Gizmo item in ritual.GetGizmoFor(this))
				{
					yield return item;
					showingGizmosForRitualsTmp.Add(ritual.sourcePattern);
				}
			}
		}
		List<LordJob_Ritual> activeRituals = Find.IdeoManager.GetActiveRituals(MapHeld);
		foreach (LordJob_Ritual item2 in activeRituals)
		{
			if (item2.selectedTarget == this)
			{
				yield return item2.GetCancelGizmo();
			}
		}
		if (ModsConfig.AnomalyActive)
		{
			Gizmo gizmo2 = AnomalyUtility.OpenCodexGizmo(this);
			if (gizmo2 != null)
			{
				yield return gizmo2;
			}
		}
		if (DebugSettings.ShowDevGizmos && this.HasAttachment(ThingDefOf.Fire))
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Extinguish",
				action = delegate
				{
					this.GetAttachment(ThingDefOf.Fire)?.Destroy();
				}
			};
		}
	}

	public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public virtual IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public virtual IEnumerable<InspectTabBase> GetInspectTabs()
	{
		return def.inspectorTabsResolved;
	}

	public virtual string GetCustomLabelNoCount(bool includeHp = true)
	{
		return GenLabel.ThingLabel(this, 1, includeHp);
	}

	public DamageWorker.DamageResult TakeDamage(DamageInfo dinfo)
	{
		if (Destroyed)
		{
			return new DamageWorker.DamageResult();
		}
		if (dinfo.Amount == 0f)
		{
			return new DamageWorker.DamageResult();
		}
		if (def.damageMultipliers != null)
		{
			for (int i = 0; i < def.damageMultipliers.Count; i++)
			{
				if (def.damageMultipliers[i].damageDef == dinfo.Def)
				{
					int num = Mathf.RoundToInt(dinfo.Amount * def.damageMultipliers[i].multiplier);
					dinfo.SetAmount(num);
				}
			}
		}
		PreApplyDamage(ref dinfo, out var absorbed);
		if (absorbed)
		{
			return new DamageWorker.DamageResult();
		}
		bool spawnedOrAnyParentSpawned = SpawnedOrAnyParentSpawned;
		Map mapHeld = MapHeld;
		DamageWorker.DamageResult damageResult = dinfo.Def.Worker.Apply(dinfo, this);
		if (dinfo.Def.harmsHealth && spawnedOrAnyParentSpawned)
		{
			mapHeld.damageWatcher.Notify_DamageTaken(this, damageResult.totalDamageDealt);
		}
		if (dinfo.Instigator is Pawn pawn)
		{
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				hediff.Notify_PawnDamagedThing(this, dinfo, damageResult);
			}
		}
		if (dinfo.Def.ExternalViolenceFor(this))
		{
			if (dinfo.SpawnFilth)
			{
				GenLeaving.DropFilthDueToDamage(this, damageResult.totalDamageDealt);
			}
			if (dinfo.Instigator != null)
			{
				if (dinfo.Instigator is Pawn pawn2)
				{
					pawn2.records.AddTo(RecordDefOf.DamageDealt, damageResult.totalDamageDealt);
				}
				if (dinfo.Instigator.Faction == Faction.OfPlayer)
				{
					QuestUtility.SendQuestTargetSignals(questTags, "TookDamageFromPlayer", this.Named("SUBJECT"), dinfo.Instigator.Named("INSTIGATOR"));
				}
			}
			QuestUtility.SendQuestTargetSignals(questTags, "TookDamage", this.Named("SUBJECT"), dinfo.Instigator.Named("INSTIGATOR"), mapHeld.Named("MAP"));
		}
		if (!Destroyed && FlammableNow && dinfo.Def.igniteChanceByTargetFlammability != null && Rand.Chance(dinfo.Def.igniteChanceByTargetFlammability.Evaluate(this.GetStatValue(StatDefOf.Flammability))))
		{
			this.TryAttachFire(Rand.Range(0.55f, 0.85f), dinfo.Instigator);
		}
		PostApplyDamage(dinfo, damageResult.totalDamageDealt);
		return damageResult;
	}

	public virtual void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
	}

	public virtual void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
	}

	public virtual bool CanStackWith(Thing other)
	{
		if (Destroyed || other.Destroyed)
		{
			return false;
		}
		if (def.category != ThingCategory.Item)
		{
			return false;
		}
		if (this.IsRelic() || other.IsRelic())
		{
			return false;
		}
		if (def == other.def)
		{
			return Stuff == other.Stuff;
		}
		return false;
	}

	public virtual bool TryAbsorbStack(Thing other, bool respectStackLimit)
	{
		if (!CanStackWith(other))
		{
			return false;
		}
		int num = ThingUtility.TryAbsorbStackNumToTake(this, other, respectStackLimit);
		if (def.useHitPoints)
		{
			HitPoints = Mathf.CeilToInt((float)(HitPoints * stackCount + other.HitPoints * num) / (float)(stackCount + num));
		}
		stackCount += num;
		other.stackCount -= num;
		if (Map != null)
		{
			DirtyMapMesh(Map);
		}
		StealAIDebugDrawer.Notify_ThingChanged(this);
		if (Spawned)
		{
			Map.listerMergeables.Notify_ThingStackChanged(this);
		}
		if (other.stackCount <= 0)
		{
			other.Destroy();
			return true;
		}
		return false;
	}

	public virtual Thing SplitOff(int count)
	{
		if (count <= 0)
		{
			throw new ArgumentException("SplitOff with count <= 0", "count");
		}
		if (count >= stackCount)
		{
			if (count > stackCount)
			{
				Log.Error("Tried to split off " + count + " of " + this?.ToString() + " but there are only " + stackCount);
			}
			DeSpawnOrDeselect();
			holdingOwner?.Remove(this);
			return this;
		}
		Thing thing = ThingMaker.MakeThing(def, Stuff);
		thing.stackCount = count;
		stackCount -= count;
		if (Map != null)
		{
			DirtyMapMesh(Map);
		}
		if (Spawned)
		{
			Map.listerMergeables.Notify_ThingStackChanged(this);
		}
		if (def.useHitPoints)
		{
			thing.HitPoints = HitPoints;
		}
		return thing;
	}

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (Stuff != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Stuff_Name".Translate(), Stuff.LabelCap, "Stat_Stuff_Desc".Translate(), 1100, null, new Dialog_InfoCard.Hyperlink[1]
			{
				new Dialog_InfoCard.Hyperlink(Stuff)
			});
		}
		if (!ModsConfig.IdeologyActive || Find.IdeoManager.classicMode)
		{
			yield break;
		}
		tmpIdeoNames.Clear();
		StyleCategoryDef styleCategoryDef = StyleDef?.Category ?? def.dominantStyleCategory;
		if (styleCategoryDef == null)
		{
			yield break;
		}
		foreach (Ideo item in Find.IdeoManager.IdeosListForReading)
		{
			if (IdeoUtility.ThingSatisfiesIdeo(this, item))
			{
				tmpIdeoNames.Add(item.name.Colorize(item.Color));
			}
		}
		yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Stat_Thing_StyleDominanceCategory".Translate(), styleCategoryDef.LabelCap, "Stat_Thing_StyleDominanceCategoryDesc".Translate() + "\n\n" + "Stat_Thing_IdeosSatisfied".Translate() + ":" + "\n" + tmpIdeoNames.ToLineList("  - "), 6005);
	}

	public virtual void Notify_ColorChanged()
	{
		graphicInt = null;
		styleGraphicInt = null;
		if (Spawned && (def.drawerType == DrawerType.MapMeshOnly || def.drawerType == DrawerType.MapMeshAndRealTime))
		{
			Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
		}
	}

	public virtual void Notify_Equipped(Pawn pawn)
	{
	}

	public virtual void Notify_Unequipped(Pawn pawn)
	{
	}

	public virtual void Notify_UsedVerb(Pawn pawn, Verb verb)
	{
	}

	public virtual void Notify_UsedWeapon(Pawn pawn)
	{
	}

	public virtual void Notify_DebugSpawned()
	{
	}

	public virtual void Notify_MinifiedThingAboutToBeDestroyed(DestroyMode mode)
	{
	}

	public virtual void Notify_RecipeProduced(Pawn pawn)
	{
	}

	public virtual void Notify_SignalReceived(Signal signal)
	{
	}

	public virtual void Notify_Explosion(Explosion explosion)
	{
	}

	public virtual void Notify_BulletImpactNearby(BulletImpactData impactData)
	{
	}

	public virtual void Notify_ThingSelected()
	{
	}

	public virtual TipSignal GetTooltip()
	{
		string text = LabelCap;
		if (def.useHitPoints)
		{
			text = text + "\n" + HitPoints + " / " + MaxHitPoints;
		}
		return new TipSignal(text, thingIDNumber * 251235);
	}

	public virtual bool BlocksPawn(Pawn p)
	{
		if (def.passability == Traversability.Impassable)
		{
			return true;
		}
		if (def.IsFence && p.FenceBlocked)
		{
			return true;
		}
		return false;
	}

	public void SetFactionDirect(Faction newFaction)
	{
		if (!def.CanHaveFaction)
		{
			Log.Error("Tried to SetFactionDirect on " + this?.ToString() + " which cannot have a faction.");
		}
		else
		{
			factionInt = newFaction;
		}
	}

	public virtual void SetFaction(Faction newFaction, Pawn recruiter = null)
	{
		if (!def.CanHaveFaction)
		{
			Log.Error("Tried to SetFaction on " + this?.ToString() + " which cannot have a faction.");
			return;
		}
		Faction previous = factionInt;
		factionInt = newFaction;
		if (Spawned && this is IAttackTarget t)
		{
			Map.attackTargetsCache.UpdateTarget(t);
		}
		QuestUtility.SendQuestTargetSignals(questTags, "ChangedFaction", this.Named("SUBJECT"), newFaction.Named("FACTION"));
		if (newFaction != Faction.OfPlayer)
		{
			QuestUtility.SendQuestTargetSignals(questTags, "ChangedFactionToNonPlayer", this.Named("SUBJECT"), newFaction.Named("FACTION"));
		}
		else
		{
			QuestUtility.SendQuestTargetSignals(questTags, "ChangedFactionToPlayer", this.Named("SUBJECT"), newFaction.Named("FACTION"));
		}
		if (Spawned)
		{
			Map.events.Notify_ThingFactionChanged(previous, factionInt);
		}
	}

	public virtual AcceptanceReport ClaimableBy(Faction by)
	{
		return false;
	}

	public virtual bool AdoptableBy(Faction by, StringBuilder reason = null)
	{
		return false;
	}

	public bool FactionPreventsClaimingOrAdopting(Faction faction, bool forClaim, out string reason)
	{
		reason = null;
		if (faction == null)
		{
			return false;
		}
		if (faction == Faction.OfInsects)
		{
			if (HiveUtility.AnyHivePreventsClaiming(this))
			{
				return true;
			}
		}
		else if (faction == Faction.OfMechanoids)
		{
			if (MechClusterUtility.PartOfActiveMechCluster(this))
			{
				return true;
			}
			foreach (IAttackTarget item in MapHeld.attackTargetsCache.TargetsHostileToFaction(Faction.OfPlayer))
			{
				if (item.Thing == null || item.Thing.Faction != faction)
				{
					continue;
				}
				if (item.Thing is Pawn pawn)
				{
					if (GenHostility.IsActiveThreatToPlayer(pawn))
					{
						if (forClaim)
						{
							reason = "MessageCannotClaimWhenPawnThreatsAreNear".Translate(this.Named("CLAIMABLE"), pawn.Named("THREAT"));
						}
						else
						{
							reason = "MessageCannotAdoptWhilePawnThreatsAreNear".Translate(this.Named("CLAIMABLE"), pawn.Named("THREAT"));
						}
						return true;
					}
					continue;
				}
				if (forClaim)
				{
					reason = "MessageCannotClaimWhenThreatsAreNear".Translate(this.Named("CLAIMABLE"), item.Named("THREAT"));
				}
				else
				{
					reason = "MessageCannotAdoptWhileThreatsAreNear".Translate(this.Named("CLAIMABLE"), item.Named("THREAT"));
				}
				return true;
			}
		}
		else
		{
			if (faction == Faction.OfAncients && Spawned && !Map.IsPlayerHome && GenHostility.AnyHostileActiveThreatToPlayer(Map, countDormantPawnsAsHostile: true, canBeFogged: true))
			{
				return true;
			}
			if (Spawned && faction != Faction.OfPlayer)
			{
				List<Pawn> list = Map.mapPawns.SpawnedPawnsInFaction(faction);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].RaceProps.ToolUser && GenHostility.IsPotentialThreat(list[i]))
					{
						if (forClaim)
						{
							reason = "MessageCannotClaimWhenThreatsAreNear".Translate(this.Named("CLAIMABLE"), list[i].Named("THREAT"));
						}
						else
						{
							reason = "MessageCannotAdoptWhileThreatsAreNear".Translate(this.Named("CLAIMABLE"), list[i].Named("THREAT"));
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	public void SetPositionDirect(IntVec3 newPos)
	{
		positionInt = newPos;
	}

	public void SetStuffDirect(ThingDef newStuff)
	{
		stuffInt = newStuff;
	}

	public override string ToString()
	{
		if (def != null)
		{
			return ThingID;
		}
		return GetType().ToString();
	}

	public bool Equals(Thing other)
	{
		if (other == null)
		{
			return false;
		}
		if (def.category == ThingCategory.Mote)
		{
			return this == other;
		}
		if (thingIDNumber == other.thingIDNumber)
		{
			return def.Equals(other.def);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (thingIDNumber == -1)
		{
			return base.GetHashCode();
		}
		return thingIDNumber;
	}

	public virtual void Discard(bool silentlyRemoveReferences = false)
	{
		if (mapIndexOrState != -2)
		{
			Log.Warning("Tried to discard " + this?.ToString() + " whose state is " + mapIndexOrState + ".");
		}
		else
		{
			mapIndexOrState = -3;
		}
	}

	public virtual void Notify_DefsHotReloaded()
	{
		graphicInt = null;
	}

	public virtual IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
	{
		if (def.butcherProducts == null)
		{
			yield break;
		}
		for (int i = 0; i < def.butcherProducts.Count; i++)
		{
			ThingDefCountClass thingDefCountClass = def.butcherProducts[i];
			int num = GenMath.RoundRandom((float)thingDefCountClass.count * efficiency);
			num = GenMath.RoundRandom((float)num * Find.Storyteller.difficulty.butcherYieldFactor);
			if (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef);
				thing.stackCount = num;
				yield return thing;
			}
		}
	}

	public virtual IEnumerable<Thing> SmeltProducts(float efficiency)
	{
		List<ThingDefCountClass> costListAdj = def.CostListAdjusted(Stuff);
		for (int i = 0; i < costListAdj.Count; i++)
		{
			if (!costListAdj[i].thingDef.intricate && costListAdj[i].thingDef.smeltable)
			{
				int num = GenMath.RoundRandom((float)costListAdj[i].count * 0.25f);
				if (num > 0)
				{
					Thing thing = ThingMaker.MakeThing(costListAdj[i].thingDef);
					thing.stackCount = num;
					yield return thing;
				}
			}
		}
		if (def.smeltProducts != null)
		{
			for (int i = 0; i < def.smeltProducts.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = def.smeltProducts[i];
				Thing thing2 = ThingMaker.MakeThing(thingDefCountClass.thingDef);
				thing2.stackCount = thingDefCountClass.count;
				yield return thing2;
			}
		}
	}

	public float Ingested(Pawn ingester, float nutritionWanted)
	{
		if (Destroyed)
		{
			Log.Error(ingester?.ToString() + " ingested destroyed thing " + this);
			return 0f;
		}
		if (!IngestibleNow)
		{
			Log.Error(ingester?.ToString() + " ingested IngestibleNow=false thing " + this);
			return 0f;
		}
		ingester.mindState.lastIngestTick = Find.TickManager.TicksGame;
		if (ingester.needs.mood != null)
		{
			List<FoodUtility.ThoughtFromIngesting> list = FoodUtility.ThoughtsFromIngesting(ingester, this, def);
			for (int i = 0; i < list.Count; i++)
			{
				Thought_Memory thought_Memory = ThoughtMaker.MakeThought(list[i].thought, list[i].fromPrecept);
				if (thought_Memory is Thought_FoodEaten thought_FoodEaten)
				{
					thought_FoodEaten.SetFood(this);
				}
				ingester.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
			}
		}
		ingester.needs.drugsDesire?.Notify_IngestedDrug(this);
		bool flag = FoodUtility.IsHumanlikeCorpseOrHumanlikeMeat(this, def);
		bool flag2 = FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(this);
		if (flag && ingester.IsColonist)
		{
			TaleRecorder.RecordTale(TaleDefOf.AteRawHumanlikeMeat, ingester);
		}
		if (flag2)
		{
			ingester.mindState.lastHumanMeatIngestedTick = Find.TickManager.TicksGame;
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
			if (flag)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeatDirect, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
			}
		}
		else if (ModsConfig.IdeologyActive && !FoodUtility.AcceptableCannibalNonHumanlikeMeatFood(def))
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteNonCannibalFood, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
		}
		if (def.ingestible.ateEvent != null)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def.ingestible.ateEvent, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
		}
		if (ModsConfig.IdeologyActive)
		{
			FoodKind foodKind = FoodUtility.GetFoodKind(this);
			if (foodKind != FoodKind.Any && !def.IsProcessedFood)
			{
				if (foodKind == FoodKind.Meat)
				{
					if (!flag2)
					{
						Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
					}
				}
				else if (!def.IsDrug && def.ingestible.CachedNutrition > 0f)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteNonMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
				}
			}
			if (FoodUtility.IsVeneratedAnimalMeatOrCorpseOrHasIngredients(this, ingester))
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
			}
			if (def.thingCategories != null && def.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
			{
				if (def.IsFungus)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteFungus, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
				}
				else
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteNonFungusPlant, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
				}
			}
		}
		CompIngredients compIngredients = this.TryGetComp<CompIngredients>();
		if (compIngredients != null)
		{
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			for (int j = 0; j < compIngredients.ingredients.Count; j++)
			{
				if (!flag3 && FoodUtility.GetMeatSourceCategory(compIngredients.ingredients[j]) == MeatSourceCategory.Humanlike)
				{
					ingester.mindState.lastHumanMeatIngestedTick = Find.TickManager.TicksGame;
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteHumanMeatAsIngredient, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
					flag3 = true;
				}
				else if (!flag4 && ingester.Ideo != null && compIngredients.ingredients[j].IsMeat && ingester.Ideo.IsVeneratedAnimal(compIngredients.ingredients[j].ingestible.sourceDef))
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
					flag4 = true;
				}
				if (!flag5 && FoodUtility.GetMeatSourceCategory(compIngredients.ingredients[j]) == MeatSourceCategory.Insect)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteInsectMeatAsIngredient, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
					flag5 = true;
				}
				if (ModsConfig.IdeologyActive && !flag6 && compIngredients.ingredients[j].thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
				{
					if (compIngredients.ingredients[j].IsFungus)
					{
						Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteFungusAsIngredient, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
						flag6 = true;
					}
					else
					{
						flag7 = true;
					}
				}
			}
			if (ModsConfig.IdeologyActive && !flag6 && flag7)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.AteNonFungusMealWithPlants, ingester.Named(HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: false);
			}
		}
		IngestedCalculateAmounts(ingester, nutritionWanted, out var numTaken, out var nutritionIngested);
		if (!ingester.Dead && ingester.needs.joy != null && Mathf.Abs(def.ingestible.joy) > 0.0001f && numTaken > 0)
		{
			ingester.needs.joy.GainJoy((float)numTaken * def.ingestible.joy, def.ingestible.joyKind ?? JoyKindDefOf.Gluttonous);
		}
		float poisonChanceOverride;
		float chance = (FoodUtility.TryGetFoodPoisoningChanceOverrideFromTraits(ingester, this, out poisonChanceOverride) ? poisonChanceOverride : (this.GetStatValue(StatDefOf.FoodPoisonChanceFixedHuman) * FoodUtility.GetFoodPoisonChanceFactor(ingester)));
		if (ingester.RaceProps.Humanlike && Rand.Chance(chance))
		{
			FoodUtility.AddFoodPoisoningHediff(ingester, this, FoodPoisonCause.DangerousFoodType);
		}
		List<Hediff> hediffs = ingester.health.hediffSet.hediffs;
		for (int k = 0; k < hediffs.Count; k++)
		{
			hediffs[k].Notify_IngestedThing(this, numTaken);
		}
		ingester.genes?.Notify_IngestedThing(this, numTaken);
		bool flag8 = false;
		if (numTaken > 0)
		{
			if (stackCount == 0)
			{
				Log.Error(this?.ToString() + " stack count is 0.");
			}
			if (numTaken == stackCount)
			{
				flag8 = true;
			}
			else
			{
				SplitOff(numTaken);
			}
		}
		PrePostIngested(ingester);
		if (flag8)
		{
			ingester.carryTracker.innerContainer.Remove(this);
		}
		if (def.ingestible.outcomeDoers != null)
		{
			for (int l = 0; l < def.ingestible.outcomeDoers.Count; l++)
			{
				def.ingestible.outcomeDoers[l].DoIngestionOutcome(ingester, this, numTaken);
			}
		}
		if (flag8 && !Destroyed)
		{
			Destroy();
		}
		PostIngested(ingester);
		return nutritionIngested;
	}

	protected virtual void PrePostIngested(Pawn ingester)
	{
	}

	protected virtual void PostIngested(Pawn ingester)
	{
	}

	protected virtual void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
	{
		float num = FoodUtility.NutritionForEater(ingester, this);
		numTaken = Mathf.CeilToInt(nutritionWanted / num);
		numTaken = Mathf.Min(numTaken, stackCount);
		if (def.ingestible.maxNumToIngestAtOnce > 0)
		{
			numTaken = Mathf.Min(numTaken, def.ingestible.maxNumToIngestAtOnce);
		}
		numTaken = Mathf.Max(numTaken, 1);
		nutritionIngested = (float)numTaken * num;
	}

	public virtual bool PreventPlayerSellingThingsNearby(out string reason)
	{
		reason = null;
		return false;
	}

	public virtual void PreSwapMap()
	{
		beingTransportedOnGravship = true;
	}

	public virtual void PostSwapMap()
	{
		beingTransportedOnGravship = false;
		QuestUtility.SendQuestTargetSignals(questTags, "SwappedMap", this.Named("SUBJECT"));
	}

	public virtual void Notify_LeftBehind()
	{
		QuestUtility.SendQuestTargetSignals(questTags, "LeftBehind", this.Named("SUBJECT"));
		if (!(this is IThingHolder thingHolder) || thingHolder.GetDirectlyHeldThings() == null)
		{
			return;
		}
		foreach (Thing item in thingHolder.GetDirectlyHeldThings().ToList())
		{
			item.Notify_LeftBehind();
		}
	}
}
