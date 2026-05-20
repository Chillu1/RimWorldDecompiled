using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

public class UnnaturalCorpseTracker : IExposable, ILoadReferenceable
{
	private static readonly FloatRange GrowingLetterDelayDays = new FloatRange(11f, 13f);

	private static readonly FloatRange AwakenDelayDays = new FloatRange(1f, 3f);

	private static readonly FloatRange DestroyResearchRequiredRange = new FloatRange(8f, 12f);

	private const float FirstLetterResearchRequired = 3f;

	private const int PawnDiedDisappearHours = 3;

	private const int PawnKilledDisappearTicks = 600;

	private const int MTBReappearHoursCorpse = 24;

	private const int MTBReappearHoursAwoken = 6;

	private const int MTBMentalBreakDays = 5;

	private const int MTBEscapeContainerHours = 24;

	private const int MinMentalBreakRefireTicks = 150000;

	private const int RisingTime = 420;

	private static readonly SimpleCurve CombatPointsPerHpRegenCurve = new SimpleCurve
	{
		new CurvePoint(300f, 150f),
		new CurvePoint(3000f, 800f)
	};

	private static readonly SimpleCurve CombatPointsSecondsToKillCurve = new SimpleCurve
	{
		new CurvePoint(300f, 25f),
		new CurvePoint(3000f, 10f)
	};

	private UnnaturalCorpse corpse;

	private Pawn awokenPawn;

	private Pawn haunted;

	private int disappearTick;

	private int lastSpawnInCheck;

	private int loadID;

	private int awakenTick;

	private int spawnedTick;

	private int minMentalBreakRefireTick;

	private int ticksToKill;

	private float research;

	private float researchToDestroy;

	private bool teleportIn;

	private bool escape;

	private bool mentalBreak;

	private bool removing;

	private bool sentLetterOne;

	private bool sentLetterTwo;

	private bool sentAwakeningLetter;

	private bool awoken;

	private bool rising;

	private bool awokenFinished;

	private List<ChoiceLetter> letters = new List<ChoiceLetter>();

	private bool applyThoughtNextTick;

	private Effecter riseEffecter;

	private Sustainer riseSustainer;

	public UnnaturalCorpse Corpse => corpse;

	public bool CorpseSpawned => corpse != null;

	public Pawn AwokenPawn => awokenPawn;

	public int SpawnedTicks => GenTicks.TicksGame - spawnedTick;

	public int TicksToKill => ticksToKill;

	public bool ShouldDisappear
	{
		get
		{
			if (!awokenFinished)
			{
				if (disappearTick != 0)
				{
					return GenTicks.TicksGame >= disappearTick;
				}
				return false;
			}
			return true;
		}
	}

	public Pawn Haunted => haunted;

	public bool CanDestroyViaResearch => research >= researchToDestroy;

	public IReadOnlyList<ChoiceLetter> Letters => letters;

	public UnnaturalCorpseTracker()
	{
	}

	public UnnaturalCorpseTracker(Pawn haunted, UnnaturalCorpse corpse)
	{
		this.haunted = haunted;
		this.corpse = corpse;
		loadID = haunted.thingIDNumber;
		researchToDestroy = DestroyResearchRequiredRange.RandomInRange;
		awakenTick = GenTicks.TicksGame + Mathf.CeilToInt(GrowingLetterDelayDays.RandomInRange * 60000f);
		spawnedTick = GenTicks.TicksGame;
		corpse.LinkToTracker(this);
	}

	public void Notify_Finished()
	{
		removing = true;
		if (awokenPawn != null && awokenPawn.Spawned)
		{
			Messages.Message("MessageAwokenDisappeared".Translate(), MessageTypeDefOf.NeutralEvent);
			SkipUtility.SkipDeSpawn(awokenPawn);
			awokenPawn.Destroy();
		}
		else if (Corpse != null)
		{
			Corpse.DoDisappear();
		}
	}

	public void Notify_CorpseDestroyed()
	{
		if (!removing && !Haunted.DestroyedOrNull())
		{
			applyThoughtNextTick = true;
		}
		lastSpawnInCheck = GenTicks.TicksGame;
		corpse = null;
	}

	public void Notify_AwokenAttackStarting()
	{
		if (awokenPawn != null && awokenPawn.health.hediffSet.TryGetHediff(HediffDefOf.AwokenCorpse, out var hediff))
		{
			hediff.TryGetComp<HediffComp_MessageStageIncreased>().sendMessages = false;
		}
	}

	private void ApplyCorpseDestroyedThoughts()
	{
		applyThoughtNextTick = false;
		haunted.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.UnnaturalCorpseDestroyed);
		if (haunted.health.hediffSet.TryGetHediff(HediffDefOf.CorpseTorment, out var hediff))
		{
			hediff.TryGetComp<HediffComp_Disappears>().ResetElapsedTicks();
			return;
		}
		haunted.health.AddHediff(HediffDefOf.CorpseTorment);
		Messages.Message("MessagePawnFainted".Translate(Haunted.Named("PAWN")), Haunted, MessageTypeDefOf.NegativeHealthEvent);
	}

	public void Notify_PawnDied()
	{
		if (disappearTick == 0)
		{
			if (awoken)
			{
				disappearTick = GenTicks.TicksGame + 600;
			}
			else
			{
				disappearTick = GenTicks.TicksGame + 7500;
			}
		}
	}

	public void Notify_PawnKilledViaAwoken()
	{
		disappearTick = GenTicks.TicksGame + 600;
		Messages.Message("MessageAwokenKilledVictim".Translate(Haunted.Named("PAWN")), Haunted, MessageTypeDefOf.NegativeEvent);
	}

	public void CorpseTick()
	{
		if (disappearTick == 0 && Haunted.SpawnedOrAnyParentSpawned && Haunted.IsMutant)
		{
			disappearTick = GenTicks.TicksGame + 7500;
		}
		if (awoken)
		{
			AwokenTick();
			return;
		}
		if (applyThoughtNextTick)
		{
			ApplyCorpseDestroyedThoughts();
		}
		UpdateCorpseState();
		UpdateMentalBreak();
		AwakeningTick();
	}

	private void UpdateMentalBreak()
	{
		if (Haunted.SpawnedOrAnyParentSpawned && CorpseSpawned)
		{
			if (!mentalBreak && !Haunted.InMentalState && GenTicks.TicksGame >= minMentalBreakRefireTick && Rand.MTBEventOccurs(5f, 60000f, 1f))
			{
				mentalBreak = true;
			}
			if (mentalBreak)
			{
				TryTriggerMentalBreak();
			}
		}
	}

	public void TryTriggerMentalBreak()
	{
		MentalBreakDef breakDef;
		if (MentalBreakDefOf.CorpseObsession.Worker.BreakCanOccur(Haunted))
		{
			mentalBreak = false;
			minMentalBreakRefireTick = GenTicks.TicksGame + 150000;
			Haunted.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_MysteriousCorpse".Translate(), MentalBreakDefOf.CorpseObsession);
		}
		else if (Haunted.mindState.mentalBreaker.TryGetRandomMentalBreak(MentalBreakIntensity.Major, out breakDef))
		{
			mentalBreak = false;
			minMentalBreakRefireTick = GenTicks.TicksGame + 150000;
			Haunted.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_MysteriousCorpse".Translate(), breakDef);
		}
	}

	private void AwokenTick()
	{
		if (awokenFinished)
		{
			return;
		}
		if (awokenPawn != null)
		{
			if (awokenPawn.Downed && !awokenPawn.Dead && !awokenPawn.health.hediffSet.HasHediff(HediffDefOf.RapidRegeneration))
			{
				awokenPawn.Kill(null, null);
			}
			if (awokenPawn.Dead)
			{
				TaggedString label = "UnnaturalCorpseAwokenDefeatedLetter".Translate();
				TaggedString text = "UnnaturalCorpseAwokenDefeatedLetterDesc".Translate(Haunted.Named("PAWN"));
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, awokenPawn);
				if (awokenPawn.SpawnedOrAnyParentSpawned)
				{
					GenDrop.TryDropSpawn(ThingMaker.MakeThing(ThingDefOf.Shard), awokenPawn.PositionHeld, awokenPawn.MapHeld, ThingPlaceMode.Near, out var _);
					FleshbeastUtility.MeatSplatter(3, awokenPawn.PositionHeld, awokenPawn.MapHeld);
					awokenPawn.Corpse.Destroy();
				}
				awokenFinished = true;
				return;
			}
		}
		if ((awokenPawn.DestroyedOrNull() || !awokenPawn.SpawnedOrAnyParentSpawned) && Haunted.SpawnedOrAnyParentSpawned)
		{
			if (!teleportIn)
			{
				int num = Mathf.Max(GenTicks.TicksGame - lastSpawnInCheck, 1);
				lastSpawnInCheck = GenTicks.TicksGame;
				if (Rand.MTBEventOccurs(6f, 2500f, num))
				{
					teleportIn = true;
				}
			}
			if (teleportIn && CellFinder.TryFindRandomReachableNearbyCell(Haunted.PositionHeld, Haunted.MapHeld, 70f, TraverseParms.For(TraverseMode.PassDoors), IsValidAwokenSpawnCell, null, out var result))
			{
				teleportIn = false;
				awokenPawn = MakeNewAwoken();
				SkipUtility.SkipTo(awokenPawn, result, Haunted.MapHeld);
				TurnAwoken(awokenPawn);
				Messages.Message("MessageAwokenReappeared".Translate(Haunted.Named("PAWN")), awokenPawn, MessageTypeDefOf.NegativeEvent);
				Find.MusicManagerPlay.CheckTransitions();
			}
		}
		else if (!awokenPawn.DestroyedOrNull() && awokenPawn.Spawned && !Haunted.DestroyedOrNull() && Haunted.MapHeld != awokenPawn.Map)
		{
			SkipUtility.SkipDeSpawn(awokenPawn);
			awokenPawn.Destroy();
			awokenPawn = null;
			Messages.Message("MessageAwokenVanished".Translate(Haunted.Named("PAWN")), awokenPawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	private bool IsValidAwokenSpawnCell(IntVec3 cell)
	{
		Map mapHeld = Haunted.MapHeld;
		if (cell.Standable(mapHeld))
		{
			return !cell.Fogged(mapHeld);
		}
		return false;
	}

	private void UpdateCorpseState()
	{
		if (Haunted.Dead)
		{
			return;
		}
		if (corpse == null && Haunted.SpawnedOrAnyParentSpawned)
		{
			if (!teleportIn)
			{
				int num = Mathf.Max(GenTicks.TicksGame - lastSpawnInCheck, 1);
				lastSpawnInCheck = GenTicks.TicksGame;
				if (Rand.MTBEventOccurs(24f, 2500f, num))
				{
					teleportIn = true;
				}
			}
			if (teleportIn && AnomalyUtility.TryGetNearbyUnseenCell(Haunted, out var pos))
			{
				SpawnNewCorpse(pos);
			}
		}
		if (corpse != null && InEscapableContainer())
		{
			if (!escape && Rand.MTBEventOccurs(24f, 2500f, 1f))
			{
				escape = true;
			}
			if (escape && AnomalyUtility.IsValidUnseenCell(Find.CameraDriver.CurrentViewRect.ExpandedBy(1), corpse.PositionHeld, corpse.MapHeld))
			{
				BreakoutOfContainer();
			}
		}
	}

	private void AwakeningTick()
	{
		if (CorpseSpawned && rising && Corpse.Spawned)
		{
			if (riseSustainer == null || riseSustainer.Ended)
			{
				SoundInfo info = SoundInfo.InMap(Corpse, MaintenanceType.PerTick);
				riseSustainer = SoundDefOf.Pawn_Shambler_Rise.TrySpawnSustainer(info);
			}
			if (riseEffecter == null)
			{
				riseEffecter = EffecterDefOf.ShamblerRaise.Spawn(Corpse, Corpse.Map);
			}
			riseSustainer.Maintain();
			riseEffecter.EffectTick(Corpse, TargetInfo.Invalid);
		}
		if (awoken || GenTicks.TicksGame <= awakenTick)
		{
			return;
		}
		if (CorpseSpawned && Haunted.SpawnedOrAnyParentSpawned && !awoken && !sentAwakeningLetter)
		{
			sentAwakeningLetter = true;
			awakenTick = GenTicks.TicksGame + Mathf.CeilToInt(AwakenDelayDays.RandomInRange * 60000f);
			TaggedString label = "UnnaturalCorpseAwakeningLetter".Translate();
			TaggedString text = "UnnaturalCorpseAwakeningLetterDesc".Translate(Haunted.Named("PAWN"));
			if (CanDestroyViaResearch)
			{
				text += "\n\n" + "UnnaturalCorpseAwakeningLetterExtra_StudyComplete".Translate();
			}
			else
			{
				text += "\n\n" + "UnnaturalCorpseAwakeningLetterExtra_StudyIncomplete".Translate();
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, corpse);
			return;
		}
		if (CorpseSpawned && Haunted.SpawnedOrAnyParentSpawned && Corpse.MapHeld == Haunted.MapHeld && !Haunted.Downed && Haunted.Awake() && !rising)
		{
			rising = true;
			awakenTick = GenTicks.TicksGame + 420;
			Corpse.InnerPawn.Drawer.renderer.SetAnimation(AnimationDefOf.ShamblerRise);
			TaggedString label2 = "UnnaturalCorpseAwokenLetter".Translate();
			TaggedString text2 = "UnnaturalCorpseAwokenLetterDesc".Translate(Haunted.Named("PAWN"));
			Find.LetterStack.ReceiveLetter(label2, text2, LetterDefOf.ThreatBig, awokenPawn);
			Find.TickManager.slower.SignalForceNormalSpeed();
			return;
		}
		if (CorpseSpawned)
		{
			awoken = true;
			IntVec3 positionHeld = Corpse.PositionHeld;
			Corpse.Destroy();
			awokenPawn = MakeNewAwoken();
			GenSpawn.Spawn(awokenPawn, positionHeld, Haunted.MapHeld);
			TurnAwoken(awokenPawn);
			awokenPawn.Drawer.renderer.SetAnimation(null);
			float numSeconds = CombatPointsSecondsToKillCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Haunted.MapHeld));
			ticksToKill = numSeconds.SecondsToTicks();
			Find.MusicManagerPlay.CheckTransitions();
		}
		awakenTick = GenTicks.TicksGame + 2500;
	}

	private void SpawnNewCorpse(IntVec3 cell)
	{
		teleportIn = false;
		UnnaturalCorpse unnaturalCorpse = AnomalyUtility.MakeUnnaturalCorpse(Haunted);
		unnaturalCorpse.LinkToTracker(this);
		GenSpawn.Spawn(unnaturalCorpse, cell, Haunted.MapHeld);
		spawnedTick = GenTicks.TicksGame;
		corpse = unnaturalCorpse;
		corpse.Forbiddable.Forbidden = true;
		TaggedString label = "LetterLabelCorpseReappeared".Translate();
		TaggedString text = "LetterCorpseReappeared".Translate(Haunted.Named("PAWN"));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, corpse);
	}

	private Pawn MakeNewAwoken()
	{
		Pawn pawn = Find.PawnDuplicator.Duplicate(Haunted);
		pawn.apparel.DestroyAll();
		return pawn;
	}

	private void TurnAwoken(Pawn pawn)
	{
		MutantUtility.SetPawnAsMutantInstantly(pawn, MutantDefOf.AwokenCorpse, pawn.GetRotStage());
		if (pawn.Faction != Faction.OfEntities)
		{
			pawn.SetFaction(Faction.OfEntities);
		}
		pawn.health.hediffSet.GetFirstHediff<Hediff_RapidRegeneration>().SetHpCapacity(CombatPointsPerHpRegenCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Haunted.MapHeld)));
		Ability ability = pawn.abilities?.GetAbility(AbilityDefOf.UnnaturalCorpseSkip, includeTemporary: true);
		ability?.StartCooldown(ability.def.cooldownTicksRange.RandomInRange);
	}

	private void BreakoutOfContainer()
	{
		if (InEscapableContainer())
		{
			escape = false;
			Thing thing = ThingOwnerUtility.SpawnedParentOrMe(corpse);
			Messages.Message("MessageCorpseEscaped".Translate(Haunted.Named("PAWN"), thing.Named("CONTAINER")), corpse, MessageTypeDefOf.NeutralEvent);
			if (thing is Building_Casket building_Casket)
			{
				building_Casket.EjectContents();
			}
			else
			{
				corpse.ParentHolder.GetDirectlyHeldThings().TryDrop(corpse, ThingPlaceMode.Near, 1, out var _);
			}
			corpse.Forbiddable.Forbidden = true;
		}
	}

	public void Notify_Studied(Pawn studier, float amount)
	{
		research += amount;
		if (research >= 3f && !sentLetterOne)
		{
			sentLetterOne = true;
			SendLetter(studier, "UnnaturalCorpseStudyLetter", "UnnaturalCorpseStudyLetterDesc");
		}
		else if (CanDestroyViaResearch && !sentLetterTwo)
		{
			sentLetterTwo = true;
			SendLetter(studier, "UnnaturalCorpseStudyCompletedLetter", "UnnaturalCorpseStudyCompletedLetterDesc");
		}
	}

	private void SendLetter(Pawn studier, string labelKey, string descKey)
	{
		TaggedString label = labelKey.Translate();
		TaggedString text = descKey.Translate(haunted.Named("VICTIM"), studier.Named("RESEARCHER"));
		Thing thing = (corpse.Spawned ? ((Thing)corpse) : ((Thing)haunted));
		Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, thing);
		ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent, thing);
		choiceLetter.arrivalTick = Find.TickManager.TicksGame;
		letters.Add(choiceLetter);
	}

	public void DevAwaken()
	{
		sentAwakeningLetter = true;
		awakenTick = GenTicks.TicksGame;
	}

	public void DevUnlockDeactivation()
	{
		sentLetterOne = true;
		sentLetterTwo = true;
		research += researchToDestroy;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref haunted, "haunted");
		Scribe_References.Look(ref awokenPawn, "awokenPawn");
		Scribe_References.Look(ref corpse, "corpse", saveDestroyedThings: true);
		Scribe_Values.Look(ref disappearTick, "disappearTick", 0);
		Scribe_Values.Look(ref awakenTick, "awakenTick", 0);
		Scribe_Values.Look(ref sentAwakeningLetter, "sentAwakeningLetter", defaultValue: false);
		Scribe_Values.Look(ref minMentalBreakRefireTick, "minMentalBreakRefireTick", 0);
		Scribe_Values.Look(ref lastSpawnInCheck, "lastSpawnInCheck", 0);
		Scribe_Values.Look(ref teleportIn, "teleportIn", defaultValue: false);
		Scribe_Values.Look(ref escape, "escape", defaultValue: false);
		Scribe_Values.Look(ref mentalBreak, "mentalBreak", defaultValue: false);
		Scribe_Values.Look(ref removing, "removing", defaultValue: false);
		Scribe_Values.Look(ref research, "research", 0f);
		Scribe_Values.Look(ref researchToDestroy, "researchToDestroy", 0f);
		Scribe_Values.Look(ref sentLetterOne, "sentLetterOne", defaultValue: false);
		Scribe_Values.Look(ref sentLetterTwo, "sentLetterTwo", defaultValue: false);
		Scribe_Values.Look(ref awoken, "awoken", defaultValue: false);
		Scribe_Values.Look(ref awokenFinished, "awokenFinished", defaultValue: false);
		Scribe_Values.Look(ref rising, "rising", defaultValue: false);
		Scribe_Values.Look(ref ticksToKill, "ticksToKill", 0);
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Collections.Look(ref letters, "letters", LookMode.Deep);
	}

	public string GetUniqueLoadID()
	{
		return $"MysteriousCorpseTracker_{loadID}";
	}

	private bool InEscapableContainer()
	{
		if (corpse == null)
		{
			return false;
		}
		if (corpse.ParentHolder == null || corpse.ParentHolder is Map)
		{
			return false;
		}
		if ((corpse.ParentHolder is Building building && building.def.building.isEscapableContainer) || corpse.ParentHolder is CompTransporter)
		{
			return true;
		}
		return false;
	}
}
