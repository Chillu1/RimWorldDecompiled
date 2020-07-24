using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class TargetingParameters
	{
		public bool canTargetLocations;

		public bool canTargetSelf;

		public bool canTargetPawns = true;

		public bool canTargetFires;

		public bool canTargetBuildings = true;

		public bool canTargetItems;

		public bool canTargetAnimals = true;

		public bool canTargetHumans = true;

		public bool canTargetMechs = true;

		public List<Faction> onlyTargetFactions;

		public Predicate<TargetInfo> validator;

		public bool onlyTargetFlammables;

		public Thing targetSpecificThing;

		public bool mustBeSelectable;

		public bool neverTargetDoors;

		public bool neverTargetIncapacitated;

		public bool onlyTargetThingsAffectingRegions;

		public bool onlyTargetDamagedThings;

		public bool mapObjectTargetsMustBeAutoAttackable = true;

		public bool onlyTargetIncapacitatedPawns;

		public bool onlyTargetControlledPawns;

		public bool onlyTargetColonists;

		public ThingCategory thingCategory;

		public bool CanTarget(TargetInfo targ)
		{
			if (validator != null && !validator(targ))
			{
				return false;
			}
			if (targ.Thing == null)
			{
				return canTargetLocations;
			}
			if (neverTargetDoors && targ.Thing.def.IsDoor)
			{
				return false;
			}
			if (onlyTargetDamagedThings && targ.Thing.HitPoints == targ.Thing.MaxHitPoints)
			{
				return false;
			}
			if (onlyTargetFlammables && !targ.Thing.FlammableNow)
			{
				return false;
			}
			if (mustBeSelectable && !ThingSelectionUtility.SelectableByMapClick(targ.Thing))
			{
				return false;
			}
			if (targetSpecificThing != null && targ.Thing == targetSpecificThing)
			{
				return true;
			}
			if (canTargetFires && targ.Thing.def == ThingDefOf.Fire)
			{
				return true;
			}
			if (canTargetPawns && targ.Thing.def.category == ThingCategory.Pawn)
			{
				Pawn pawn = (Pawn)targ.Thing;
				if (pawn.Downed)
				{
					if (neverTargetIncapacitated)
					{
						return false;
					}
				}
				else if (onlyTargetIncapacitatedPawns)
				{
					return false;
				}
				if (onlyTargetFactions != null && !onlyTargetFactions.Contains(targ.Thing.Faction))
				{
					return false;
				}
				if (pawn.NonHumanlikeOrWildMan())
				{
					if (pawn.Faction == Faction.OfMechanoids)
					{
						if (!canTargetMechs)
						{
							return false;
						}
					}
					else if (!canTargetAnimals)
					{
						return false;
					}
				}
				if (!pawn.NonHumanlikeOrWildMan() && !canTargetHumans)
				{
					return false;
				}
				if (onlyTargetControlledPawns && !pawn.IsColonistPlayerControlled)
				{
					return false;
				}
				if (onlyTargetColonists && (!pawn.IsColonist || pawn.HostFaction != null))
				{
					return false;
				}
				return true;
			}
			if (canTargetBuildings && targ.Thing.def.category == ThingCategory.Building)
			{
				if (onlyTargetThingsAffectingRegions && !targ.Thing.def.AffectsRegions)
				{
					return false;
				}
				if (onlyTargetFactions != null && !onlyTargetFactions.Contains(targ.Thing.Faction))
				{
					return false;
				}
				return true;
			}
			if (canTargetItems)
			{
				if (mapObjectTargetsMustBeAutoAttackable && !targ.Thing.def.isAutoAttackableMapObject)
				{
					return false;
				}
				if (thingCategory == ThingCategory.None || thingCategory == targ.Thing.def.category)
				{
					return true;
				}
			}
			return false;
		}

		public static TargetingParameters ForSelf(Pawn p)
		{
			return new TargetingParameters
			{
				targetSpecificThing = p,
				canTargetPawns = false,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false
			};
		}

		public static TargetingParameters ForArrest(Pawn arrester)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = delegate(TargetInfo targ)
				{
					if (!targ.HasThing)
					{
						return false;
					}
					Pawn pawn = targ.Thing as Pawn;
					if (pawn == null || pawn == arrester || !pawn.CanBeArrestedBy(arrester))
					{
						return false;
					}
					return (!pawn.Downed) ? true : false;
				}
			};
		}

		public static TargetingParameters ForAttackHostile()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = true,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = true,
				validator = delegate(TargetInfo targ)
				{
					if (!targ.HasThing)
					{
						return false;
					}
					if (targ.Thing.HostileTo(Faction.OfPlayer))
					{
						return true;
					}
					Pawn pawn = targ.Thing as Pawn;
					return (pawn != null && pawn.NonHumanlikeOrWildMan()) ? true : false;
				}
			};
		}

		public static TargetingParameters ForAttackAny()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = true,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = true
			};
		}

		public static TargetingParameters ForRescue(Pawn p)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				onlyTargetIncapacitatedPawns = true,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false
			};
		}

		public static TargetingParameters ForStrip(Pawn p)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo targ) => targ.HasThing && StrippableUtility.CanBeStrippedByColony(targ.Thing))
			};
		}

		public static TargetingParameters ForTrade()
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo x) => (x.Thing as ITrader)?.CanTradeNow ?? false)
			};
		}

		public static TargetingParameters ForDropPodsDestination()
		{
			return new TargetingParameters
			{
				canTargetLocations = true,
				canTargetSelf = false,
				canTargetPawns = false,
				canTargetFires = false,
				canTargetBuildings = false,
				canTargetItems = false,
				validator = ((TargetInfo x) => DropCellFinder.IsGoodDropSpot(x.Cell, x.Map, allowFogged: false, canRoofPunch: true))
			};
		}

		public static TargetingParameters ForQuestPawnsWhoWillJoinColony(Pawn p)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = delegate(TargetInfo x)
				{
					Pawn pawn = x.Thing as Pawn;
					return pawn != null && !pawn.Dead && pawn.mindState.WillJoinColonyIfRescued;
				}
			};
		}

		public static TargetingParameters ForOpen(Pawn p)
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = ((TargetInfo x) => (x.Thing as IOpenable)?.CanOpen ?? false)
			};
		}

		public static TargetingParameters ForShuttle(Pawn hauler)
		{
			return new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = false,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = delegate(TargetInfo targ)
				{
					if (!targ.HasThing)
					{
						return false;
					}
					Pawn pawn = targ.Thing as Pawn;
					if (pawn == null || pawn.Dead || pawn == hauler)
					{
						return false;
					}
					if (pawn.Downed)
					{
						return true;
					}
					return pawn.IsPrisonerOfColony ? pawn.guest.PrisonerIsSecure : pawn.AnimalOrWildMan();
				}
			};
		}
	}
}
