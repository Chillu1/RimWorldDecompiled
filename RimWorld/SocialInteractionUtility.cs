using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class SocialInteractionUtility
{
	public const float MaxInteractRange = 6f;

	public static bool CanInitiateInteraction(Pawn pawn, InteractionDef interactionDef = null)
	{
		if (pawn.interactions == null)
		{
			return false;
		}
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
		{
			return false;
		}
		if (!pawn.Awake())
		{
			return false;
		}
		if (pawn.IsBurning())
		{
			return false;
		}
		if (pawn.IsMutant && pawn.mutant.Def.incapableOfSocialInteractions)
		{
			return false;
		}
		if (pawn.IsInteractionBlocked(interactionDef, isInitiator: true, isRandom: false))
		{
			return false;
		}
		return true;
	}

	public static IntVec3 GetAdjacentInteractionCell(Pawn pawn, Thing entityThing, bool forced)
	{
		TryGetAdjacentInteractionCell(pawn, entityThing, forced, out var cell);
		return cell;
	}

	public static bool TryGetAdjacentInteractionCell(Pawn pawn, Thing entityThing, bool forced, out IntVec3 cell)
	{
		if (entityThing is Pawn)
		{
			if (CellFinder.TryFindRandomCellNear(entityThing.Position, pawn.Map, 2, (IntVec3 x) => CellIsValid(x, checkDoors: true), out var result) || CellFinder.TryFindRandomCellNear(entityThing.Position, pawn.Map, 2, (IntVec3 x) => CellIsValid(x, checkDoors: false), out result))
			{
				cell = result;
				return true;
			}
			cell = pawn.Position;
			return true;
		}
		if ((from x in GenAdj.CellsAdjacentCardinal(entityThing)
			where CellIsValid(x, checkDoors: true)
			select x).TryRandomElement(out var result2) || (from x in GenAdj.CellsAdjacentCardinal(entityThing)
			where CellIsValid(x, checkDoors: false)
			select x).TryRandomElement(out result2))
		{
			cell = result2;
			return true;
		}
		cell = pawn.Position;
		return false;
		bool CellIsValid(IntVec3 c, bool checkDoors)
		{
			if (c.InBounds(pawn.Map) && !c.Fogged(pawn.Map) && (!c.IsForbidden(pawn) || forced) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, forced ? Danger.Deadly : Danger.Some, 1, -1, null, forced) && c.Standable(pawn.Map))
			{
				if (checkDoors)
				{
					return c.GetDoor(pawn.Map) == null;
				}
				return true;
			}
			return false;
		}
	}

	public static bool CanReceiveInteraction(Pawn pawn, InteractionDef interactionDef = null)
	{
		if (!pawn.Awake())
		{
			return false;
		}
		if (pawn.IsBurning())
		{
			return false;
		}
		if (pawn.IsMutant && pawn.mutant.Def.incapableOfSocialInteractions)
		{
			return false;
		}
		if (pawn.IsInteractionBlocked(interactionDef, isInitiator: false, isRandom: false))
		{
			return false;
		}
		return true;
	}

	public static bool CanInitiateRandomInteraction(Pawn p)
	{
		if (!CanInitiateInteraction(p))
		{
			return false;
		}
		if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState || p.IsInteractionBlocked(null, isInitiator: true, isRandom: true))
		{
			return false;
		}
		if (p.Faction == null)
		{
			return false;
		}
		if (!p.ageTracker.CurLifeStage.canInitiateSocialInteraction)
		{
			return false;
		}
		if (p.Inhumanized())
		{
			return false;
		}
		return true;
	}

	public static bool CanReceiveRandomInteraction(Pawn p)
	{
		if (!CanReceiveInteraction(p))
		{
			return false;
		}
		if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState)
		{
			return false;
		}
		return true;
	}

	public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
	{
		return IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
	}

	public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
	{
		if (cell.InHorDistOf(recipientCell, 6f))
		{
			return GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
		}
		return false;
	}

	public static IntVec3 BestInteractableCell(Pawn actor, Pawn targetPawn)
	{
		IntVec3 intVec = IntVec3.Invalid;
		for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
		{
			IntVec3 intVec2 = targetPawn.Position + GenAdj.AdjacentCellsAndInside[i];
			if (intVec2.InBounds(actor.Map) && intVec2.Walkable(actor.Map) && intVec2 != actor.Position && IsGoodPositionForInteraction(intVec2, targetPawn.Position, actor.Map) && actor.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly) && (!intVec.IsValid || actor.Position.DistanceToSquared(intVec2) < actor.Position.DistanceToSquared(intVec)))
			{
				intVec = intVec2;
			}
		}
		return intVec;
	}

	public static bool HasAnyVerbForSocialFight(Pawn p)
	{
		if (p.Dead)
		{
			return false;
		}
		List<Verb> allVerbs = p.verbTracker.AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			if (allVerbs[i].IsMeleeAttack && allVerbs[i].IsStillUsableBy(p))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryGetRandomVerbForSocialFight(Pawn p, out Verb verb)
	{
		if (p.Dead)
		{
			verb = null;
			return false;
		}
		return p.verbTracker.AllVerbs.Where((Verb x) => x.IsMeleeAttack && x.IsStillUsableBy(p)).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p) * (x.tool?.chanceFactor ?? 1f), out verb);
	}

	public static void ImitateSocialInteractionWithManyPawns(Pawn initiator, List<Pawn> targets, InteractionDef intDef)
	{
		List<Pawn> list = targets.Except(initiator).ToList();
		if (targets.NullOrEmpty())
		{
			Log.Error(initiator?.ToString() + " tried to do interaction " + intDef?.ToString() + " with no targets. ");
			return;
		}
		if (intDef.initiatorXpGainSkill != null)
		{
			initiator.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount);
		}
		foreach (Pawn item in list)
		{
			if (initiator != item && initiator.interactions.CanInteractNowWith(item, intDef))
			{
				if (intDef.recipientThought != null && item.needs.mood != null)
				{
					Pawn_InteractionsTracker.AddInteractionThought(item, initiator, intDef.recipientThought);
				}
				if (intDef.recipientXpGainSkill != null && item.RaceProps.Humanlike)
				{
					item.skills.Learn(intDef.recipientXpGainSkill, intDef.recipientXpGainAmount);
				}
			}
		}
		LogEntry entry;
		if (list.Count > 0)
		{
			entry = new PlayLogEntry_InteractionWithMany(intDef, initiator, list, null);
			MoteMaker.MakeInteractionBubble(initiator, list.RandomElement(), intDef.interactionMote, intDef.GetSymbol(initiator.Faction, initiator.Ideo), intDef.GetSymbolColor(initiator.Faction));
		}
		else
		{
			entry = new PlayLogEntry_InteractionSinglePawn(intDef, initiator, null);
			MoteMaker.MakeInteractionBubble(initiator, null, intDef.interactionMote, intDef.GetSymbol(), intDef.GetSymbolColor());
		}
		Find.PlayLog.Add(entry);
	}

	public static void ImitateInteractionWithNoPawn(Pawn initiator, InteractionDef intDef)
	{
		MoteMaker.MakeInteractionBubble(initiator, null, intDef.interactionMote, intDef.GetSymbol(), intDef.GetSymbolColor());
		Find.PlayLog.Add(new PlayLogEntry_InteractionSinglePawn(intDef, initiator, null));
	}
}
