using System;
using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class RoyalTitlePermitWorker_CallShuttle : RoyalTitlePermitWorker_Targeted
	{
		private Faction calledFaction;

		private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallShuttle");

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			if (!CanHitTarget(target))
			{
				if (target.IsValid)
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
			GenDraw.DrawRadiusRing(caller.Position, def.royalAid.targetingRange, Color.white);
			DrawShuttleGhost(target, map);
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
			targetingParameters.validator = (TargetInfo target) => (!(def.royalAid.targetingRange > 0f) || !(target.Cell.DistanceTo(caller.Position) > def.royalAid.targetingRange)) ? true : false;
			base.caller = caller;
			base.map = map;
			calledFaction = faction;
			base.free = free;
			Find.Targeter.BeginTargeting(this);
		}

		private void CallShuttle(IntVec3 landingCell)
		{
			if (caller.Spawned)
			{
				QuestScriptDef permit_CallShuttle = QuestScriptDefOf.Permit_CallShuttle;
				Slate slate = new Slate();
				slate.Set("asker", caller);
				slate.Set("map", caller.Map);
				slate.Set("landingCell", landingCell);
				slate.Set("permitFaction", calledFaction);
				QuestUtility.GenerateQuestAndMakeAvailable(permit_CallShuttle, slate);
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
			int maxLaunchDistance = ThingDefOf.Shuttle.GetCompProperties<CompProperties_Launchable>().fixedLaunchDistanceMax;
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(caravan));
			Find.WorldSelector.ClearSelection();
			int caravanTile = caravan.Tile;
			Find.WorldTargeter.BeginTargeting_NewTemp(ChoseWorldTarget, canTargetTiles: true, CompLaunchable.TargeterMouseAttachment, closeWorldTabWhenFinished: false, delegate
			{
				GenDraw.DrawWorldRadiusRing(caravanTile, maxLaunchDistance);
			}, (GlobalTargetInfo target) => CompLaunchable.TargetingLabelGetter(target, caravanTile, maxLaunchDistance, Gen.YieldSingle(caravan), Launch, null));
			bool ChoseWorldTarget(GlobalTargetInfo target)
			{
				return CompLaunchable.ChoseWorldTarget(target, caravan.Tile, Gen.YieldSingle(caravan), maxLaunchDistance, Launch, null);
			}
			void Launch(int tile, TransportPodsArrivalAction arrivalAction)
			{
				ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
				activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(CaravanInventoryUtility.AllInventoryItems(caravan));
				activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(caravan.GetDirectlyHeldThings());
				caravan.Destroy();
				TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingShuttle);
				travelingTransportPods.Tile = caravan.Tile;
				travelingTransportPods.SetFaction(Faction.OfPlayer);
				travelingTransportPods.destinationTile = tile;
				travelingTransportPods.arrivalAction = arrivalAction;
				travelingTransportPods.AddPod(activeDropPodInfo, justLeftTheMap: true);
				Find.WorldObjects.Add(travelingTransportPods);
				Find.WorldTargeter.StopTargeting();
				caller.royalty.GetPermit(def, faction).Notify_Used();
				if (!free)
				{
					caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
				}
			}
		}

		public static void DrawShuttleGhost(LocalTargetInfo target, Map map)
		{
			Color ghostCol = (ShuttleCanLandHere(target, map).Accepted ? Designator_Place.CanPlaceColor : Designator_Place.CannotPlaceColor);
			GhostDrawer.DrawGhostThing_NewTmp(target.Cell, Rot4.North, ThingDefOf.Shuttle, ThingDefOf.Shuttle.graphic, ghostCol, AltitudeLayer.Blueprint);
			Vector3 position = (target.Cell + IntVec3.South * 2).ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, GenDraw.InteractionCellMaterial, 0);
		}

		public static AcceptanceReport ShuttleCanLandHere(LocalTargetInfo target, Map map)
		{
			TaggedString t = "CannotCallShuttle".Translate() + ": ";
			if (!target.IsValid)
			{
				return new AcceptanceReport(t + "MessageTransportPodsDestinationIsInvalid".Translate().CapitalizeFirst());
			}
			foreach (IntVec3 item in GenAdj.OccupiedRect(target.Cell, Rot4.North, ThingDefOf.Shuttle.size))
			{
				string reportFromCell = GetReportFromCell(item, map);
				if (reportFromCell != null)
				{
					return new AcceptanceReport(t + reportFromCell);
				}
			}
			string reportFromCell2 = GetReportFromCell(target.Cell + CompShuttle.DropoffSpotOffset, map);
			if (reportFromCell2 != null)
			{
				return new AcceptanceReport(t + reportFromCell2);
			}
			return AcceptanceReport.WasAccepted;
		}

		private static string GetReportFromCell(IntVec3 cell, Map map)
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
			RoofDef roof = cell.GetRoof(map);
			if (roof != null && (roof.isNatural || roof.isThickRoof))
			{
				return "MessageTransportPodsDestinationIsInvalid".Translate().CapitalizeFirst();
			}
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is IActiveDropPod || thing is Skyfaller || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building)
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
