using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

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

	public bool canTargetPlants;

	public bool canTargetSubhumans = true;

	public bool canTargetEntities = true;

	public List<Faction> onlyTargetFactions;

	public Predicate<TargetInfo> validator;

	public bool onlyTargetFlammables;

	public Thing targetSpecificThing;

	public bool mustBeSelectable;

	public bool neverTargetDoors;

	public bool neverTargetIncapacitated;

	public bool neverTargetHostileFaction;

	public bool onlyTargetSameIdeo;

	public bool onlyTargetThingsAffectingRegions;

	public bool onlyTargetDamagedThings;

	public bool mapObjectTargetsMustBeAutoAttackable = true;

	public bool onlyTargetIncapacitatedPawns;

	public bool onlyTargetColonistsOrPrisoners;

	public bool onlyTargetColonistsOrPrisonersOrSlaves;

	public bool onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks;

	public bool onlyTargetControlledPawns;

	public bool onlyTargetColonists;

	public bool onlyTargetPrisonersOfColony;

	public bool onlyTargetPsychicSensitive;

	public bool onlyTargetAnimaTrees;

	public bool canTargetBloodfeeders = true;

	public bool onlyRepairableMechs;

	public ThingCategory thingCategory;

	public bool onlyTargetDoors;

	public bool canTargetCorpses;

	public bool onlyTargetCorpses;

	public int mapBoundsContractedBy;

	public bool CanTarget(TargetInfo targ, ITargetingSource source = null)
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
		if (onlyTargetColonistsOrPrisoners && targ.Thing.def.category != ThingCategory.Pawn)
		{
			return false;
		}
		if (onlyTargetColonistsOrPrisonersOrSlaves && targ.Thing.def.category != ThingCategory.Pawn)
		{
			return false;
		}
		if (onlyTargetDoors && !targ.Thing.def.IsDoor)
		{
			return false;
		}
		Corpse corpse = (targ.Thing as Corpse) ?? (targ.Thing as Pawn)?.Corpse;
		if (canTargetCorpses && corpse != null)
		{
			if (!canTargetMechs && corpse.InnerPawn.RaceProps.IsMechanoid)
			{
				return false;
			}
			if (!canTargetAnimals && corpse.InnerPawn.RaceProps.Animal)
			{
				return false;
			}
			if (!canTargetHumans && corpse.InnerPawn.RaceProps.Humanlike)
			{
				return false;
			}
			if (!canTargetSubhumans && corpse.InnerPawn.IsSubhuman)
			{
				return false;
			}
			return true;
		}
		if (onlyTargetCorpses)
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
				if (pawn.Faction != null && pawn.RaceProps.IsMechanoid)
				{
					if (!canTargetMechs)
					{
						return false;
					}
					if (onlyRepairableMechs && !MechRepairUtility.CanRepair(pawn))
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
			if (!canTargetEntities && pawn.IsEntity)
			{
				return false;
			}
			if (!canTargetSubhumans && pawn.IsSubhuman)
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
			if (onlyTargetPrisonersOfColony && !pawn.IsPrisonerOfColony)
			{
				return false;
			}
			if (onlyTargetColonistsOrPrisoners && !pawn.IsColonistPlayerControlled && !pawn.IsPrisonerOfColony)
			{
				return false;
			}
			if (onlyTargetColonistsOrPrisonersOrSlaves && !pawn.IsColonistPlayerControlled && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
			{
				return false;
			}
			if (onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks)
			{
				if (!pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony && (!pawn.IsColonist || (pawn.HostFaction != null && !pawn.IsSlave)))
				{
					return false;
				}
				MentalStateDef mentalStateDef = pawn.MentalStateDef;
				if (mentalStateDef != null && mentalStateDef.IsAggro)
				{
					return false;
				}
			}
			if (onlyTargetPsychicSensitive && pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
			{
				return false;
			}
			if (neverTargetHostileFaction && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
			{
				Faction homeFaction = pawn.HomeFaction;
				if (homeFaction != null && homeFaction.HostileTo(Faction.OfPlayer))
				{
					return false;
				}
			}
			if (onlyTargetSameIdeo)
			{
				if (source == null)
				{
					Log.Error("Source passed in is null but targeting parameters have onlyTargetSameIdeo set.");
				}
				else if (source is Verb { CasterPawn: not null } verb)
				{
					Ideo ideo = ((targ.Thing is Pawn pawn2) ? pawn2.Ideo : null);
					if (verb.CasterPawn.Ideo != ideo)
					{
						return false;
					}
				}
				else
				{
					Log.Error("Source passed in is incompatible type but targeting parameters have onlyTargetSameIdeo set.");
				}
			}
			if (!canTargetBloodfeeders && ModsConfig.BiotechActive && pawn.IsBloodfeeder())
			{
				return false;
			}
			return true;
		}
		if (canTargetBuildings && targ.Thing.def.category == ThingCategory.Building)
		{
			if (mapObjectTargetsMustBeAutoAttackable && !targ.Thing.def.building.isTargetable)
			{
				return false;
			}
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
		if (canTargetPlants && targ.Thing.def.category == ThingCategory.Plant)
		{
			if (ModsConfig.RoyaltyActive && onlyTargetAnimaTrees && targ.Thing.def != ThingDefOf.Plant_TreeAnima)
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

	public static TargetingParameters ForForceWear(Pawn selectedPawnForJob)
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetAnimals = false,
			canTargetMechs = false,
			canTargetItems = false,
			canTargetBuildings = true,
			mapObjectTargetsMustBeAutoAttackable = false,
			validator = delegate(TargetInfo targ)
			{
				if (!targ.HasThing)
				{
					return false;
				}
				if (!(targ.Thing is Pawn pawn))
				{
					if (ModsConfig.OdysseyActive)
					{
						return targ.Thing is Building_OutfitStand;
					}
					return false;
				}
				if (pawn == selectedPawnForJob)
				{
					return false;
				}
				if (!pawn.kindDef.canStrip)
				{
					return false;
				}
				if (pawn.Downed && pawn.RaceProps.Humanlike)
				{
					return true;
				}
				if (pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure)
				{
					return true;
				}
				if (pawn.IsQuestLodger())
				{
					return false;
				}
				return pawn.IsColonist ? true : false;
			}
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
			validator = (TargetInfo x) => DropCellFinder.IsGoodDropSpot(x.Cell, x.Map, allowFogged: false, canRoofPunch: true)
		};
	}

	public static TargetingParameters ForBuilding(ThingDef def = null)
	{
		return new TargetingParameters
		{
			canTargetPawns = false,
			canTargetItems = false,
			canTargetBuildings = true,
			validator = delegate(TargetInfo targ)
			{
				if (!targ.HasThing)
				{
					return false;
				}
				return def == null || targ.Thing.def == def;
			}
		};
	}

	public static TargetingParameters ForColonist()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = false,
			onlyTargetColonists = true
		};
	}

	public static TargetingParameters ForPawns()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = false
		};
	}

	public static TargetingParameters ForThing()
	{
		return new TargetingParameters
		{
			canTargetPawns = true,
			canTargetBuildings = true,
			canTargetItems = true,
			canTargetPlants = true,
			canTargetFires = true,
			mapObjectTargetsMustBeAutoAttackable = false
		};
	}

	public static TargetingParameters ForCell()
	{
		return new TargetingParameters
		{
			canTargetLocations = true,
			canTargetBuildings = false,
			canTargetHumans = false,
			canTargetMechs = false,
			canTargetAnimals = false,
			onlyTargetColonists = false
		};
	}
}
