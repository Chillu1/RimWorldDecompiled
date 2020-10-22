using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class TravelingTransportPods : WorldObject, IThingHolder
	{
		public int destinationTile = -1;

		public TransportPodsArrivalAction arrivalAction;

		private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();

		private bool arrived;

		private int initialTile = -1;

		private float traveledPct;

		private const float TravelSpeed = 0.00025f;

		private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

		private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

		public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

		public override bool ExpandingIconFlipHorizontal => GenWorldUI.WorldToUIPosition(Start).x > GenWorldUI.WorldToUIPosition(End).x;

		public override float ExpandingIconRotation
		{
			get
			{
				if (!def.rotateGraphicWhenTraveling)
				{
					return base.ExpandingIconRotation;
				}
				Vector2 vector = GenWorldUI.WorldToUIPosition(Start);
				Vector2 vector2 = GenWorldUI.WorldToUIPosition(End);
				float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
				if (num > 180f)
				{
					num -= 180f;
				}
				return num + 90f;
			}
		}

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = Start;
				Vector3 end = End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return 0.00025f / num;
			}
		}

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				for (int i = 0; i < pods.Count; i++)
				{
					ThingOwner innerContainer = pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, base.Faction))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool PodsHaveAnyFreeColonist
		{
			get
			{
				for (int i = 0; i < pods.Count; i++)
				{
					ThingOwner innerContainer = pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public IEnumerable<Pawn> Pawns
		{
			get
			{
				for (int i = 0; i < pods.Count; i++)
				{
					ThingOwner things = pods[i].innerContainer;
					for (int j = 0; j < things.Count; j++)
					{
						Pawn pawn = things[j] as Pawn;
						if (pawn != null)
						{
							yield return pawn;
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pods, "pods", LookMode.Deep);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
			Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
			Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
			Scribe_Values.Look(ref initialTile, "initialTile", 0);
			Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < pods.Count; i++)
				{
					pods[i].parent = this;
				}
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			initialTile = base.Tile;
		}

		public override void Tick()
		{
			base.Tick();
			traveledPct += TraveledPctStepPerTick;
			if (traveledPct >= 1f)
			{
				traveledPct = 1f;
				Arrived();
			}
		}

		public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
		{
			contents.parent = this;
			pods.Add(contents);
			ThingOwner innerContainer = contents.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null && !pawn.IsWorldPawn())
				{
					if (!base.Spawned)
					{
						Log.Warning(string.Concat("Passing pawn ", pawn, " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs."));
					}
					if (justLeftTheMap)
					{
						pawn.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn);
					}
				}
			}
			contents.savePawnsWithReferenceMode = true;
		}

		public bool ContainsPawn(Pawn p)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				if (pods[i].innerContainer.Contains(p))
				{
					return true;
				}
			}
			return false;
		}

		private void Arrived()
		{
			if (arrived)
			{
				return;
			}
			arrived = true;
			if (arrivalAction == null || !arrivalAction.StillValid(pods.Cast<IThingHolder>(), destinationTile))
			{
				arrivalAction = null;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (maps[i].Tile == destinationTile)
					{
						arrivalAction = new TransportPodsArrivalAction_LandInSpecificCell(maps[i].Parent, DropCellFinder.RandomDropSpot(maps[i]));
						break;
					}
				}
				if (arrivalAction == null)
				{
					if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods.Cast<IThingHolder>(), destinationTile))
					{
						arrivalAction = new TransportPodsArrivalAction_FormCaravan();
					}
					else
					{
						List<Caravan> caravans = Find.WorldObjects.Caravans;
						for (int j = 0; j < caravans.Count; j++)
						{
							if (caravans[j].Tile == destinationTile && (bool)TransportPodsArrivalAction_GiveToCaravan.CanGiveTo(pods.Cast<IThingHolder>(), caravans[j]))
							{
								arrivalAction = new TransportPodsArrivalAction_GiveToCaravan(caravans[j]);
								break;
							}
						}
					}
				}
			}
			if (arrivalAction != null && arrivalAction.ShouldUseLongEvent(pods, destinationTile))
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					DoArrivalAction();
				}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
			}
			else
			{
				DoArrivalAction();
			}
		}

		private void DoArrivalAction()
		{
			for (int i = 0; i < pods.Count; i++)
			{
				pods[i].savePawnsWithReferenceMode = false;
				pods[i].parent = null;
			}
			if (arrivalAction != null)
			{
				try
				{
					arrivalAction.Arrived(pods, destinationTile);
				}
				catch (Exception arg)
				{
					Log.Error("Exception in transport pods arrival action: " + arg);
				}
				arrivalAction = null;
			}
			else
			{
				for (int j = 0; j < pods.Count; j++)
				{
					for (int k = 0; k < pods[j].innerContainer.Count; k++)
					{
						Pawn pawn = pods[j].innerContainer[k] as Pawn;
						if (pawn != null && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
						{
							PawnBanishUtility.Banish(pawn, destinationTile);
						}
					}
				}
				for (int l = 0; l < pods.Count; l++)
				{
					pods[l].innerContainer.ClearAndDestroyContentsOrPassToWorld();
				}
				string key = "MessageTransportPodsArrivedAndLost";
				if (def == WorldObjectDefOf.TravelingShuttle)
				{
					key = "MessageShuttleArrivedContentsLost";
				}
				Messages.Message(key.Translate(), new GlobalTargetInfo(destinationTile), MessageTypeDefOf.NegativeEvent);
			}
			pods.Clear();
			Destroy();
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			for (int i = 0; i < pods.Count; i++)
			{
				outChildren.Add(pods[i]);
			}
		}
	}
}
