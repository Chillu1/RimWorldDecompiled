using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_ExitMapAndEscortCarriers : LordToil
{
	public override bool AllowSatisfyLongNeeds => false;

	public override bool AllowSelfTend => false;

	public override void UpdateAllDuties()
	{
		UpdateTraderDuty(out var trader);
		UpdateCarriersDuties(trader);
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			TraderCaravanRole traderCaravanRole = pawn.GetTraderCaravanRole();
			if (traderCaravanRole != TraderCaravanRole.Carrier && traderCaravanRole != TraderCaravanRole.Trader && pawn.Spawned)
			{
				UpdateDutyForChattelOrGuard(pawn, trader);
			}
		}
	}

	private void UpdateTraderDuty(out Pawn trader)
	{
		trader = TraderCaravanUtility.FindTrader(lord);
		if (trader != null)
		{
			trader.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBestAndDefendSelf);
			trader.mindState.duty.radius = 18f;
			trader.mindState.duty.locomotion = LocomotionUrgency.Jog;
		}
	}

	private void UpdateCarriersDuties(Pawn trader)
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn.GetTraderCaravanRole() == TraderCaravanRole.Carrier)
			{
				if (trader != null)
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.Follow, trader, 5f);
					continue;
				}
				pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
				pawn.mindState.duty.locomotion = LocomotionUrgency.Jog;
			}
		}
	}

	private void UpdateDutyForChattelOrGuard(Pawn p, Pawn trader)
	{
		if (p.GetTraderCaravanRole() == TraderCaravanRole.Chattel)
		{
			if (trader != null)
			{
				p.mindState.duty = new PawnDuty(DutyDefOf.Escort, trader, 14f);
			}
			else if (!TryToDefendClosestCarrier(p, 14f))
			{
				p.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBestAndDefendSelf);
				p.mindState.duty.radius = 10f;
				p.mindState.duty.locomotion = LocomotionUrgency.Jog;
			}
		}
		else if (!TryToDefendClosestCarrier(p, 26f))
		{
			if (trader != null)
			{
				p.mindState.duty = new PawnDuty(DutyDefOf.Escort, trader, 26f);
				return;
			}
			p.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBestAndDefendSelf);
			p.mindState.duty.radius = 18f;
			p.mindState.duty.locomotion = LocomotionUrgency.Jog;
		}
	}

	private bool TryToDefendClosestCarrier(Pawn p, float escortRadius)
	{
		if (!p.Spawned)
		{
			return false;
		}
		Pawn closestCarrier = GetClosestCarrier(p);
		Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.Corpse), PathEndMode.ClosestTouch, TraverseParms.For(p), 20f, delegate(Thing x)
		{
			Pawn innerPawn = ((Corpse)x).InnerPawn;
			return innerPawn.Faction == p.Faction && innerPawn.RaceProps.packAnimal;
		}, null, 0, 15);
		Thing thing2 = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(p), 20f, delegate(Thing x)
		{
			Pawn pawn = (Pawn)x;
			return pawn.Downed && pawn.Faction == p.Faction && pawn.GetTraderCaravanRole() == TraderCaravanRole.Carrier;
		}, null, 0, 15);
		Thing thing3 = null;
		if (closestCarrier != null)
		{
			thing3 = closestCarrier;
		}
		if (thing != null && (thing3 == null || thing.Position.DistanceToSquared(p.Position) < thing3.Position.DistanceToSquared(p.Position)))
		{
			thing3 = thing;
		}
		if (thing2 != null && (thing3 == null || thing2.Position.DistanceToSquared(p.Position) < thing3.Position.DistanceToSquared(p.Position)))
		{
			thing3 = thing2;
		}
		if (thing3 == null)
		{
			return false;
		}
		if (thing3 is Pawn && !((Pawn)thing3).Downed)
		{
			p.mindState.duty = new PawnDuty(DutyDefOf.Escort, thing3, escortRadius);
			return true;
		}
		if (!GenHostility.AnyHostileActiveThreatTo(base.Map, lord.faction, countDormantPawnsAsHostile: true))
		{
			return false;
		}
		p.mindState.duty = new PawnDuty(DutyDefOf.Defend, thing3.Position, 16f);
		return true;
	}

	public static bool IsDefendingPosition(Pawn pawn)
	{
		if (pawn.mindState.duty != null)
		{
			return pawn.mindState.duty.def == DutyDefOf.Defend;
		}
		return false;
	}

	public static bool IsAnyDefendingPosition(List<Pawn> pawns)
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			if (IsDefendingPosition(pawns[i]))
			{
				return true;
			}
		}
		return false;
	}

	private Pawn GetClosestCarrier(Pawn closestTo)
	{
		Pawn pawn = null;
		float num = 0f;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn2 = lord.ownedPawns[i];
			if (pawn2.GetTraderCaravanRole() == TraderCaravanRole.Carrier)
			{
				float num2 = pawn2.Position.DistanceToSquared(closestTo.Position);
				if (pawn == null || num2 < num)
				{
					pawn = pawn2;
					num = num2;
				}
			}
		}
		return pawn;
	}
}
