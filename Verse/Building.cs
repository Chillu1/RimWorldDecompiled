using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;
using Verse.Sound;

namespace Verse;

public class Building : ThingWithComps
{
	private ColorDef paintColorDef;

	public bool canChangeTerrainOnDestroyed = true;

	private static readonly SimpleCurve ShakeAmountPerAreaCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0.07f),
		new CurvePoint(2f, 0.07f),
		new CurvePoint(4f, 0.1f),
		new CurvePoint(9f, 0.2f),
		new CurvePoint(16f, 0.5f)
	};

	private const float ChanceToGeneratePaintedFromTrader = 0.1f;

	private const int BuildingMinTickRate = 6;

	public CompPower PowerComp => GetComp<CompPower>();

	public ColorDef PaintColorDef => paintColorDef;

	public override Color DrawColor
	{
		get
		{
			if (paintColorDef != null)
			{
				return paintColorDef.color;
			}
			return base.DrawColor;
		}
	}

	public virtual bool TransmitsPowerNow => PowerComp?.Props.transmitsPower ?? false;

	public override int HitPoints
	{
		set
		{
			int hitPoints = HitPoints;
			base.HitPoints = value;
			BuildingsDamageSectionLayerUtility.Notify_BuildingHitPointsChanged(this, hitPoints);
			if (base.Spawned)
			{
				base.Map.events.Notify_BuildingHitPointsChanged(this);
			}
		}
	}

	public virtual int MaxItemsInCell => def.building.maxItemsInCell;

	public bool IsClearableFreeBuilding
	{
		get
		{
			if (!def.useHitPoints && !def.scatterableOnMapGen && def.passability == Traversability.Standable)
			{
				return this.GetStatValue(StatDefOf.WorkToBuild) == 0f;
			}
			return false;
		}
	}

	public virtual bool IsAirtight
	{
		get
		{
			if (def.building.isAirtight)
			{
				return true;
			}
			if (def.building.isStuffableAirtight && base.Stuff.stuffProps.isAirtight)
			{
				return true;
			}
			return false;
		}
	}

	public virtual bool ExchangeVacuum
	{
		get
		{
			if (IsAirtight)
			{
				return def.building.alwaysExchangeVacuum;
			}
			return true;
		}
	}

	protected override int MinTickIntervalRate => 6;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref canChangeTerrainOnDestroyed, "canChangeTerrainOnDestroyed", defaultValue: true);
		Scribe_Defs.Look(ref paintColorDef, "paintColorDef");
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (def.IsEdifice())
		{
			map.edificeGrid.Register(this);
			if (def.Fillage == FillCategory.Full)
			{
				map.terrainGrid.Drawer.SetDirty();
			}
			if (def.AffectsFertility)
			{
				map.fertilityGrid.Drawer.SetDirty();
			}
		}
		base.SpawnSetup(map, respawningAfterLoad);
		base.Map.listerBuildings.Add(this);
		if (def.coversFloor)
		{
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
		}
		CellRect cellRect = this.OccupiedRect();
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				IntVec3 intVec = new IntVec3(j, 0, i);
				base.Map.mapDrawer.MapMeshDirty(intVec, MapMeshFlagDefOf.Buildings);
				if (def.blockLight)
				{
					base.Map.glowGrid.LightBlockerAdded(intVec);
				}
				if (!SnowGrid.CanCoexistWithSnow(def))
				{
					base.Map.snowGrid.SetDepth(intVec, 0f);
				}
				if (ModsConfig.OdysseyActive && !SandGrid.CanCoexistWithSand(def))
				{
					base.Map.sandGrid.SetDepth(intVec, 0f);
				}
			}
		}
		if (base.Faction == Faction.OfPlayer && def.building != null && def.building.spawnedConceptLearnOpportunity != null)
		{
			LessonAutoActivator.TeachOpportunity(def.building.spawnedConceptLearnOpportunity, OpportunityType.GoodToKnow);
		}
		AutoHomeAreaMaker.Notify_BuildingSpawned(this);
		base.Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
		base.Map.listerArtificialBuildingsForMeditation.Notify_BuildingSpawned(this);
		base.Map.listerBuldingOfDefInProximity.Notify_BuildingSpawned(this);
		base.Map.listerBuildingWithTagInProximity.Notify_BuildingSpawned(this);
		if (!this.CanBeSeenOver())
		{
			base.Map.exitMapGrid.Notify_LOSBlockerSpawned();
		}
		SmoothSurfaceDesignatorUtility.Notify_BuildingSpawned(this);
		map.events.Notify_BuildingSpawned(this);
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		List<Thing> attachedBuildings = GenConstruct.GetAttachedBuildings(this);
		base.DeSpawn(mode);
		if ((!def.IsFrame || mode != DestroyMode.Vanish) && (!def.building.isNaturalRock || mode != DestroyMode.WillReplace) && mode != DestroyMode.WillReplace)
		{
			foreach (Thing item in attachedBuildings)
			{
				if (item.def.Minifiable)
				{
					GenSpawn.Spawn(item.MakeMinified(), item.Position, map);
				}
				else
				{
					item.Destroy(mode);
				}
			}
		}
		if (def.IsEdifice())
		{
			map.edificeGrid.DeRegister(this);
			if (def.Fillage == FillCategory.Full)
			{
				map.terrainGrid.Drawer.SetDirty();
				if (ModsConfig.BiotechActive)
				{
					map.pollutionGrid.Drawer.SetDirty();
				}
			}
			if (def.AffectsFertility)
			{
				map.fertilityGrid.Drawer.SetDirty();
			}
		}
		if (mode != DestroyMode.WillReplace)
		{
			if (def.MakeFog)
			{
				map.fogGrid.Notify_FogBlockerRemoved(this);
			}
			if (def.holdsRoof)
			{
				RoofCollapseCellsFinder.Notify_RoofHolderDespawned(this, map);
			}
			if (def.IsSmoothable)
			{
				SmoothSurfaceDesignatorUtility.Notify_BuildingDespawned(this, map);
			}
		}
		CellRect cellRect = this.OccupiedRect();
		for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
		{
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				IntVec3 intVec = new IntVec3(j, 0, i);
				ulong num = MapMeshFlagDefOf.Buildings;
				if (def.coversFloor)
				{
					num |= (ulong)MapMeshFlagDefOf.Terrain;
				}
				if (def.Fillage == FillCategory.Full)
				{
					num |= (ulong)MapMeshFlagDefOf.Roofs;
					num |= (ulong)MapMeshFlagDefOf.Snow;
					if (ModsConfig.OdysseyActive)
					{
						num |= (ulong)MapMeshFlagDefOf.Sand;
					}
				}
				map.mapDrawer.MapMeshDirty(intVec, num);
				if (def.blockLight)
				{
					map.glowGrid.LightBlockerRemoved(intVec);
				}
			}
		}
		map.listerBuildings.Remove(this);
		map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
		map.listerArtificialBuildingsForMeditation.Notify_BuildingDeSpawned(this);
		map.listerBuldingOfDefInProximity.Notify_BuildingDeSpawned(this);
		map.listerBuildingWithTagInProximity.Notify_BuildingDeSpawned(this);
		if (def.building.leaveTerrain != null && Current.ProgramState == ProgramState.Playing && canChangeTerrainOnDestroyed)
		{
			foreach (IntVec3 item2 in this.OccupiedRect())
			{
				map.terrainGrid.SetTerrain(item2, def.building.leaveTerrain);
			}
		}
		map.designationManager.Notify_BuildingDespawned(this);
		if (!this.CanBeSeenOver())
		{
			map.exitMapGrid.Notify_LOSBlockerDespawned();
		}
		if (def.building.hasFuelingPort)
		{
			CompLaunchable compLaunchable = FuelingPortUtility.LaunchableAt(FuelingPortUtility.GetFuelingPortCell(base.Position, base.Rotation), map);
			if (compLaunchable != null && !base.BeingTransportedOnGravship)
			{
				compLaunchable.Notify_FuelingPortSourceDeSpawned();
			}
		}
		map.events.Notify_BuildingDespawned(this);
		if (MaxItemsInCell < 2)
		{
			return;
		}
		foreach (IntVec3 item3 in this.OccupiedRect())
		{
			int itemCount = item3.GetItemCount(map);
			if (itemCount <= 1 || itemCount <= item3.GetMaxItemsAllowedInCell(map))
			{
				continue;
			}
			for (int k = 0; k < itemCount - 1; k++)
			{
				Thing firstItem = item3.GetFirstItem(map);
				if (firstItem == null)
				{
					break;
				}
				firstItem.DeSpawn();
				GenPlace.TryPlaceThing(firstItem, item3, map, ThingPlaceMode.Near);
				if (item3.GetItemCount(map) <= item3.GetMaxItemsAllowedInCell(map))
				{
					break;
				}
			}
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		bool spawned = base.Spawned;
		Map map = base.Map;
		SmoothableWallUtility.Notify_BuildingDestroying(this, mode);
		this.GetLord()?.Notify_BuildingLost(this);
		base.Destroy(mode);
		InstallBlueprintUtility.CancelBlueprintsFor(this);
		if (spawned)
		{
			switch (mode)
			{
			case DestroyMode.Deconstruct:
				SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(base.Position, map));
				break;
			case DestroyMode.KillFinalize:
				DoDestroyEffects(map);
				break;
			}
		}
		if (spawned && ThingUtility.CheckAutoRebuildOnDestroyed(this, mode, map, def) is Blueprint_Storage blueprint_Storage && this is Building_Storage building_Storage)
		{
			blueprint_Storage.SetStorageGroup(building_Storage.storageGroup);
			blueprint_Storage.settings = new StorageSettings();
			blueprint_Storage.settings.CopyFrom(building_Storage.settings);
		}
	}

	public override void SetFaction(Faction newFaction, Pawn recruiter = null)
	{
		if (base.Spawned)
		{
			base.Map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
			base.Map.listerBuildingWithTagInProximity.Notify_BuildingDeSpawned(this);
			base.Map.listerBuildings.Remove(this);
		}
		base.SetFaction(newFaction, recruiter);
		if (base.Spawned)
		{
			base.Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
			base.Map.listerArtificialBuildingsForMeditation.Notify_BuildingSpawned(this);
			base.Map.listerBuildingWithTagInProximity.Notify_BuildingSpawned(this);
			base.Map.listerBuildings.Add(this);
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
			if (newFaction == Faction.OfPlayer)
			{
				AutoHomeAreaMaker.Notify_BuildingClaimed(this);
			}
		}
	}

	public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		if (def.building != null && !def.building.canBeDamagedByAttacks)
		{
			absorbed = true;
			return;
		}
		if (base.Faction != null && base.Spawned && base.Faction != Faction.OfPlayer)
		{
			for (int i = 0; i < base.Map.lordManager.lords.Count; i++)
			{
				Lord lord = base.Map.lordManager.lords[i];
				if (lord.faction == base.Faction)
				{
					lord.Notify_BuildingDamaged(this, dinfo);
				}
			}
		}
		base.PreApplyDamage(ref dinfo, out absorbed);
		if (!absorbed && base.Faction != null)
		{
			base.Faction.Notify_BuildingTookDamage(this, dinfo);
		}
		if (!absorbed)
		{
			GetComp<CompStunnable>()?.ApplyDamage(dinfo);
		}
	}

	public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		base.PostApplyDamage(dinfo, totalDamageDealt);
		if (base.Spawned)
		{
			base.Map.listerBuildingsRepairable.Notify_BuildingTookDamage(this);
		}
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
		if (blueprint_Install != null)
		{
			GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (((def.BuildableByPlayer && def.passability != Traversability.Impassable && !def.IsDoor) || def.building.forceShowRoomStats) && Gizmo_RoomStats.GetRoomToShowStatsFor(this) != null && Find.Selector.SingleSelectedObject == this)
		{
			yield return new Gizmo_RoomStats(this);
		}
		Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
		if (selectMonumentMarkerGizmo != null)
		{
			yield return selectMonumentMarkerGizmo;
		}
		if (def.Minifiable && (base.Faction == Faction.OfPlayer || def.building.alwaysUninstallable))
		{
			yield return InstallationDesignatorDatabase.DesignatorFor(def);
		}
		ColorInt? glowerColorOverride = null;
		CompGlower comp = GetComp<CompGlower>();
		if (comp != null && comp.HasGlowColorOverride)
		{
			glowerColorOverride = comp.GlowColor;
		}
		if (!def.building.neverBuildable)
		{
			Command command = BuildCopyCommandUtility.BuildCopyCommand(def, base.Stuff, base.StyleSourcePrecept as Precept_Building, StyleDef, styleOverridden: true, glowerColorOverride);
			if (command != null)
			{
				yield return command;
			}
		}
		if (base.Faction == Faction.OfPlayer || def.building.alwaysShowRelatedBuildCommands)
		{
			foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def))
			{
				yield return item;
			}
		}
		Lord lord = this.GetLord();
		if (lord == null || lord.CurLordToil == null)
		{
			yield break;
		}
		foreach (Gizmo buildingGizmo in lord.CurLordToil.GetBuildingGizmos(this))
		{
			yield return buildingGizmo;
		}
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		if (PaintColorDef != null && !PaintColorDef.label.NullOrEmpty())
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Building, "Stat_Building_PaintColor".Translate(), PaintColorDef.LabelCap, "Stat_Building_PaintColorDesc".Translate(), 6000);
		}
	}

	public override AcceptanceReport ClaimableBy(Faction by)
	{
		if (!def.Claimable)
		{
			return false;
		}
		if (base.Faction == by)
		{
			return false;
		}
		for (int i = 0; i < base.AllComps.Count; i++)
		{
			if (base.AllComps[i].CompPreventClaimingBy(by))
			{
				return false;
			}
		}
		if (FactionPreventsClaimingOrAdopting(base.Faction ?? base.Map?.ParentFaction, forClaim: true, out var reason))
		{
			return reason;
		}
		return true;
	}

	public virtual AcceptanceReport DeconstructibleBy(Faction faction)
	{
		for (int i = 0; i < base.AllComps.Count; i++)
		{
			if (base.AllComps[i].CompForceDeconstructable())
			{
				return true;
			}
		}
		if (!def.building.IsDeconstructible)
		{
			return false;
		}
		if (DebugSettings.godMode)
		{
			return true;
		}
		if (base.Faction == faction)
		{
			return true;
		}
		if (def.building.alwaysDeconstructible)
		{
			return true;
		}
		return ClaimableBy(faction);
	}

	public virtual ushort PathWalkCostFor(Pawn p)
	{
		return 0;
	}

	public virtual bool IsDangerousFor(Pawn p)
	{
		return false;
	}

	public virtual bool IsWorking()
	{
		return true;
	}

	private void DoDestroyEffects(Map map)
	{
		if (def.building.destroyEffecter != null && !base.Position.Fogged(map))
		{
			Effecter effecter = def.building.destroyEffecter.Spawn(base.Position, map);
			effecter.Trigger(new TargetInfo(base.Position, map), TargetInfo.Invalid);
			effecter.Cleanup();
		}
		else
		{
			if (!def.IsEdifice())
			{
				return;
			}
			GetDestroySound()?.PlayOneShot(new TargetInfo(base.Position, map));
			foreach (IntVec3 item in this.OccupiedRect())
			{
				int num = (def.building.isNaturalRock ? 1 : Rand.RangeInclusive(3, 5));
				for (int i = 0; i < num; i++)
				{
					FleckMaker.ThrowDustPuffThick(item.ToVector3Shifted(), map, Rand.Range(1.5f, 2f), Color.white);
				}
			}
			if (Find.CurrentMap == map)
			{
				float num2 = def.building.destroyShakeAmount;
				if (num2 < 0f)
				{
					num2 = ShakeAmountPerAreaCurve.Evaluate(def.Size.Area);
				}
				CompLifespan compLifespan = this.TryGetComp<CompLifespan>();
				if (compLifespan == null || compLifespan.age < compLifespan.Props.lifespanTicks)
				{
					Find.CameraDriver.shaker.DoShake(num2);
				}
			}
		}
	}

	private SoundDef GetDestroySound()
	{
		if (!def.building.destroySound.NullOrUndefined())
		{
			return def.building.destroySound;
		}
		StuffCategoryDef stuffCategoryDef;
		if (def.MadeFromStuff && base.Stuff != null && !base.Stuff.stuffProps.categories.NullOrEmpty())
		{
			stuffCategoryDef = base.Stuff.stuffProps.categories[0];
		}
		else
		{
			if (def.CostList.NullOrEmpty() || !def.CostList[0].thingDef.IsStuff || def.CostList[0].thingDef.stuffProps.categories.NullOrEmpty())
			{
				return null;
			}
			stuffCategoryDef = def.CostList[0].thingDef.stuffProps.categories[0];
		}
		switch (def.building.buildingSizeCategory)
		{
		case BuildingSizeCategory.Small:
			if (!stuffCategoryDef.destroySoundSmall.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundSmall;
			}
			break;
		case BuildingSizeCategory.Medium:
			if (!stuffCategoryDef.destroySoundMedium.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundMedium;
			}
			break;
		case BuildingSizeCategory.Large:
			if (!stuffCategoryDef.destroySoundLarge.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundLarge;
			}
			break;
		case BuildingSizeCategory.None:
		{
			int area = def.Size.Area;
			if (area <= 1 && !stuffCategoryDef.destroySoundSmall.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundSmall;
			}
			if (area <= 4 && !stuffCategoryDef.destroySoundMedium.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundMedium;
			}
			if (!stuffCategoryDef.destroySoundLarge.NullOrUndefined())
			{
				return stuffCategoryDef.destroySoundLarge;
			}
			break;
		}
		}
		return null;
	}

	public override void PostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
	{
		base.PostGeneratedForTrader(trader, forTile, forFaction);
		if (def.building.paintable && Rand.Value < 0.1f)
		{
			ChangePaint(DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Structure).RandomElement());
		}
		else if (def.colorGeneratorInTraderStock != null)
		{
			this.SetColor(def.colorGeneratorInTraderStock.NewRandomizedColor());
		}
	}

	public override string GetInspectStringLowPriority()
	{
		string text = base.GetInspectStringLowPriority();
		if (!DeconstructibleBy(Faction.OfPlayer) && (def.IsNonDeconstructibleAttackableBuilding || def.building.quickTargetable) && def.building.displayAttackToDestroyOnInspectPane)
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text += "AttackToDestroy".Translate();
		}
		return text;
	}

	public void ChangePaint(ColorDef colorDef)
	{
		paintColorDef = colorDef;
		Notify_ColorChanged();
		if (base.Spawned)
		{
			DrawerType drawerType = def.drawerType;
			if (drawerType == DrawerType.MapMeshOnly || drawerType == DrawerType.MapMeshAndRealTime)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Buildings);
			}
		}
	}

	public static Gizmo SelectContainedItemGizmo(Thing container, Thing item)
	{
		if (!container.Faction.IsPlayerSafe())
		{
			return null;
		}
		return ContainingSelectionUtility.SelectCarriedThingGizmo(container, item);
	}

	public virtual int HaulToContainerDuration(Thing thing)
	{
		return def.building.haulToContainerDuration;
	}
}
