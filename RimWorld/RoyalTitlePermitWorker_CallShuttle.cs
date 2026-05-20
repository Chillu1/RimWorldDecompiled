using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class RoyalTitlePermitWorker_CallShuttle : RoyalTitlePermitWorker_Targeted
	{
		private Faction calledFaction;

		private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallShuttle");

		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!CanHitTarget(target))
			{
				if (target.IsValid && showMessages)
				{
					Messages.Message(def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), MessageTypeDefOf.RejectInput);
				}
				return false;
			}
			AcceptanceReport acceptanceReport = ShuttleCanLandHere(target, map);
			if (!acceptanceReport.Accepted)
			{
				Messages.Message(acceptanceReport.Reason, new LookTargets(target.Cell, map), MessageTypeDefOf.RejectInput, historical: false);
			}
			return acceptanceReport.Accepted;
		}

		public override void DrawHighlight(LocalTargetInfo target)
		{
			GenDraw.DrawRadiusRing(caller.Position, base.RangeClamped, Color.white);
			DrawShuttleGhost(target, map, ThingDefOf.Shuttle, ThingDefOf.Shuttle.defaultPlacingRot);
		}

		public override void OrderForceTarget(LocalTargetInfo target)
		{
			CallShuttle(target.Cell);
		}

		public override void OnGUI(LocalTargetInfo target)
		{
			if (!target.IsValid || !ShuttleCanLandHere(target, map).Accepted)
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}

		public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
		{
			if (map.generatorDef?.isUnderground ?? false)
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION")), null);
				yield break;
			}
			if (faction.HostileTo(Faction.OfPlayer))
			{
				yield return new FloatMenuOption(def.LabelCap + ": " + "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
				yield break;
			}
			string description = def.LabelCap + ": ";
			Action action = null;
			if (FillAidOption(pawn, faction, ref description, out var free))
			{
				action = delegate
				{
					BeginCallShuttle(pawn, pawn.MapHeld, faction, free);
				};
			}
			yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
		}

		public override IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction)
		{
			if (FillCaravanAidOption(pawn, faction, out var description, out free, out var disableNotEnoughFavor))
			{
				Command_Action command_Action = new Command_Action
				{
					defaultLabel = def.LabelCap + " (" + pawn.LabelShort + ")",
					defaultDesc = description,
					icon = CommandTex,
					action = delegate
					{
						CallShuttleToCaravan(pawn, faction, free);
					}
				};
				if (pawn.MapHeld != null && pawn.MapHeld.generatorDef.isUnderground)
				{
					command_Action.Disable("CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION")));
				}
				if (faction.HostileTo(Faction.OfPlayer))
				{
					command_Action.Disable("CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
				}
				if (disableNotEnoughFavor)
				{
					command_Action.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
				}
				yield return command_Action;
			}
		}

		private void BeginCallShuttle(Pawn caller, Map map, Faction faction, bool free)
		{
			targetingParameters = new TargetingParameters();
			targetingParameters.canTargetLocations = true;
			targetingParameters.canTargetSelf = false;
			targetingParameters.canTargetPawns = false;
			targetingParameters.canTargetFires = false;
			targetingParameters.canTargetBuildings = true;
			targetingParameters.canTargetItems = true;
			base.caller = caller;
			base.map = map;
			calledFaction = faction;
			base.free = free;
			float rangeActual = base.RangeClamped;
			targetingParameters.validator = (TargetInfo target) => (!(rangeActual > 0f) || !(target.Cell.DistanceTo(caller.Position) > rangeActual)) ? true : false;
			Find.Targeter.BeginTargeting(this);
		}

		private void CallShuttle(IntVec3 landingCell)
		{
			if (caller.Spawned)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Shuttle);
				CompShuttle compShuttle = thing.TryGetComp<CompShuttle>();
				compShuttle.permitShuttle = true;
				compShuttle.acceptChildren = true;
				TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, thing);
				transportShip.ArriveAt(landingCell, map.Parent);
				transportShip.AddJobs(ShipJobDefOf.WaitForever, ShipJobDefOf.Unload_Destination, ShipJobDefOf.FlyAway);
				caller.royalty.GetPermit(def, calledFaction).Notify_Used();
				if (!free)
				{
					caller.royalty.TryRemoveFavor(calledFaction, def.royalAid.favorCost);
				}
			}
		}

		private void CallShuttleToCaravan(Pawn caller, Faction faction, bool free)
		{
			Caravan caravan = caller.GetCaravan();
			int maxLaunchDistance = TransportShipDefOf.Ship_Shuttle.maxLaunchDistance;
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(caravan));
			Find.WorldSelector.ClearSelection();
			PlanetTile caravanTile = caravan.Tile;
			Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, CompLaunchable.TargeterMouseAttachment, closeWorldTabWhenFinished: false, delegate
			{
				if (maxLaunchDistance > 0)
				{
					PlanetTile center = caravanTile;
					if (caravanTile.Layer != Find.WorldSelector.SelectedLayer)
					{
						center = Find.WorldSelector.SelectedLayer.GetClosestTile_NewTemp(caravanTile);
					}
					GenDraw.DrawWorldRadiusRing(center, maxLaunchDistance);
				}
			}, (GlobalTargetInfo target) => CompLaunchable.TargetingLabelGetter(target, caravanTile, maxLaunchDistance, Gen.YieldSingle(caravan), Launch, null));
			bool ChoseWorldTarget(GlobalTargetInfo target)
			{
				return CompLaunchable.ChoseWorldTarget(target, caravan.Tile, Gen.YieldSingle(caravan), maxLaunchDistance, Launch, null);
			}
			void Launch(PlanetTile tile, TransportersArrivalAction arrivalAction)
			{
				ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
				activeTransporterInfo.innerContainer.TryAddRangeOrTransfer(CaravanInventoryUtility.AllInventoryItems(caravan));
				activeTransporterInfo.innerContainer.TryAddRangeOrTransfer(caravan.GetDirectlyHeldThings());
				caravan.Destroy();
				TravellingTransporters travellingTransporters = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingShuttle);
				travellingTransporters.Tile = caravan.Tile;
				travellingTransporters.SetFaction(Faction.OfPlayer);
				travellingTransporters.destinationTile = tile;
				travellingTransporters.arrivalAction = arrivalAction;
				travellingTransporters.AddTransporter(activeTransporterInfo, justLeftTheMap: true);
				Find.WorldObjects.Add(travellingTransporters);
				Find.WorldTargeter.StopTargeting();
				caller.royalty.GetPermit(def, faction).Notify_Used();
				if (!free)
				{
					caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
				}
			}
		}

		public static void DrawShuttleGhost(LocalTargetInfo target, Map map, ThingDef shuttleDef, Rot4 rot)
		{
			Color ghostCol = (ShuttleCanLandHere(target, map, shuttleDef, rot).Accepted ? Designator_Place.CanPlaceColor : Designator_Place.CannotPlaceColor);
			GhostDrawer.DrawGhostThing(target.Cell, rot, shuttleDef, shuttleDef.graphic, ghostCol, AltitudeLayer.Blueprint);
			Vector3 position = ThingUtility.InteractionCellWhenAt(shuttleDef, target.Cell, rot, map).ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
		}

		public static AcceptanceReport ShuttleCanLandHere(LocalTargetInfo target, Map map, ThingDef shuttleDef = null, Rot4? rot = null)
		{
			TaggedString taggedString = "ShuttleCannotLand".Translate() + ": ";
			if (!target.IsValid)
			{
				return new AcceptanceReport(taggedString + "MessageTransportPodsDestinationIsInvalid".Translate().CapitalizeFirst());
			}
			if (shuttleDef == null)
			{
				shuttleDef = ThingDefOf.Shuttle;
			}
			Rot4 valueOrDefault = rot.GetValueOrDefault();
			if (!rot.HasValue)
			{
				valueOrDefault = shuttleDef.defaultPlacingRot;
				rot = valueOrDefault;
			}
			foreach (IntVec3 item in GenAdj.OccupiedRect(target.Cell, rot.Value, shuttleDef.size))
			{
				string reportFromCell = GetReportFromCell(item, map, interactionSpot: false, shuttleDef);
				if (reportFromCell != null)
				{
					return new AcceptanceReport(taggedString + reportFromCell);
				}
			}
			string reportFromCell2 = GetReportFromCell(ThingUtility.InteractionCellWhenAt(shuttleDef, target.Cell, rot.Value, map), map, interactionSpot: true, shuttleDef);
			if (reportFromCell2 != null)
			{
				return new AcceptanceReport(taggedString + reportFromCell2);
			}
			return AcceptanceReport.WasAccepted;
		}

		private static string GetReportFromCell(IntVec3 cell, Map map, bool interactionSpot, ThingDef shuttleDef)
		{
			if (!cell.InBounds(map))
			{
				return "OutOfBounds".Translate().CapitalizeFirst();
			}
			if (cell.Fogged(map))
			{
				return "ShuttleCannotLand_Fogged".Translate().CapitalizeFirst();
			}
			if (!cell.Walkable(map))
			{
				return "ShuttleCannotLand_Unwalkable".Translate().CapitalizeFirst();
			}
			if (!cell.GetAffordances(map).Contains(ThingDefOf.Shuttle.terrainAffordanceNeeded))
			{
				return "ShuttleCannotLand_Unaffordable".Translate(shuttleDef.label).CapitalizeFirst();
			}
			RoofDef roof = cell.GetRoof(map);
			if (roof != null && (roof.isNatural || roof.isThickRoof))
			{
				return "MessageTransportPodsDestinationIsInvalid".Translate().CapitalizeFirst();
			}
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is IActiveTransporter || thing is Skyfaller || (thing.def.category == ThingCategory.Building && !thing.def.building.isPowerConduit))
				{
					return "BlockedBy".Translate(thing).CapitalizeFirst();
				}
				PlantProperties plant = thing.def.plant;
				if (plant != null && plant.IsTree)
				{
					return "BlockedBy".Translate(thing).CapitalizeFirst();
				}
			}
			return null;
		}
	}
}
