using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class TravellingTransporters : WorldObject, IThingHolder
{
	public PlanetTile destinationTile = PlanetTile.Invalid;

	public TransportersArrivalAction arrivalAction;

	private List<ActiveTransporterInfo> transporters = new List<ActiveTransporterInfo>();

	private bool arrived;

	private PlanetTile initialTile = PlanetTile.Invalid;

	private float traveledPct;

	private const float TravelSpeed = 0.00025f;

	public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

	private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

	private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

	public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

	public override bool ExpandingIconFlipHorizontal => GenWorldUI.WorldToUIPosition(Start).x > GenWorldUI.WorldToUIPosition(End).x;

	private Thing Shuttle => transporters.FirstOrDefault()?.GetShuttle();

	public override Color ExpandingIconColor => base.Faction.Color;

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
			for (int i = 0; i < transporters.Count; i++)
			{
				ThingOwner innerContainer = transporters[i].innerContainer;
				for (int j = 0; j < innerContainer.Count; j++)
				{
					if (innerContainer[j] is Pawn pawn && CaravanUtility.IsOwner(pawn, base.Faction))
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
			for (int i = 0; i < transporters.Count; i++)
			{
				ThingOwner innerContainer = transporters[i].innerContainer;
				for (int j = 0; j < innerContainer.Count; j++)
				{
					if (innerContainer[j] is Pawn { IsColonist: not false, HostFaction: null })
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
			for (int i = 0; i < transporters.Count; i++)
			{
				ThingOwner things = transporters[i].innerContainer;
				for (int j = 0; j < things.Count; j++)
				{
					if (things[j] is Pawn pawn)
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
		Scribe_Collections.Look(ref transporters, "pods", LookMode.Deep);
		Scribe_Values.Look(ref destinationTile, "destinationTile");
		Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
		Scribe_Values.Look(ref initialTile, "initialTile");
		Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
		Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			for (int i = 0; i < transporters.Count; i++)
			{
				transporters[i].parent = this;
			}
		}
	}

	public override void PostAdd()
	{
		base.PostAdd();
		initialTile = base.Tile;
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		traveledPct += TraveledPctStepPerTick * (float)delta;
		if (traveledPct >= 1f)
		{
			traveledPct = 1f;
			Arrived();
		}
	}

	public void AddTransporter(ActiveTransporterInfo contents, bool justLeftTheMap)
	{
		contents.parent = this;
		transporters.Add(contents);
		ThingOwner innerContainer = contents.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (innerContainer[i] is Pawn pawn && !pawn.IsWorldPawn())
			{
				if (!base.Spawned)
				{
					Log.Warning($"Passing pawn {pawn} to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
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
		for (int i = 0; i < transporters.Count; i++)
		{
			if (transporters[i].innerContainer.Contains(p))
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
		if (arrivalAction == null || !arrivalAction.StillValid(transporters, destinationTile))
		{
			arrivalAction = null;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].Tile == destinationTile)
				{
					arrivalAction = new TransportersArrivalAction_LandInSpecificCell(maps[i].Parent, DropCellFinder.RandomDropSpot(maps[i]));
					break;
				}
			}
			if (arrivalAction == null)
			{
				if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(transporters, destinationTile))
				{
					arrivalAction = new TransportersArrivalAction_FormCaravan();
				}
				else
				{
					List<Caravan> caravans = Find.WorldObjects.Caravans;
					for (int j = 0; j < caravans.Count; j++)
					{
						if (caravans[j].Tile == destinationTile && (bool)TransportersArrivalAction_GiveToCaravan.CanGiveTo(transporters, caravans[j]))
						{
							arrivalAction = new TransportersArrivalAction_GiveToCaravan(caravans[j]);
							break;
						}
					}
				}
			}
		}
		if (arrivalAction != null && arrivalAction.ShouldUseLongEvent(transporters, destinationTile))
		{
			LongEventHandler.QueueLongEvent(DoArrivalAction, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		}
		else
		{
			DoArrivalAction();
		}
	}

	private void DoArrivalAction()
	{
		for (int i = 0; i < transporters.Count; i++)
		{
			transporters[i].savePawnsWithReferenceMode = false;
			transporters[i].parent = null;
		}
		if (arrivalAction != null)
		{
			try
			{
				arrivalAction.Arrived(transporters, destinationTile);
			}
			catch (Exception ex)
			{
				Log.Error("Exception in transport pods arrival action: " + ex);
			}
			arrivalAction = null;
		}
		else
		{
			for (int j = 0; j < transporters.Count; j++)
			{
				for (int k = 0; k < transporters[j].innerContainer.Count; k++)
				{
					if (transporters[j].innerContainer[k] is Pawn pawn && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
					{
						PawnBanishUtility.Banish(pawn, destinationTile);
					}
				}
			}
			bool flag = true;
			if (ModsConfig.BiotechActive)
			{
				flag = false;
				for (int l = 0; l < transporters.Count; l++)
				{
					if (flag)
					{
						break;
					}
					for (int m = 0; m < transporters[l].innerContainer.Count; m++)
					{
						if (transporters[l].innerContainer[m].def != ThingDefOf.Wastepack)
						{
							flag = true;
							break;
						}
					}
				}
			}
			for (int n = 0; n < transporters.Count; n++)
			{
				for (int num = 0; num < transporters[n].innerContainer.Count; num++)
				{
					transporters[n].innerContainer[num].Notify_AbandonedAtTile(destinationTile);
				}
			}
			for (int num2 = 0; num2 < transporters.Count; num2++)
			{
				transporters[num2].innerContainer.ClearAndDestroyContentsOrPassToWorld();
			}
			if (flag)
			{
				string key = "MessageTransportPodsArrivedAndLost";
				if (def == WorldObjectDefOf.TravelingShuttle)
				{
					key = "MessageShuttleArrivedContentsLost";
				}
				Messages.Message(key.Translate(), new GlobalTargetInfo(destinationTile), MessageTypeDefOf.NegativeEvent);
			}
		}
		transporters.Clear();
		Destroy();
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return null;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		for (int i = 0; i < transporters.Count; i++)
		{
			outChildren.Add(transporters[i]);
		}
	}
}
