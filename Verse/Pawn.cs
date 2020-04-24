using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class Pawn : ThingWithComps, IStrippable, IBillGiver, IVerbOwner, ITrader, IAttackTarget, ILoadReferenceable, IAttackTargetSearcher, IThingHolder
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

		public Pawn_CarryTracker carryTracker;

		public Pawn_NeedsTracker needs;

		public Pawn_MindState mindState;

		public Pawn_RotationTracker rotationTracker;

		public Pawn_PathFollower pather;

		public Pawn_Thinker thinker;

		public Pawn_JobTracker jobs;

		public Pawn_StanceTracker stances;

		public Pawn_NativeVerbs natives;

		public Pawn_FilthTracker filth;

		public Pawn_EquipmentTracker equipment;

		public Pawn_ApparelTracker apparel;

		public Pawn_Ownership ownership;

		public Pawn_SkillTracker skills;

		public Pawn_StoryTracker story;

		public Pawn_GuestTracker guest;

		public Pawn_GuiltTracker guilt;

		public Pawn_RoyaltyTracker royalty;

		public Pawn_AbilityTracker abilities;

		public Pawn_WorkSettings workSettings;

		public Pawn_TraderTracker trader;

		public Pawn_TrainingTracker training;

		public Pawn_CallTracker caller;

		public Pawn_RelationsTracker relations;

		public Pawn_PsychicEntropyTracker psychicEntropy;

		public Pawn_InteractionsTracker interactions;

		public Pawn_PlayerSettings playerSettings;

		public Pawn_OutfitTracker outfits;

		public Pawn_DrugPolicyTracker drugs;

		public Pawn_FoodRestrictionTracker foodRestriction;

		public Pawn_TimetableTracker timetable;

		public Pawn_DraftController drafter;

		private Pawn_DrawTracker drawer;

		public int becameWorldPawnTickAbs = -1;

		private const float HumanSizedHeatOutput = 0.3f;

		private const float AnimalHeatOutputFactor = 0.6f;

		private static string NotSurgeryReadyTrans;

		private static string CannotReachTrans;

		public const int MaxMoveTicks = 450;

		private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

		private static List<string> states = new List<string>();

		private int lastSleepDisturbedTick;

		private const int SleepDisturbanceMinInterval = 300;

		private List<WorkTypeDef> cachedDisabledWorkTypes;

		private List<WorkTypeDef> cachedDisabledWorkTypesPermanent;

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

		public Job CurJob
		{
			get
			{
				if (jobs == null)
				{
					return null;
				}
				return jobs.curJob;
			}
		}

		public JobDef CurJobDef
		{
			get
			{
				if (CurJob == null)
				{
					return null;
				}
				return CurJob.def;
			}
		}

		public bool Downed => health.Downed;

		public bool Dead => health.Dead;

		public string KindLabel => GenLabel.BestKindLabel(this);

		public bool InMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				return mindState.mentalStateHandler.InMentalState;
			}
		}

		public MentalState MentalState
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurState;
			}
		}

		public MentalStateDef MentalStateDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.mentalStateHandler.CurStateDef;
			}
		}

		public bool InAggroMentalState
		{
			get
			{
				if (Dead)
				{
					return false;
				}
				if (mindState.mentalStateHandler.InMentalState)
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
				if (Dead)
				{
					return false;
				}
				return mindState.inspirationHandler.Inspired;
			}
		}

		public Inspiration Inspiration
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurState;
			}
		}

		public InspirationDef InspirationDef
		{
			get
			{
				if (Dead)
				{
					return null;
				}
				return mindState.inspirationHandler.CurStateDef;
			}
		}

		public override Vector3 DrawPos => Drawer.DrawPos;

		public VerbTracker VerbTracker => verbTracker;

		public List<VerbProperties> VerbProperties => def.Verbs;

		public List<Tool> Tools => def.tools;

		public bool IsColonist
		{
			get
			{
				if (base.Faction != null && base.Faction.IsPlayer)
				{
					return RaceProps.Humanlike;
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

		public Faction HostFaction
		{
			get
			{
				if (guest == null)
				{
					return null;
				}
				return guest.HostFaction;
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

		public bool IsColonistPlayerControlled
		{
			get
			{
				if (base.Spawned && IsColonist && MentalStateDef == null)
				{
					return HostFaction == null;
				}
				return false;
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
				if (base.ParentHolder == null)
				{
					return null;
				}
				return (base.ParentHolder as Pawn_CarryTracker)?.pawn;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (Name != null)
				{
					if (story == null || story.TitleShortCap.NullOrEmpty())
					{
						return Name.ToStringShort;
					}
					return Name.ToStringShort + ", " + story.TitleShortCap;
				}
				return KindLabel;
			}
		}

		public override string LabelShort
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringShort;
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
					if (story == null || story.TitleShortCap.NullOrEmpty())
					{
						return Name.ToStringShort.Colorize(ColoredText.NameColor);
					}
					return Name.ToStringShort.Colorize(ColoredText.NameColor) + ", " + story.TitleShortCap;
				}
				return KindLabel;
			}
		}

		public TaggedString NameShortColored
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringShort.Colorize(ColoredText.NameColor);
				}
				return KindLabel;
			}
		}

		public TaggedString NameFullColored
		{
			get
			{
				if (Name != null)
				{
					return Name.ToStringFull.Colorize(ColoredText.NameColor);
				}
				return KindLabel;
			}
		}

		public Pawn_DrawTracker Drawer
		{
			get
			{
				if (drawer == null)
				{
					drawer = new Pawn_DrawTracker(this);
				}
				return drawer;
			}
		}

		public Faction FactionOrExtraHomeFaction
		{
			get
			{
				if (base.Faction != null && base.Faction.IsPlayer)
				{
					return this.GetExtraHomeFaction() ?? base.Faction;
				}
				return base.Faction;
			}
		}

		public BillStack BillStack => health.surgeryBills;

		public override IntVec3 InteractionCell
		{
			get
			{
				Building_Bed building_Bed = this.CurrentBed();
				if (building_Bed != null)
				{
					IntVec3 position = base.Position;
					IntVec3 position2 = base.Position;
					IntVec3 position3 = base.Position;
					IntVec3 position4 = base.Position;
					if (building_Bed.Rotation.IsHorizontal)
					{
						position.z++;
						position2.z--;
						position3.x--;
						position4.x++;
					}
					else
					{
						position.x--;
						position2.x++;
						position3.z++;
						position4.z--;
					}
					if (position.Standable(base.Map) && position.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position.GetDoor(base.Map) == null)
					{
						return position;
					}
					if (position2.Standable(base.Map) && position2.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position2.GetDoor(base.Map) == null)
					{
						return position2;
					}
					if (position3.Standable(base.Map) && position3.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position3.GetDoor(base.Map) == null)
					{
						return position3;
					}
					if (position4.Standable(base.Map) && position4.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null && position4.GetDoor(base.Map) == null)
					{
						return position4;
					}
					if (position.Standable(base.Map) && position.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position;
					}
					if (position2.Standable(base.Map) && position2.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position2;
					}
					if (position3.Standable(base.Map) && position3.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position3;
					}
					if (position4.Standable(base.Map) && position4.GetThingList(base.Map).Find((Thing x) => x.def.IsBed) == null)
					{
						return position4;
					}
					if (position.Standable(base.Map))
					{
						return position;
					}
					if (position2.Standable(base.Map))
					{
						return position2;
					}
					if (position3.Standable(base.Map))
					{
						return position3;
					}
					if (position4.Standable(base.Map))
					{
						return position4;
					}
				}
				return base.InteractionCell;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				if (trader == null)
				{
					return null;
				}
				return trader.traderKind;
			}
		}

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
				Building_Turret building_Turret = this.MannedThing() as Building_Turret;
				if (building_Turret != null)
				{
					return building_Turret.AttackVerb;
				}
				return TryGetAttackVerb(null, !IsColonist);
			}
		}

		Thing IVerbOwner.ConstantCaster => this;

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.Bodypart;

		public int TicksPerMoveCardinal => TicksPerMove(diagonal: false);

		public int TicksPerMoveDiagonal => TicksPerMove(diagonal: true);

		public TradeCurrency TradeCurrency => TraderKind.tradeCurrency;

		public WorkTags CombinedDisabledWorkTags
		{
			get
			{
				WorkTags workTags = (story != null) ? story.DisabledWorkTagsBackstoryAndTraits : WorkTags.None;
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
				if (health != null && health.hediffSet != null)
				{
					foreach (Hediff hediff in health.hediffSet.hediffs)
					{
						HediffStage curStage = hediff.CurStage;
						if (curStage != null)
						{
							workTags |= curStage.disabledWorkTags;
						}
					}
					return workTags;
				}
				return workTags;
			}
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return GetUniqueLoadID();
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			return p == this;
		}

		public int GetRootTile()
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

		public string GetKindLabelPlural(int count = -1)
		{
			return GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: false, plural: true, count);
		}

		public static void ResetStaticData()
		{
			NotSurgeryReadyTrans = "NotSurgeryReady".Translate();
			CannotReachTrans = "CannotReach".Translate();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
			Scribe_Values.Look(ref gender, "gender", Gender.Male);
			Scribe_Values.Look(ref becameWorldPawnTickAbs, "becameWorldPawnTickAbs", -1);
			Scribe_Deep.Look(ref nameInt, "name");
			Scribe_Deep.Look(ref mindState, "mindState", this);
			Scribe_Deep.Look(ref jobs, "jobs", this);
			Scribe_Deep.Look(ref stances, "stances", this);
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
			Scribe_Deep.Look(ref needs, "needs", this);
			Scribe_Deep.Look(ref guest, "guest", this);
			Scribe_Deep.Look(ref guilt, "guilt");
			Scribe_Deep.Look(ref royalty, "royalty", this);
			Scribe_Deep.Look(ref relations, "social", this);
			Scribe_Deep.Look(ref psychicEntropy, "psychicEntropy", this);
			Scribe_Deep.Look(ref ownership, "ownership", this);
			Scribe_Deep.Look(ref interactions, "interactions", this);
			Scribe_Deep.Look(ref skills, "skills", this);
			Scribe_Deep.Look(ref abilities, "abilities", this);
			Scribe_Deep.Look(ref workSettings, "workSettings", this);
			Scribe_Deep.Look(ref trader, "trader", this);
			Scribe_Deep.Look(ref outfits, "outfits", this);
			Scribe_Deep.Look(ref drugs, "drugs", this);
			Scribe_Deep.Look(ref foodRestriction, "foodRestriction", this);
			Scribe_Deep.Look(ref timetable, "timetable", this);
			Scribe_Deep.Look(ref playerSettings, "playerSettings", this);
			Scribe_Deep.Look(ref training, "training", this);
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
			if (RaceProps.IsFlesh)
			{
				relations.everSeenByPlayer = true;
			}
			AddictionUtility.CheckDrugAddictionTeachOpportunity(this);
			if (needs != null && needs.mood != null && needs.mood.recentMemory != null)
			{
				needs.mood.recentMemory.Notify_Spawned(respawningAfterLoad);
			}
			if (equipment != null)
			{
				equipment.Notify_PawnSpawned();
			}
			if (respawningAfterLoad)
			{
				return;
			}
			records.AccumulateStoryEvent(StoryEventDefOf.Seen);
			Find.GameEnder.CheckOrUpdateGameOver();
			if (base.Faction == Faction.OfPlayer)
			{
				Find.StoryWatcher.statsRecord.UpdateGreatestPopulation();
				Find.World.StoryState.RecordPopulationIncrease();
			}
			PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(this);
			if (!this.IsQuestLodger())
			{
				return;
			}
			for (int num = health.hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				if (health.hediffSet.hediffs[num].def.removeOnQuestLodgers)
				{
					health.RemoveHediff(health.hediffSet.hediffs[num]);
				}
			}
		}

		public override void PostMapInit()
		{
			base.PostMapInit();
			pather.TryResumePathingAfterLoading();
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Drawer.DrawAt(drawLoc);
		}

		public override void DrawGUIOverlay()
		{
			Drawer.ui.DrawPawnGUIOverlay();
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (IsColonistPlayerControlled)
			{
				if (pather.curPath != null)
				{
					pather.curPath.DrawPath(this);
				}
				jobs.DrawLinesBetweenTargets();
			}
		}

		public override void TickRare()
		{
			base.TickRare();
			if (!base.Suspended)
			{
				if (apparel != null)
				{
					apparel.ApparelTrackerTickRare();
				}
				inventory.InventoryTrackerTickRare();
			}
			if (training != null)
			{
				training.TrainingTrackerTickRare();
			}
			if (base.Spawned && RaceProps.IsFlesh)
			{
				GenTemperature.PushHeat(this, 0.3f * BodySize * 4.16666651f * (def.race.Humanlike ? 1f : 0.6f));
			}
		}

		public override void Tick()
		{
			if (DebugSettings.noAnimals && base.Spawned && RaceProps.Animal)
			{
				Destroy();
				return;
			}
			base.Tick();
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				TickRare();
			}
			bool suspended = base.Suspended;
			if (!suspended)
			{
				if (base.Spawned)
				{
					pather.PatherTick();
				}
				if (base.Spawned)
				{
					stances.StanceTrackerTick();
					verbTracker.VerbsTick();
					natives.NativeVerbsTick();
				}
				if (base.Spawned)
				{
					jobs.JobTrackerTick();
				}
				if (base.Spawned)
				{
					Drawer.DrawTrackerTick();
					rotationTracker.RotationTrackerTick();
				}
				health.HealthTick();
				if (!Dead)
				{
					mindState.MindStateTick();
					carryTracker.CarryHandsTick();
				}
			}
			if (!Dead)
			{
				needs.NeedsTrackerTick();
			}
			if (!suspended)
			{
				if (equipment != null)
				{
					equipment.EquipmentTrackerTick();
				}
				if (apparel != null)
				{
					apparel.ApparelTrackerTick();
				}
				if (interactions != null && base.Spawned)
				{
					interactions.InteractionsTrackerTick();
				}
				if (caller != null)
				{
					caller.CallTrackerTick();
				}
				if (skills != null)
				{
					skills.SkillsTick();
				}
				if (abilities != null)
				{
					abilities.AbilitiesTick();
				}
				if (inventory != null)
				{
					inventory.InventoryTrackerTick();
				}
				if (drafter != null)
				{
					drafter.DraftControllerTick();
				}
				if (relations != null)
				{
					relations.RelationsTrackerTick();
				}
				if (psychicEntropy != null)
				{
					psychicEntropy.PsychicEntropyTrackerTick();
				}
				if (RaceProps.Humanlike)
				{
					guest.GuestTrackerTick();
				}
				if (royalty != null && ModsConfig.RoyaltyActive)
				{
					royalty.RoyaltyTrackerTick();
				}
				ageTracker.AgeTick();
				records.RecordsTick();
			}
		}

		public void TickMothballed(int interval)
		{
			if (!base.Suspended)
			{
				ageTracker.AgeTickMothballed(interval);
				records.RecordsTickMothballed(interval);
			}
		}

		public void Notify_Teleported(bool endCurrentJob = true, bool resetTweenedPos = true)
		{
			if (resetTweenedPos)
			{
				Drawer.tweener.ResetTweenedPosToRoot();
			}
			pather.Notify_Teleported_Int();
			if (endCurrentJob && jobs != null && jobs.curJob != null)
			{
				jobs.EndCurrentJob(JobCondition.InterruptForced);
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
				ClearMind();
			}
			if (relations != null)
			{
				relations.Notify_PassedToWorld();
			}
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
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
			if (dinfo.Def.makesBlood && !dinfo.InstantPermanentInjury && totalDamageDealt > 0f && Rand.Chance(0.5f))
			{
				health.DropBloodFilth();
			}
			records.AccumulateStoryEvent(StoryEventDefOf.DamageTaken);
			health.PostApplyDamage(dinfo, totalDamageDealt);
			if (!Dead)
			{
				mindState.Notify_DamageTaken(dinfo);
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

		private int TicksPerMove(bool diagonal)
		{
			float num = this.GetStatValue(StatDefOf.MoveSpeed);
			if (RestraintsUtility.InRestraints(this))
			{
				num *= 0.35f;
			}
			if (carryTracker != null && carryTracker.CarriedThing != null && carryTracker.CarriedThing.def.category == ThingCategory.Pawn)
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
			return Mathf.Clamp(Mathf.RoundToInt(num3), 1, 450);
		}

		public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			IntVec3 positionHeld = base.PositionHeld;
			Map map = base.Map;
			Map mapHeld = base.MapHeld;
			bool flag = base.Spawned;
			bool spawnedOrAnyParentSpawned = base.SpawnedOrAnyParentSpawned;
			bool wasWorldPawn = this.IsWorldPawn();
			Caravan caravan = this.GetCaravan();
			Building_Grave assignedGrave = null;
			if (ownership != null)
			{
				assignedGrave = ownership.AssignedGrave;
			}
			bool flag2 = this.InBed();
			float bedRotation = 0f;
			if (flag2)
			{
				bedRotation = this.CurrentBed().Rotation.AsAngle;
			}
			ThingOwner thingOwner = null;
			bool inContainerEnclosed = InContainerEnclosed;
			if (inContainerEnclosed)
			{
				thingOwner = holdingOwner;
				thingOwner.Remove(this);
			}
			bool flag3 = false;
			bool flag4 = false;
			if (Current.ProgramState == ProgramState.Playing && map != null)
			{
				flag3 = (map.designationManager.DesignationOn(this, DesignationDefOf.Hunt) != null);
				flag4 = (map.designationManager.DesignationOn(this, DesignationDefOf.Slaughter) != null);
			}
			bool flag5 = PawnUtility.ShouldSendNotificationAbout(this) && (!flag4 || !dinfo.HasValue || dinfo.Value.Def != DamageDefOf.ExecutionCut);
			float num = 0f;
			Thing attachment = this.GetAttachment(ThingDefOf.Fire);
			if (attachment != null)
			{
				num = ((Fire)attachment).CurrentSize();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Died);
			}
			if (IsColonist)
			{
				Find.StoryWatcher.statsRecord.Notify_ColonistKilled();
			}
			if (flag && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(this))
			{
				LifeStageUtility.PlayNearestLifestageSound(this, (LifeStageAge ls) => ls.soundDeath);
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null)
			{
				Pawn pawn = dinfo.Value.Instigator as Pawn;
				if (pawn != null)
				{
					RecordsUtility.Notify_PawnKilled(this, pawn);
					if (IsColonist)
					{
						pawn.records.AccumulateStoryEvent(StoryEventDefOf.KilledPlayer);
					}
				}
			}
			TaleUtility.Notify_PawnDied(this, dinfo);
			if (flag)
			{
				Find.BattleLog.Add(new BattleLogEntry_StateTransition(this, RaceProps.DeathActionWorker.DeathRules, dinfo.HasValue ? (dinfo.Value.Instigator as Pawn) : null, exactCulprit, dinfo.HasValue ? dinfo.Value.HitPart : null));
			}
			health.surgeryBills.Clear();
			if (apparel != null)
			{
				apparel.Notify_PawnKilled(dinfo);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnKilled(dinfo, map);
			}
			meleeVerbs.Notify_PawnKilled();
			for (int i = 0; i < health.hediffSet.hediffs.Count; i++)
			{
				health.hediffSet.hediffs[i].Notify_PawnKilled();
			}
			Pawn_CarryTracker pawn_CarryTracker = base.ParentHolder as Pawn_CarryTracker;
			if (pawn_CarryTracker != null && holdingOwner.TryDrop(this, pawn_CarryTracker.pawn.Position, pawn_CarryTracker.pawn.Map, ThingPlaceMode.Near, out Thing _))
			{
				map = pawn_CarryTracker.pawn.Map;
				flag = true;
			}
			PawnDiedOrDownedThoughtsUtility.RemoveLostThoughts(this);
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(this, dinfo, PawnDiedOrDownedThoughtsKind.Died);
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
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.IncappedOrKilled, dinfo);
			if (flag)
			{
				DropAndForbidEverything();
			}
			if (flag)
			{
				DeSpawn();
			}
			if (royalty != null)
			{
				royalty.Notify_PawnKilled();
			}
			Corpse corpse = null;
			if (!PawnGenerator.IsBeingGenerated(this))
			{
				if (inContainerEnclosed)
				{
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
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
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
					if (GenPlace.TryPlaceThing(corpse, positionHeld, mapHeld, ThingPlaceMode.Direct))
					{
						corpse.Rotation = base.Rotation;
						if (HuntJobUtility.WasKilledByHunter(this, dinfo))
						{
							((Pawn)dinfo.Value.Instigator).Reserve(corpse, ((Pawn)dinfo.Value.Instigator).CurJob);
						}
						else if (!flag3 && !flag4)
						{
							corpse.SetForbiddenIfOutsideHomeArea();
						}
						if (num > 0f)
						{
							FireUtility.TryStartFireIn(corpse.Position, corpse.Map, num);
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
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
					caravan.AddPawnOrItem(corpse, addCarriedPawnToWorldPawnsIfAny: true);
				}
				else if (holdingOwner != null || this.IsWorldPawn())
				{
					Corpse.PostCorpseDestroy(this);
				}
				else
				{
					corpse = MakeCorpse(assignedGrave, flag2, bedRotation);
				}
			}
			if (corpse != null)
			{
				Hediff firstHediffOfDef = health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				CompRottable comp = corpse.GetComp<CompRottable>();
				if (firstHediffOfDef != null && Rand.Value < firstHediffOfDef.Severity)
				{
					comp?.RotImmediately();
				}
			}
			if (!base.Destroyed)
			{
				Destroy(DestroyMode.KillFinalize);
			}
			PawnComponentsUtility.RemoveComponentsOnKilled(this);
			health.hediffSet.DirtyCache();
			PortraitsCache.SetDirty(this);
			for (int j = 0; j < health.hediffSet.hediffs.Count; j++)
			{
				health.hediffSet.hediffs[j].Notify_PawnDied();
			}
			FactionOrExtraHomeFaction?.Notify_MemberDied(this, dinfo, wasWorldPawn, mapHeld);
			if (corpse != null)
			{
				if (RaceProps.DeathActionWorker != null && flag)
				{
					RaceProps.DeathActionWorker.PawnDied(corpse);
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
				GenHostility.Notify_PawnLostForTutor(this, mapHeld);
			}
			if (base.Faction != null && base.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			if (flag5)
			{
				health.NotifyPlayerOfKilled(dinfo, exactCulprit, caravan);
			}
			Find.QuestManager.Notify_PawnKilled(this, dinfo);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != 0 && mode != DestroyMode.KillFinalize)
			{
				Log.Error("Destroyed pawn " + this + " with unsupported mode " + mode + ".");
			}
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
			ClearMind(ifLayingKeepLaying: false, clearInspiration: true);
			Lord lord = this.GetLord();
			if (lord != null)
			{
				PawnLostCondition cond = (mode != DestroyMode.KillFinalize) ? PawnLostCondition.Vanished : PawnLostCondition.IncappedOrKilled;
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
			if (mode != DestroyMode.KillFinalize)
			{
				if (equipment != null)
				{
					equipment.DestroyAllEquipment();
				}
				inventory.DestroyAll();
				if (apparel != null)
				{
					apparel.DestroyAll();
				}
			}
			WorldPawns worldPawns = Find.WorldPawns;
			if (!worldPawns.IsBeingDiscarded(this) && !worldPawns.Contains(this))
			{
				worldPawns.PassToWorld(this);
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			if (jobs != null && jobs.curJob != null)
			{
				jobs.StopAll();
			}
			base.DeSpawn(mode);
			if (pather != null)
			{
				pather.StopDead();
			}
			if (needs != null && needs.mood != null)
			{
				needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
			}
			if (meleeVerbs != null)
			{
				meleeVerbs.Notify_PawnDespawned();
			}
			ClearAllReservations(releaseDestinationsOnlyIfObsolete: false);
			map?.mapPawns.DeRegisterPawn(this);
			PawnComponentsUtility.RemoveComponentsOnDespawned(this);
		}

		public override void Discard(bool silentlyRemoveReferences = false)
		{
			if (Find.WorldPawns.Contains(this))
			{
				Log.Warning("Tried to discard a world pawn " + this + ".");
				return;
			}
			base.Discard(silentlyRemoveReferences);
			if (relations != null)
			{
				relations.ClearAllRelations();
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
				if (item.needs != null && item.needs.mood != null)
				{
					item.needs.mood.thoughts.memories.Notify_PawnDiscarded(this);
				}
			}
			Corpse.PostCorpseDestroy(this);
		}

		public Corpse MakeCorpse(Building_Grave assignedGrave, bool inBed, float bedRotation)
		{
			if (holdingOwner != null)
			{
				Log.Warning("We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" + base.ParentHolder);
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

		public void ExitMap(bool allowedToJoinOrCreateCaravan, Rot4 exitDir)
		{
			if (this.IsWorldPawn())
			{
				Log.Warning("Called ExitMap() on world pawn " + this);
				return;
			}
			if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this))
			{
				CaravanExitMapUtility.ExitMapAndJoinOrCreateCaravan(this, exitDir);
				return;
			}
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ExitedMap);
			if (carryTracker != null && carryTracker.CarriedThing != null)
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
						carryTracker.innerContainer.Remove(pawn);
						pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
					}
				}
				else
				{
					carryTracker.CarriedThing.Destroy();
				}
				carryTracker.innerContainer.Clear();
			}
			bool flag = !this.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(this) && (!IsPrisoner || base.ParentHolder == null || base.ParentHolder is CompShuttle || (guest != null && guest.Released));
			if (base.Faction != null)
			{
				base.Faction.Notify_MemberExitedMap(this, flag);
			}
			if (ownership != null && flag)
			{
				ownership.UnclaimAll();
			}
			if (guest != null)
			{
				bool isPrisonerOfColony = IsPrisonerOfColony;
				if (flag)
				{
					guest.SetGuestStatus(null);
				}
				guest.Released = false;
				if (isPrisonerOfColony)
				{
					guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
				}
			}
			if (base.Spawned)
			{
				DeSpawn();
			}
			inventory.UnloadEverything = false;
			if (flag)
			{
				ClearMind();
			}
			if (relations != null)
			{
				relations.Notify_ExitedMap();
			}
			Find.WorldPawns.PassToWorld(this);
			QuestUtility.SendQuestTargetSignals(questTags, "LeftMap", this.Named("SUBJECT"));
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			if (base.SpawnedOrAnyParentSpawned)
			{
				DropAndForbidEverything();
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (action == TradeAction.PlayerSells)
			{
				Faction faction = this.GetExtraHomeFaction() ?? this.GetExtraHostFaction();
				if (faction != null && faction != Faction.OfPlayer)
				{
					faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: true, "GoodwillChangedReason_SoldPawn".Translate(this), this);
				}
			}
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			switch (action)
			{
			case TradeAction.PlayerBuys:
				if (needs.mood != null)
				{
					needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FreedFromSlavery);
				}
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
				if (RaceProps.Humanlike)
				{
					GenGuest.AddPrisonerSoldThoughts(this);
				}
				break;
			}
			ClearMind();
		}

		public void PreKidnapped(Pawn kidnapper)
		{
			Find.Storyteller.Notify_PawnEvent(this, AdaptationEvent.Kidnapped);
			if (IsColonist && kidnapper != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.KidnappedColonist, kidnapper, this);
			}
			if (ownership != null)
			{
				ownership.UnclaimAll();
			}
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			if (RaceProps.IsFlesh)
			{
				relations.Notify_PawnKidnapped();
			}
			ClearMind();
		}

		public override void SetFaction(Faction newFaction, Pawn recruiter = null)
		{
			if (newFaction == base.Faction)
			{
				Log.Warning("Used SetFaction to change " + this.ToStringSafe() + " to same faction " + newFaction.ToStringSafe());
				return;
			}
			Faction faction = base.Faction;
			if (guest != null)
			{
				guest.SetGuestStatus(null);
			}
			if (base.Spawned)
			{
				base.Map.mapPawns.DeRegisterPawn(this);
				base.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(this);
				base.Map.designationManager.RemoveAllDesignationsOn(this);
			}
			if ((newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer) && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
			this.GetLord()?.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
			if (PawnUtility.IsFactionLeader(this) && newFaction != PawnUtility.GetFactionLeaderFaction(this) && !this.HasExtraHomeFaction(PawnUtility.GetFactionLeaderFaction(this)))
			{
				base.Faction.Notify_LeaderLost();
			}
			if (newFaction == Faction.OfPlayer && RaceProps.Humanlike && !this.IsQuestLodger())
			{
				ChangeKind(newFaction.def.basicMemberKind);
			}
			base.SetFaction(newFaction);
			PawnComponentsUtility.AddAndRemoveDynamicComponents(this);
			if (base.Faction != null && base.Faction.IsPlayer)
			{
				if (workSettings != null)
				{
					workSettings.EnableAndInitialize();
				}
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
			if (playerSettings != null)
			{
				playerSettings.ResetMedicalCare();
			}
			ClearMind(ifLayingKeepLaying: true);
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
			if (needs != null)
			{
				needs.AddOrRemoveNeedsAsAppropriate();
			}
			if (playerSettings != null)
			{
				playerSettings.Notify_FactionChanged();
			}
			if (relations != null)
			{
				relations.Notify_ChangedFaction();
			}
			if (RaceProps.Animal && newFaction == Faction.OfPlayer)
			{
				training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
				training.Train(TrainableDefOf.Tameness, recruiter, complete: true);
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
		}

		public void ClearMind(bool ifLayingKeepLaying = false, bool clearInspiration = false, bool clearMentalState = true)
		{
			if (pather != null)
			{
				pather.StopDead();
			}
			if (mindState != null)
			{
				mindState.Reset(clearInspiration, clearMentalState);
			}
			if (jobs != null)
			{
				jobs.StopAll(ifLayingKeepLaying);
			}
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
				maps[i].physicalInteractionReservationManager.ReleaseClaimedBy(this, job);
				maps[i].attackTargetReservationManager.ReleaseClaimedBy(this, job);
			}
		}

		public void VerifyReservations()
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
					Log.ErrorOnce($"Reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj.ToStringSafe()}", 0x5D3DFA5 ^ thingIDNumber);
					flag = true;
				}
				LocalTargetInfo obj2 = maps[i].physicalInteractionReservationManager.FirstReservationFor(this);
				if (obj2.IsValid)
				{
					Log.ErrorOnce($"Physical interaction reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {obj2.ToStringSafe()}", 0x12ADECD ^ thingIDNumber);
					flag = true;
				}
				IAttackTarget attackTarget = maps[i].attackTargetReservationManager.FirstReservationFor(this);
				if (attackTarget != null)
				{
					Log.ErrorOnce($"Attack target reservation manager failed to clean up properly; {this.ToStringSafe()} still reserving {attackTarget.ToStringSafe()}", 0x5FD7206 ^ thingIDNumber);
					flag = true;
				}
				IntVec3 obj3 = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationFor(this);
				if (obj3.IsValid)
				{
					Job job = maps[i].pawnDestinationReservationManager.FirstObsoleteReservationJobFor(this);
					Log.ErrorOnce($"Pawn destination reservation manager failed to clean up properly; {this.ToStringSafe()}/{job.ToStringSafe()}/{job.def.ToStringSafe()} still reserving {obj3.ToStringSafe()}", 0x1DE312 ^ thingIDNumber);
					flag = true;
				}
			}
			if (flag)
			{
				ClearAllReservations();
			}
		}

		public void DropAndForbidEverything(bool keepInventoryAndEquipmentIfInBed = false)
		{
			if (kindDef.destroyGearOnDrop)
			{
				equipment.DestroyAllEquipment();
				apparel.DestroyAll();
			}
			if (InContainerEnclosed)
			{
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.innerContainer.TryTransferToContainer(carryTracker.CarriedThing, holdingOwner);
				}
				if (equipment != null && equipment.Primary != null)
				{
					equipment.TryTransferEquipmentToContainer(equipment.Primary, holdingOwner);
				}
				if (inventory != null)
				{
					inventory.innerContainer.TryTransferAllToContainer(holdingOwner);
				}
			}
			else
			{
				if (!base.SpawnedOrAnyParentSpawned)
				{
					return;
				}
				if (carryTracker != null && carryTracker.CarriedThing != null)
				{
					carryTracker.TryDropCarriedThing(base.PositionHeld, ThingPlaceMode.Near, out Thing _);
				}
				if (!keepInventoryAndEquipmentIfInBed || !this.InBed())
				{
					if (equipment != null)
					{
						equipment.DropAllEquipment(base.PositionHeld);
					}
					if (inventory != null && inventory.innerContainer.TotalStackCount > 0)
					{
						inventory.DropAllNearPawn(base.PositionHeld, forbid: true);
					}
				}
			}
		}

		public void GenerateNecessaryName()
		{
			if (base.Faction == Faction.OfPlayer && RaceProps.Animal && Name == null)
			{
				Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
			}
		}

		public Verb TryGetAttackVerb(Thing target, bool allowManualCastWeapons = false)
		{
			if (equipment != null && equipment.Primary != null && equipment.PrimaryEq.PrimaryVerb.Available() && ((!equipment.PrimaryEq.PrimaryVerb.verbProps.onlyManualCast || (CurJob != null && CurJob.def != JobDefOf.Wait_Combat)) | allowManualCastWeapons))
			{
				return equipment.PrimaryEq.PrimaryVerb;
			}
			return meleeVerbs.TryGetMeleeVerb(target);
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
			return TryGetAttackVerb(targ.Thing, allowManualCastWeapons)?.TryStartCastOn(targ) ?? false;
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
			if (lifeStage.butcherBodyPart == null || (gender != 0 && (gender != Gender.Male || !lifeStage.butcherBodyPart.allowMale) && (gender != Gender.Female || !lifeStage.butcherBodyPart.allowFemale)))
			{
				yield break;
			}
			while (true)
			{
				BodyPartRecord bodyPartRecord = (from x in health.hediffSet.GetNotMissingParts()
					where x.IsInGroup(lifeStage.butcherBodyPart.bodyPartGroup)
					select x).FirstOrDefault();
				if (bodyPartRecord != null)
				{
					health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this, bodyPartRecord));
					yield return (lifeStage.butcherBodyPart.thing == null) ? ThingMaker.MakeThing(bodyPartRecord.def.spawnThingOnRemoved) : ThingMaker.MakeThing(lifeStage.butcherBodyPart.thing);
					continue;
				}
				break;
			}
		}

		public string MainDesc(bool writeFaction)
		{
			bool flag = base.Faction == null || !base.Faction.IsPlayer;
			string text = (gender == Gender.None) ? "" : gender.GetLabel(this.AnimalOrWildMan());
			if (RaceProps.Animal || RaceProps.IsMechanoid)
			{
				string str = GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
				if (Name != null)
				{
					text = text + " " + str;
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
			if ((!RaceProps.Animal && !RaceProps.IsMechanoid) & flag)
			{
				if (text.Length > 0)
				{
					text += ", ";
				}
				text += GenLabel.BestKindLabel(this, mustNoteGender: false, mustNoteLifeStage: true);
			}
			if (writeFaction)
			{
				tmpExtraFactions.Clear();
				QuestUtility.GetExtraFactionsFromQuestParts(this, tmpExtraFactions);
				if (base.Faction != null && !base.Faction.def.hidden)
				{
					text = ((tmpExtraFactions.Count != 0) ? "PawnMainDescUnderFactionedWrap".Translate(text, base.Faction.NameColored).Resolve() : "PawnMainDescFactionedWrap".Translate(text, base.Faction.NameColored).Resolve());
				}
				for (int i = 0; i < tmpExtraFactions.Count; i++)
				{
					text += $"\n{tmpExtraFactions[i].factionType.GetLabel().CapitalizeFirst()}: {tmpExtraFactions[i].faction.NameColored.Resolve()}";
				}
				tmpExtraFactions.Clear();
			}
			return text.CapitalizeFirst();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(MainDesc(writeFaction: true));
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
				stringBuilder.AppendLine(MentalState.InspectLine);
			}
			states.Clear();
			if (stances != null && stances.stunner != null && stances.stunner.Stunned)
			{
				states.AddDistinct("StunLower".Translate());
			}
			if (health != null && health.hediffSet != null)
			{
				List<Hediff> hediffs = health.hediffSet.hediffs;
				for (int i = 0; i < hediffs.Count; i++)
				{
					Hediff hediff = hediffs[i];
					if (!hediff.def.battleStateLabel.NullOrEmpty())
					{
						states.AddDistinct(hediff.def.battleStateLabel);
					}
				}
			}
			if (states.Count > 0)
			{
				states.Sort();
				stringBuilder.AppendLine(string.Format("{0}: {1}", "State".Translate(), states.ToCommaList().CapitalizeFirst()));
				states.Clear();
			}
			if (Inspired)
			{
				stringBuilder.AppendLine(Inspiration.InspectLine);
			}
			if (equipment != null && equipment.Primary != null)
			{
				stringBuilder.AppendLine("Equipped".TranslateSimple() + ": " + ((equipment.Primary != null) ? equipment.Primary.Label : "EquippedNothing".TranslateSimple()).CapitalizeFirst());
			}
			if (carryTracker != null && carryTracker.CarriedThing != null)
			{
				stringBuilder.Append("Carrying".Translate() + ": ");
				stringBuilder.AppendLine(carryTracker.CarriedThing.LabelCap);
			}
			if ((base.Faction == Faction.OfPlayer || HostFaction == Faction.OfPlayer) && !InMentalState)
			{
				string text = null;
				Lord lord = this.GetLord();
				if (lord != null && lord.LordJob != null)
				{
					text = lord.LordJob.GetReport(this);
				}
				if (jobs.curJob != null)
				{
					try
					{
						string text2 = jobs.curDriver.GetReport().CapitalizeFirst();
						text = (text.NullOrEmpty() ? text2 : (text + ": " + text2));
					}
					catch (Exception arg)
					{
						Log.Error("JobDriver.GetReport() exception: " + arg);
					}
				}
				if (!text.NullOrEmpty())
				{
					stringBuilder.AppendLine(text);
				}
			}
			if (jobs.curJob != null && jobs.jobQueue.Count > 0)
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
				catch (Exception arg2)
				{
					Log.Error("JobDriver.GetReport() exception: " + arg2);
				}
			}
			if (RestraintsUtility.ShouldShowRestraintsInfo(this))
			{
				stringBuilder.AppendLine("InRestraints".Translate());
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (IsColonistPlayerControlled)
			{
				foreach (Gizmo gizmo in base.GetGizmos())
				{
					yield return gizmo;
				}
				if (drafter != null)
				{
					foreach (Gizmo gizmo2 in drafter.GetGizmos())
					{
						yield return gizmo2;
					}
				}
				foreach (Gizmo attackGizmo in PawnAttackGizmoUtility.GetAttackGizmos(this))
				{
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
			if (psychicEntropy != null && psychicEntropy.NeedToShowGizmo())
			{
				yield return psychicEntropy.GetGizmo();
			}
			if (IsColonistPlayerControlled)
			{
				if (abilities != null)
				{
					foreach (Gizmo gizmo4 in abilities.GetGizmos())
					{
						yield return gizmo4;
					}
				}
				if (playerSettings != null)
				{
					foreach (Gizmo gizmo5 in playerSettings.GetGizmos())
					{
						yield return gizmo5;
					}
				}
			}
			if (apparel != null)
			{
				foreach (Gizmo gizmo6 in apparel.GetGizmos())
				{
					yield return gizmo6;
				}
			}
			foreach (Gizmo gizmo7 in mindState.GetGizmos())
			{
				yield return gizmo7;
			}
			if (royalty != null && IsColonistPlayerControlled)
			{
				if (royalty.HasAidPermit)
				{
					yield return royalty.RoyalAidGizmo();
				}
				foreach (RoyalTitle item in royalty.AllTitlesForReading)
				{
					if (item.def.permits != null)
					{
						Faction faction = item.faction;
						foreach (RoyalTitlePermitDef permit in item.def.permits)
						{
							IEnumerable<Gizmo> pawnGizmos = permit.Worker.GetPawnGizmos(this, faction);
							if (pawnGizmos != null)
							{
								foreach (Gizmo item2 in pawnGizmos)
								{
									yield return item2;
								}
							}
						}
					}
				}
			}
			foreach (Gizmo questRelatedGizmo in QuestUtility.GetQuestRelatedGizmos(this))
			{
				yield return questRelatedGizmo;
			}
		}

		public virtual IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
		{
			yield break;
		}

		public override TipSignal GetTooltip()
		{
			string value = "";
			string text = "";
			if (gender != 0)
			{
				value = (LabelCap.EqualsIgnoreCase(KindLabel) ? this.GetGenderLabel() : ((string)"PawnTooltipGenderAndKindLabel".Translate(this.GetGenderLabel(), KindLabel)));
			}
			else if (!LabelCap.EqualsIgnoreCase(KindLabel))
			{
				value = KindLabel;
			}
			string generalConditionLabel = HealthUtility.GetGeneralConditionLabel(this);
			bool flag = !string.IsNullOrEmpty(value);
			text = ((equipment != null && equipment.Primary != null) ? ((!flag) ? ((string)"PawnTooltipWithPrimaryEquipNoDesc".Translate(LabelCap, value, generalConditionLabel)) : ((string)"PawnTooltipWithDescAndPrimaryEquip".Translate(LabelCap, value, equipment.Primary.LabelCap, generalConditionLabel))) : ((!flag) ? ((string)"PawnTooltipNoDescNoPrimaryEquip".Translate(LabelCap, generalConditionLabel)) : ((string)"PawnTooltipWithDescNoPrimaryEquip".Translate(LabelCap, value, generalConditionLabel))));
			return new TipSignal(text, thingIDNumber * 152317, TooltipPriority.Pawn);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "BodySize".Translate(), BodySize.ToString("F2"), "Stat_Race_BodySize_Desc".Translate(), 500);
			if (this.IsWildMan())
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Wildness".Translate(), 0.75f.ToStringPercent(), TrainableUtility.GetWildnessExplanation(def), 2050);
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

		public bool AnythingToStrip()
		{
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

		public void Strip()
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
				return;
			}
			IntVec3 pos = (Corpse != null) ? Corpse.PositionHeld : base.PositionHeld;
			if (equipment != null)
			{
				equipment.DropAllEquipment(pos, forbid: false);
			}
			if (apparel != null)
			{
				apparel.DropAll(pos, forbid: false, base.Destroyed);
			}
			if (inventory != null)
			{
				inventory.DropAllNearPawn(pos);
			}
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

		public void HearClamor(Thing source, ClamorDef type)
		{
			if (Dead || Downed)
			{
				return;
			}
			if (type == ClamorDefOf.Movement)
			{
				Pawn pawn = source as Pawn;
				if (pawn != null)
				{
					CheckForDisturbedSleep(pawn);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Harm && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction == source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Construction && base.Faction != Faction.OfPlayer && !this.Awake() && base.Faction != source.Faction && HostFaction == null)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
				if (CurJob != null)
				{
					jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				NotifyLordOfClamor(source, type);
			}
			if (type == ClamorDefOf.Ability && base.Faction != Faction.OfPlayer && base.Faction != source.Faction && HostFaction == null)
			{
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
			if (type == ClamorDefOf.Impact)
			{
				mindState.canSleepTick = Find.TickManager.TicksGame + 1000;
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

		public override void Notify_Explosion(Explosion explosion)
		{
			base.Notify_Explosion(explosion);
			mindState.Notify_Explosion(explosion);
		}

		private void CheckForDisturbedSleep(Pawn source)
		{
			if (needs.mood != null && !this.Awake() && base.Faction == Faction.OfPlayer && Find.TickManager.TicksGame >= lastSleepDisturbedTick + 300 && (source == null || (!LovePartnerRelationUtility.LovePartnerRelationExists(this, source) && !(source.RaceProps.petness > 0f) && (source.relations == null || !source.relations.DirectRelations.Any((DirectPawnRelation dr) => dr.def == PawnRelationDefOf.Bond)))))
			{
				lastSleepDisturbedTick = Find.TickManager.TicksGame;
				needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed);
			}
		}

		public float GetAcceptArrestChance(Pawn arrester)
		{
			float num = StatDefOf.ArrestSuccessChance.Worker.IsDisabledFor(arrester) ? StatDefOf.ArrestSuccessChance.valueIfMissing : arrester.GetStatValue(StatDefOf.ArrestSuccessChance);
			if (this.IsWildMan())
			{
				return num * 0.5f;
			}
			return num;
		}

		public bool CheckAcceptArrest(Pawn arrester)
		{
			if (health.Downed)
			{
				return true;
			}
			if (WorkTagIsDisabled(WorkTags.Violent))
			{
				return true;
			}
			Faction factionOrExtraHomeFaction = FactionOrExtraHomeFaction;
			if (factionOrExtraHomeFaction != null && factionOrExtraHomeFaction != arrester.factionInt)
			{
				factionOrExtraHomeFaction.Notify_MemberCaptured(this, arrester.Faction);
			}
			float acceptArrestChance = GetAcceptArrestChance(arrester);
			if (Rand.Value < acceptArrestChance)
			{
				return true;
			}
			Messages.Message("MessageRefusedArrest".Translate(LabelShort, this), this, MessageTypeDefOf.ThreatSmall);
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
			if (Downed)
			{
				if (disabledFor == null)
				{
					return true;
				}
				Pawn pawn = disabledFor.Thing as Pawn;
				if (pawn == null || pawn.mindState == null || pawn.mindState.duty == null || !pawn.mindState.duty.attackDownedIfStarving || !pawn.Starving())
				{
					return true;
				}
			}
			if (this.IsInvisible())
			{
				return true;
			}
			return false;
		}

		public List<WorkTypeDef> GetDisabledWorkTypes(bool permanentOnly = false)
		{
			if (permanentOnly)
			{
				if (cachedDisabledWorkTypesPermanent == null)
				{
					cachedDisabledWorkTypesPermanent = new List<WorkTypeDef>();
				}
				FillList(cachedDisabledWorkTypesPermanent);
				return cachedDisabledWorkTypesPermanent;
			}
			if (cachedDisabledWorkTypes == null)
			{
				cachedDisabledWorkTypes = new List<WorkTypeDef>();
			}
			FillList(cachedDisabledWorkTypes);
			return cachedDisabledWorkTypes;
			void FillList(List<WorkTypeDef> list)
			{
				if (story != null)
				{
					foreach (Backstory allBackstory in story.AllBackstories)
					{
						foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
						{
							if (!list.Contains(disabledWorkType))
							{
								list.Add(disabledWorkType);
							}
						}
					}
					for (int i = 0; i < story.traits.allTraits.Count; i++)
					{
						foreach (WorkTypeDef disabledWorkType2 in story.traits.allTraits[i].GetDisabledWorkTypes())
						{
							if (!list.Contains(disabledWorkType2))
							{
								list.Add(disabledWorkType2);
							}
						}
					}
				}
				if (royalty != null && !permanentOnly)
				{
					foreach (RoyalTitle item in royalty.AllTitlesForReading)
					{
						if (item.conceited)
						{
							foreach (WorkTypeDef disabledWorkType3 in item.def.DisabledWorkTypes)
							{
								if (!list.Contains(disabledWorkType3))
								{
									list.Add(disabledWorkType3);
								}
							}
						}
					}
				}
			}
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
			workSettings?.Notify_DisabledWorkTypesChanged();
		}

		public bool WorkTagIsDisabled(WorkTags w)
		{
			return (CombinedDisabledWorkTags & w) != 0;
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (InAggroMentalState || (base.Faction.HostileTo(Faction.OfPlayer) && HostFaction == null && !Downed && !InMentalState))
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
				if (kindDef == PawnKindDefOf.WildMan)
				{
					mindState.WildManEverReachedOutside = false;
					ReachabilityUtility.ClearCacheFor(this);
				}
			}
		}
	}
}
