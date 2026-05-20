using Verse;
using Verse.AI;

namespace RimWorld;

public class PawnComponentsUtility
{
	public static void CreateInitialComponents(Pawn pawn)
	{
		if (pawn.ageTracker == null)
		{
			pawn.ageTracker = new Pawn_AgeTracker(pawn);
		}
		if (pawn.health == null)
		{
			pawn.health = new Pawn_HealthTracker(pawn);
		}
		if (pawn.records == null)
		{
			pawn.records = new Pawn_RecordsTracker(pawn);
		}
		if (pawn.inventory == null)
		{
			pawn.inventory = new Pawn_InventoryTracker(pawn);
		}
		if (pawn.meleeVerbs == null)
		{
			pawn.meleeVerbs = new Pawn_MeleeVerbs(pawn);
		}
		if (pawn.verbTracker == null)
		{
			pawn.verbTracker = new VerbTracker(pawn);
		}
		if (pawn.carryTracker == null)
		{
			pawn.carryTracker = new Pawn_CarryTracker(pawn);
		}
		if (pawn.needs == null)
		{
			pawn.needs = new Pawn_NeedsTracker(pawn);
		}
		if (pawn.mindState == null)
		{
			pawn.mindState = new Pawn_MindState(pawn);
		}
		if (pawn.ownership == null)
		{
			pawn.ownership = new Pawn_Ownership(pawn);
		}
		if (pawn.thinker == null)
		{
			pawn.thinker = new Pawn_Thinker(pawn);
		}
		if (pawn.jobs == null)
		{
			pawn.jobs = new Pawn_JobTracker(pawn);
		}
		if (pawn.stances == null)
		{
			pawn.stances = new Pawn_StanceTracker(pawn);
		}
		if (ModsConfig.AnomalyActive && pawn.duplicate == null)
		{
			pawn.duplicate = new Pawn_DuplicateTracker(pawn);
		}
		if (pawn.RaceProps.ToolUser)
		{
			if (pawn.equipment == null)
			{
				pawn.equipment = new Pawn_EquipmentTracker(pawn);
			}
			if (pawn.apparel == null)
			{
				pawn.apparel = new Pawn_ApparelTracker(pawn);
			}
		}
		if (pawn.RaceProps.Humanlike)
		{
			if (pawn.skills == null)
			{
				pawn.skills = new Pawn_SkillTracker(pawn);
			}
			if (pawn.story == null)
			{
				pawn.story = new Pawn_StoryTracker(pawn);
			}
			if (pawn.guest == null)
			{
				pawn.guest = new Pawn_GuestTracker(pawn);
			}
			if (pawn.guilt == null)
			{
				pawn.guilt = new Pawn_GuiltTracker(pawn);
			}
			if (pawn.workSettings == null)
			{
				pawn.workSettings = new Pawn_WorkSettings(pawn);
			}
			if (pawn.royalty == null)
			{
				pawn.royalty = new Pawn_RoyaltyTracker(pawn);
			}
			if (pawn.ideo == null)
			{
				pawn.ideo = new Pawn_IdeoTracker(pawn);
			}
			if (pawn.style == null)
			{
				pawn.style = new Pawn_StyleTracker(pawn);
			}
			if (pawn.surroundings == null)
			{
				pawn.surroundings = new Pawn_SurroundingsTracker(pawn);
			}
			if (pawn.genes == null)
			{
				pawn.genes = new Pawn_GeneTracker(pawn);
			}
			if (ModsConfig.IdeologyActive && pawn.styleObserver == null)
			{
				pawn.styleObserver = new Pawn_StyleObserverTracker(pawn);
			}
			if (ModsConfig.AnomalyActive && pawn.creepjoiner == null && pawn.kindDef is CreepJoinerFormKindDef form)
			{
				pawn.creepjoiner = new Pawn_CreepJoinerTracker(pawn)
				{
					form = form
				};
			}
		}
		if (ModsConfig.IdeologyActive && pawn.connections == null)
		{
			pawn.connections = new Pawn_ConnectionsTracker(pawn);
		}
		if ((pawn.RaceProps.ShouldHaveAbilityTracker || !pawn.kindDef.abilities.NullOrEmpty()) && pawn.abilities == null)
		{
			pawn.abilities = new Pawn_AbilityTracker(pawn);
		}
		if (pawn.RaceProps.IsFlesh)
		{
			if (pawn.relations == null)
			{
				pawn.relations = new Pawn_RelationsTracker(pawn);
			}
			if (ModsConfig.RoyaltyActive && pawn.psychicEntropy == null)
			{
				pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
			}
		}
		AddAndRemoveDynamicComponents(pawn);
	}

	public static void AddComponentsForSpawn(Pawn pawn)
	{
		if (pawn.rotationTracker == null)
		{
			pawn.rotationTracker = new Pawn_RotationTracker(pawn);
		}
		if (pawn.pather == null)
		{
			pawn.pather = new Pawn_PathFollower(pawn);
		}
		if (pawn.natives == null)
		{
			pawn.natives = new Pawn_NativeVerbs(pawn);
		}
		if (pawn.filth == null)
		{
			pawn.filth = new Pawn_FilthTracker(pawn);
		}
		if (pawn.roping == null)
		{
			pawn.roping = new Pawn_RopeTracker(pawn);
		}
		if (pawn.flight == null)
		{
			pawn.flight = new Pawn_FlightTracker(pawn);
		}
		if (ModsConfig.IdeologyActive && pawn.connections == null)
		{
			pawn.connections = new Pawn_ConnectionsTracker(pawn);
		}
		if (((int)pawn.RaceProps.intelligence <= 1 || (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.ShouldHaveCallTracker)) && pawn.caller == null)
		{
			pawn.caller = new Pawn_CallTracker(pawn);
		}
		if ((pawn.RaceProps.Humanlike || (ModsConfig.BiotechActive && pawn.RaceProps.IsMechanoid)) && pawn.abilities == null)
		{
			pawn.abilities = new Pawn_AbilityTracker(pawn);
		}
		if (pawn.RaceProps.Humanlike && ModsConfig.IdeologyActive && pawn.styleObserver == null)
		{
			pawn.styleObserver = new Pawn_StyleObserverTracker(pawn);
		}
		if (pawn.RaceProps.IsFlesh)
		{
			if (pawn.interactions == null)
			{
				pawn.interactions = new Pawn_InteractionsTracker(pawn);
			}
			if (ModsConfig.RoyaltyActive && pawn.psychicEntropy == null)
			{
				pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
			}
		}
		AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
	}

	public static void RemoveComponentsOnKilled(Pawn pawn)
	{
		pawn.carryTracker = null;
		pawn.needs = null;
		pawn.mindState = null;
		pawn.thinker = null;
		pawn.jobs = null;
		pawn.stances = null;
		pawn.duplicate = null;
		pawn.workSettings = null;
		pawn.trader = null;
		pawn.mechanitor = null;
		pawn.infectionVectors = null;
	}

	public static void RemoveComponentsOnDespawned(Pawn pawn)
	{
		pawn.rotationTracker = null;
		pawn.pather = null;
		pawn.natives = null;
		pawn.filth = null;
		pawn.roping = null;
		pawn.flight = null;
		pawn.caller = null;
		pawn.interactions = null;
		pawn.drafter = null;
	}

	public static void AddAndRemoveDynamicComponents(Pawn pawn, bool actAsIfSpawned = false)
	{
		bool flag = pawn.Faction != null && pawn.Faction.IsPlayer;
		bool flag2 = pawn.HostFaction != null && pawn.HostFaction.IsPlayer;
		if (pawn.RaceProps.Humanlike && !pawn.Dead)
		{
			if (pawn.mindState.wantsToTradeWithColony)
			{
				if (pawn.trader == null)
				{
					pawn.trader = new Pawn_TraderTracker(pawn);
				}
			}
			else
			{
				pawn.trader = null;
			}
		}
		if (!pawn.Dead && pawn.RaceProps.Humanlike)
		{
			if (pawn.mindState.wantsToTradeWithColony)
			{
				if (pawn.trader == null)
				{
					pawn.trader = new Pawn_TraderTracker(pawn);
				}
			}
			else
			{
				pawn.trader = null;
			}
			if (ModsConfig.AnomalyActive)
			{
				if (pawn.infectionVectors == null)
				{
					pawn.infectionVectors = new Pawn_InfectionVectorTracker(pawn);
				}
				if (pawn.duplicate == null)
				{
					pawn.duplicate = new Pawn_DuplicateTracker(pawn);
				}
			}
		}
		if (pawn.RaceProps.Humanlike)
		{
			if ((flag || flag2) && pawn.foodRestriction == null)
			{
				pawn.foodRestriction = new Pawn_FoodRestrictionTracker(pawn);
			}
			if (flag)
			{
				if (pawn.outfits == null)
				{
					pawn.outfits = new Pawn_OutfitTracker(pawn);
				}
				if (pawn.drugs == null)
				{
					pawn.drugs = new Pawn_DrugPolicyTracker(pawn);
				}
				if (pawn.timetable == null)
				{
					pawn.timetable = new Pawn_TimetableTracker(pawn);
				}
				if (pawn.reading == null)
				{
					pawn.reading = new Pawn_ReadingTracker(pawn);
				}
				if (pawn.inventoryStock == null)
				{
					pawn.inventoryStock = new Pawn_InventoryStockTracker(pawn);
				}
				if (ModsConfig.BiotechActive)
				{
					if (MechanitorUtility.ShouldBeMechanitor(pawn) && pawn.mechanitor == null)
					{
						pawn.mechanitor = new Pawn_MechanitorTracker(pawn);
					}
					else if (!MechanitorUtility.ShouldBeMechanitor(pawn) && pawn.mechanitor != null)
					{
						pawn.mechanitor = null;
					}
					if (pawn.ageTracker.CurLifeStage != null)
					{
						if (pawn.DevelopmentalStage.Child() && pawn.learning == null)
						{
							pawn.learning = new Pawn_LearningTracker(pawn);
						}
						else if (!pawn.DevelopmentalStage.Child() && pawn.learning != null)
						{
							pawn.learning = null;
						}
					}
				}
				if ((pawn.Spawned || actAsIfSpawned) && pawn.drafter == null)
				{
					pawn.drafter = new Pawn_DraftController(pawn);
				}
			}
			else
			{
				pawn.drafter = null;
			}
		}
		if (pawn.RaceProps.IsMechanoid)
		{
			if (pawn.IsColonyMech)
			{
				if ((pawn.Spawned || actAsIfSpawned) && pawn.drafter == null)
				{
					pawn.drafter = new Pawn_DraftController(pawn);
				}
			}
			else
			{
				pawn.drafter = null;
			}
		}
		if ((flag || flag2 || pawn.IsOnHoldingPlatform) && pawn.playerSettings == null)
		{
			pawn.playerSettings = new Pawn_PlayerSettings(pawn);
		}
		if ((int)pawn.RaceProps.intelligence <= 1 && pawn.Faction != null && !pawn.RaceProps.IsMechanoid && pawn.training == null)
		{
			pawn.training = new Pawn_TrainingTracker(pawn);
		}
		if (ModsConfig.BiotechActive && MechanitorUtility.IsPlayerOverseerSubject(pawn))
		{
			if (pawn.relations == null)
			{
				pawn.relations = new Pawn_RelationsTracker(pawn);
			}
			if (pawn.workSettings == null)
			{
				pawn.workSettings = new Pawn_WorkSettings(pawn);
			}
		}
		if (pawn.needs != null)
		{
			pawn.needs.AddOrRemoveNeedsAsAppropriate();
		}
	}

	public static bool HasSpawnedComponents(Pawn p)
	{
		return p.pather != null;
	}
}
