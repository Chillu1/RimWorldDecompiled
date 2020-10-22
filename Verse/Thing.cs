using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class Thing : Entity, IExposable, ISelectable, ILoadReferenceable, ISignalReceiver
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

		private int hitPointsInt = -1;

		public ThingOwner holdingOwner;

		public List<string> questTags;

		protected const sbyte UnspawnedState = -1;

		private const sbyte MemoryState = -2;

		private const sbyte DiscardedState = -3;

		public static bool allowDestroyNonDestroyable = false;

		private static List<string> tmpDeteriorationReasons = new List<string>();

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

		public int MaxHitPoints => Mathf.RoundToInt(this.GetStatValue(StatDefOf.MaxHitPoints));

		public float MarketValue => this.GetStatValue(StatDefOf.MarketValue);

		public virtual float RoyalFavorValue => this.GetStatValue(StatDefOf.RoyalFavorValue);

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
				if (mapIndexOrState < 0)
				{
					return false;
				}
				if (mapIndexOrState < Find.Maps.Count)
				{
					return true;
				}
				Log.ErrorOnce("Thing is associated with invalid map index", 64664487);
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

		public Map Map
		{
			get
			{
				if (mapIndexOrState >= 0)
				{
					return Find.Maps[mapIndexOrState];
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
				if (ParentHolder != null)
				{
					return ThingOwnerUtility.GetRootMap(ParentHolder);
				}
				return null;
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
				}
				positionInt = value;
				if (Spawned)
				{
					Map.thingGrid.Register(this);
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
				if (value == rotationInt)
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

		public IThingHolder ParentHolder
		{
			get
			{
				if (holdingOwner == null)
				{
					return null;
				}
				return holdingOwner.Owner;
			}
		}

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

		public virtual Graphic Graphic => DefaultGraphic;

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
				if (Tile >= 0)
				{
					return GenTemperature.GetTemperatureAtTile(Tile);
				}
				return 21f;
			}
		}

		public int Tile
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
				return -1;
			}
		}

		public bool Suspended
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

		public virtual string DescriptionDetailed => def.DescriptionDetailed;

		public virtual string DescriptionFlavor => def.description;

		public TerrainAffordanceDef TerrainAffordanceNeeded => def.GetTerrainAffordanceNeed(stuffInt);

		public virtual Vector3 DrawPos => this.TrueCenter();

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
				Log.Error(string.Concat("Cannot set instance color on non-ThingWithComps ", LabelCap, " at ", Position, "."));
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

		public static int IDNumberFromThingID(string thingID)
		{
			string value = Regex.Match(thingID, "\\d+$").Value;
			int result = 0;
			try
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				result = Convert.ToInt32(value, invariantCulture);
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Could not convert id number from thingID=" + thingID + ", numString=" + value + " Exception=" + ex.ToString());
				return result;
			}
		}

		public virtual void PostMake()
		{
			ThingIDMaker.GiveIDTo(this);
			if (def.useHitPoints)
			{
				HitPoints = Mathf.RoundToInt((float)MaxHitPoints * Mathf.Clamp01(def.startingHpRange.RandomInRange));
			}
		}

		public string GetUniqueLoadID()
		{
			return "Thing_" + ThingID;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (Destroyed)
			{
				Log.Error(string.Concat("Spawning destroyed thing ", this, " at ", Position, ". Correcting."));
				mapIndexOrState = -1;
				if (HitPoints <= 0 && def.useHitPoints)
				{
					HitPoints = 1;
				}
			}
			if (Spawned)
			{
				Log.Error(string.Concat("Tried to spawn already-spawned thing ", this, " at ", Position));
				return;
			}
			int num = Find.Maps.IndexOf(map);
			if (num < 0)
			{
				Log.Error(string.Concat("Tried to spawn thing ", this, ", but the map provided does not exist."));
				return;
			}
			if (stackCount > def.stackLimit)
			{
				Log.Error(string.Concat("Spawned ", this, " with stackCount ", stackCount, " but stackLimit is ", def.stackLimit, ". Truncating."));
				stackCount = def.stackLimit;
			}
			mapIndexOrState = (sbyte)num;
			RegionListersUpdater.RegisterInRegions(this, map);
			if (!map.spawnedThings.TryAdd(this, canMergeWithExistingStacks: false))
			{
				Log.Error(string.Concat("Couldn't add thing ", this, " to spawned things."));
			}
			map.listerThings.Add(this);
			map.thingGrid.Register(this);
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
			if (def.graphicData != null && def.graphicData.Linked)
			{
				map.linkGrid.Notify_LinkerCreatedOrDestroyed(this);
				map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, regenAdjacentCells: true, regenAdjacentSections: false);
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
				map.pathGrid.RecalculatePerceivedPathCostUnderThing(this);
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
			(map.regionGrid.GetValidRegionAt_NoRebuild(Position)?.Room)?.Notify_ContainedThingSpawnedOrDespawned(this);
			StealAIDebugDrawer.Notify_ThingChanged(this);
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				map.haulDestinationManager.AddHaulDestination(haulDestination);
			}
			if (this is IThingHolder && Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			if (def.category == ThingCategory.Item)
			{
				SlotGroup slotGroup = Position.GetSlotGroup(map);
				if (slotGroup != null && slotGroup.parent != null)
				{
					slotGroup.parent.Notify_ReceivedThing(this);
				}
			}
			if (def.receivesSignals)
			{
				Find.SignalManager.RegisterReceiver(this);
			}
			if (!respawningAfterLoad)
			{
				QuestUtility.SendQuestTargetSignals(questTags, "Spawned", this.Named("SUBJECT"));
			}
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
			if (def.graphicData != null && def.graphicData.Linked)
			{
				map.linkGrid.Notify_LinkerCreatedOrDestroyed(this);
				map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, regenAdjacentCells: true, regenAdjacentSections: false);
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
			(map.regionGrid.GetValidRegionAt_NoRebuild(Position)?.Room)?.Notify_ContainedThingSpawnedOrDespawned(this);
			if (def.AffectsRegions)
			{
				map.regionDirtyer.Notify_ThingAffectingRegionsDespawned(this);
			}
			if (def.pathCost != 0 || def.passability == Traversability.Impassable)
			{
				map.pathGrid.RecalculatePerceivedPathCostUnderThing(this);
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
			StealAIDebugDrawer.Notify_ThingChanged(this);
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				map.haulDestinationManager.RemoveHaulDestination(haulDestination);
			}
			if (this is IThingHolder && Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			if (def.category == ThingCategory.Item)
			{
				SlotGroup slotGroup = Position.GetSlotGroup(map);
				if (slotGroup != null && slotGroup.parent != null)
				{
					slotGroup.parent.Notify_LostThing(this);
				}
			}
			QuestUtility.SendQuestTargetSignals(questTags, "Despawned", this.Named("SUBJECT"));
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
			if (Spawned)
			{
				DeSpawn(mode);
			}
			mapIndexOrState = -2;
			if (def.DiscardOnDestroyed)
			{
				Discard();
			}
			CompExplosive compExplosive = this.TryGetComp<CompExplosive>();
			if (spawned)
			{
				List<Thing> list = ((compExplosive != null) ? new List<Thing>() : null);
				GenLeaving.DoLeavingsFor(this, map, mode, list);
				compExplosive?.AddThingsIgnoredByExplosion(list);
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
				QuestUtility.SendQuestTargetSignals(questTags, "Killed", this.Named("SUBJECT"));
			}
		}

		public virtual void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
		}

		public virtual void PostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
			if (def.colorGeneratorInTraderStock != null)
			{
				this.SetColor(def.colorGeneratorInTraderStock.NewRandomizedColor());
			}
		}

		public virtual void Notify_MyMapRemoved()
		{
			if (def.receivesSignals)
			{
				Find.SignalManager.DeregisterReceiver(this);
			}
			if (!ThingOwnerUtility.AnyParentIs<Pawn>(this))
			{
				mapIndexOrState = -3;
			}
			RemoveAllReservationsAndDesignationsOnThis();
		}

		public virtual void Notify_LordDestroyed()
		{
		}

		public void ForceSetStateToUnspawned()
		{
			mapIndexOrState = -1;
		}

		public void DecrementMapIndex()
		{
			if (mapIndexOrState <= 0)
			{
				Log.Warning(string.Concat("Tried to decrement map index for ", this, ", but mapIndexOrState=", mapIndexOrState));
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
				IAttackTarget attackTarget = this as IAttackTarget;
				if (attackTarget != null)
				{
					maps[i].attackTargetReservationManager.ReleaseAllForTarget(attackTarget);
				}
				maps[i].designationManager.RemoveAllDesignationsOn(this);
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			if (def.HasThingIDNumber)
			{
				string value = ThingID;
				Scribe_Values.Look(ref value, "id");
				ThingID = value;
			}
			Scribe_Values.Look<sbyte>(ref mapIndexOrState, "map", -1);
			if (Scribe.mode == LoadSaveMode.LoadingVars && mapIndexOrState >= 0)
			{
				mapIndexOrState = -1;
			}
			Scribe_Values.Look(ref positionInt, "pos", IntVec3.Invalid);
			Scribe_Values.Look(ref rotationInt, "rot", Rot4.North);
			if (def.useHitPoints)
			{
				Scribe_Values.Look(ref hitPointsInt, "health", -1);
			}
			bool flag = def.tradeability != 0 && def.category == ThingCategory.Item;
			if (def.stackLimit > 1 || flag)
			{
				Scribe_Values.Look(ref stackCount, "stackCount", 0, forceSave: true);
			}
			Scribe_Defs.Look(ref stuffInt, "stuff");
			string facID = ((factionInt != null) ? factionInt.GetUniqueLoadID() : "null");
			Scribe_Values.Look(ref facID, "faction", "null");
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (facID == "null")
				{
					factionInt = null;
				}
				else if (Find.World != null && Find.FactionManager != null)
				{
					factionInt = Find.FactionManager.AllFactions.FirstOrDefault((Faction fa) => fa.GetUniqueLoadID() == facID);
				}
			}
			Scribe_Collections.Look(ref questTags, "questTags", LookMode.Value);
			BackCompatibility.PostExposeData(this);
		}

		public virtual void PostMapInit()
		{
		}

		public virtual void Draw()
		{
			DrawAt(DrawPos);
		}

		public virtual void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
		}

		public virtual void Print(SectionLayer layer)
		{
			Graphic.Print(layer, this);
		}

		public void DirtyMapMesh(Map map)
		{
			if (def.drawerType == DrawerType.RealtimeOnly)
			{
				return;
			}
			foreach (IntVec3 item in this.OccupiedRect())
			{
				map.mapDrawer.MapMeshDirty(item, MapMeshFlag.Things);
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
			if (def.hasInteractionCell)
			{
				GenDraw.DrawInteractionCell(def, Position, rotationInt);
			}
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
			SteadyEnvironmentEffects.FinalDeteriorationRate(this, tmpDeteriorationReasons);
			if (tmpDeteriorationReasons.Count != 0)
			{
				result = string.Format("{0}: {1}", "DeterioratingBecauseOf".Translate(), tmpDeteriorationReasons.ToCommaList().CapitalizeFirst());
			}
			return result;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			yield break;
		}

		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			yield break;
		}

		public virtual IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			yield break;
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
			if (dinfo.Def.ExternalViolenceFor(this))
			{
				GenLeaving.DropFilthDueToDamage(this, damageResult.totalDamageDealt);
				if (dinfo.Instigator != null)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (pawn != null)
					{
						pawn.records.AddTo(RecordDefOf.DamageDealt, damageResult.totalDamageDealt);
						pawn.records.AccumulateStoryEvent(StoryEventDefOf.DamageDealt);
					}
				}
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
					Log.Error(string.Concat("Tried to split off ", count, " of ", this, " but there are only ", stackCount));
				}
				if (Spawned)
				{
					DeSpawn();
				}
				if (holdingOwner != null)
				{
					holdingOwner.Remove(this);
				}
				return this;
			}
			Thing thing = ThingMaker.MakeThing(def, Stuff);
			thing.stackCount = count;
			stackCount -= count;
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
		}

		public virtual void Notify_ColorChanged()
		{
			graphicInt = null;
			if (Spawned && (def.drawerType == DrawerType.MapMeshOnly || def.drawerType == DrawerType.MapMeshAndRealTime))
			{
				Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
			}
		}

		public virtual void Notify_Equipped(Pawn pawn)
		{
		}

		public virtual void Notify_UsedWeapon(Pawn pawn)
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
			return def.passability == Traversability.Impassable;
		}

		public void SetFactionDirect(Faction newFaction)
		{
			if (!def.CanHaveFaction)
			{
				Log.Error(string.Concat("Tried to SetFactionDirect on ", this, " which cannot have a faction."));
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
				Log.Error(string.Concat("Tried to SetFaction on ", this, " which cannot have a faction."));
				return;
			}
			factionInt = newFaction;
			if (Spawned)
			{
				IAttackTarget attackTarget = this as IAttackTarget;
				if (attackTarget != null)
				{
					Map.attackTargetsCache.UpdateTarget(attackTarget);
				}
			}
			QuestUtility.SendQuestTargetSignals(questTags, "ChangedFaction", this.Named("SUBJECT"), newFaction.Named("FACTION"));
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

		public override int GetHashCode()
		{
			return thingIDNumber;
		}

		public virtual void Discard(bool silentlyRemoveReferences = false)
		{
			if (mapIndexOrState != -2)
			{
				Log.Warning(string.Concat("Tried to discard ", this, " whose state is ", mapIndexOrState, "."));
			}
			else
			{
				mapIndexOrState = -3;
			}
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
			for (int j = 0; j < costListAdj.Count; j++)
			{
				if (!costListAdj[j].thingDef.intricate)
				{
					int num = GenMath.RoundRandom((float)costListAdj[j].count * 0.25f);
					if (num > 0)
					{
						Thing thing = ThingMaker.MakeThing(costListAdj[j].thingDef);
						thing.stackCount = num;
						yield return thing;
					}
				}
			}
			if (def.smeltProducts != null)
			{
				for (int j = 0; j < def.smeltProducts.Count; j++)
				{
					ThingDefCountClass thingDefCountClass = def.smeltProducts[j];
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
				Log.Error(string.Concat(ingester, " ingested destroyed thing ", this));
				return 0f;
			}
			if (!IngestibleNow)
			{
				Log.Error(string.Concat(ingester, " ingested IngestibleNow=false thing ", this));
				return 0f;
			}
			ingester.mindState.lastIngestTick = Find.TickManager.TicksGame;
			if (ingester.needs.mood != null)
			{
				List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(ingester, this, def);
				for (int i = 0; i < list.Count; i++)
				{
					ingester.needs.mood.thoughts.memories.TryGainMemory(list[i]);
				}
			}
			if (ingester.needs.drugsDesire != null)
			{
				ingester.needs.drugsDesire.Notify_IngestedDrug(this);
			}
			if (ingester.IsColonist && FoodUtility.IsHumanlikeMeatOrHumanlikeCorpse(this))
			{
				TaleRecorder.RecordTale(TaleDefOf.AteRawHumanlikeMeat, ingester);
			}
			IngestedCalculateAmounts(ingester, nutritionWanted, out var numTaken, out var nutritionIngested);
			if (!ingester.Dead && ingester.needs.joy != null && Mathf.Abs(def.ingestible.joy) > 0.0001f && numTaken > 0)
			{
				JoyKindDef joyKind = ((def.ingestible.joyKind != null) ? def.ingestible.joyKind : JoyKindDefOf.Gluttonous);
				ingester.needs.joy.GainJoy((float)numTaken * def.ingestible.joy, joyKind);
			}
			if (ingester.RaceProps.Humanlike && Rand.Chance(this.GetStatValue(StatDefOf.FoodPoisonChanceFixedHuman) * FoodUtility.GetFoodPoisonChanceFactor(ingester)))
			{
				FoodUtility.AddFoodPoisoningHediff(ingester, this, FoodPoisonCause.DangerousFoodType);
			}
			bool flag = false;
			if (numTaken > 0)
			{
				if (stackCount == 0)
				{
					Log.Error(string.Concat(this, " stack count is 0."));
				}
				if (numTaken == stackCount)
				{
					flag = true;
				}
				else
				{
					SplitOff(numTaken);
				}
			}
			PrePostIngested(ingester);
			if (flag)
			{
				ingester.carryTracker.innerContainer.Remove(this);
			}
			if (def.ingestible.outcomeDoers != null)
			{
				for (int j = 0; j < def.ingestible.outcomeDoers.Count; j++)
				{
					def.ingestible.outcomeDoers[j].DoIngestionOutcome(ingester, this);
				}
			}
			if (flag)
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
			numTaken = Mathf.CeilToInt(nutritionWanted / this.GetStatValue(StatDefOf.Nutrition));
			numTaken = Mathf.Min(numTaken, def.ingestible.maxNumToIngestAtOnce, stackCount);
			numTaken = Mathf.Max(numTaken, 1);
			nutritionIngested = (float)numTaken * this.GetStatValue(StatDefOf.Nutrition);
		}

		public virtual bool PreventPlayerSellingThingsNearby(out string reason)
		{
			reason = null;
			return false;
		}

		public virtual ushort PathFindCostFor(Pawn p)
		{
			return 0;
		}
	}
}
