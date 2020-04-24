using RimWorld;
using System.Collections.Generic;
using Verse.AI.Group;
using Verse.Sound;

namespace Verse
{
	public class Building : ThingWithComps
	{
		private Sustainer sustainerAmbient;

		public bool canChangeTerrainOnDestroyed = true;

		public CompPower PowerComp => GetComp<CompPower>();

		public virtual bool TransmitsPowerNow => PowerComp?.Props.transmitsPower ?? false;

		public override int HitPoints
		{
			set
			{
				int hitPoints = HitPoints;
				base.HitPoints = value;
				BuildingsDamageSectionLayerUtility.Notify_BuildingHitPointsChanged(this, hitPoints);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref canChangeTerrainOnDestroyed, "canChangeTerrainOnDestroyed", defaultValue: true);
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
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
			}
			CellRect cellRect = this.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					base.Map.mapDrawer.MapMeshDirty(intVec, MapMeshFlag.Buildings);
					base.Map.glowGrid.MarkGlowGridDirty(intVec);
					if (!SnowGrid.CanCoexistWithSnow(def))
					{
						base.Map.snowGrid.SetDepth(intVec, 0f);
					}
				}
			}
			if (base.Faction == Faction.OfPlayer && def.building != null && def.building.spawnedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(def.building.spawnedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
			AutoHomeAreaMaker.Notify_BuildingSpawned(this);
			if (def.building != null && !def.building.soundAmbient.NullOrUndefined())
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					SoundInfo info = SoundInfo.InMap(this);
					sustainerAmbient = def.building.soundAmbient.TrySpawnSustainer(info);
				});
			}
			base.Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
			if (!this.CanBeSeenOver())
			{
				base.Map.exitMapGrid.Notify_LOSBlockerSpawned();
			}
			SmoothSurfaceDesignatorUtility.Notify_BuildingSpawned(this);
			map.avoidGrid.Notify_BuildingSpawned(this);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			if (def.IsEdifice())
			{
				map.edificeGrid.DeRegister(this);
				if (def.Fillage == FillCategory.Full)
				{
					map.terrainGrid.Drawer.SetDirty();
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
					map.fogGrid.Notify_FogBlockerRemoved(base.Position);
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
			if (sustainerAmbient != null)
			{
				sustainerAmbient.End();
			}
			CellRect cellRect = this.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 loc = new IntVec3(j, 0, i);
					MapMeshFlag mapMeshFlag = MapMeshFlag.Buildings;
					if (def.coversFloor)
					{
						mapMeshFlag |= MapMeshFlag.Terrain;
					}
					if (def.Fillage == FillCategory.Full)
					{
						mapMeshFlag |= MapMeshFlag.Roofs;
						mapMeshFlag |= MapMeshFlag.Snow;
					}
					map.mapDrawer.MapMeshDirty(loc, mapMeshFlag);
					map.glowGrid.MarkGlowGridDirty(loc);
				}
			}
			map.listerBuildings.Remove(this);
			map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
			if (def.building.leaveTerrain != null && Current.ProgramState == ProgramState.Playing && canChangeTerrainOnDestroyed)
			{
				foreach (IntVec3 item in this.OccupiedRect())
				{
					map.terrainGrid.SetTerrain(item, def.building.leaveTerrain);
				}
			}
			map.designationManager.Notify_BuildingDespawned(this);
			if (!this.CanBeSeenOver())
			{
				map.exitMapGrid.Notify_LOSBlockerDespawned();
			}
			if (def.building.hasFuelingPort)
			{
				FuelingPortUtility.LaunchableAt(FuelingPortUtility.GetFuelingPortCell(base.Position, base.Rotation), map)?.Notify_FuelingPortSourceDeSpawned();
			}
			map.avoidGrid.Notify_BuildingDespawned(this);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			SmoothableWallUtility.Notify_BuildingDestroying(this, mode);
			this.GetLord()?.Notify_BuildingLost(this);
			base.Destroy(mode);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
			if (mode == DestroyMode.Deconstruct && spawned)
			{
				SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(base.Position, map));
			}
			if (spawned)
			{
				ThingUtility.CheckAutoRebuildOnDestroyed(this, mode, map, def);
			}
		}

		public override void Draw()
		{
			if (def.drawerType == DrawerType.RealtimeOnly)
			{
				base.Draw();
			}
			else
			{
				Comps_PostDraw();
			}
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (base.Spawned)
			{
				base.Map.listerBuildingsRepairable.Notify_BuildingDeSpawned(this);
				base.Map.listerBuildings.Remove(this);
			}
			base.SetFaction(newFaction, recruiter);
			if (base.Spawned)
			{
				base.Map.listerBuildingsRepairable.Notify_BuildingSpawned(this);
				base.Map.listerBuildings.Add(this);
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.PowerGrid, regenAdjacentCells: true, regenAdjacentSections: false);
				if (newFaction == Faction.OfPlayer)
				{
					AutoHomeAreaMaker.Notify_BuildingClaimed(this);
				}
			}
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
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
			if (def.Minifiable && base.Faction == Faction.OfPlayer)
			{
				yield return InstallationDesignatorDatabase.DesignatorFor(def);
			}
			Command command = BuildCopyCommandUtility.BuildCopyCommand(def, base.Stuff);
			if (command != null)
			{
				yield return command;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				foreach (Command item in BuildFacilityCommandUtility.BuildFacilityCommands(def))
				{
					yield return item;
				}
			}
		}

		public virtual bool ClaimableBy(Faction by)
		{
			if (!def.Claimable)
			{
				return false;
			}
			if (base.Faction != null)
			{
				if (base.Faction == by)
				{
					return false;
				}
				if (by == Faction.OfPlayer)
				{
					if (base.Faction == Faction.OfInsects)
					{
						if (HiveUtility.AnyHivePreventsClaiming(this))
						{
							return false;
						}
					}
					else
					{
						if (base.Faction == Faction.OfMechanoids)
						{
							return false;
						}
						if (base.Spawned)
						{
							List<Pawn> list = base.Map.mapPawns.SpawnedPawnsInFaction(base.Faction);
							for (int i = 0; i < list.Count; i++)
							{
								if (list[i].RaceProps.ToolUser && GenHostility.IsActiveThreatToPlayer(list[i]))
								{
									return false;
								}
							}
						}
					}
				}
			}
			return true;
		}

		public virtual bool DeconstructibleBy(Faction faction)
		{
			if (DebugSettings.godMode)
			{
				return true;
			}
			if (!def.building.IsDeconstructible)
			{
				return false;
			}
			if (base.Faction != faction && !ClaimableBy(faction))
			{
				return def.building.alwaysDeconstructible;
			}
			return true;
		}

		public virtual ushort PathWalkCostFor(Pawn p)
		{
			return 0;
		}

		public virtual bool IsDangerousFor(Pawn p)
		{
			return false;
		}
	}
}
