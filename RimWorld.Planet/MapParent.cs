using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class MapParent : WorldObject, IThingHolder
	{
		public bool forceRemoveWorldObjectWhenMapRemoved;

		public bool doorsAlwaysOpenForPlayerPawns;

		private HashSet<IncidentTargetTagDef> hibernatableIncidentTargets;

		private static readonly Texture2D ShowMapCommand = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap");

		private static Rot4 shuttleRotation = Rot4.East;

		public bool HasMap => Map != null;

		public virtual AcceptanceReport CanBeSettled => true;

		protected virtual bool UseGenericEnterMapFloatMenuOption => true;

		public Map Map => Current.Game.FindMap(this);

		public virtual MapGeneratorDef MapGeneratorDef => def.mapGenerator ?? MapGeneratorDefOf.Encounter;

		public virtual IEnumerable<GenStepWithParams> ExtraGenStepDefs => Enumerable.Empty<GenStepWithParams>();

		public override bool ExpandMore
		{
			get
			{
				if (!base.ExpandMore)
				{
					return HasMap;
				}
				return true;
			}
		}

		public virtual bool HandlesConditionCausers => false;

		public virtual Vector3 WorldCameraPosition => base.Tile.Layer.GetTileCenter(base.Tile);

		public virtual void PostMapGenerate()
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMapGenerate();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "MapGenerated", this.Named("SUBJECT"));
		}

		public virtual void Notify_MyMapAboutToBeRemoved()
		{
		}

		public virtual void Notify_MyMapRemoved(Map map)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMyMapRemoved();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "MapRemoved", this.Named("SUBJECT"));
		}

		public virtual void Notify_MyMapSettled(Map map)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMyMapSettled();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "MapSettled", this.Named("SUBJECT"));
		}

		public virtual void Notify_CaravanFormed(Caravan caravan)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostCaravanFormed(caravan);
			}
		}

		public virtual void Notify_HibernatableChanged()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual void FinalizeLoading()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (HasMap)
			{
				Current.Game.DeinitAndRemoveMap(Map, notifyPlayer: true);
			}
		}

		public virtual void Abandon(bool wasGravshipLaunch)
		{
			bool flag = false;
			int count = Find.Maps.Count;
			for (int i = 0; i < count; i++)
			{
				if (Find.Maps[i].IsPlayerHome && Find.Maps[i] != Map)
				{
					flag = true;
					break;
				}
			}
			if (Map.IsPlayerHome && !flag)
			{
				foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
				{
					allMapsCaravansAndTravellingTransporters_Alive_Colonist.genes?.Notify_NewColony();
					if (allMapsCaravansAndTravellingTransporters_Alive_Colonist.BeingTransportedOnGravship)
					{
						continue;
					}
					MemoryThoughtHandler memoryThoughtHandler = allMapsCaravansAndTravellingTransporters_Alive_Colonist.needs?.mood?.thoughts?.memories;
					if (memoryThoughtHandler != null)
					{
						memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyOptimism);
						memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyHope);
						if (allMapsCaravansAndTravellingTransporters_Alive_Colonist.IsFreeNonSlaveColonist)
						{
							memoryThoughtHandler.TryGainMemory(ThoughtDefOf.NewColonyOptimism);
						}
					}
				}
			}
			Destroy();
			if (wasGravshipLaunch && base.Tile.LayerDef == PlanetLayerDefOf.Surface)
			{
				WorldObject worldObject = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.GravshipLaunch);
				worldObject.Tile = base.Tile;
				worldObject.SetFaction(base.Faction);
				Find.WorldObjects.Add(worldObject);
			}
		}

		public override void Destroy()
		{
			if (Map != null)
			{
				for (int num = Map.listerThings.AllThings.Count - 1; num >= 0; num--)
				{
					Map.listerThings.AllThings[num].Notify_LeftBehind();
				}
			}
			base.Destroy();
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			CheckRemoveMapNow();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forceRemoveWorldObjectWhenMapRemoved, "forceRemoveWorldObjectWhenMapRemoved", defaultValue: false);
			Scribe_Values.Look(ref doorsAlwaysOpenForPlayerPawns, "doorsAlwaysOpenForPlayerPawns", defaultValue: false);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (!HasMap)
			{
				yield break;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandShowMap".Translate();
			command_Action.defaultDesc = "CommandShowMapDesc".Translate();
			command_Action.icon = ShowMapCommand;
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			command_Action.action = delegate
			{
				Current.Game.CurrentMap = Map;
				if (!CameraJumper.TryHideWorld())
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			};
			yield return command_Action;
		}

		public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			foreach (IncidentTargetTagDef item in base.IncidentTargetTags())
			{
				yield return item;
			}
			if (hibernatableIncidentTargets != null && hibernatableIncidentTargets.Count > 0)
			{
				foreach (IncidentTargetTagDef hibernatableIncidentTarget in hibernatableIncidentTargets)
				{
					yield return hibernatableIncidentTarget;
				}
			}
			if (HasMap && Map.wasSpawnedViaGravShipLanding)
			{
				yield return IncidentTargetTagDefOf.Map_PlayerHome;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
			{
				yield return floatMenuOption;
			}
			if (!UseGenericEnterMapFloatMenuOption)
			{
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_Enter.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption2;
			}
		}

		public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			foreach (FloatMenuOption transportersFloatMenuOption in base.GetTransportersFloatMenuOptions(pods, launchAction))
			{
				yield return transportersFloatMenuOption;
			}
			if (!TransportersArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, this))
			{
				yield break;
			}
			yield return new FloatMenuOption("LandInExistingMap".Translate(Label), delegate
			{
				Map map = Map;
				Current.Game.CurrentMap = map;
				CameraJumper.TryHideWorld();
				Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate(LocalTargetInfo x)
				{
					launchAction(base.Tile, new TransportersArrivalAction_LandInSpecificCell(this, x.Cell, Rot4.North, landInShuttle: false));
				}, null, null, CompLaunchable.TargeterMouseAttachment);
			});
		}

		public override IEnumerable<FloatMenuOption> GetShuttleFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			ThingWithComps shuttle = ((pods.FirstOrDefault() is CompTransporter compTransporter) ? compTransporter.parent : null);
			if (!TransportersArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, this))
			{
				yield break;
			}
			yield return new FloatMenuOption("LandInExistingMap".Translate(Label), delegate
			{
				Map map = Map;
				Current.Game.CurrentMap = map;
				CameraJumper.TryHideWorld();
				ThingDef shuttleDef = shuttle?.def ?? ThingDefOf.Shuttle;
				shuttleRotation = shuttleDef.defaultPlacingRot;
				Find.Targeter.BeginTargeting(TargetingParameters.ForCell(), delegate(LocalTargetInfo x)
				{
					launchAction(base.Tile, new TransportersArrivalAction_LandInSpecificCell(this, x.Cell, shuttleRotation, landInShuttle: true));
				}, delegate(LocalTargetInfo x)
				{
					RoyalTitlePermitWorker_CallShuttle.DrawShuttleGhost(x, Map, shuttleDef, shuttleRotation);
				}, delegate(LocalTargetInfo x)
				{
					AcceptanceReport acceptanceReport = RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(x, Map, shuttleDef, shuttleRotation);
					if (!acceptanceReport.Accepted)
					{
						Messages.Message(acceptanceReport.Reason, new LookTargets(this), MessageTypeDefOf.RejectInput, historical: false);
					}
					return acceptanceReport.Accepted;
				}, null, null, CompLaunchable.TargeterMouseAttachment, playSoundOnAction: true, delegate
				{
					if (shuttleDef.rotatable)
					{
						if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
						{
							shuttleRotation = shuttleRotation.Rotated(RotationDirection.Clockwise);
						}
						if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
						{
							shuttleRotation = shuttleRotation.Rotated(RotationDirection.Counterclockwise);
						}
					}
				});
			});
		}

		public void CheckRemoveMapNow()
		{
			if (HasMap && ShouldRemoveMapNow(out var alsoRemoveWorldObject))
			{
				Map map = Map;
				Current.Game.DeinitAndRemoveMap(map, notifyPlayer: true);
				if (!base.Destroyed && (alsoRemoveWorldObject || forceRemoveWorldObjectWhenMapRemoved))
				{
					Destroy();
				}
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (HasMap && GravshipUtility.TryGetNameOfGravshipOnMap(Map, out var name))
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "GravshipOnTileInspectString".Translate().CapitalizeFirst() + ": " + name;
			}
			if (this.EnterCooldownBlocksEntering())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "EnterCooldown".Translate(this.EnterCooldownTicksLeft().ToStringTicksToPeriod());
			}
			if (!HandlesConditionCausers && HasMap)
			{
				List<Thing> list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ConditionCauser);
				for (int i = 0; i < list.Count; i++)
				{
					text += "\n" + list[i].LabelShortCap + " (" + "ConditionCauserRadius".Translate(list[i].TryGetComp<CompCauseGameCondition>().Props.worldRange) + ")";
				}
			}
			return text;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (!HandlesConditionCausers && HasMap)
			{
				int num = 0;
				List<Thing> list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ConditionCauser);
				for (int i = 0; i < list.Count; i++)
				{
					num = Mathf.Max(num, list[i].TryGetComp<CompCauseGameCondition>().Props.worldRange);
				}
				if (num > 0)
				{
					GenDraw.DrawWorldRadiusRing(base.Tile, num);
				}
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public virtual void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (HasMap)
			{
				outChildren.Add(Map);
			}
		}

		private void RecalculateHibernatableIncidentTargets()
		{
			hibernatableIncidentTargets = null;
			foreach (ThingWithComps item in Map.listerThings.ThingsOfDef(ThingDefOf.Ship_Reactor).OfType<ThingWithComps>())
			{
				CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Starting && compHibernatable.Props.incidentTargetWhileStarting != null)
				{
					if (hibernatableIncidentTargets == null)
					{
						hibernatableIncidentTargets = new HashSet<IncidentTargetTagDef>();
					}
					hibernatableIncidentTargets.Add(compHibernatable.Props.incidentTargetWhileStarting);
				}
			}
		}
	}
}
