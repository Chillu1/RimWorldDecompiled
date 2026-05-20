using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Verse;

public class Pawn : ThingWithComps, IStrippable, IBillGiver, IVerbOwner, ITrader, IAttackTarget, ILoadReferenceable, IAttackTargetSearcher, IThingHolder, IObservedThoughtGiver, ISearchableContents, IEquatable<Pawn>
{
	public PawnKindDef kindDef;

	private Name nameInt;

	public Gender gender;

	public Pawn_AgeTracker ageTracker;

	public Pawn_HealthTracker health;

	public Pawn_RecordsTracker records;

	public Pawn_InventoryTracker inventory;

	public Pawn_MeleeVerbs meleeVerbs;

	public VerbTracker verbTracker;

	public Pawn_Ownership ownership;

	public Pawn_CarryTracker carryTracker;

	public Pawn_NeedsTracker needs;

	public Pawn_MindState mindState;

	public Pawn_SurroundingsTracker surroundings;

	public Pawn_Thinker thinker;

	public Pawn_JobTracker jobs;

	public Pawn_StanceTracker stances;

	public Pawn_InfectionVectorTracker infectionVectors;

	public Pawn_DuplicateTracker duplicate;

	public Pawn_RotationTracker rotationTracker;

	public Pawn_PathFollower pather;

	public Pawn_NativeVerbs natives;

	public Pawn_FilthTracker filth;

	public Pawn_RopeTracker roping;

	public Pawn_FlightTracker flight;

	public Pawn_EquipmentTracker equipment;

	public Pawn_ApparelTracker apparel;

	public Pawn_SkillTracker skills;

	public Pawn_StoryTracker story;

	public Pawn_GuestTracker guest;

	public Pawn_GuiltTracker guilt;

	public Pawn_RoyaltyTracker royalty;

	public Pawn_AbilityTracker abilities;

	public Pawn_IdeoTracker ideo;

	public Pawn_GeneTracker genes;

	public Pawn_CreepJoinerTracker creepjoiner;

	public Pawn_WorkSettings workSettings;

	public Pawn_TraderTracker trader;

	public Pawn_StyleTracker style;

	public Pawn_StyleObserverTracker styleObserver;

	public Pawn_ConnectionsTracker connections;

	public Pawn_TrainingTracker training;

	public Pawn_CallTracker caller;

	public Pawn_PsychicEntropyTracker psychicEntropy;

	public Pawn_MutantTracker mutant;

	public Pawn_RelationsTracker relations;

	public Pawn_InteractionsTracker interactions;

	public Pawn_PlayerSettings playerSettings;

	public Pawn_OutfitTracker outfits;

	public Pawn_DrugPolicyTracker drugs;

	public Pawn_FoodRestrictionTracker foodRestriction;

	public Pawn_TimetableTracker timetable;

	public Pawn_InventoryStockTracker inventoryStock;

	public Pawn_MechanitorTracker mechanitor;

	public Pawn_LearningTracker learning;

	public Pawn_ReadingTracker reading;

	public Pawn_DraftController drafter;

	public Lord lord;

	public bool markedForDiscard;

	private Pawn_DrawTracker drawer;

	public int becameWorldPawnTickAbs = -1;

	public bool teleporting;

	public bool forceNoDeathNotification;

	public int showNamePromptOnTick = -1;

	public int babyNamingDeadline = -1;

	private Sustainer sustainerAmbient;

	private Sustainer sustainerMoving;

	public bool addCorpseToLord;

	public int timesRaisedAsShambler;

	private int lastSleepDisturbedTick;

	public int lastVacuumBurntTick;

	public Map prevMap;

	private Faction deadlifeDustFaction;

	private int deadlifeDustFactionTick;

	public bool wasLeftBehindStartingPawn;

	public bool debugMaxMoveSpeed;

	public bool wasDraftedBeforeSkip;

	public bool dontGivePreArrivalPathway;

	public bool everLostEgo;

	private const float HumanSizedHeatOutput = 0.3f;

	private const float AnimalHeatOutputFactor = 0.6f;

	public const int DefaultBabyNamingPeriod = 60000;

	public const int DefaultGrowthMomentChoicePeriod = 120000;

	private const int SleepDisturbanceMinInterval = 300;

	private const int DeadlifeFactionExpiryTicks = 12500;

	private const float HeatPushMaxTemperature = 40f;

	public const int MaxMoveTicks = 450;

	private static string NotSurgeryReadyTrans;

	private static string CannotReachTrans;

	[Unsaved(false)]
	private CompOverseerSubject overseerSubject;

	[Unsaved(false)]
	public CompCanBeDormant canBeDormant;

	[Unsaved(false)]
	public CompActivity activity;

	private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

	private static List<string> states = new List<string>();

	private List<WorkTypeDef> cachedDisabledWorkTypes;

	private List<WorkTypeDef> cachedDisabledWorkTypesPermanent;

	private Dictionary<WorkTypeDef, List<string>> cachedReasonsForDisabledWorkTypes;

	public Name Name
	{
		get
		{
			return nameInt;
		}
		set
		{
			nameInt = value;
		}
	}

	public RaceProperties RaceProps => def.race;

	public Job CurJob => jobs?.curJob;

	public JobDef CurJobDef => CurJob?.def;

	public bool Downed => health.Downed;

	public bool Crawling
	{
		get
		{
			if (Downed && health.CanCrawl && CurJobDef != null && CurJobDef.isCrawlingIfDowned)
			{
				return !this.InBed();
			}
			return false;
		}
	}

	public bool CanAttackWhileCrawling
	{
		get
		{
			if (IsMutant)
			{
				return mutant.Def.canAttackWhileCrawling;
			}
			return false;
		}
	}

	public bool Flying => flight?.Flying ?? false;

	public bool Swimming => CurJob?.swimming ?? false;

	public bool Dead => health.Dead;

	public bool DeadOrDowned
	{
		get
		{
			if (!Dead)
			{
				return Downed;
			}
			return true;
		}
	}

	public string KindLabel => GenLabel.BestKindLabel(this);

	public bool InMentalState
	{
		get
		{
			if (!Dead)
			{
				return mindState.mentalStateHandler.InMentalState;
			}
			return false;
		}
	}

	public MentalState MentalState
	{
		get
		{
			if (!Dead)
			{
				return mindState.mentalStateHandler.CurState;
			}
			return null;
		}
	}

	public MentalStateDef MentalStateDef
	{
		get
		{
			if (!Dead)
			{
				return mindState.mentalStateHandler.CurStateDef;
			}
			return null;
		}
	}

	public bool InAggroMentalState
	{
		get
		{
			if (!Dead && mindState.mentalStateHandler.InMentalState)
			{
				return mindState.mentalStateHandler.CurStateDef.IsAggro;
			}
			return false;
		}
	}

	public bool Inspired
	{
		get
		{
			if (!Dead && mindState?.inspirationHandler != null)
			{
				return mindState.inspirationHandler.Inspired;
			}
			return false;
		}
	}

	public Inspiration Inspiration
	{
		get
		{
			if (!Dead)
			{
				return mindState.inspirationHandler.CurState;
			}
			return null;
		}
	}

	public InspirationDef InspirationDef
	{
		get
		{
			if (!Dead)
			{
				return mindState.inspirationHandler.CurStateDef;
			}
			return null;
		}
	}

	public override Vector3 DrawPos => Drawer.DrawPos;

	public VerbTracker VerbTracker => verbTracker;

	public List<VerbProperties> VerbProperties => def.Verbs;

	public List<Tool> Tools => def.tools;

	public bool ShouldAvoidFences
	{
		get
		{
			if (!FenceBlocked)
			{
				if (roping != null)
				{
					return roping.AnyRopeesFenceBlocked;
				}
				return false;
			}
			return true;
		}
	}

	public float? RoamMtbDays
	{
		get
		{
			Pawn_HealthTracker pawn_HealthTracker = health;
			if (pawn_HealthTracker != null)
			{
				HediffSet hediffSet = pawn_HealthTracker.hediffSet;
				if (hediffSet != null && hediffSet.RemoveRoamMtb)
				{
					return null;
				}
			}
			return RaceProps?.roamMtbDays;
		}
	}

	public bool Roamer => RoamMtbDays.HasValue;

	public bool FenceBlocked
	{
		get
		{
			if (Roamer)
			{
				if (CurJobDef != null)
				{
					return !CurJobDef.ignoreFenceBlocked;
				}
				return true;
			}
			return false;
		}
	}

	public bool CanPassFences => !FenceBlocked;

	public bool DrawNonHumanlikeSwimmingGraphic
	{
		get
		{
			if (!base.Spawned || !WaterCellCost.HasValue || RaceProps.Humanlike)
			{
				return false;
			}
			if (ageTracker.CurKindLifeStage.swimmingGraphicData != null)
			{
				return base.Position.GetTerrain(base.Map).IsWater;
			}
			return false;
		}
	}

	public bool DrawNonHumanlikeStationaryGraphic
	{
		get
		{
			if (!base.Spawned || RaceProps.Humanlike)
			{
				return false;
			}
			if (pather.Moving)
			{
				return false;
			}
			return ageTracker.CurKindLifeStage.stationaryGraphicData != null;
		}
	}

	public bool CanOpenDoors
	{
		get
		{
			if (IsMutant && !mutant.Def.canOpenDoors)
			{
				return false;
			}
			if (!kindDef.canOpenDoors)
			{
				return false;
			}
			return true;
		}
	}

	public bool CanOpenAnyDoor
	{
		get
		{
			if (WildManUtility.WildManShouldReachOutsideNow(this))
			{
				return true;
			}
			if (lord?.LordJob != null && lord.LordJob.CanOpenAnyDoor(this))
			{
				return true;
			}
			if (IsMutant && mutant.Def.canOpenAnyDoor)
			{
				return true;
			}
			if (kindDef.canOpenAnyDoor)
			{
				return true;
			}
			return false;
		}
	}

	public int? WaterCellCost
	{
		get
		{
			if (Flying)
			{
				return 1;
			}
			if (ModsConfig.BiotechActive && genes != null && genes.WaterCellCost.HasValue)
			{
				return genes.WaterCellCost;
			}
			if (def.race.waterCellCost.HasValue)
			{
				return def.race.waterCellCost;
			}
			if (!Swimming)
			{
				return null;
			}
			return 1;
		}
	}

	public bool IsColonist
	{
		get
		{
			if (base.Faction != null && base.Faction.IsPlayer && RaceProps.Humanlike && (!IsSlave || guest.SlaveIsSecure))
			{
				return !IsSubhuman;
			}
			return false;
		}
	}

	public bool IsFreeColonist
	{
		get
		{
			if (IsColonist)
			{
				return HostFaction == null;
			}
			return false;
		}
	}

	public bool IsFreeNonSlaveColonist
	{
		get
		{
			if (IsFreeColonist)
			{
				return !IsSlave;
			}
			return false;
		}
	}

	public bool CanTakeOrder
	{
		get
		{
			if (!IsColonistPlayerControlled && !IsColonyMech)
			{
				return IsColonySubhumanPlayerControlled;
			}
			return true;
		}
	}

	public bool IsCreepJoiner
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				return creepjoiner != null;
			}
			return false;
		}
	}

	public Faction HostFaction => guest?.HostFaction;

	public Faction SlaveFaction => guest?.SlaveFaction;

	public Ideo Ideo => ideo?.Ideo;

	public bool ShouldHaveIdeo
	{
		get
		{
			if (!DevelopmentalStage.Baby() && !kindDef.preventIdeo)
			{
				if (IsMutant)
				{
					return !mutant.Def.disablesIdeo;
				}
				return true;
			}
			return false;
		}
	}

	public bool Drafted
	{
		get
		{
			if (drafter != null)
			{
				return drafter.Drafted;
			}
			return false;
		}
	}

	public bool IsPrisoner
	{
		get
		{
			if (guest != null)
			{
				return guest.IsPrisoner;
			}
			return false;
		}
	}

	public bool IsPrisonerOfColony
	{
		get
		{
			if (guest != null && guest.IsPrisoner)
			{
				return guest.HostFaction.IsPlayer;
			}
			return false;
		}
	}

	public bool IsSlave
	{
		get
		{
			if (guest != null)
			{
				return guest.IsSlave;
			}
			return false;
		}
	}

	public bool IsSlaveOfColony
	{
		get
		{
			if (IsSlave)
			{
				return base.Faction.IsPlayer;
			}
			return false;
		}
	}

	public bool IsFreeman => HostFaction == null;

	public bool IsMutant => mutant != null;

	public bool IsSubhuman
	{
		get
		{
			if (IsMutant)
			{
				return mutant.Def.consideredSubhuman;
			}
			return false;
		}
	}

	public bool IsDuplicate
	{
		get
		{
			if (ModsConfig.AnomalyActive && duplicate != null && duplicate.duplicateOf != int.MinValue)
			{
				return duplicate.duplicateOf != thingIDNumber;
			}
			return false;
		}
	}

	public bool IsEntity
	{
		get
		{
			if (ModsConfig.AnomalyActive && (!RaceProps.Humanlike || IsSubhuman || IsShambler))
			{
				return base.Faction == Faction.OfEntities;
			}
			return false;
		}
	}

	public bool IsShambler
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				if (!IsMutant || mutant.Def != MutantDefOf.Shambler)
				{
					return health.hediffSet.HasHediff(HediffDefOf.ShamblerCorpse);
				}
				return true;
			}
			return false;
		}
	}

	public bool IsGhoul
	{
		get
		{
			if (ModsConfig.AnomalyActive && IsMutant)
			{
				return mutant.Def == MutantDefOf.Ghoul;
			}
			return false;
		}
	}

	public bool IsAwokenCorpse
	{
		get
		{
			if (ModsConfig.AnomalyActive && IsMutant)
			{
				return mutant.Def == MutantDefOf.AwokenCorpse;
			}
			return false;
		}
	}

	public bool IsAnimal
	{
		get
		{
			if (RaceProps.Animal)
			{
				return !IsSubhuman;
			}
			return false;
		}
	}

	public bool HasShowGizmosOnCorpseHediff
	{
		get
		{
			if (health?.hediffSet != null)
			{
				return health.hediffSet.HasHediffShowGizmosOnCorpse();
			}
			return false;
		}
	}

	public DevelopmentalStage DevelopmentalStage => ageTracker?.CurLifeStage?.developmentalStage ?? DevelopmentalStage.Adult;

	public GuestStatus? GuestStatus
	{
		get
		{
			if (guest != null && (HostFaction != null || guest.GuestStatus != RimWorld.GuestStatus.Guest))
			{
				return guest.GuestStatus;
			}
			return null;
		}
	}

	public bool IsColonistPlayerControlled
	{
		get
		{
			if (base.Spawned && IsColonist && MentalStateDef == null)
			{
				if (HostFaction != null)
				{
					return IsSlave;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsColonyMech
	{
		get
		{
			if (ModsConfig.BiotechActive && RaceProps.IsMechanoid && base.Faction == Faction.OfPlayer && MentalStateDef == null)
			{
				if (HostFaction != null)
				{
					return IsSlave;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsColonyMechPlayerControlled
	{
		get
		{
			if (base.Spawned && IsColonyMech && OverseerSubject != null)
			{
				return OverseerSubject.State == OverseerSubjectState.Overseen;
			}
			return false;
		}
	}

	public bool IsColonySubhuman
	{
		get
		{
			if (IsSubhuman)
			{
				return base.Faction == Faction.OfPlayer;
			}
			return false;
		}
	}

	public bool IsColonySubhumanPlayerControlled
	{
		get
		{
			if (base.Spawned && IsColonySubhuman)
			{
				return mutant.Def.canBeDrafted;
			}
			return false;
		}
	}

	public bool IsColonyAnimal
	{
		get
		{
			if (IsAnimal)
			{
				return base.Faction == Faction.OfPlayer;
			}
			return false;
		}
	}

	public bool IsPlayerControlled
	{
		get
		{
			if (!IsColonistPlayerControlled && !IsColonyMechPlayerControlled)
			{
				return IsColonySubhumanPlayerControlled;
			}
			return true;
		}
	}

	public IEnumerable<IntVec3> IngredientStackCells
	{
		get
		{
			yield return InteractionCell;
		}
	}

	public bool InContainerEnclosed => base.ParentHolder.IsEnclosingContainer();

	public Corpse Corpse => base.ParentHolder as Corpse;

	public Pawn CarriedBy
	{
		get
		{
			if (!(base.ParentHolder is Pawn_CarryTracker pawn_CarryTracker))
			{
				return null;
			}
			return pawn_CarryTracker.pawn;
		}
	}

	public virtual bool CanAttackWhenPathingBlocked => !IsAwokenCorpse;

	public bool HarmedByVacuum
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (RaceProps.IsMechanoid || (IsMutant && !mutant.Def.breathesAir))
			{
				return false;
			}
			return this.GetStatValue(StatDefOf.VacuumResistance, applyPostProcess: true, 60) < 1f;
		}
	}

	public bool ConcernedByVacuum
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			if (RaceProps.IsMechanoid || (IsMutant && !mutant.Def.breathesAir))
			{
				return false;
			}
			return this.GetStatValue(StatDefOf.VacuumResistance, applyPostProcess: true, 60) < 0.75f;
		}
	}

	private string LabelPrefix
	{
		get
		{
			if (IsMutant && mutant.HasTurned)
			{
				return mutant.Def.namePrefix;
			}
			return string.Empty;
		}
	}

	public override string LabelNoCount
	{
		get
		{
			if (Name != null)
			{
				if (story == null || story.TitleShortCap.NullOrEmpty() || IsSubhuman)
				{
					return LabelPrefix + Name.ToStringShort;
				}
				return LabelPrefix + Name.ToStringShort + (", " + story.TitleShortCap).Colorize(ColoredText.SubtleGrayColor);
			}
			return LabelPrefix + KindLabel;
		}
	}

	public override string LabelShort
	{
		get
		{
			if (Name != null)
			{
				return LabelPrefix + Name.ToStringShort;
			}
			return LabelNoCount;
		}
	}

	public TaggedString LabelNoCountColored
	{
		get
		{
			if (Name != null)
			{
				if (story == null || story.TitleShortCap.NullOrEmpty() || IsSubhuman)
				{
					return LabelPrefix + Name.ToStringShort.Colorize(ColoredText.NameColor);
				}
				return LabelPrefix + Name.ToStringShort.Colorize(ColoredText.NameColor) + (", " + story.TitleShortCap).Colorize(ColoredText.SubtleGrayColor);
			}
			return LabelPrefix + KindLabel;
		}
	}

	public TaggedString NameShortColored
	{
		get
		{
			if (Name != null)
			{
				return LabelPrefix + Name.ToStringShort.Colorize(ColoredText.NameColor);
			}
			return LabelPrefix + KindLabel;
		}
	}

	public TaggedString NameFullColored
	{
		get
		{
			if (Name != null)
			{
				return LabelPrefix + Name.ToStringFull.Colorize(ColoredText.NameColor);
			}
			return LabelPrefix + KindLabel;
		}
	}

	public TaggedString LegalStatus
	{
		get
		{
			if (IsSlave)
			{
				return "Slave".Translate().CapitalizeFirst();
			}
			if (base.Faction != null)
			{
				return new TaggedString(base.Faction.def.pawnSingular);
			}
			return "Colonist".Translate();
		}
	}

	public float TicksPerMoveCardinal => TicksPerMove(diagonal: false);

	public float TicksPerMoveDiagonal => TicksPerMove(diagonal: true);

	public override string DescriptionDetailed => DescriptionFlavor;

	public override string DescriptionFlavor
	{
		get
		{
			if (ModsConfig.AnomalyActive && IsSubhuman && !mutant.Def.description.NullOrEmpty())
			{
				return mutant.Def.description;
			}
			if (this.IsBaseliner())
			{
				return def.description;
			}
			string text = ((genes.Xenotype != XenotypeDefOf.Baseliner) ? genes.Xenotype.description : ((genes.CustomXenotype == null) ? genes.Xenotype.description : ((string)"UniqueXenotypeDesc".Translate())));
			return "StatsReport_NonBaselinerDescription".Translate(genes.XenotypeLabel) + "\n\n" + text;
		}
	}

	public override IEnumerable<DefHyperlink> DescriptionHyperlinks
	{
		get
		{
			foreach (DefHyperlink descriptionHyperlink in base.DescriptionHyperlinks)
			{
				yield return descriptionHyperlink;
			}
			if (!this.IsBaseliner() && genes.CustomXenotype == null)
			{
				yield return new DefHyperlink(genes.Xenotype);
			}
		}
	}

	public Pawn_DrawTracker Drawer => drawer ?? (drawer = new Pawn_DrawTracker(this));

	public Faction HomeFaction
	{
		get
		{
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				if (IsSlave && SlaveFaction != null)
				{
					return SlaveFaction;
				}
				if (this.HasExtraMiniFaction())
				{
					return this.GetExtraMiniFaction();
				}
				return this.GetExtraHomeFaction() ?? base.Faction;
			}
			return base.Faction;
		}
	}

	public bool Deathresting
	{
		get
		{
			if (ModsConfig.BiotechActive)
			{
				return health.hediffSet.HasHediff(HediffDefOf.Deathrest);
			}
			return false;
		}
	}

	public bool HasDeathRefusalOrResurrecting
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				if (!health.hediffSet.HasHediff<Hediff_DeathRefusal>())
				{
					return health.hediffSet.HasHediff(HediffDefOf.Rising);
				}
				return true;
			}
			return false;
		}
	}

	public override bool Suspended
	{
		get
		{
			if (base.Suspended)
			{
				return true;
			}
			if (Find.WorldPawns.GetSituation(this) == WorldPawnSituation.ReservedByQuest)
			{
				return true;
			}
			return false;
		}
	}

	public Faction DeadlifeDustFaction
	{
		get
		{
			if (GenTicks.TicksGame - deadlifeDustFactionTick < 12500 && deadlifeDustFaction != null)
			{
				return deadlifeDustFaction;
			}
			return Faction.OfPlayer;
		}
	}

	public bool HasPsylink => psychicEntropy?.Psylink != null;

	public CompOverseerSubject OverseerSubject
	{
		get
		{
			if (ModsConfig.BiotechActive && overseerSubject == null && RaceProps.IsMechanoid)
			{
				overseerSubject = GetComp<CompOverseerSubject>();
			}
			return overseerSubject;
		}
	}

	public override int UpdateRateTicks
	{
		get
		{
			if (!RaceProps.Animal)
			{
				return base.UpdateRateTicks;
			}
			return 15;
		}
	}

	public WorkTags CombinedDisabledWorkTags
	{
		get
		{
			WorkTags workTags = story?.DisabledWorkTagsBackstoryTraitsAndGenes ?? WorkTags.None;
			workTags |= kindDef.disabledWorkTags;
			if (royalty != null)
			{
				foreach (RoyalTitle item in royalty.AllTitlesForReading)
				{
					if (item.conceited)
					{
						workTags |= item.def.disabledWorkTags;
					}
				}
			}
			if (ModsConfig.IdeologyActive && Ideo != null)
			{
				Precept_Role role = Ideo.GetRole(this);
				if (role != null)
				{
					workTags |= role.def.roleDisabledWorkTags;
				}
			}
			if (health?.hediffSet != null)
			{
				foreach (Hediff hediff in health.hediffSet.hediffs)
				{
					HediffStage curStage = hediff.CurStage;
					if (curStage != null)
					{
						workTags |= curStage.disabledWorkTags;
					}
				}
			}
			foreach (QuestPart_WorkDisabled item2 in QuestUtility.GetWorkDisabledQuestPart(this))
			{
				workTags |= item2.disabledWorkTags;
			}
			if (IsMutant)
			{
				workTags |= mutant.Def.workDisables;
				if (!mutant.IsPassive)
				{
					workTags &= ~WorkTags.Violent;
				}
			}
			return workTags;
		}
	}

	public TraderKindDef TraderKind => trader?.traderKind;

	public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

	public IEnumerable<Thing> Goods => trader.Goods;

	public int RandomPriceFactorSeed => trader.RandomPriceFactorSeed;

	public string TraderName => trader.TraderName;

	public bool CanTradeNow
	{
		get
		{
			if (trader != null)
			{
				return trader.CanTradeNow;
			}
			return false;
		}
	}

	public float TradePriceImprovementOffsetForPlayer => 0f;

	public float BodySize => ageTracker.CurLifeStage.bodySizeFactor * RaceProps.baseBodySize;

	public float HealthScale => ageTracker.CurLifeStage.healthScaleFactor * RaceProps.baseHealthScale;

	public IEnumerable<Thing> EquippedWornOrInventoryThings => inventory.innerContainer.ConcatIfNotNull(apparel?.WornApparel).ConcatIfNotNull(equipment?.AllEquipmentListForReading);

	Thing IAttackTarget.Thing => this;

	public float TargetPriorityFactor => 1f;

	public LocalTargetInfo TargetCurrentlyAimingAt
	{
		get
		{
			if (!base.Spawned)
			{
				return LocalTargetInfo.Invalid;
			}
			Stance curStance = stances.curStance;
			if (curStance is Stance_Warmup || curStance is Stance_Cooldown)
			{
				return ((Stance_Busy)curStance).focusTarg;
			}
			return LocalTargetInfo.Invalid;
		}
	}

	Thing IAttackTargetSearcher.Thing => this;

	public LocalTargetInfo LastAttackedTarget => mindState.lastAttackedTarget;

	public int LastAttackTargetTick => mindState.lastAttackTargetTick;

	public Verb CurrentEffectiveVerb
	{
		get
		{
			if (this.MannedThing() is Building_Turret building_Turret)
			{
				return building_Turret.AttackVerb;
			}
			return TryGetAttackVerb(null, !IsColonist);
		}
	}

	private bool ForceNoDeathNotification
	{
		get
		{
			if (!forceNoDeathNotification)
			{
				return kindDef.forceNoDeathNotification;
			}
			return true;
		}
	}

	Thing IVerbOwner.ConstantCaster => this;

	ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Bodypart;

	public BillStack BillStack => health.surgeryBills;

	public override IntVec3 InteractionCell => this.CurrentBed()?.FindPreferredInteractionCell(base.Position) ?? base.InteractionCell;

	public ThingOwner SearchableContents => carryTracker?.innerContainer;

	public virtual bool ShouldShowQuestionMark()
	{
		if (ModsConfig.AnomalyActive && creepjoiner != null)
		{
			return creepjoiner.IsOnEntryLord;
		}
		return CanTradeNow;
	}

	public string GetKindLabelSingular()
	{
		return GenLabel.BestKindLabel(this);
	}

	public string GetKindLabelPlural(int count = -1)
	{
		return GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: false, plural: true, count);
	}

	public static void ResetStaticData()
	{
		NotSurgeryReadyTrans = "NotSurgeryReady".Translate();
		CannotReachTrans = "CannotReach".Translate();
	}

	public override void Notify_DefsHotReloaded()
	{
		base.Notify_DefsHotReloaded();
		Drawer.renderer.SetAllGraphicsDirty();
	}

	public void MarkDeadlifeDustForFaction(Faction faction)
	{
		deadlifeDustFaction = faction;
		deadlifeDustFactionTick = GenTicks.TicksGame;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (Dead)
		{
			Log.Warning("Tried to spawn Dead Pawn " + this.ToStringSafe() + ". Replacing with corpse.");
			Corpse obj = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
			obj.InnerPawn = this;
			GenSpawn.Spawn(obj, base.Position, map);
			return;
		}
		if (def == null || kindDef == null)
		{
			Log.Warning("Tried to spawn pawn without def " + this.ToStringSafe() + ".");
			return;
		}
		base.SpawnSetup(map, respawningAfterLoad);
		if (Find.WorldPawns.Contains(this))
		{
			Find.WorldPawns.RemovePawn(this);
		}
		PawnComponentsUtility.AddComponentsForSpawn(this);
		if (!PawnUtility.InValidState(this))
		{
			Log.Error("Pawn " + this.ToStringSafe() + " spawned in invalid state. Destroying...");
			try
			{
				DeSpawn();
			}
			catch (Exception ex)
			{
				Log.Error("Tried to despawn " + this.ToStringSafe() + " because of the previous error but couldn't: " + ex);
			}
			Find.WorldPawns.PassToWorld(this, PawnDiscardDecideMode.Discard);
			return;
		}
		Drawer.Notify_Spawned();
		rotationTracker.Notify_Spawned();
		if (!respawningAfterLoad)
		{
			pather.ResetToCurrentPosition();
		}
		base.Map.mapPawns.RegisterPawn(this);
		base.Map.autoSlaughterManager.Notify_PawnSpawned();
		if (relations != null)
		{
			relations.everSeenByPlayer = true;
		}
		AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
		needs?.mood?.recentMemory?.Notify_Spawned(respawningAfterLoad);
		equipment?.Notify_PawnSpawned();
		health?.Notify_Spawned();
		mechanitor?.Notify_PawnSpawned(respawningAfterLoad);
		mutant?.Notify_Spawned(respawningAfterLoad);
		infectionVectors?.NotifySpawned(respawningAfterLoad);
		if (base.Faction == Faction.OfPlayer)
		{
			Ideo?.RecacheColonistBelieverCount();
		}
		if (!respawningAfterLoad)
		{
			if ((base.Faction == Faction.OfPlayer || IsPlayerControlled) && base.Position.Fogged(map))
			{
				FloodFillerFog.FloodUnfog(base.Position, map);
			}
			Find.GameEnder.CheckOrUpdateGameOver();
			if (base.Faction == Faction.OfPlayer)
			{
				Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
				Find.World.StoryState.RecordPopulationIncrease();
			}
			if (!IsSubhuman)
			{
				PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(this);
			}
			if (this.IsQuestLodger())
			{
				for (int num = health.hediffSet.hediffs.Count - 1; num >= 0; num--)
				{
					if (health.hediffSet.hediffs[num].def.removeOnQuestLodgers)
					{
						health.RemoveHediff(health.hediffSet.hediffs[num]);
					}
				}
			}
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (IsPlayerControlled && base.PositionHeld.Fogged(base.Map))
			{
				FloodFillerFog.FloodUnfog(base.PositionHeld, base.Map);
			}
		});
		if (RaceProps.soundAmbience != null)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				sustainerAmbient = RaceProps.soundAmbience.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}
		if (RaceProps.soundMoving != null)
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				sustainerMoving = RaceProps.soundMoving.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}
		if (Ideo != null && Ideo.hidden)
		{
			Ideo.hidden = false;
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		canBeDormant = GetComp<CompCanBeDormant>();
		activity = GetComp<CompActivity>();
	}

	public override void PostMapInit()
	{
		base.PostMapInit();
		pather.TryResumePathingAfterLoading();
	}

	public void DrawShadowAt(Vector3 drawLoc)
	{
		Drawer.DrawShadowAt(drawLoc);
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		base.DynamicDrawPhaseAt(phase, drawLoc, flip);
		Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc);
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		Comps_PostDraw();
		mechanitor?.DrawCommandRadius();
	}

	public override void DrawGUIOverlay()
	{
		Drawer.ui.DrawPawnGUIOverlay();
		for (int i = 0; i < base.AllComps.Count; i++)
		{
			base.AllComps[i].DrawGUIOverlay();
		}
		SilhouetteUtility.DrawGUISilhouette(this);
		if (DebugViewSettings.drawPatherState)
		{
			pather.DrawDebugGUI();
		}
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (IsPlayerControlled)
		{
			pather.curPath?.DrawPath(this);
			jobs.DrawLinesBetweenTargets();
		}
	}

	public override void TickRare()
	{
		base.TickRare();
		if (!Suspended)
		{
			apparel?.ApparelTrackerTickRare();
		}
		training?.TrainingTrackerTickRare();
		if (base.Spawned && RaceProps.IsFlesh && base.AmbientTemperature < 40f)
		{
			GenTemperature.PushHeat(this, 0.3f * BodySize * 4.1666665f * (def.race.Humanlike ? 1f : 0.6f));
		}
	}

	protected override void Tick()
	{
		if (DebugSettings.noAnimals && base.Spawned && IsAnimal)
		{
			Destroy();
			return;
		}
		base.Tick();
		if (this.IsHashIntervalTick(250))
		{
			TickRare();
		}
		bool suspended = Suspended;
		if (!suspended)
		{
			if (base.Spawned)
			{
				pather.PatherTick();
			}
			if (base.Spawned)
			{
				verbTracker.VerbsTick();
			}
			if (base.Spawned)
			{
				roping?.RopingTick();
				flight?.FlightTick();
				natives.NativeVerbsTick();
			}
			if (base.Spawned)
			{
				stances.StanceTrackerTick();
			}
			if (!this.IsWorldPawn())
			{
				jobs?.JobTrackerTick();
			}
			health.HealthTick();
			if (base.Spawned && this.IsHiddenFromPlayer() && Find.Selector.IsSelected(this))
			{
				Find.Selector.Deselect(this);
			}
		}
		if (!suspended)
		{
			if (equipment != null)
			{
				using (ProfilerBlock.Scope("equipment"))
				{
					equipment.EquipmentTrackerTick();
				}
			}
			abilities?.AbilitiesTick();
			inventory?.InventoryTrackerTick();
			genes?.GeneTrackerTick();
			if (ModsConfig.AnomalyActive && base.Spawned)
			{
				mutant?.MutantTrackerTick();
				BloodRainUtility.BloodRainTick(this);
			}
		}
		if (base.Spawned && !base.Position.Fogged(base.Map))
		{
			if (RaceProps.soundAmbience != null && (sustainerAmbient == null || sustainerAmbient.Ended))
			{
				sustainerAmbient = RaceProps.soundAmbience.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			}
			sustainerAmbient?.Maintain();
			if (pather != null && pather.Moving && RaceProps.soundMoving != null)
			{
				if (sustainerMoving == null || sustainerMoving.Ended)
				{
					sustainerMoving = RaceProps.soundMoving.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
				}
				sustainerMoving?.Maintain();
			}
		}
		drawer?.renderer.EffectersTick(suspended || this.IsWorldPawn());
	}

	protected override void TickInterval(int delta)
	{
		if (DebugSettings.noAnimals && base.Spawned && IsAnimal)
		{
			Destroy();
			return;
		}
		base.TickInterval(delta);
		bool suspended = Suspended;
		if (!suspended)
		{
			if (!this.IsWorldPawn())
			{
				using (ProfilerBlock.Scope("jobs interval"))
				{
					jobs?.JobTrackerTickInterval(delta);
				}
			}
			using (ProfilerBlock.Scope("health interval"))
			{
				health.HealthTickInterval(delta);
			}
			if (!Dead)
			{
				using (ProfilerBlock.Scope("mind state interval"))
				{
					mindState.MindStateTickInterval(delta);
				}
				carryTracker.CarryHandsTickInterval(delta);
				if (!base.InCryptosleep && RaceProps.Humanlike)
				{
					infectionVectors?.InfectionTickInterval(delta);
				}
				if (showNamePromptOnTick != -1 && showNamePromptOnTick == Find.TickManager.TicksGame)
				{
					Find.WindowStack.Add(this.NamePawnDialog());
				}
			}
		}
		if (!base.Spawned)
		{
			Thing firstParentThing = ThingOwnerUtility.GetFirstParentThing(this);
			if (firstParentThing != null)
			{
				PawnUtility.GainComfortFromThingIfPossible(this, firstParentThing, delta);
			}
		}
		if (!Dead)
		{
			needs.NeedsTrackerTickInterval(delta);
		}
		if (!suspended)
		{
			apparel?.ApparelTrackerTickInterval(delta);
			if (interactions != null && base.Spawned)
			{
				using (ProfilerBlock.Scope("interactions"))
				{
					interactions.InteractionsTrackerTickInterval(delta);
				}
			}
			caller?.CallTrackerTickInterval(delta);
			skills?.SkillsTickInterval(delta);
			drafter?.DraftControllerTickInterval(delta);
			relations?.RelationsTrackerTickInterval(delta);
			if (ModsConfig.RoyaltyActive && psychicEntropy != null)
			{
				psychicEntropy.PsychicEntropyTrackerTickInterval(delta);
			}
			if (RaceProps.Humanlike)
			{
				guest.GuestTrackerTickInterval(delta);
			}
			ideo?.IdeoTrackerTickInterval(delta);
			genes?.GeneTrackerTickInterval(delta);
			if (royalty != null && ModsConfig.RoyaltyActive)
			{
				royalty.RoyaltyTrackerTickInterval(delta);
			}
			if (style != null && ModsConfig.IdeologyActive)
			{
				style.StyleTrackerTickInterval(delta);
			}
			if (styleObserver != null && ModsConfig.IdeologyActive)
			{
				styleObserver.StyleObserverTickInterval(delta);
			}
			if (surroundings != null && ModsConfig.IdeologyActive)
			{
				surroundings.SurroundingsTrackerTickInterval(delta);
			}
			if (ModsConfig.BiotechActive)
			{
				learning?.LearningTickInterval(delta);
				PollutionUtility.PawnPollutionTickInterval(this, delta);
			}
			GasUtility.PawnGasEffectsTickInterval(this, delta);
			ToxicUtility.PawnToxicTickInterval(this, delta);
			VacuumUtility.PawnVacuumTickInterval(this, delta);
			if (ModsConfig.AnomalyActive && base.Spawned)
			{
				mutant?.MutantTrackerTickInterval(delta);
				creepjoiner?.TickInterval(delta);
			}
			if (!IsMutant || !mutant.Def.disableAging)
			{
				ageTracker.AgeTickInterval(delta);
			}
			records.RecordsTickInterval(delta);
		}
		guilt?.GuiltTrackerTickInterval(delta);
	}

	public void ProcessPostTickVisuals(int ticksPassed, CellRect viewRect)
	{
		if (!Suspended && base.Spawned)
		{
			if (Current.ProgramState != ProgramState.Playing || viewRect.Contains(base.Position))
			{
				Drawer.ProcessPostTickVisuals(ticksPassed);
			}
			rotationTracker.ProcessPostTickVisuals(ticksPassed);
		}
	}

	public void TickMothballed(int interval)
	{
		if (!Suspended)
		{
			ageTracker.AgeTickMothballed(interval);
			records.RecordsTickMothballed(interval);
		}
	}

	public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
	{
		if (resetTweenedPos)
		{
			Drawer.tweener.Notify_Teleported();
		}
		pather.Notify_Teleported_Int();
		if (endCurrentJob && jobs?.curJob != null)
		{
			jobs.EndCurrentJob(JobCondition.InterruptForced, jobs.curJob.startTick != GenTicks.TicksGame);
		}
	}

	public virtual SurgicalInspectionOutcome DoSurgicalInspection(Pawn surgeon, out string desc)
	{
		if (!ModsConfig.AnomalyActive)
		{
			desc = "";
			return SurgicalInspectionOutcome.Nothing;
		}
		bool flag = false;
		bool flag2 = false;
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = health.hediffSet.hediffs.Count - 1; num >= 0; num--)
		{
			Hediff hediff = health.hediffSet.hediffs[num];
			if (hediff.TryGetComp<HediffComp_SurgeryInspectable>(out var comp))
			{
				if (hediff.Visible)
				{
					comp.DoSurgicalInspectionVisible(surgeon);
					if (comp.Props.preventLetterIfPreviouslyDetected)
					{
						flag2 = true;
					}
				}
				else
				{
					switch (comp.DoSurgicalInspection(surgeon))
					{
					case SurgicalInspectionOutcome.DetectedNoLetter:
						flag2 = true;
						hediff.SetVisible();
						break;
					case SurgicalInspectionOutcome.Detected:
						flag = true;
						if (!string.IsNullOrEmpty(comp.Props.surgicalDetectionDesc))
						{
							stringBuilder.Append("\n\n" + comp.Props.surgicalDetectionDesc.Formatted(this.Named("PAWN"), surgeon.Named("SURGEON")));
						}
						hediff.SetVisible();
						break;
					}
				}
			}
		}
		if (IsCreepJoiner && creepjoiner.DoSurgicalInspection(surgeon, stringBuilder))
		{
			flag = true;
		}
		desc = stringBuilder.ToString();
		if (flag2)
		{
			return SurgicalInspectionOutcome.DetectedNoLetter;
		}
		if (!flag)
		{
			return SurgicalInspectionOutcome.Nothing;
		}
		return SurgicalInspectionOutcome.Detected;
	}

	public void Notify_BecameVisible()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_BecameVisible();
		}
	}

	public void Notify_BecameInvisible()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_BecameInvisible();
		}
	}

	public void Notify_ForcedVisible()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_ForcedVisible();
		}
	}

	public void Notify_PassedToWorld()
	{
		if (((base.Faction == null && RaceProps.Humanlike) || (base.Faction != null && base.Faction.IsPlayer) || base.Faction == Faction.OfAncients || base.Faction == Faction.OfAncientsHostile) && !Dead && Find.WorldPawns.GetSituation(this) == WorldPawnSituation.Free)
		{
			bool tryMedievalOrBetter = base.Faction != null && (int)base.Faction.def.techLevel >= 3;
			Faction faction;
			if (this.HasExtraHomeFaction() && !this.GetExtraHomeFaction().IsPlayer)
			{
				if (base.Faction != this.GetExtraHomeFaction())
				{
					SetFaction(this.GetExtraHomeFaction());
				}
			}
			else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter))
			{
				if (base.Faction != faction)
				{
					SetFaction(faction);
				}
			}
			else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, allowDefeated: true))
			{
				if (base.Faction != faction)
				{
					SetFaction(faction);
				}
			}
			else if (base.Faction != null)
			{
				SetFaction(null);
			}
		}
		becameWorldPawnTickAbs = GenTicks.TicksAbs;
		if (!this.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(this))
		{
			ClearMind_NewTemp();
		}
		relations?.Notify_PassedToWorld();
		foreach (ThingComp allComp in base.AllComps)
		{
			allComp.Notify_PassedToWorld();
		}
		drawer?.renderer?.renderTree?.SetDirty();
	}

	public override void Notify_LeftBehind()
	{
		base.Notify_LeftBehind();
		relations?.Notify_PawnLeftBehind();
	}

	public void Notify_AddBedThoughts()
	{
		foreach (ThingComp allComp in base.AllComps)
		{
			allComp.Notify_AddBedThoughts(this);
		}
		Ideo?.Notify_AddBedThoughts(this);
	}

	public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		float num = 1f;
		if (ModsConfig.BiotechActive && genes != null)
		{
			num *= genes.FactorForDamage(dinfo);
		}
		num *= health.FactorForDamage(dinfo);
		dinfo.SetAmount(dinfo.Amount * num);
		base.PreApplyDamage(ref dinfo, out absorbed);
		if (!absorbed)
		{
			health.PreApplyDamage(dinfo, out absorbed);
		}
	}

	public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		base.PostApplyDamage(dinfo, totalDamageDealt);
		if (dinfo.Def.ExternalViolenceFor(this))
		{
			records.AddTo(RecordDefOf.DamageTaken, totalDamageDealt);
		}
		if (dinfo.Def.makesBlood && health.CanBleed && !dinfo.InstantPermanentInjury && totalDamageDealt > 0f && Rand.Chance(0.5f))
		{
			health.DropBloodFilth();
		}
		health.PostApplyDamage(dinfo, totalDamageDealt);
		if (Dead)
		{
			return;
		}
		mindState.Notify_DamageTaken(dinfo);
		if (ModsConfig.AnomalyActive && dinfo.Instigator is Pawn pawn)
		{
			List<InfectionPathwayDef> list = (dinfo.Def.isRanged ? pawn.kindDef.rangedAttackInfectionPathways : pawn.kindDef.meleeAttackInfectionPathways);
			if (list != null)
			{
				InfectionPathwayUtility.AddInfectionPathways(list, this, pawn);
			}
		}
	}

	public override Thing SplitOff(int count)
	{
		if (count <= 0 || count >= stackCount)
		{
			return base.SplitOff(count);
		}
		throw new NotImplementedException("Split off on Pawns is not supported (unless we're taking a full stack).");
	}

	private float TicksPerMove(bool diagonal)
	{
		float num = this.GetStatValue(StatDefOf.MoveSpeed);
		if (Downed && health.CanCrawl)
		{
			num = this.GetStatValue(StatDefOf.CrawlSpeed);
		}
		if (RestraintsUtility.InRestraints(this))
		{
			num *= 0.35f;
		}
		if (carryTracker?.CarriedThing != null && carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
		{
			num *= 0.6f;
		}
		float num2 = num / 60f;
		float num3;
		if (num2 == 0f)
		{
			num3 = 450f;
		}
		else
		{
			num3 = 1f / num2;
			if (base.Spawned && !base.Map.roofGrid.Roofed(base.Position))
			{
				num3 /= base.Map.weatherManager.CurMoveSpeedMultiplier;
			}
			if (diagonal)
			{
				num3 *= 1.41421f;
			}
		}
		num3 = Mathf.Clamp(num3, 1f, 450f);
		if (debugMaxMoveSpeed)
		{
			return 1f;
		}
		return num3;
	}

	private void DoKillSideEffects(DamageInfo? dinfo, Hediff exactCulprit, bool spawned)
	{
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Died);
		}
		if (IsColonist && !wasLeftBehindStartingPawn)
		{
			Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
		}
		if (spawned && ((dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(this)) || (exactCulprit?.sourceDef != null && exactCulprit.sourceDef.IsWeapon)))
		{
			LifeStageUtility.PlayNearestLifestageSound(this, (LifeStageAge lifeStage) => lifeStage.soundDeath, (GeneDef gene) => gene.soundDeath, (MutantDef mutantDef) => mutantDef.soundDeath);
		}
		if (dinfo?.Instigator != null && dinfo.Value.Instigator is Pawn pawn)
		{
			RecordsUtility.Notify_PawnKilled(this, pawn);
			pawn.equipment?.Notify_KilledPawn();
			if (RaceProps.Humanlike && pawn.needs != null && pawn.needs.TryGetNeed(out Need_KillThirst need))
			{
				need.Notify_KilledPawn(dinfo);
			}
			if (pawn.health.hediffSet != null)
			{
				for (int num = 0; num < pawn.health.hediffSet.hediffs.Count; num++)
				{
					pawn.health.hediffSet.hediffs[num].Notify_KilledPawn(this, dinfo);
				}
			}
			if (HistoryEventUtility.IsKillingInnocentAnimal(pawn, this))
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer), this.Named(HistoryEventArgsNames.Victim)));
			}
		}
		TaleUtility.Notify_PawnDied(this, dinfo);
		if (spawned)
		{
			Find.BattleLog.Add(new BattleLogEntry_StateTransition(this, RaceProps.DeathActionWorker.DeathRules, dinfo?.Instigator as Pawn, exactCulprit, dinfo?.HitPart));
		}
	}

	private void PreDeathPawnModifications(DamageInfo? dinfo, Map map)
	{
		health.surgeryBills.Clear();
		for (int i = 0; i < health.hediffSet.hediffs.Count; i++)
		{
			health.hediffSet.hediffs[i].Notify_PawnKilled();
		}
		apparel?.Notify_PawnKilled(dinfo);
		relations?.Notify_PawnKilled(dinfo, map);
		connections?.Notify_PawnKilled();
		meleeVerbs.Notify_PawnKilled();
	}

	private void DropBeforeDying(DamageInfo? dinfo, ref Map map, ref bool spawned)
	{
		if (base.ParentHolder is Pawn_CarryTracker pawn_CarryTracker && holdingOwner.TryDrop(this, pawn_CarryTracker.pawn.Position, pawn_CarryTracker.pawn.Map, ThingPlaceMode.Near, out var _))
		{
			map = pawn_CarryTracker.pawn.Map;
			spawned = true;
		}
		PawnDiedOrDownedThoughtsUtility.RemoveLostThoughts(this);
		PawnDiedOrDownedThoughtsUtility.RemoveResuedRelativeThought(this);
		PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this, dinfo, PawnDiedOrDownedThoughtsKind.Died);
		if (IsAnimal)
		{
			PawnDiedOrDownedThoughtsUtility.GiveVeneratedAnimalDiedThoughts(this, map);
		}
	}

	private void RemoveFromHoldingContainer(ref Map map, ref bool spawned, DamageInfo? dinfo)
	{
		if (ModsConfig.AnomalyActive && base.ParentHolder is Building_HoldingPlatform { Spawned: not false } building_HoldingPlatform)
		{
			building_HoldingPlatform.Notify_PawnDied(this, dinfo);
			spawned = true;
			map = building_HoldingPlatform.Map;
		}
		if (base.ParentHolder is CompTransporter compTransporter)
		{
			compTransporter.innerContainer.TryDrop(this, ThingPlaceMode.Near, out var _);
		}
	}

	public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
	{
		int num = 0;
		health.isBeingKilled = true;
		try
		{
			num = 1;
			IntVec3 positionHeld = base.PositionHeld;
			Map map = base.Map;
			Map map2 = (prevMap = base.MapHeld);
			Lord prevLord = this.GetLord();
			bool spawned = base.Spawned;
			bool spawnedOrAnyParentSpawned = base.SpawnedOrAnyParentSpawned;
			bool wasWorldPawn = this.IsWorldPawn();
			bool? flag = guilt?.IsGuilty;
			Caravan caravan = this.GetCaravan();
			bool isShambler = IsShambler;
			Building_Grave assignedGrave = null;
			if (ownership != null)
			{
				assignedGrave = ownership.AssignedGrave;
			}
			Building_Bed currentBed = this.CurrentBed();
			RemoveFromHoldingContainer(ref map, ref spawned, dinfo);
			ThingOwner thingOwner = null;
			bool inContainerEnclosed = InContainerEnclosed;
			if (inContainerEnclosed)
			{
				thingOwner = holdingOwner;
				thingOwner.Remove(this);
			}
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (Current.ProgramState == ProgramState.Playing && map != null)
			{
				flag2 = map.designationManager.DesignationOn(this, DesignationDefOf.Hunt) != null;
				flag3 = this.ShouldBeSlaughtered();
				foreach (Lord lord2 in map.lordManager.lords)
				{
					if (lord2.LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.pawnsDeathIgnored.Contains(this))
					{
						flag4 = true;
						break;
					}
				}
			}
			bool flag5 = PawnUtility.ShouldSendNotificationAbout(this) && (!(flag3 || flag4) || !dinfo.HasValue || dinfo.Value.Def != DamageDefOf.ExecutionCut) && !ForceNoDeathNotification;
			num = 2;
			DoKillSideEffects(dinfo, exactCulprit, spawned);
			num = 3;
			PreDeathPawnModifications(dinfo, map);
			num = 4;
			DropBeforeDying(dinfo, ref map, ref spawned);
			num = 5;
			health.SetDead();
			if (health.deflectionEffecter != null)
			{
				health.deflectionEffecter.Cleanup();
				health.deflectionEffecter = null;
			}
			if (health.woundedEffecter != null)
			{
				health.woundedEffecter.Cleanup();
				health.woundedEffecter = null;
			}
			caravan?.Notify_MemberDied(this);
			Lord lord = this.GetLord();
			lord?.Notify_PawnLost(this, PawnLostCondition.Killed, dinfo);
			if (ModsConfig.AnomalyActive)
			{
				Find.Anomaly.Notify_PawnDied(this);
			}
			MeditationFocusTypeAvailabilityCache.Notify_PawnDiedOrDestroyed(this);
			bool num2 = DeSpawnOrDeselect();
			if (royalty != null)
			{
				royalty.Notify_PawnKilled();
			}
			Corpse corpse = null;
			if (!PawnGenerator.IsPawnBeingGeneratedAndNotAllowsDead(this) && RaceProps.corpseDef != null)
			{
				if (inContainerEnclosed)
				{
					corpse = MakeCorpse(assignedGrave, currentBed);
					if (!thingOwner.TryAdd(corpse))
					{
						corpse.Destroy();
						corpse = null;
					}
				}
				else if (spawnedOrAnyParentSpawned)
				{
					if (holdingOwner != null)
					{
						holdingOwner.Remove(this);
					}
					corpse = MakeCorpse(assignedGrave, currentBed);
					if (GenPlace.TryPlaceThing(corpse, positionHeld, map2, ThingPlaceMode.Direct) || GenPlace.TryPlaceThing(corpse, positionHeld, map2, ThingPlaceMode.Near))
					{
						corpse.Rotation = base.Rotation;
						if (HuntJobUtility.WasKilledByHunter(this, dinfo))
						{
							((Pawn)dinfo.Value.Instigator).Reserve(corpse, ((Pawn)dinfo.Value.Instigator).CurJob);
						}
						else if (!flag2 && !flag3)
						{
							corpse.SetForbiddenIfOutsideHomeArea();
						}
						if (this.GetAttachment(ThingDefOf.Fire) is Fire fire)
						{
							FireUtility.TryStartFireIn(corpse.Position, corpse.Map, fire.CurrentSize(), fire.instigator);
						}
					}
					else
					{
						corpse.Destroy();
						corpse = null;
					}
				}
				else if (caravan != null && caravan.Spawned)
				{
					corpse = MakeCorpse(assignedGrave, currentBed);
					caravan.AddPawnOrItem(corpse, addCarriedPawnToWorldPawnsIfAny: true);
				}
				else if (holdingOwner != null || this.IsWorldPawn())
				{
					Corpse.PostCorpseDestroy(this);
				}
				else
				{
					corpse = MakeCorpse(assignedGrave, currentBed);
				}
			}
			if (spawned)
			{
				DropAndForbidEverything();
			}
			if (spawned)
			{
				GenLeaving.DoLeavingsFor(this, map, DestroyMode.KillFinalize);
			}
			if (corpse != null)
			{
				Hediff firstHediffOfDef = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				Hediff firstHediffOfDef2 = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Scaria);
				CompRottable comp = corpse.GetComp<CompRottable>();
				if (comp != null && ((firstHediffOfDef != null && Rand.Value < firstHediffOfDef.Severity) || (firstHediffOfDef2 != null && Rand.Chance(Find.Storyteller.difficulty.scariaRotChance))))
				{
					comp.RotImmediately();
				}
				if (addCorpseToLord)
				{
					lord?.AddCorpse(corpse);
				}
			}
			Drawer.renderer.SetAllGraphicsDirty();
			if (ModsConfig.AnomalyActive && kindDef == PawnKindDefOf.Revenant)
			{
				RevenantUtility.OnRevenantDeath(this, map);
			}
			duplicate?.Notify_PawnKilled();
			Drawer.renderer.SetAnimation(null);
			if (!base.Destroyed)
			{
				base.Kill(dinfo, exactCulprit);
			}
			PawnComponentsUtility.RemoveComponentsOnKilled(this);
			health.hediffSet.DirtyCache();
			PortraitsCache.SetDirty(this);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(this);
			if (num2 && corpse != null && !corpse.Destroyed)
			{
				Find.Selector.Select(corpse, playSound: false, forceDesignatorDeselect: false);
			}
			num = 6;
			health.hediffSet.Notify_PawnDied(dinfo, exactCulprit);
			if (IsMutant)
			{
				mutant.Notify_Died(corpse, dinfo, exactCulprit);
			}
			genes?.Notify_PawnDied(dinfo, exactCulprit);
			HomeFaction?.Notify_MemberDied(this, dinfo, wasWorldPawn, flag == true, map2);
			if (corpse != null)
			{
				if (RaceProps.DeathActionWorker != null && spawned && !isShambler)
				{
					RaceProps.DeathActionWorker.PawnDied(corpse, prevLord);
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_PawnDied(corpse);
				}
			}
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				BillUtility.Notify_ColonistUnavailable(this);
			}
			if (spawnedOrAnyParentSpawned)
			{
				GenHostility.Notify_PawnLostForTutor(this, map2);
			}
			if (base.Faction != null && base.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			psychicEntropy?.Notify_PawnDied();
			try
			{
				Ideo?.Notify_MemberDied(this);
				Ideo?.Notify_MemberLost(this, map);
			}
			catch (Exception ex)
			{
				Log.Error("Error while notifying ideo of pawn death: " + ex);
			}
			if (IsMutant && mutant.Def.clearMutantStatusOnDeath)
			{
				if (mutant.HasTurned)
				{
					mutant.Revert(beingKilled: true);
				}
				else
				{
					mutant = null;
				}
			}
			if (flag5)
			{
				health.NotifyPlayerOfKilled(dinfo, exactCulprit, caravan);
			}
			Find.QuestManager.Notify_PawnKilled(this, dinfo);
			Find.FactionManager.Notify_PawnKilled(this);
			Find.IdeoManager.Notify_PawnKilled(this);
			if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(this))
			{
				Find.History.Notify_MechanitorDied();
			}
			Notify_DisabledWorkTypesChanged();
			Find.BossgroupManager.Notify_PawnKilled(this);
			if (IsCreepJoiner)
			{
				creepjoiner.Notify_CreepJoinerKilled();
			}
			prevMap = null;
			health.isBeingKilled = false;
		}
		catch (Exception arg)
		{
			Log.Error($"Error while killing {this.ToStringSafe()} during phase {num}: {arg}");
		}
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.Vanish && mode != DestroyMode.KillFinalize)
		{
			Log.Error("Destroyed pawn " + this?.ToString() + " with unsupported mode " + mode.ToString() + ".");
		}
		_ = base.MapHeld;
		base.Destroy(mode);
		Find.WorldPawns.Notify_PawnDestroyed(this);
		if (ownership != null)
		{
			Building_Grave assignedGrave = ownership.AssignedGrave;
			ownership.UnclaimAll();
			if (mode == DestroyMode.KillFinalize)
			{
				assignedGrave?.CompAssignableToPawn.TryAssignPawn(this);
			}
		}
		ClearMind_NewTemp(ifLayingKeepLaying: false, clearInspiration: true);
		Lord lord = this.GetLord();
		if (lord != null)
		{
			PawnLostCondition cond = ((mode != DestroyMode.KillFinalize) ? PawnLostCondition.Vanished : PawnLostCondition.Killed);
			lord.Notify_PawnLost(this, cond);
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.GameEnder.CheckOrUpdateGameOver();
			Find.TaleManager.Notify_PawnDestroyed(this);
		}
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive.Where((Pawn p) => p.playerSettings != null && p.playerSettings.Master == this))
		{
			item.playerSettings.Master = null;
		}
		equipment?.Notify_PawnDied();
		if (ModsConfig.AnomalyActive && Find.Anomaly != null)
		{
			Find.Anomaly.Notify_PawnDied(this);
		}
		if (mode != DestroyMode.KillFinalize)
		{
			equipment?.DestroyAllEquipment();
			inventory?.DestroyAll();
			apparel?.DestroyAll();
		}
		WorldPawns worldPawns = Find.WorldPawns;
		if (!worldPawns.IsBeingDiscarded(this) && !worldPawns.Contains(this))
		{
			worldPawns.PassToWorld(this);
		}
		if (base.Faction.IsPlayerSafe())
		{
			Ideo?.RecacheColonistBelieverCount();
		}
		relations?.Notify_PawnDestroyed(mode);
		MeditationFocusTypeAvailabilityCache.Notify_PawnDiedOrDestroyed(this);
		Drawer?.renderer?.renderTree?.SetDirty();
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		if (jobs?.curJob != null)
		{
			jobs.StopAll();
		}
		base.DeSpawn(mode);
		pather?.StopDead();
		roping?.Notify_DeSpawned();
		mindState.droppedWeapon = null;
		needs?.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
		meleeVerbs?.Notify_PawnDespawned();
		mechanitor?.Notify_DeSpawned(mode);
		MeditationFocusTypeAvailabilityCache.Notify_PawnDiedOrDestroyed(this);
		ClearAllReservations(releaseDestinationsOnlyIfObsolete: false);
		if (map != null)
		{
			map.mapPawns.DeRegisterPawn(this);
			map.autoSlaughterManager.Notify_PawnDespawned();
		}
		PawnComponentsUtility.RemoveComponentsOnDespawned(this);
		if (sustainerAmbient != null)
		{
			sustainerAmbient.End();
			sustainerAmbient = null;
		}
		if (sustainerMoving != null)
		{
			sustainerMoving.End();
			sustainerMoving = null;
		}
	}

	public override void Discard(bool silentlyRemoveReferences = false)
	{
		if (Find.WorldPawns.Contains(this))
		{
			Log.Warning("Tried to discard a world pawn " + this?.ToString() + ".");
			return;
		}
		base.Discard(silentlyRemoveReferences);
		if (relations != null)
		{
			if (RaceProps.Humanlike && relations.Children.Count((Pawn x) => !x.markedForDiscard) > 1)
			{
				foreach (Pawn child in relations.Children)
				{
					if (!child.markedForDiscard)
					{
						DirectPawnRelation directRelation = child.relations.GetDirectRelation(PawnRelationDefOf.Parent, this);
						child.relations.ElevateToVirtualRelation(directRelation);
					}
				}
			}
			relations.ClearAllRelations();
		}
		if (pather != null)
		{
			pather.DisposeAndClearCurPathRequest();
			pather.DisposeAndClearCurPath();
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			Find.PlayLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
			Find.BattleLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
			Find.TaleManager.Notify_PawnDiscarded(this, silentlyRemoveReferences);
			Find.QuestManager.Notify_PawnDiscarded(this);
		}
		foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive)
		{
			item.needs?.mood?.thoughts.memories.Notify_PawnDiscarded(this);
		}
		Corpse.PostCorpseDestroy(this, discarded: true);
	}

	public Corpse MakeCorpse(Building_Grave assignedGrave, Building_Bed currentBed)
	{
		return MakeCorpse(assignedGrave, currentBed != null, currentBed?.Rotation.AsAngle ?? 0f);
	}

	public Corpse MakeCorpse(Building_Grave assignedGrave, bool inBed, float bedRotation)
	{
		if (holdingOwner != null)
		{
			Log.Warning("We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" + base.ParentHolder);
			return null;
		}
		if (RaceProps.corpseDef == null)
		{
			return null;
		}
		Corpse corpse = (Corpse)ThingMaker.MakeThing(RaceProps.corpseDef);
		corpse.InnerPawn = this;
		if (assignedGrave != null)
		{
			corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);
		}
		if (inBed)
		{
			corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);
		}
		return corpse;
	}

	public virtual void ExitMap(bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
	{
		if (this.IsWorldPawn())
		{
			Log.Warning("Called ExitMap() on world pawn " + this);
			return;
		}
		Ideo?.Notify_MemberLost(this, base.Map);
		if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this))
		{
			CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan(this, exitDir);
			return;
		}
		this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
		if (carryTracker?.CarriedThing != null)
		{
			Pawn pawn = carryTracker.CarriedThing as Pawn;
			if (pawn != null)
			{
				if (base.Faction != null && base.Faction != pawn.Faction)
				{
					base.Faction.kidnapped.Kidnap(pawn, this);
				}
				else
				{
					if (!teleporting)
					{
						carryTracker.innerContainer.Remove(pawn);
					}
					pawn.teleporting = teleporting;
					pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
					pawn.teleporting = false;
				}
			}
			else
			{
				carryTracker.CarriedThing.Destroy();
			}
			if (!teleporting || pawn == null)
			{
				carryTracker.innerContainer.Clear();
			}
		}
		bool flag = ThingOwnerUtility.AnyParentIs<ActiveTransporterInfo>(this) || ThingOwnerUtility.AnyParentIs<TravellingTransporters>(this);
		bool flag2 = this.IsCaravanMember() || teleporting || flag;
		bool flag3 = !flag2 || (!IsPrisoner && !IsSlave && !flag) || (guest != null && guest.Released);
		bool flag4 = flag3 && (IsPrisoner || IsSlave) && guest != null && guest.Released;
		bool flag5 = flag4 || (guest != null && guest.HostFaction == Faction.OfPlayer);
		if (flag3 && !flag2)
		{
			foreach (Thing equippedWornOrInventoryThing in EquippedWornOrInventoryThings)
			{
				equippedWornOrInventoryThing.GetStyleSourcePrecept()?.Notify_ThingLost(equippedWornOrInventoryThing);
			}
		}
		base.Faction?.Notify_MemberExitedMap(this, flag4);
		if (base.Faction == Faction.OfPlayer && IsSlave && SlaveFaction != null && SlaveFaction != Faction.OfPlayer && guest.Released)
		{
			SlaveFaction.Notify_MemberExitedMap(this, flag4);
		}
		if (ownership != null && flag5)
		{
			ownership.UnclaimAll();
		}
		if (guest != null)
		{
			bool isPrisonerOfColony = IsPrisonerOfColony;
			if (flag4)
			{
				guest.SetGuestStatus(null);
			}
			if (isPrisonerOfColony)
			{
				guest.SetNoInteraction();
				if (!guest.Released && flag3)
				{
					GuestUtility.Notify_PrisonerEscaped(this);
				}
			}
			guest.Released = false;
		}
		DeSpawnOrDeselect();
		inventory.UnloadEverything = false;
		if (flag3)
		{
			ClearMind_NewTemp();
		}
		relations?.Notify_ExitedMap();
		Find.WorldPawns.PassToWorld(this);
		QuestUtility.SendQuestTargetSignals(questTags, "LeftMap", this.Named("SUBJECT"));
		Find.FactionManager.Notify_PawnLeftMap(this);
		Find.IdeoManager.Notify_PawnLeftMap(this);
	}

	public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
	{
		base.PreTraded(action, playerNegotiator, trader);
		if (base.SpawnedOrAnyParentSpawned)
		{
			DropAndForbidEverything();
		}
		ownership?.UnclaimAll();
		if (action == TradeAction.PlayerSells)
		{
			Faction faction = this.GetExtraHomeFaction() ?? this.GetExtraHostFaction();
			if (faction != null && faction != Faction.OfPlayer)
			{
				Faction.OfPlayer.TryAffectGoodwillWith(faction, Faction.OfPlayer.GoodwillToMakeHostile(faction), canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.MemberSold, this);
			}
		}
		guest?.SetGuestStatus(null);
		switch (action)
		{
		case TradeAction.PlayerBuys:
			if (guest != null && guest.joinStatus == JoinStatus.JoinAsSlave)
			{
				guest.SetGuestStatus(Faction.OfPlayer, RimWorld.GuestStatus.Slave);
				break;
			}
			needs.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.FreedFromSlavery);
			SetFaction(Faction.OfPlayer);
			break;
		case TradeAction.PlayerSells:
			if (RaceProps.Humanlike)
			{
				TaleRecorder.RecordTale(TaleDefOf.SoldPrisoner, playerNegotiator, this, trader);
			}
			if (base.Faction != null)
			{
				SetFaction(null);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnSold(playerNegotiator);
			}
			break;
		}
		ClearMind_NewTemp();
	}

	public void PreKidnapped(Pawn kidnapper)
	{
		Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Kidnapped);
		if (IsColonist && kidnapper != null)
		{
			TaleRecorder.RecordTale(TaleDefOf.KidnappedColonist, kidnapper, this);
		}
		ownership?.UnclaimAll();
		if (guest != null && !guest.IsSlave)
		{
			guest.SetGuestStatus(null);
		}
		if (RaceProps.IsFlesh)
		{
			relations.Notify_PawnKidnapped();
		}
		ClearMind_NewTemp();
	}

	public override AcceptanceReport ClaimableBy(Faction by)
	{
		return false;
	}

	public override bool AdoptableBy(Faction by, StringBuilder reason = null)
	{
		if (base.Faction == by)
		{
			return false;
		}
		Pawn_AgeTracker pawn_AgeTracker = ageTracker;
		if (pawn_AgeTracker != null && pawn_AgeTracker.CurLifeStage?.claimable == false)
		{
			return false;
		}
		if (FactionPreventsClaimingOrAdopting(base.Faction, forClaim: false, out var reason2))
		{
			reason?.Append(reason2);
			return false;
		}
		return true;
	}

	public override void SetFaction(Faction newFaction, Pawn recruiter = null)
	{
		if (newFaction == base.Faction)
		{
			Log.Warning("Used SetFaction to change " + this.ToStringSafe() + " to same faction " + newFaction.ToStringSafe());
			return;
		}
		Faction faction = base.Faction;
		guest?.SetGuestStatus(null);
		if (base.Spawned)
		{
			base.Map.mapPawns.DeRegisterPawn(this);
			base.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
			base.Map.designationManager.RemoveAllDesignationsOn(this);
			base.Map.autoSlaughterManager.Notify_PawnChangedFaction();
		}
		if ((newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer) && Current.ProgramState == ProgramState.Playing)
		{
			Find.ColonistBar.MarkColonistsDirty();
		}
		this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
		if (PawnUtility.IsFactionLeader(this))
		{
			Faction factionLeaderFaction = PawnUtility.GetFactionLeaderFaction(this);
			if (newFaction != factionLeaderFaction && !this.HasExtraHomeFaction(factionLeaderFaction) && !this.HasExtraMiniFaction(factionLeaderFaction))
			{
				factionLeaderFaction.Notify_LeaderLost();
			}
		}
		if (newFaction == Faction.OfPlayer && RaceProps.Humanlike && !this.IsQuestLodger())
		{
			ChangeKind(newFaction.def.basicMemberKind);
		}
		base.SetFaction(newFaction);
		PawnComponentsUtility.AddAndRemoveDynamicComponents(this);
		if (base.Faction != null && base.Faction.IsPlayer)
		{
			workSettings?.EnableAndInitialize();
			Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(this, PopAdaptationEvent.GainedColonist);
		}
		if (Drafted)
		{
			drafter.Drafted = false;
		}
		ReachabilityUtility.ClearCacheFor(this);
		health.surgeryBills.Clear();
		if (base.Spawned)
		{
			base.Map.mapPawns.RegisterPawn(this);
		}
		GenerateNecessaryName();
		playerSettings?.ResetMedicalCare();
		ClearMind_NewTemp(ifLayingKeepLaying: true);
		if (!Dead && needs.mood != null)
		{
			needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
		}
		if (base.Spawned)
		{
			base.Map.attackTargetsCache.UpdateTarget(this);
		}
		Find.GameEnder.CheckOrUpdateGameOver();
		AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
		needs?.AddOrRemoveNeedsAsAppropriate();
		playerSettings?.Notify_FactionChanged();
		relations?.Notify_ChangedFaction();
		if (IsAnimal && newFaction == Faction.OfPlayer)
		{
			training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
			training.Train(TrainableDefOf.Tameness, recruiter, complete: true);
			if (Roamer && mindState != null)
			{
				mindState.lastStartRoamCooldownTick = Find.TickManager.TicksGame;
			}
		}
		if (faction == Faction.OfPlayer)
		{
			BillUtility.Notify_ColonistUnavailable(this);
		}
		if (newFaction == Faction.OfPlayer)
		{
			Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
			Find.World.StoryState.RecordPopulationIncrease();
		}
		newFaction?.Notify_PawnJoined(this);
		Ideo?.Notify_MemberChangedFaction(this, faction, newFaction);
		ageTracker?.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.Recruited);
		roping?.BreakAllRopes();
		if (ModsConfig.BiotechActive)
		{
			mechanitor?.Notify_ChangedFaction();
		}
		creepjoiner?.Notify_ChangedFaction();
		if (faction != null)
		{
			Find.FactionManager.Notify_PawnLeftFaction(faction);
		}
	}

	[Obsolete]
	public void ClearMind(bool ifLayingKeepLaying = false, bool clearInspiration = false, bool clearMentalState = true)
	{
		ClearMind_NewTemp(ifLayingKeepLaying, clearInspiration, clearMentalState);
	}

	public void ClearMind_NewTemp(bool ifLayingKeepLaying = false, bool clearInspiration = false, bool clearMentalState = true, bool wasDowned = false)
	{
		pather?.StopDead();
		mindState?.Reset(clearInspiration, clearMentalState, wasDowned);
		jobs?.StopAll(ifLayingKeepLaying);
		VerifyReservations();
	}

	public void ClearAllReservations(bool releaseDestinationsOnlyIfObsolete = true)
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			if (releaseDestinationsOnlyIfObsolete)
			{
				maps[i].pawnDestinationReservationManager.ReleaseAllObsoleteClaimedBy(this);
			}
			else
			{
				maps[i].pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
			}
			maps[i].reservationManager.ReleaseAllClaimedBy(this);
			maps[i].enrouteManager.ReleaseAllClaimedBy(this);
			maps[i].physicalInteractionReservationManager.ReleaseAllClaimedBy(this);
			maps[i].attackTargetReservationManager.ReleaseAllClaimedBy(this);
		}
	}

	public void ClearReservationsForJob(Job job)
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			maps[i].pawnDestinationReservationManager.ReleaseClaimedBy(this, job);
			maps[i].reservationManager.ReleaseClaimedBy(this, job);
			maps[i].enrouteManager.ReleaseAllClaimedBy(this);
			maps[i].physicalInteractionReservationManager.ReleaseClaimedBy(this, job);
			maps[i].attackTargetReservationManager.ReleaseClaimedBy(this, job);
		}
	}

	public void VerifyReservations(Job prevJob = null)
	{
		if (jobs == null || CurJob != null || jobs.jobQueue.Count > 0 || jobs.startingNewJob)
		{
			return;
		}
		bool flag = false;
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			LocalTargetInfo obj = maps[i].reservationManager.FirstReservationFor(this);
			if (obj.IsValid)
			{
				Log.ErrorOnce($"Reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj.ToStringSafe()}, prev job: {prevJob}", 0x5D3DFA5 ^ thingIDNumber);
				flag = true;
			}
			LocalTargetInfo obj2 = maps[i].physicalInteractionReservationManager.FirstReservationFor(this);
			if (obj2.IsValid)
			{
				Log.ErrorOnce("Physical interaction reservation manager failed to clean up properly; " + this.ToStringSafe() + " still reserving " + obj2.ToStringSafe() + ", prev job: {prevJob}", 0x12ADECD ^ thingIDNumber);
				flag = true;
			}
			IAttackTarget attackTarget = maps[i].attackTargetReservationManager.FirstReservationFor(this);
			if (attackTarget != null)
			{
				Log.ErrorOnce("Attack target reservation manager failed to clean up properly; " + this.ToStringSafe() + " still reserving " + attackTarget.ToStringSafe() + ", prev job: {prevJob}", 0x5FD7206 ^ thingIDNumber);
				flag = true;
			}
			IntVec3 obj3 = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationFor(this);
			if (obj3.IsValid)
			{
				Job job = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationJobFor(this);
				Log.ErrorOnce("Pawn destination reservation manager failed to clean up properly; " + this.ToStringSafe() + "/" + job.ToStringSafe() + "/" + job.def.ToStringSafe() + " still reserving " + obj3.ToStringSafe() + ", prev job: {prevJob}", 0x1DE312 ^ thingIDNumber);
				flag = true;
			}
		}
		if (flag)
		{
			ClearAllReservations();
		}
	}

	public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false, bool rememberPrimary = false)
	{
		if (kindDef.destroyGearOnDrop)
		{
			equipment.DestroyAllEquipment();
			apparel.DestroyAll();
		}
		if (InContainerEnclosed)
		{
			if (carryTracker?.CarriedThing != null)
			{
				carryTracker.innerContainer.TryTransferToContainer(carryTracker.CarriedThing, holdingOwner);
			}
			if (equipment?.Primary != null)
			{
				equipment.TryTransferEquipmentToContainer(equipment.Primary, holdingOwner);
			}
			inventory?.innerContainer.TryTransferAllToContainer(holdingOwner);
		}
		else
		{
			if (!base.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			if (carryTracker?.CarriedThing != null)
			{
				carryTracker.TryDropCarriedThing(base.PositionHeld, ThingPlaceMode.Near, out var _);
			}
			if (!keepInventoryAndEquipmentIfInBed || !this.InBed())
			{
				equipment?.DropAllEquipment(base.PositionHeld, forbid: true, rememberPrimary);
				if (inventory != null && inventory.innerContainer.TotalStackCount > 0)
				{
					inventory.DropAllNearPawn(base.PositionHeld, forbid: true);
				}
			}
		}
	}

	public void GenerateNecessaryName()
	{
		if (Name == null && base.Faction == Faction.OfPlayer && (RaceProps.Animal || (ModsConfig.BiotechActive && RaceProps.IsMechanoid)))
		{
			Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
		}
	}

	public Verb TryGetAttackVerb(Thing target, bool allowManualCastWeapons = false, bool allowTurrets = false)
	{
		if (equipment?.Primary != null && equipment.PrimaryEq.PrimaryVerb.Available() && (!equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast || (CurJob != null && CurJob.def != JobDefOf.Wait_Combat) || allowManualCastWeapons))
		{
			return equipment.PrimaryEq.PrimaryVerb;
		}
		if (allowManualCastWeapons && apparel != null)
		{
			Verb firstApparelVerb = apparel.FirstApparelVerb;
			if (firstApparelVerb != null && firstApparelVerb.Available())
			{
				return firstApparelVerb;
			}
		}
		if (allowTurrets)
		{
			List<ThingComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				if (allComps[i] is CompTurretGun { TurretDestroyed: false } compTurretGun && compTurretGun.GunCompEq.PrimaryVerb.Available())
				{
					return compTurretGun.GunCompEq.PrimaryVerb;
				}
			}
		}
		if (kindDef.canMeleeAttack)
		{
			return meleeVerbs.TryGetMeleeVerb(target);
		}
		return null;
	}

	public bool TryStartAttack(LocalTargetInfo targ)
	{
		if (stances.FullBodyBusy)
		{
			return false;
		}
		if (WorkTagIsDisabled(WorkTags.Violent))
		{
			return false;
		}
		bool allowManualCastWeapons = !IsColonist;
		Verb verb = TryGetAttackVerb(targ.Thing, allowManualCastWeapons);
		return verb?.TryStartCastOn(verb.verbProps.ai_RangedAlawaysShootGroundBelowTarget ? ((LocalTargetInfo)targ.Cell) : targ) ?? false;
	}

	public override IEnumerable<Thing> ButcherProducts(Pawn butcher, float efficiency)
	{
		if (RaceProps.meatDef != null)
		{
			int num = GenMath.RoundRandom(this.GetStatValue(StatDefOf.MeatAmount) * efficiency);
			if (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(RaceProps.meatDef);
				thing.stackCount = num;
				yield return thing;
			}
		}
		foreach (Thing item in base.ButcherProducts(butcher, efficiency))
		{
			yield return item;
		}
		if (RaceProps.leatherDef != null)
		{
			int num2 = GenMath.RoundRandom(this.GetStatValue(StatDefOf.LeatherAmount) * efficiency);
			if (num2 > 0)
			{
				Thing thing2 = ThingMaker.MakeThing(RaceProps.leatherDef);
				thing2.stackCount = num2;
				yield return thing2;
			}
		}
		if (RaceProps.Humanlike)
		{
			yield break;
		}
		PawnKindLifeStage lifeStage = ageTracker.CurKindLifeStage;
		if (lifeStage.butcherBodyPart == null || (gender != Gender.None && (gender != Gender.Male || !lifeStage.butcherBodyPart.allowMale) && (gender != Gender.Female || !lifeStage.butcherBodyPart.allowFemale)))
		{
			yield break;
		}
		while (true)
		{
			BodyPartRecord bodyPartRecord = health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.IsInGroup(lifeStage.butcherBodyPart.bodyPartGroup));
			if (bodyPartRecord != null)
			{
				health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this, bodyPartRecord));
				yield return ThingMaker.MakeThing(lifeStage.butcherBodyPart.thing ?? bodyPartRecord.def.spawnThingOnRemoved);
				continue;
			}
			break;
		}
	}

	public TaggedString FactionDesc(TaggedString name, bool extraFactionsInfo, string nameLabel, string genderLabel)
	{
		tmpExtraFactions.Clear();
		QuestUtility.GetExtraFactionsFromQuestParts(this, tmpExtraFactions);
		GuestUtility.GetExtraFactionsFromGuestStatus(this, tmpExtraFactions);
		TaggedString result = ((base.Faction == null || base.Faction.Hidden) ? name : ((tmpExtraFactions.Count != 0 || SlaveFaction != null) ? "PawnMainDescUnderFactionedWrap".Translate(name, base.Faction.NameColored) : "PawnMainDescFactionedWrap".Translate(name, base.Faction.NameColored, nameLabel.Named("NAME"), genderLabel.Named("GENDER"))));
		if (extraFactionsInfo)
		{
			for (int i = 0; i < tmpExtraFactions.Count; i++)
			{
				if (base.Faction != tmpExtraFactions[i].faction && !tmpExtraFactions[i].faction.Hidden)
				{
					result += "\n" + tmpExtraFactions[i].factionType.GetLabel().CapitalizeFirst() + ": " + tmpExtraFactions[i].faction.NameColored.Resolve();
				}
			}
		}
		tmpExtraFactions.Clear();
		return result;
	}

	public string MainDesc(bool writeFaction, bool writeGender = true)
	{
		bool flag = base.Faction == null || !base.Faction.IsPlayer;
		string text = ((!writeGender) ? string.Empty : ((gender == Gender.None) ? string.Empty : gender.GetLabel(this.AnimalOrWildMan())));
		string text2 = string.Empty;
		if (RaceProps.Animal || RaceProps.IsMechanoid)
		{
			text2 = GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
			if (Name != null)
			{
				if (!text.NullOrEmpty())
				{
					text += " ";
				}
				text += text2;
			}
		}
		if (ageTracker != null)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += "AgeIndicator".Translate(ageTracker.AgeNumberString);
		}
		if (IsMutant && mutant.HasTurned && mutant.Def.overrideLabel)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text += mutant.Def.label;
		}
		else if (!RaceProps.Animal && !RaceProps.IsMechanoid && flag && !IsCreepJoiner)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text2 = GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
			text += text2;
		}
		if (writeFaction)
		{
			text = FactionDesc(text, extraFactionsInfo: true, text2, gender.GetLabel(RaceProps.Animal)).Resolve();
		}
		return text.CapitalizeFirst();
	}

	public string GetJobReport()
	{
		try
		{
			return (this.GetLord()?.LordJob?.GetJobReport(this) ?? jobs?.curDriver?.GetReport())?.CapitalizeFirst();
		}
		catch (Exception ex)
		{
			Log.Error("JobDriver.GetReport() exception: " + ex);
			return null;
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!def.hideMainDesc)
		{
			stringBuilder.AppendLine(MainDesc(PawnUtility.ShouldDisplayFactionInInspectString(this)));
		}
		RoyalTitle royalTitle = royalty?.MostSeniorTitle;
		if (royalTitle != null)
		{
			stringBuilder.AppendLine("PawnTitleDescWrap".Translate(royalTitle.def.GetLabelCapFor(this), royalTitle.faction.NameColored).Resolve());
		}
		string inspectString = base.GetInspectString();
		if (!inspectString.NullOrEmpty())
		{
			stringBuilder.AppendLine(inspectString);
		}
		if (TraderKind != null)
		{
			stringBuilder.AppendLine(TraderKind.LabelCap);
		}
		if (InMentalState)
		{
			string inspectLine = MentalState.InspectLine;
			if (!string.IsNullOrEmpty(inspectLine))
			{
				stringBuilder.AppendLine(inspectLine);
			}
		}
		states.Clear();
		if (health?.hediffSet != null)
		{
			List<Hediff> hediffs = health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff hediff = hediffs[i];
				if (!hediff.def.battleStateLabel.NullOrEmpty())
				{
					states.AddUnique(hediff.def.battleStateLabel);
				}
				string inspectString2 = hediff.GetInspectString();
				if (!inspectString2.NullOrEmpty())
				{
					stringBuilder.AppendLine(inspectString2);
				}
			}
		}
		if (states.Count > 0)
		{
			states.Sort();
			stringBuilder.AppendLine(string.Format("{0}: {1}", "State".Translate(), states.ToCommaList().CapitalizeFirst()));
			states.Clear();
		}
		string text = flight?.GetStatusString();
		if (!text.NullOrEmpty())
		{
			stringBuilder.AppendLine(text);
		}
		if (stances?.stunner != null && stances.stunner.Stunned)
		{
			if (stances.stunner.Hypnotized)
			{
				stringBuilder.AppendLine("InTrance".Translate());
			}
			else if (stances.stunner.StunFromEMP)
			{
				stringBuilder.AppendLine("StunnedByEMP".Translate() + ": " + stances.stunner.StunTicksLeft.ToStringSecondsFromTicks());
			}
			else
			{
				stringBuilder.AppendLine("StunLower".Translate().CapitalizeFirst() + ": " + stances.stunner.StunTicksLeft.ToStringSecondsFromTicks());
			}
		}
		if (stances?.stagger != null && stances.stagger.Staggered)
		{
			stringBuilder.AppendLine("SlowedByDamage".Translate() + ": " + stances.stagger.StaggerTicksLeft.ToStringSecondsFromTicks());
		}
		if (Inspired)
		{
			stringBuilder.AppendLine(Inspiration.InspectLine);
		}
		if (equipment?.Primary != null)
		{
			stringBuilder.AppendLine("Equipped".TranslateSimple() + ": " + ((equipment.Primary != null) ? equipment.Primary.Label : "EquippedNothing".TranslateSimple()).CapitalizeFirst());
		}
		if (abilities != null)
		{
			for (int j = 0; j < abilities.AllAbilitiesForReading.Count; j++)
			{
				string inspectString3 = abilities.AllAbilitiesForReading[j].GetInspectString();
				if (!inspectString3.NullOrEmpty())
				{
					stringBuilder.AppendLine(inspectString3);
				}
			}
		}
		if (carryTracker?.CarriedThing != null && (CurJob == null || CurJob.showCarryingInspectLine))
		{
			stringBuilder.Append("Carrying".Translate() + ": ");
			stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
		}
		Pawn_RopeTracker pawn_RopeTracker = roping;
		if (pawn_RopeTracker != null && pawn_RopeTracker.IsRoped)
		{
			stringBuilder.AppendLine(roping.InspectLine);
		}
		if (ModsConfig.BiotechActive && IsColonyMech && needs.energy != null)
		{
			TaggedString taggedString = "MechEnergy".Translate() + ": " + needs.energy.CurLevelPercentage.ToStringPercent();
			float maxLevel = needs.energy.MaxLevel;
			if (this.IsCharging())
			{
				taggedString += " (+" + "PerDay".Translate((50f / maxLevel).ToStringPercent()) + ")";
			}
			else if (this.IsSelfShutdown())
			{
				taggedString += " (+" + "PerDay".Translate((1f / maxLevel).ToStringPercent()) + ")";
			}
			else
			{
				taggedString += " (-" + "PerDay".Translate((needs.energy.FallPerDay / maxLevel).ToStringPercent()) + ")";
			}
			stringBuilder.AppendLine(taggedString);
		}
		string text2 = null;
		if (PawnUtility.ShouldDisplayLordReport(this))
		{
			Lord lord = this.GetLord();
			if (lord?.LordJob != null)
			{
				text2 = lord.LordJob.GetReport(this);
			}
		}
		if (PawnUtility.ShouldDisplayJobReport(this))
		{
			string jobReport = GetJobReport();
			if (text2.NullOrEmpty())
			{
				text2 = jobReport;
			}
			else if (!jobReport.NullOrEmpty())
			{
				text2 = text2 + ": " + jobReport;
			}
		}
		if (!text2.NullOrEmpty())
		{
			stringBuilder.AppendLine(text2.CapitalizeFirst().EndWithPeriod());
		}
		if (jobs?.curJob != null)
		{
			Pawn_JobTracker pawn_JobTracker = jobs;
			if (pawn_JobTracker != null && pawn_JobTracker.jobQueue.Count > 0)
			{
				try
				{
					string text3 = jobs.jobQueue[0].job.GetReport(this).CapitalizeFirst();
					if (jobs.jobQueue.Count > 1)
					{
						text3 = text3 + " (+" + (jobs.jobQueue.Count - 1) + ")";
					}
					stringBuilder.AppendLine("Queued".Translate() + ": " + text3);
				}
				catch (Exception ex)
				{
					Log.Error("JobDriver.GetReport() exception: " + ex);
				}
			}
		}
		if (IsMutant && mutant.Def.overrideInspectString)
		{
			string inspectString4 = mutant.GetInspectString();
			if (!inspectString4.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString4);
			}
		}
		if (ModsConfig.AnomalyActive)
		{
			if (health?.hediffSet != null)
			{
				Hediff_MetalhorrorImplant firstHediff = health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
				if (firstHediff != null && firstHediff.Emerging)
				{
					stringBuilder.AppendLine("Emerging".Translate());
				}
			}
			if (IsCreepJoiner)
			{
				string inspectString5 = creepjoiner.GetInspectString();
				if (!inspectString5.NullOrEmpty())
				{
					stringBuilder.AppendLine(inspectString5);
				}
			}
		}
		if (ModsConfig.BiotechActive && needs?.energy != null && needs.energy.IsLowEnergySelfShutdown)
		{
			stringBuilder.AppendLine("MustBeCarriedToRecharger".Translate());
		}
		if (RestraintsUtility.ShouldShowRestraintsInfo(this))
		{
			stringBuilder.AppendLine("InRestraints".Translate());
		}
		if (guest != null && !guest.Recruitable && !IsSubhuman && !IsCreepJoiner)
		{
			if (base.Faction == null)
			{
				stringBuilder.AppendLine("UnrecruitableNoFaction".Translate().CapitalizeFirst());
			}
			else if (base.Faction != Faction.OfPlayer || IsSlaveOfColony || IsPrisonerOfColony)
			{
				stringBuilder.AppendLine("Unrecruitable".Translate().CapitalizeFirst());
			}
		}
		if (Prefs.DevMode && DebugSettings.showLocomotionUrgency && CurJob != null)
		{
			stringBuilder.AppendLine("Locomotion Urgency: " + CurJob.locomotionUrgency);
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (IsColonistPlayerControlled || IsColonyMech || IsColonySubhumanPlayerControlled)
		{
			AcceptanceReport allowsDrafting = this.GetLord()?.AllowsDrafting(this) ?? ((AcceptanceReport)true);
			if (drafter != null)
			{
				foreach (Gizmo gizmo2 in drafter.GetGizmos())
				{
					if (!allowsDrafting && !gizmo2.Disabled)
					{
						gizmo2.Disabled = true;
						gizmo2.disabledReason = allowsDrafting.Reason;
					}
					yield return gizmo2;
				}
			}
			foreach (Gizmo attackGizmo in PawnAttackGizmoUtility.GetAttackGizmos(this))
			{
				if (!allowsDrafting && !attackGizmo.Disabled)
				{
					attackGizmo.Disabled = true;
					attackGizmo.disabledReason = allowsDrafting.Reason;
				}
				yield return attackGizmo;
			}
		}
		if (equipment != null)
		{
			foreach (Gizmo gizmo3 in equipment.GetGizmos())
			{
				yield return gizmo3;
			}
		}
		if (carryTracker != null)
		{
			foreach (Gizmo gizmo4 in carryTracker.GetGizmos())
			{
				yield return gizmo4;
			}
		}
		if (needs != null)
		{
			foreach (Gizmo gizmo5 in needs.GetGizmos())
			{
				yield return gizmo5;
			}
		}
		if (Find.Selector.SingleSelectedThing == this && psychicEntropy != null && psychicEntropy.NeedToShowGizmo())
		{
			yield return psychicEntropy.GetGizmo();
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Psyfocus -20%",
					action = delegate
					{
						psychicEntropy.OffsetPsyfocusDirectly(-0.2f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Psyfocus +20%",
					action = delegate
					{
						psychicEntropy.OffsetPsyfocusDirectly(0.2f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Neural heat -20",
					action = delegate
					{
						psychicEntropy.TryAddEntropy(-20f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Neural heat +20",
					action = delegate
					{
						psychicEntropy.TryAddEntropy(20f);
					}
				};
			}
		}
		if (ModsConfig.BiotechActive)
		{
			if (MechanitorUtility.IsMechanitor(this))
			{
				foreach (Gizmo gizmo6 in mechanitor.GetGizmos())
				{
					yield return gizmo6;
				}
			}
			if (RaceProps.IsMechanoid)
			{
				foreach (Gizmo mechGizmo in MechanitorUtility.GetMechGizmos(this))
				{
					yield return mechGizmo;
				}
			}
			if (RaceProps.Humanlike && ageTracker.AgeBiologicalYears < 13 && !Drafted && Find.Selector.SelectedPawns.Count < 2 && DevelopmentalStage.Child())
			{
				yield return new Gizmo_GrowthTier(this);
				if (DebugSettings.ShowDevGizmos)
				{
					yield return new Command_Action
					{
						defaultLabel = "DEV: Set growth tier",
						action = delegate
						{
							List<FloatMenuOption> list = new List<FloatMenuOption>();
							for (int i = 0; i < GrowthUtility.GrowthTiers.Length; i++)
							{
								int tier = i;
								list.Add(new FloatMenuOption(tier.ToString(), delegate
								{
									ageTracker.growthPoints = GrowthUtility.GrowthTiers[tier].pointsRequirement;
								}));
							}
							Find.WindowStack.Add(new FloatMenu(list));
						}
					};
				}
			}
		}
		if (IsMutant)
		{
			foreach (Gizmo gizmo7 in mutant.GetGizmos())
			{
				yield return gizmo7;
			}
		}
		if (ModsConfig.AnomalyActive && IsCreepJoiner)
		{
			foreach (Gizmo gizmo8 in creepjoiner.GetGizmos())
			{
				yield return gizmo8;
			}
		}
		if (abilities != null)
		{
			foreach (Gizmo gizmo9 in abilities.GetGizmos())
			{
				yield return gizmo9;
			}
		}
		if (IsColonistPlayerControlled || IsColonyMech || IsPrisonerOfColony)
		{
			if (playerSettings != null)
			{
				foreach (Gizmo gizmo10 in playerSettings.GetGizmos())
				{
					yield return gizmo10;
				}
			}
			foreach (Gizmo gizmo11 in health.GetGizmos())
			{
				yield return gizmo11;
			}
		}
		if (Dead && HasShowGizmosOnCorpseHediff)
		{
			foreach (Gizmo gizmo12 in health.GetGizmos())
			{
				yield return gizmo12;
			}
		}
		if (apparel != null)
		{
			foreach (Gizmo gizmo13 in apparel.GetGizmos())
			{
				yield return gizmo13;
			}
		}
		if (inventory != null)
		{
			foreach (Gizmo gizmo14 in inventory.GetGizmos())
			{
				yield return gizmo14;
			}
		}
		if (mindState != null)
		{
			foreach (Gizmo gizmo15 in mindState.GetGizmos())
			{
				yield return gizmo15;
			}
		}
		if (royalty != null && IsColonistPlayerControlled)
		{
			bool anyPermitOnCooldown = false;
			foreach (FactionPermit allFactionPermit in royalty.AllFactionPermits)
			{
				if (allFactionPermit.OnCooldown)
				{
					anyPermitOnCooldown = true;
				}
				IEnumerable<Gizmo> pawnGizmos = allFactionPermit.Permit.Worker.GetPawnGizmos(this, allFactionPermit.Faction);
				if (pawnGizmos == null)
				{
					continue;
				}
				foreach (Gizmo item in pawnGizmos)
				{
					yield return item;
				}
			}
			if (royalty.HasAidPermit)
			{
				yield return royalty.RoyalAidGizmo();
			}
			if (DebugSettings.ShowDevGizmos && anyPermitOnCooldown)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Reset permit cooldowns";
				command_Action.action = delegate
				{
					foreach (FactionPermit allFactionPermit2 in royalty.AllFactionPermits)
					{
						allFactionPermit2.ResetCooldown();
					}
				};
				yield return command_Action;
			}
			foreach (RoyalTitle item2 in royalty.AllTitlesForReading)
			{
				if (item2.def.permits == null)
				{
					continue;
				}
				Faction faction = item2.faction;
				foreach (RoyalTitlePermitDef permit in item2.def.permits)
				{
					IEnumerable<Gizmo> pawnGizmos2 = permit.Worker.GetPawnGizmos(this, faction);
					if (pawnGizmos2 == null)
					{
						continue;
					}
					foreach (Gizmo item3 in pawnGizmos2)
					{
						yield return item3;
					}
				}
			}
		}
		foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
		{
			yield return questRelatedGizmo;
		}
		if (royalty != null && ModsConfig.RoyaltyActive)
		{
			foreach (Gizmo gizmo16 in royalty.GetGizmos())
			{
				yield return gizmo16;
			}
		}
		if (connections != null && ModsConfig.IdeologyActive)
		{
			foreach (Gizmo gizmo17 in connections.GetGizmos())
			{
				yield return gizmo17;
			}
		}
		if (genes != null)
		{
			foreach (Gizmo gizmo18 in genes.GetGizmos())
			{
				yield return gizmo18;
			}
		}
		if (training != null)
		{
			foreach (Gizmo gizmo19 in training.GetGizmos())
			{
				yield return gizmo19;
			}
		}
		Lord lord = this.GetLord();
		if (lord?.LordJob != null)
		{
			foreach (Gizmo pawnGizmo in lord.LordJob.GetPawnGizmos(this))
			{
				yield return pawnGizmo;
			}
			if (lord.CurLordToil != null)
			{
				foreach (Gizmo pawnGizmo2 in lord.CurLordToil.GetPawnGizmos(this))
				{
					yield return pawnGizmo2;
				}
			}
		}
		if (DebugSettings.ShowDevGizmos && ModsConfig.BiotechActive && (relations?.IsTryRomanceOnCooldown ?? false))
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Reset try romance cooldown";
			command_Action2.action = delegate
			{
				relations.romanceEnableTick = -1;
			};
			yield return command_Action2;
		}
	}

	public virtual IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!ModsConfig.AnomalyActive || creepjoiner == null)
		{
			yield break;
		}
		foreach (FloatMenuOption floatMenuOption2 in creepjoiner.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption2;
		}
	}

	public override TipSignal GetTooltip()
	{
		string text = "";
		string text2 = "";
		if (gender != Gender.None)
		{
			text = (LabelCap.EqualsIgnoreCase(KindLabel) ? this.GetGenderLabel() : ((string)"PawnTooltipGenderAndKindLabel".Translate(this.GetGenderLabel(), KindLabel)));
		}
		else if (!LabelCap.EqualsIgnoreCase(KindLabel))
		{
			text = KindLabel;
		}
		string generalConditionLabel = HealthUtility.GetGeneralConditionLabel(this);
		bool flag = !string.IsNullOrEmpty(text);
		text2 = ((equipment?.Primary != null) ? ((!flag) ? ((string)"PawnTooltipWithPrimaryEquipNoDesc".Translate(LabelCap, text, generalConditionLabel)) : ((string)"PawnTooltipWithDescAndPrimaryEquip".Translate(LabelCap, text, equipment.Primary.LabelCap, generalConditionLabel))) : ((!flag) ? ((string)"PawnTooltipNoDescNoPrimaryEquip".Translate(LabelCap, generalConditionLabel)) : ((string)"PawnTooltipWithDescNoPrimaryEquip".Translate(LabelCap, text, generalConditionLabel))));
		return new TipSignal(text2, thingIDNumber * 152317, TooltipPriority.Pawn);
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		foreach (StatDrawEntry item in base.SpecialDisplayStats())
		{
			yield return item;
		}
		if (ModsConfig.BiotechActive && genes != null && genes.Xenotype != XenotypeDefOf.Baseliner)
		{
			string reportText = (genes.UniqueXenotype ? "UniqueXenotypeDesc".Translate().ToString() : DescriptionFlavor);
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Race".Translate(), def.LabelCap + " (" + genes.XenotypeLabel + ")", reportText, 4205, null, genes.UniqueXenotype ? null : Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(genes.Xenotype)));
		}
		if (ModsConfig.BiotechActive && RaceProps.Humanlike && !Mathf.Approximately(ageTracker.BiologicalTicksPerTick, 1f))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PawnHealth, "StatsReport_AgeRateMultiplier".Translate(), ageTracker.BiologicalTicksPerTick.ToStringPercent(), "StatsReport_AgeRateMultiplier_Desc".Translate(), 4195);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "BodySize".Translate(), BodySize.ToString("F2"), "Stat_Race_BodySize_Desc".Translate(), 4195);
		if (RaceProps.lifeStageAges.Count > 1 && RaceProps.Animal)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Growth".Translate(), ageTracker.Growth.ToStringPercent(), "Stat_Race_Growth_Desc".Translate(), 4203);
		}
		if (ModsConfig.RoyaltyActive && RaceProps.intelligence == Intelligence.Humanlike)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PawnPsyfocus, "MeditationFocuses".Translate(), MeditationUtility.FocusTypesAvailableForPawnString(this).CapitalizeFirst(), ("MeditationFocusesPawnDesc".Translate() + "\n\n" + MeditationUtility.FocusTypeAvailableExplanation(this)).Resolve(), 4011, null, MeditationUtility.FocusObjectsForPawnHyperlinks(this));
		}
		if (apparel != null && !apparel.AllRequirements.EnumerableNullOrEmpty())
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ApparelRequirementWithSource allRequirement in apparel.AllRequirements)
			{
				string text = null;
				if (!ApparelUtility.IsRequirementActive(allRequirement.requirement, allRequirement.Source, this, out var disabledByLabel))
				{
					text = " [" + "ApparelRequirementDisabledLabel".Translate() + ": " + disabledByLabel + "]";
				}
				stringBuilder.Append("- ");
				bool flag = true;
				foreach (ThingDef item2 in allRequirement.requirement.AllRequiredApparelForPawn(this, ignoreGender: false, includeWorn: true))
				{
					if (!flag)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(item2.LabelCap);
					flag = false;
				}
				if (allRequirement.Source == ApparelRequirementSource.Title)
				{
					stringBuilder.Append(" ");
					if (ModsConfig.BiotechActive)
					{
						stringBuilder.Append("ApparelRequirementOrAnyPsycasterOrPrestigeApparelOrMechlord".Translate());
					}
					else
					{
						stringBuilder.Append("ApparelRequirementOrAnyPsycasterOrPrestigeApparel".Translate());
					}
				}
				stringBuilder.Append(" (");
				stringBuilder.Append("Source".Translate());
				stringBuilder.Append(": ");
				stringBuilder.Append(allRequirement.SourceLabelCap);
				stringBuilder.Append(")");
				if (text != null)
				{
					stringBuilder.Append(text);
				}
				stringBuilder.AppendLine();
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_Pawn_RequiredApparel_Name".Translate(), "", "Stat_Pawn_RequiredApparel_Name".Translate() + ":\n\n" + stringBuilder.ToString(), 100);
		}
		if (ModsConfig.IdeologyActive && Ideo != null)
		{
			foreach (StatDrawEntry item3 in DarknessCombatUtility.GetStatEntriesForPawn(this))
			{
				yield return item3;
			}
		}
		if (genes != null)
		{
			foreach (StatDrawEntry item4 in genes.SpecialDisplayStats())
			{
				yield return item4;
			}
		}
		if (ModsConfig.BiotechActive && RaceProps.Humanlike)
		{
			TaggedString taggedString = "DevelopmentStage_Adult".Translate();
			TaggedString taggedString2 = "StatsReport_DevelopmentStageDesc_Adult".Translate();
			if (ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Child)
			{
				taggedString = "DevelopmentStage_Child".Translate();
				taggedString2 = "StatsReport_DevelopmentStageDesc_ChildPart1".Translate() + ":\n\n" + (from d in RaceProps.lifeStageWorkSettings
					where d.minAge > 0 && d.workType.visible
					select (d.workType.labelShort + " (" + "AgeIndicator".Translate(d.minAge) + ")").RawText).ToLineList("  - ", capitalizeItems: true) + "\n\n" + "StatsReport_DevelopmentStageDesc_ChildPart2".Translate();
			}
			else if (ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Baby)
			{
				taggedString = "DevelopmentStage_Baby".Translate();
				taggedString2 = "StatsReport_DevelopmentStageDesc_Baby".Translate();
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_DevelopmentStage".Translate(), taggedString, taggedString2, 4200);
		}
		if (!IsMutant)
		{
			yield break;
		}
		foreach (StatDrawEntry item5 in mutant.SpecialDisplayStats())
		{
			yield return item5;
		}
	}

	public PathingContext GetPathContext(Pathing pathing)
	{
		if (Flying)
		{
			return pathing.Flying;
		}
		if (ShouldAvoidFences && (CurJob == null || !CurJob.canBashFences))
		{
			return pathing.FenceBlocked;
		}
		return pathing.Normal;
	}

	public bool Sterile()
	{
		if (!ageTracker.CurLifeStage.reproductive)
		{
			return true;
		}
		if (RaceProps.Humanlike)
		{
			if (!ModsConfig.BiotechActive)
			{
				return true;
			}
			if (this.GetStatValue(StatDefOf.Fertility) <= 0f)
			{
				return true;
			}
		}
		if (health.hediffSet.HasHediffPreventsPregnancy())
		{
			return true;
		}
		if (this.SterileGenes())
		{
			return true;
		}
		return false;
	}

	public bool AnythingToStrip()
	{
		if (!kindDef.canStrip)
		{
			return false;
		}
		if (equipment != null && equipment.HasAnything())
		{
			return true;
		}
		if (inventory != null && inventory.innerContainer.Count > 0)
		{
			return true;
		}
		if (apparel != null)
		{
			if (base.Destroyed)
			{
				if (apparel.AnyApparel)
				{
					return true;
				}
			}
			else if (apparel.AnyApparelUnlocked)
			{
				return true;
			}
		}
		return false;
	}

	public void Strip(bool notifyFaction = true)
	{
		Caravan caravan = this.GetCaravan();
		if (caravan != null)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(this, caravan.PawnsListForReading);
			if (apparel != null)
			{
				CaravanInventoryUtility.MoveAllApparelToSomeonesInventory(this, caravan.PawnsListForReading, base.Destroyed);
			}
			if (equipment != null)
			{
				CaravanInventoryUtility.MoveAllEquipmentToSomeonesInventory(this, caravan.PawnsListForReading);
			}
		}
		else
		{
			IntVec3 pos = Corpse?.PositionHeld ?? base.PositionHeld;
			equipment?.DropAllEquipment(pos, forbid: false);
			apparel?.DropAll(pos, forbid: false, base.Destroyed);
			inventory?.DropAllNearPawn(pos);
		}
		if (notifyFaction && base.Faction != null)
		{
			base.Faction.Notify_MemberStripped(this, Faction.OfPlayer);
		}
	}

	public Thought_Memory GiveObservedThought(Pawn observer)
	{
		if (ModsConfig.AnomalyActive && base.Spawned && !Downed && mindState?.duty?.def == DutyDefOf.ChimeraAttack && this.TryGetComp<CompChimera>(out var comp))
		{
			return comp.GiveObservedThought(observer);
		}
		return null;
	}

	public HistoryEventDef GiveObservedHistoryEvent(Pawn observer)
	{
		return null;
	}

	public void HearClamor(Thing source, ClamorDef type)
	{
		if (Dead || Downed || Deathresting || this.IsSelfShutdown())
		{
			return;
		}
		if (type == ClamorDefOf.Movement || type == ClamorDefOf.BabyCry)
		{
			if (source is Pawn source2)
			{
				CheckForDisturbedSleep(source2);
			}
			NotifyLordOfClamor(source, type);
		}
		else if (type == ClamorDefOf.Harm)
		{
			if (base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction == source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
		}
		else if (type == ClamorDefOf.Construction)
		{
			if (base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction != source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
		}
		else if (type == ClamorDefOf.Ability)
		{
			if (base.Faction == Faction.OfPlayer || base.Faction == source.Faction || HostFaction != null)
			{
				return;
			}
			if (!this.Awake())
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
			NotifyLordOfClamor(source, type);
		}
		else if (type == ClamorDefOf.Impact)
		{
			mindState.Notify_ClamorImpact(source);
			if (CurJob != null && !this.Awake())
			{
				jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			NotifyLordOfClamor(source, type);
		}
	}

	private void NotifyLordOfClamor(Thing source, ClamorDef type)
	{
		this.GetLord()?.Notify_Clamor(source, type);
	}

	public override void Notify_UsedVerb(Pawn pawn, Verb verb)
	{
		base.Notify_UsedVerb(pawn, verb);
		if (Rand.Chance((IsMutant && mutant.Def.soundAttackChance > 0f) ? mutant.Def.soundAttackChance : ageTracker.CurLifeStage.soundAttackChance))
		{
			LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge lifeStage) => lifeStage.soundAttack, null, (MutantDef mutantDef) => mutantDef.soundAttack);
		}
		UpdatePyroVerbThought(verb);
	}

	public override void Notify_Explosion(Explosion explosion)
	{
		base.Notify_Explosion(explosion);
		mindState.Notify_Explosion(explosion);
	}

	public override void Notify_BulletImpactNearby(BulletImpactData impactData)
	{
		apparel?.Notify_BulletImpactNearby(impactData);
	}

	public virtual void Notify_Downed()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_Downed();
		}
	}

	public virtual void Notify_Released()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_Released();
		}
		if (ModsConfig.AnomalyActive)
		{
			creepjoiner?.Notify_Released();
		}
	}

	public virtual void Notify_PrisonBreakout()
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_PrisonBreakout();
		}
		if (ModsConfig.AnomalyActive)
		{
			creepjoiner?.Notify_PrisonBreakout();
		}
	}

	public virtual void Notify_DuplicatedFrom(Pawn source)
	{
		List<ThingComp> allComps = base.AllComps;
		for (int i = 0; i < allComps.Count; i++)
		{
			allComps[i].Notify_DuplicatedFrom(source);
		}
		if (ModsConfig.AnomalyActive)
		{
			creepjoiner?.Notify_DuplicatedFrom(source);
		}
	}

	private void CheckForDisturbedSleep(Pawn source)
	{
		if (needs.mood != null && !this.Awake() && base.Faction == Faction.OfPlayer && Find.TickManager.TicksGame >= lastSleepDisturbedTick + 300 && !Deathresting && (source == null || (!LovePartnerRelationUtility.LovePartnerRelationExists(this, source) && !(source.RaceProps.petness > 0f) && (source.relations == null || !source.relations.DirectRelations.Any((DirectPawnRelation dr) => dr.def == PawnRelationDefOf.Bond)))))
		{
			lastSleepDisturbedTick = Find.TickManager.TicksGame;
			needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed);
		}
	}

	public float GetAcceptArrestChance(Pawn arrester)
	{
		if (Downed || WorkTagIsDisabled(WorkTags.Violent) || (guilt != null && guilt.IsGuilty && IsColonist && !this.IsQuestLodger()))
		{
			return 1f;
		}
		if (ModsConfig.AnomalyActive && health.hediffSet.HasHediff(HediffDefOf.RevenantHypnosis))
		{
			return 1f;
		}
		if (ModsConfig.BiotechActive && genes != null && genes.AggroMentalBreakSelectionChanceFactor <= 0f)
		{
			return 1f;
		}
		return (StatDefOf.ArrestSuccessChance.Worker.IsDisabledFor(arrester) ? StatDefOf.ArrestSuccessChance.valueIfMissing : arrester.GetStatValue(StatDefOf.ArrestSuccessChance)) * kindDef.acceptArrestChanceFactor;
	}

	public bool CheckAcceptArrest(Pawn arrester)
	{
		float acceptArrestChance = GetAcceptArrestChance(arrester);
		Faction homeFaction = HomeFaction;
		if (homeFaction != null && homeFaction != arrester.factionInt)
		{
			homeFaction.Notify_MemberCaptured(this, arrester.Faction);
		}
		List<ThingComp> allComps = base.AllComps;
		if (Downed || WorkTagIsDisabled(WorkTags.Violent) || Rand.Value < acceptArrestChance)
		{
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].Notify_Arrested(succeeded: true);
				if (ModsConfig.AnomalyActive)
				{
					creepjoiner?.Notify_Arrested(succeeded: true);
				}
			}
			return true;
		}
		Messages.Message("MessageRefusedArrest".Translate(LabelShort, this), this, MessageTypeDefOf.ThreatSmall);
		for (int j = 0; j < allComps.Count; j++)
		{
			allComps[j].Notify_Arrested(succeeded: false);
		}
		if (ModsConfig.AnomalyActive)
		{
			creepjoiner?.Notify_Arrested(succeeded: false);
		}
		if (base.Faction == null || !arrester.HostileTo(this))
		{
			mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
		}
		return false;
	}

	public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
	{
		if (!base.Spawned)
		{
			return true;
		}
		if (!InMentalState && this.GetTraderCaravanRole() == TraderCaravanRole.Carrier && !(jobs.curDriver is JobDriver_AttackMelee))
		{
			return true;
		}
		if (mindState.duty != null && mindState.duty.def.threatDisabled)
		{
			return true;
		}
		if (!mindState.Active)
		{
			return true;
		}
		if (this.IsColonyMechRequiringMechanitor())
		{
			return true;
		}
		Pawn pawn = disabledFor?.Thing as Pawn;
		if (Downed && (!CanAttackWhileCrawling || !Crawling))
		{
			if (disabledFor == null)
			{
				return true;
			}
			if (pawn?.mindState?.duty == null || !pawn.mindState.duty.attackDownedIfStarving || !pawn.Starving())
			{
				return true;
			}
		}
		if (ModsConfig.AnomalyActive && this.TryGetComp<CompActivity>(out var comp) && comp.IsDormant)
		{
			return true;
		}
		if (this.IsPsychologicallyInvisible())
		{
			return true;
		}
		if (ThreatDisabledBecauseNonAggressiveRoamer(pawn) || (pawn != null && pawn.ThreatDisabledBecauseNonAggressiveRoamer(this)))
		{
			return true;
		}
		return false;
	}

	public bool ThreatDisabledBecauseNonAggressiveRoamer(Pawn otherPawn)
	{
		if (!Roamer || base.Faction != Faction.OfPlayer)
		{
			return false;
		}
		Lord lord = otherPawn?.GetLord();
		if (lord != null && lord.CurLordToil.AllowAggressiveTargetingOfRoamers)
		{
			return false;
		}
		if (InAggroMentalState || this.IsFighting() || Find.TickManager.TicksGame < mindState.lastEngageTargetTick + 360)
		{
			return false;
		}
		return true;
	}

	private void UpdatePyroVerbThought(Verb verb)
	{
		if (story == null || !story.traits.HasTrait(TraitDefOf.Pyromaniac) || (!verb.IsIncendiary_Melee() && !verb.IsIncendiary_Ranged()))
		{
			return;
		}
		if (verb.CurrentTarget.Pawn != null && verb.CurrentTarget.Pawn.Spawned && IsValidPyroThoughtTarget(verb.CurrentTarget.Pawn))
		{
			needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PyroUsed);
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(verb.CurrentTarget.Cell, verb.EffectiveRange, useCenter: true))
		{
			if (!item.InBounds(base.MapHeld))
			{
				continue;
			}
			foreach (Thing thing in item.GetThingList(base.MapHeld))
			{
				if (IsValidPyroThoughtTarget(thing))
				{
					needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.PyroUsed);
					break;
				}
			}
		}
	}

	private bool IsValidPyroThoughtTarget(Thing thing)
	{
		if (thing is Pawn { Downed: false } pawn && !pawn.IsPsychologicallyInvisible() && !pawn.Fogged())
		{
			return pawn.HostileTo(Faction.OfPlayer);
		}
		return false;
	}

	public List<WorkTypeDef> GetDisabledWorkTypes(bool permanentOnly = false)
	{
		if (Scribe.mode != LoadSaveMode.Inactive)
		{
			cachedDisabledWorkTypesPermanent = null;
			cachedDisabledWorkTypes = null;
		}
		if (permanentOnly)
		{
			if (cachedDisabledWorkTypesPermanent == null)
			{
				cachedDisabledWorkTypesPermanent = new List<WorkTypeDef>();
				FillList(cachedDisabledWorkTypesPermanent);
			}
			return cachedDisabledWorkTypesPermanent;
		}
		if (cachedDisabledWorkTypes == null)
		{
			cachedDisabledWorkTypes = new List<WorkTypeDef>();
			FillList(cachedDisabledWorkTypes);
		}
		return cachedDisabledWorkTypes;
		void FillList(List<WorkTypeDef> list)
		{
			if (IsMutant && mutant.HasTurned)
			{
				List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				for (int i = 0; i < allDefsListForReading.Count; i++)
				{
					foreach (WorkTypeDef disabledWorkType in mutant.Def.DisabledWorkTypes)
					{
						if (!list.Contains(disabledWorkType))
						{
							list.Add(disabledWorkType);
						}
					}
				}
			}
			else
			{
				if (story != null && !IsSlave)
				{
					foreach (BackstoryDef allBackstory in story.AllBackstories)
					{
						foreach (WorkTypeDef disabledWorkType2 in allBackstory.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType2))
							{
								list.Add(disabledWorkType2);
							}
						}
					}
					for (int j = 0; j < story.traits.allTraits.Count; j++)
					{
						if (!story.traits.allTraits[j].Suppressed)
						{
							foreach (WorkTypeDef disabledWorkType3 in story.traits.allTraits[j].GetDisabledWorkTypes())
							{
								if (!list.Contains(disabledWorkType3))
								{
									list.Add(disabledWorkType3);
								}
							}
						}
					}
				}
				if (ModsConfig.BiotechActive && IsColonyMech)
				{
					List<WorkTypeDef> allDefsListForReading2 = DefDatabase<WorkTypeDef>.AllDefsListForReading;
					for (int k = 0; k < allDefsListForReading2.Count; k++)
					{
						if (!RaceProps.mechEnabledWorkTypes.Contains(allDefsListForReading2[k]) && !list.Contains(allDefsListForReading2[k]))
						{
							list.Add(allDefsListForReading2[k]);
						}
					}
				}
				if (!permanentOnly)
				{
					if (health != null)
					{
						foreach (WorkTypeDef disabledWorkType4 in health.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType4))
							{
								list.Add(disabledWorkType4);
							}
						}
					}
					if (royalty != null && !IsSlave)
					{
						foreach (RoyalTitle item in royalty.AllTitlesForReading)
						{
							if (item.conceited)
							{
								foreach (WorkTypeDef disabledWorkType5 in item.def.DisabledWorkTypes)
								{
									if (!list.Contains(disabledWorkType5))
									{
										list.Add(disabledWorkType5);
									}
								}
							}
						}
					}
					if (ModsConfig.IdeologyActive && Ideo != null)
					{
						Precept_Role role = Ideo.GetRole(this);
						if (role != null)
						{
							foreach (WorkTypeDef disabledWorkType6 in role.DisabledWorkTypes)
							{
								if (!list.Contains(disabledWorkType6))
								{
									list.Add(disabledWorkType6);
								}
							}
						}
					}
					if (ModsConfig.BiotechActive && genes != null)
					{
						foreach (Gene item2 in genes.GenesListForReading)
						{
							foreach (WorkTypeDef disabledWorkType7 in item2.DisabledWorkTypes)
							{
								if (!list.Contains(disabledWorkType7))
								{
									list.Add(disabledWorkType7);
								}
							}
						}
					}
					foreach (QuestPart_WorkDisabled item3 in QuestUtility.GetWorkDisabledQuestPart(this))
					{
						foreach (WorkTypeDef disabledWorkType8 in item3.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType8))
							{
								list.Add(disabledWorkType8);
							}
						}
					}
					if (guest != null)
					{
						foreach (WorkTypeDef disabledWorkType9 in guest.GetDisabledWorkTypes())
						{
							if (!list.Contains(disabledWorkType9))
							{
								list.Add(disabledWorkType9);
							}
						}
					}
					for (int l = 0; l < RaceProps.lifeStageWorkSettings.Count; l++)
					{
						LifeStageWorkSettings lifeStageWorkSettings = RaceProps.lifeStageWorkSettings[l];
						if (lifeStageWorkSettings.IsDisabled(this) && !list.Contains(lifeStageWorkSettings.workType))
						{
							list.Add(lifeStageWorkSettings.workType);
						}
					}
				}
			}
		}
	}

	public List<string> GetReasonsForDisabledWorkType(WorkTypeDef workType)
	{
		if (cachedReasonsForDisabledWorkTypes != null && cachedReasonsForDisabledWorkTypes.TryGetValue(workType, out var value))
		{
			return value;
		}
		List<string> list = new List<string>();
		foreach (BackstoryDef allBackstory in story.AllBackstories)
		{
			foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
			{
				if (workType == disabledWorkType)
				{
					list.Add("WorkDisabledByBackstory".Translate(allBackstory.TitleCapFor(gender)));
					break;
				}
			}
		}
		for (int i = 0; i < story.traits.allTraits.Count; i++)
		{
			Trait trait = story.traits.allTraits[i];
			foreach (WorkTypeDef disabledWorkType2 in trait.GetDisabledWorkTypes())
			{
				if (disabledWorkType2 == workType && !trait.Suppressed)
				{
					list.Add("WorkDisabledByTrait".Translate(trait.LabelCap));
					break;
				}
			}
		}
		if (royalty != null)
		{
			foreach (RoyalTitle item in royalty.AllTitlesForReading)
			{
				if (!item.conceited)
				{
					continue;
				}
				foreach (WorkTypeDef disabledWorkType3 in item.def.DisabledWorkTypes)
				{
					if (workType == disabledWorkType3)
					{
						list.Add("WorkDisabledByRoyalTitle".Translate(item.Label));
						break;
					}
				}
			}
		}
		if (ModsConfig.IdeologyActive && Ideo != null)
		{
			Precept_Role role = Ideo.GetRole(this);
			if (role != null)
			{
				foreach (WorkTypeDef disabledWorkType4 in role.DisabledWorkTypes)
				{
					if (workType == disabledWorkType4)
					{
						list.Add("WorkDisabledRole".Translate(role.LabelForPawn(this)));
						break;
					}
				}
			}
		}
		if (IsMutant)
		{
			foreach (WorkTypeDef disabledWorkType5 in mutant.Def.DisabledWorkTypes)
			{
				if (workType == disabledWorkType5)
				{
					list.Add(mutant.Def.LabelCap);
					break;
				}
			}
		}
		foreach (QuestPart_WorkDisabled item2 in QuestUtility.GetWorkDisabledQuestPart(this))
		{
			foreach (WorkTypeDef disabledWorkType6 in item2.DisabledWorkTypes)
			{
				if (workType == disabledWorkType6)
				{
					list.Add("WorkDisabledByQuest".Translate(item2.quest.name));
					break;
				}
			}
		}
		if (guest != null && guest.IsSlave)
		{
			foreach (WorkTypeDef disabledWorkType7 in guest.GetDisabledWorkTypes())
			{
				if (workType == disabledWorkType7)
				{
					list.Add("WorkDisabledSlave".Translate());
					break;
				}
			}
		}
		if (health != null)
		{
			foreach (WorkTypeDef disabledWorkType8 in health.DisabledWorkTypes)
			{
				if (workType == disabledWorkType8)
				{
					list.Add("WorkDisabledHealth".Translate());
					break;
				}
			}
		}
		if (this.IsWorkTypeDisabledByAge(workType, out var minAgeRequired))
		{
			list.Add("WorkDisabledAge".Translate(this, ageTracker.AgeBiologicalYears, workType.labelShort, minAgeRequired));
		}
		if (cachedReasonsForDisabledWorkTypes == null)
		{
			cachedReasonsForDisabledWorkTypes = new Dictionary<WorkTypeDef, List<string>>();
		}
		cachedReasonsForDisabledWorkTypes[workType] = list;
		return list;
	}

	public bool WorkTypeIsDisabled(WorkTypeDef w)
	{
		return GetDisabledWorkTypes().Contains(w);
	}

	public bool OneOfWorkTypesIsDisabled(List<WorkTypeDef> wts)
	{
		for (int i = 0; i < wts.Count; i++)
		{
			if (WorkTypeIsDisabled(wts[i]))
			{
				return true;
			}
		}
		return false;
	}

	public void Notify_DisabledWorkTypesChanged()
	{
		cachedDisabledWorkTypes = null;
		cachedDisabledWorkTypesPermanent = null;
		cachedReasonsForDisabledWorkTypes = null;
		workSettings?.Notify_DisabledWorkTypesChanged();
		skills?.Notify_SkillDisablesChanged();
	}

	public bool WorkTagIsDisabled(WorkTags w)
	{
		return (CombinedDisabledWorkTags & w) != 0;
	}

	public override bool PreventPlayerSellingThingsNearby(out string reason)
	{
		if (base.Faction.HostileTo(Faction.OfPlayer) && HostFaction == null && !Downed && !InMentalState)
		{
			reason = "Enemies".Translate();
			return true;
		}
		reason = null;
		return false;
	}

	public void ChangeKind(PawnKindDef newKindDef)
	{
		if (kindDef != newKindDef)
		{
			kindDef = newKindDef;
			if (this.IsWildMan())
			{
				mindState.WildManEverReachedOutside = false;
				ReachabilityUtility.ClearCacheFor(this);
			}
		}
	}

	public bool CompsWantHoldWeapon()
	{
		foreach (ThingComp allComp in base.AllComps)
		{
			if (allComp.WantHoldWeapon(this))
			{
				return true;
			}
		}
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			canBeDormant = GetComp<CompCanBeDormant>();
			activity = GetComp<CompActivity>();
		}
		Scribe_Defs.Look(ref kindDef, "kindDef");
		Scribe_Values.Look(ref gender, "gender", Gender.Male);
		Scribe_Values.Look(ref becameWorldPawnTickAbs, "becameWorldPawnTickAbs", -1);
		Scribe_Values.Look(ref teleporting, "teleporting", defaultValue: false);
		Scribe_Values.Look(ref showNamePromptOnTick, "showNamePromptOnTick", -1);
		Scribe_Values.Look(ref babyNamingDeadline, "babyNamingDeadline", -1);
		Scribe_Values.Look(ref addCorpseToLord, "addCorpseToLord", defaultValue: false);
		Scribe_Values.Look(ref timesRaisedAsShambler, "timesRaisedAsShambler", 0);
		Scribe_Values.Look(ref lastSleepDisturbedTick, "lastSleepDisturbedTick", 0);
		Scribe_Values.Look(ref dontGivePreArrivalPathway, "dontGivePreArrivalPathway", defaultValue: false);
		Scribe_Values.Look(ref lastVacuumBurntTick, "lastVacuumBurntTick", 0);
		Scribe_Deep.Look(ref nameInt, "name");
		if (Scribe.mode == LoadSaveMode.Saving && GenTicks.TicksGame - deadlifeDustFactionTick > 12500)
		{
			deadlifeDustFactionTick = 0;
			deadlifeDustFaction = null;
		}
		Scribe_Values.Look(ref deadlifeDustFactionTick, "deadlifeDustFactionTick", 0);
		Scribe_References.Look(ref deadlifeDustFaction, "deadlifeDustFaction");
		Scribe_Deep.Look(ref mindState, "mindState", this);
		Scribe_Deep.Look(ref jobs, "jobs", this);
		Scribe_Deep.Look(ref stances, "stances", this);
		Scribe_Deep.Look(ref infectionVectors, "infectionVectors", this);
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		Scribe_Deep.Look(ref natives, "natives", this);
		Scribe_Deep.Look(ref meleeVerbs, "meleeVerbs", this);
		Scribe_Deep.Look(ref rotationTracker, "rotationTracker", this);
		Scribe_Deep.Look(ref pather, "pather", this);
		Scribe_Deep.Look(ref carryTracker, "carryTracker", this);
		Scribe_Deep.Look(ref apparel, "apparel", this);
		Scribe_Deep.Look(ref story, "story", this);
		Scribe_Deep.Look(ref equipment, "equipment", this);
		Scribe_Deep.Look(ref drafter, "drafter", this);
		Scribe_Deep.Look(ref ageTracker, "ageTracker", this);
		Scribe_Deep.Look(ref health, "healthTracker", this);
		Scribe_Deep.Look(ref records, "records", this);
		Scribe_Deep.Look(ref inventory, "inventory", this);
		Scribe_Deep.Look(ref filth, "filth", this);
		Scribe_Deep.Look(ref roping, "roping", this);
		Scribe_Deep.Look(ref needs, "needs", this);
		Scribe_Deep.Look(ref guest, "guest", this);
		Scribe_Deep.Look(ref guilt, "guilt", this);
		Scribe_Deep.Look(ref royalty, "royalty", this);
		Scribe_Deep.Look(ref relations, "social", this);
		Scribe_Deep.Look(ref psychicEntropy, "psychicEntropy", this);
		Scribe_Deep.Look(ref mutant, "shambler", this);
		Scribe_Deep.Look(ref ownership, "ownership", this);
		Scribe_Deep.Look(ref interactions, "interactions", this);
		Scribe_Deep.Look(ref skills, "skills", this);
		Scribe_Deep.Look(ref abilities, "abilities", this);
		Scribe_Deep.Look(ref ideo, "ideo", this);
		Scribe_Deep.Look(ref workSettings, "workSettings", this);
		Scribe_Deep.Look(ref trader, "trader", this);
		Scribe_Deep.Look(ref outfits, "outfits", this);
		Scribe_Deep.Look(ref drugs, "drugs", this);
		Scribe_Deep.Look(ref foodRestriction, "foodRestriction", this);
		Scribe_Deep.Look(ref timetable, "timetable", this);
		Scribe_Deep.Look(ref playerSettings, "playerSettings", this);
		Scribe_Deep.Look(ref training, "training", this);
		Scribe_Deep.Look(ref style, "style", this);
		Scribe_Deep.Look(ref styleObserver, "styleObserver", this);
		Scribe_Deep.Look(ref connections, "connections", this);
		Scribe_Deep.Look(ref inventoryStock, "inventoryStock", this);
		Scribe_Deep.Look(ref surroundings, "treeSightings", this);
		Scribe_Deep.Look(ref thinker, "thinker", this);
		Scribe_Deep.Look(ref mechanitor, "mechanitor", this);
		Scribe_Deep.Look(ref genes, "genes", this);
		Scribe_Deep.Look(ref learning, "learning", this);
		Scribe_Deep.Look(ref reading, "reading", this);
		Scribe_Deep.Look(ref creepjoiner, "creepjoiner", this);
		Scribe_Deep.Look(ref duplicate, "duplicate", this);
		Scribe_Deep.Look(ref flight, "flight", this);
		Scribe_Values.Look(ref wasLeftBehindStartingPawn, "wasLeftBehindStartingPawn", defaultValue: false);
		Scribe_Values.Look(ref everLostEgo, "everBrainWiped", defaultValue: false);
		Scribe_Values.Look(ref wasDraftedBeforeSkip, "wasDraftedBeforeSkip", defaultValue: false);
		BackCompatibility.PostExposeData(this);
	}

	public override string ToString()
	{
		if (story != null)
		{
			return LabelShort;
		}
		if (thingIDNumber > 0)
		{
			return base.ThingID;
		}
		if (kindDef != null)
		{
			return KindLabel + "_" + base.ThingID;
		}
		if (def != null)
		{
			return base.ThingID;
		}
		return GetType().ToString();
	}

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		return trader.ColonyThingsWillingToBuy(playerNegotiator);
	}

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		trader.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
	}

	public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		trader.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
	}

	string IVerbOwner.UniqueVerbOwnerID()
	{
		return GetUniqueLoadID();
	}

	bool IVerbOwner.VerbsStillUsableBy(Pawn p)
	{
		return p == this;
	}

	public PlanetTile GetRootTile()
	{
		return base.Tile;
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return null;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		if (inventory != null)
		{
			outChildren.Add(inventory);
		}
		if (carryTracker != null)
		{
			outChildren.Add(carryTracker);
		}
		if (equipment != null)
		{
			outChildren.Add(equipment);
		}
		if (apparel != null)
		{
			outChildren.Add(apparel);
		}
	}

	public bool CurrentlyUsableForBills()
	{
		if (!this.InBed())
		{
			JobFailReason.Is(NotSurgeryReadyTrans);
			return false;
		}
		if (!InteractionCell.IsValid)
		{
			JobFailReason.Is(CannotReachTrans);
			return false;
		}
		return true;
	}

	public bool UsableForBillsAfterFueling()
	{
		return CurrentlyUsableForBills();
	}

	public void Notify_BillDeleted(Bill bill)
	{
		bill.xenogerm?.Notify_BillRemoved();
	}

	public bool Equals(Pawn other)
	{
		if (def.defName == other.def.defName)
		{
			return thingIDNumber == other.thingIDNumber;
		}
		return false;
	}
}
