using System;
using System.Linq;
using Verse;

namespace RimWorld;

public class PassingShip : IExposable, ICommunicable, ILoadReferenceable
{
	public PassingShipManager passingShipManager;

	private Faction faction;

	public string name = "Nameless";

	protected int loadID = -1;

	public int ticksUntilDeparture = 40000;

	public virtual string FullTitle => "ErrorFullTitle";

	public bool Departed => ticksUntilDeparture <= 0;

	public Map Map => passingShipManager?.map;

	public Faction Faction => faction;

	public PassingShip()
	{
	}

	public PassingShip(Faction faction)
	{
		this.faction = faction;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref name, "name");
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref ticksUntilDeparture, "ticksUntilDeparture", 0);
		Scribe_References.Look(ref faction, "faction");
	}

	public virtual void PassingShipTick()
	{
		ticksUntilDeparture--;
		if (Departed)
		{
			Depart();
		}
	}

	public virtual void Depart()
	{
		if (Map.listerBuildings.ColonistsHaveBuilding((Thing b) => b.def.IsCommsConsole))
		{
			Messages.Message("MessageShipHasLeftCommsRange".Translate(FullTitle), MessageTypeDefOf.SituationResolved);
		}
		passingShipManager.RemoveShip(this);
	}

	public virtual void TryOpenComms(Pawn negotiator)
	{
		throw new NotImplementedException();
	}

	public virtual string GetCallLabel()
	{
		return name;
	}

	public string GetInfoText()
	{
		return FullTitle;
	}

	Faction ICommunicable.GetFaction()
	{
		return null;
	}

	protected virtual AcceptanceReport CanCommunicateWith(Pawn negotiator)
	{
		return AcceptanceReport.WasAccepted;
	}

	public FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator)
	{
		string label = "CallOnRadio".Translate(GetCallLabel());
		Action action = null;
		AcceptanceReport canCommunicate = CanCommunicateWith(negotiator);
		if (!canCommunicate.Accepted)
		{
			if (!canCommunicate.Reason.NullOrEmpty())
			{
				action = delegate
				{
					Messages.Message(canCommunicate.Reason, console, MessageTypeDefOf.RejectInput, historical: false);
				};
			}
		}
		else
		{
			action = delegate
			{
				if (!Building_OrbitalTradeBeacon.AllPowered(Map).Any())
				{
					Messages.Message("MessageNeedBeaconToTradeWithShip".Translate(), console, MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					console.GiveUseCommsJob(negotiator, this);
				}
			};
		}
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.InitiateSocial), negotiator, console);
	}

	public string GetUniqueLoadID()
	{
		return "PassingShip_" + loadID;
	}
}
