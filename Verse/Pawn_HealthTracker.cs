using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class Pawn_HealthTracker : IExposable
{
	private const float CrawlingManipulationRequirement = 0.15f;

	private const int CrawlingAgeRequirement = 8;

	private static FloatRange BloodFilthDropDistanceRangeFromBleedRate = new FloatRange(0.7f, 0.25f);

	private readonly Pawn pawn;

	private PawnHealthState healthState = PawnHealthState.Mobile;

	[Unsaved(false)]
	public Effecter woundedEffecter;

	[Unsaved(false)]
	public Effecter deflectionEffecter;

	[LoadAlias("forceIncap")]
	public bool forceDowned;

	public bool beCarriedByCaravanIfSick;

	public bool killedByRitual;

	public int lastReceivedNeuralSuperchargeTick = -1;

	public float overrideDeathOnDownedChance = -1f;

	private Vector3? lastSmearDropPos;

	public bool isBeingKilled;

	public bool couldCrawl;

	public HediffSet hediffSet;

	public PawnCapacitiesHandler capacities;

	public BillStack surgeryBills;

	public SummaryHealthHandler summaryHealth;

	public ImmunityHandler immunity;

	private List<Hediff_Injury> tmpMechInjuries = new List<Hediff_Injury>();

	private List<Hediff_Injury> tmpHediffInjuries = new List<Hediff_Injury>();

	private List<Hediff_MissingPart> tmpHediffMissing = new List<Hediff_MissingPart>();

	private static readonly List<Hediff> tmpHediffs = new List<Hediff>(100);

	private static readonly HashSet<Hediff> tmpRemovedHediffs = new HashSet<Hediff>();

	public PawnHealthState State => healthState;

	public bool Downed => healthState == PawnHealthState.Down;

	public bool Dead => healthState == PawnHealthState.Dead;

	public float LethalDamageThreshold => 150f * pawn.HealthScale;

	public bool CanBleed
	{
		get
		{
			if (pawn.IsMutant && !pawn.mutant.Def.canBleed)
			{
				return false;
			}
			if (pawn.RaceProps.BloodDef == null)
			{
				return false;
			}
			if (pawn.RaceProps.bleedRateFactor <= 0f)
			{
				return false;
			}
			return pawn.RaceProps.IsFlesh;
		}
	}

	public bool InPainShock
	{
		get
		{
			if (!pawn.kindDef.ignoresPainShock)
			{
				return hediffSet.PainTotal >= pawn.GetStatValue(StatDefOf.PainShockThreshold);
			}
			return false;
		}
	}

	public bool CanCrawlOrMove
	{
		get
		{
			if (!CanCrawl)
			{
				return !Downed;
			}
			return true;
		}
	}

	public bool CanCrawl
	{
		get
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return false;
			}
			if (!pawn.Awake())
			{
				return false;
			}
			if (capacities.GetLevel(PawnCapacityDefOf.Manipulation) < 0.15f)
			{
				return false;
			}
			if (pawn.ageTracker.AgeBiologicalYears < 8)
			{
				return false;
			}
			if (hediffSet.AnyHediffPreventsCrawling)
			{
				return false;
			}
			return true;
		}
	}

	public IEnumerable<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			if (hediffSet == null)
			{
				yield break;
			}
			WorkTags tags = WorkTags.None;
			foreach (Hediff hediff in hediffSet.hediffs)
			{
				HediffStage curStage = hediff.CurStage;
				if (curStage != null)
				{
					tags |= curStage.disabledWorkTags;
				}
			}
			List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if ((tags & list[i].workTags) != WorkTags.None)
				{
					yield return list[i];
				}
			}
		}
	}

	public Pawn_HealthTracker(Pawn pawn)
	{
		this.pawn = pawn;
		hediffSet = new HediffSet(pawn);
		capacities = new PawnCapacitiesHandler(pawn);
		summaryHealth = new SummaryHealthHandler(pawn);
		surgeryBills = new BillStack(pawn);
		immunity = new ImmunityHandler(pawn);
		beCarriedByCaravanIfSick = pawn.RaceProps.Humanlike;
	}

	public void Reset()
	{
		healthState = PawnHealthState.Mobile;
		hediffSet.Clear();
		capacities.Clear();
		summaryHealth.Notify_HealthChanged();
		surgeryBills.Clear();
		immunity = new ImmunityHandler(pawn);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref healthState, "healthState", PawnHealthState.Mobile);
		Scribe_Values.Look(ref forceDowned, "forceDowned", defaultValue: false);
		Scribe_Values.Look(ref beCarriedByCaravanIfSick, "beCarriedByCaravanIfSick", defaultValue: true);
		Scribe_Values.Look(ref killedByRitual, "killedByRitual", defaultValue: false);
		Scribe_Values.Look(ref lastReceivedNeuralSuperchargeTick, "lastReceivedNeuralSuperchargeTick", -1);
		Scribe_Deep.Look(ref hediffSet, "hediffSet", pawn);
		Scribe_Deep.Look(ref surgeryBills, "surgeryBills", pawn);
		Scribe_Deep.Look(ref immunity, "immunity", pawn);
		Scribe_Values.Look(ref lastSmearDropPos, "lastSmearDropPos");
		Scribe_Values.Look(ref overrideDeathOnDownedChance, "overrideDeathOnDownedChance", -1f);
	}

	public Hediff AddHediff(HediffDef def, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
	{
		Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
		AddHediff(hediff, part, dinfo, result);
		return hediff;
	}

	public Hediff GetOrAddHediff(HediffDef def, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
	{
		if (!hediffSet.TryGetHediff(def, out var hediff))
		{
			return AddHediff(def, part, dinfo, result);
		}
		return hediff;
	}

	public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
	{
		if (part == null && hediff.def.defaultInstallPart != null)
		{
			part = pawn.RaceProps.body.AllParts.Where((BodyPartRecord x) => x.def == hediff.def.defaultInstallPart).RandomElement();
		}
		if (part != null)
		{
			hediff.Part = part;
		}
		hediffSet.AddDirect(hediff, dinfo, result);
		CheckForStateChange(dinfo, hediff);
		if (pawn.RaceProps.hediffGiverSets != null)
		{
			for (int num = 0; num < pawn.RaceProps.hediffGiverSets.Count; num++)
			{
				HediffGiverSetDef hediffGiverSetDef = pawn.RaceProps.hediffGiverSets[num];
				for (int num2 = 0; num2 < hediffGiverSetDef.hediffGivers.Count; num2++)
				{
					hediffGiverSetDef.hediffGivers[num2].OnHediffAdded(pawn, hediff);
				}
			}
		}
		if (hediff.def.hairColorOverride.HasValue)
		{
			pawn.story.HairColor = hediff.def.hairColorOverride.Value;
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		if (hediff.def.HasDefinedGraphicProperties)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		if (hediff.def.givesInfectionPathways == null || !pawn.RaceProps.Humanlike)
		{
			return;
		}
		foreach (InfectionPathwayDef givesInfectionPathway in hediff.def.givesInfectionPathways)
		{
			pawn.infectionVectors.AddInfectionVector(givesInfectionPathway);
		}
	}

	public void RemoveHediff(Hediff hediff)
	{
		hediff.PreRemoved();
		hediffSet.hediffs.Remove(hediff);
		hediffSet.DirtyCache();
		hediff.PostRemoved();
		CheckForStateChange(null, hediff);
		if (hediff.def.HasDefinedGraphicProperties || hediff.def.forceRenderTreeRecache)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
		tmpRemovedHediffs.Add(hediff);
	}

	public void RemoveAllHediffs()
	{
		for (int num = hediffSet.hediffs.Count - 1; num >= 0; num--)
		{
			RemoveHediff(hediffSet.hediffs[num]);
		}
	}

	public void Notify_HediffChanged(Hediff hediff)
	{
		hediffSet.DirtyCache();
		CheckForStateChange(null, hediff);
	}

	public void Notify_UsedVerb(Verb verb, LocalTargetInfo target)
	{
		foreach (Hediff hediff in hediffSet.hediffs)
		{
			hediff.Notify_PawnUsedVerb(verb, target);
		}
	}

	public void Notify_Spawned()
	{
		foreach (Hediff hediff in hediffSet.hediffs)
		{
			hediff.Notify_Spawned();
		}
	}

	public void Notify_PawnCorpseDestroyed()
	{
		foreach (Hediff hediff in hediffSet.hediffs)
		{
			hediff.Notify_PawnCorpseDestroyed();
		}
	}

	public void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
	{
		Faction homeFaction = this.pawn.HomeFaction;
		if (dinfo.Instigator != null && homeFaction != null && homeFaction.IsPlayer && !this.pawn.InAggroMentalState && !dinfo.Def.consideredHelpful && !this.pawn.IsSubhuman)
		{
			Pawn pawn = dinfo.Instigator as Pawn;
			if (dinfo.InstigatorGuilty && pawn != null && pawn.guilt != null && pawn.mindState != null)
			{
				pawn.guilt.Notify_Guilty();
			}
		}
		if (this.pawn.Spawned)
		{
			if (!this.pawn.Position.Fogged(this.pawn.Map))
			{
				this.pawn.mindState.Active = true;
			}
			this.pawn.GetLord()?.Notify_PawnDamaged(this.pawn, dinfo);
			if (dinfo.Def.ExternalViolenceFor(this.pawn))
			{
				GenClamor.DoClamor(this.pawn, 18f, ClamorDefOf.Harm);
			}
		}
		if (homeFaction != null)
		{
			homeFaction.Notify_MemberTookDamage(this.pawn, dinfo);
			if (Current.ProgramState == ProgramState.Playing && homeFaction == Faction.OfPlayer && dinfo.Def.ExternalViolenceFor(this.pawn) && this.pawn.SpawnedOrAnyParentSpawned)
			{
				this.pawn.MapHeld.dangerWatcher.Notify_ColonistHarmedExternally();
			}
		}
		if (this.pawn.apparel != null && !dinfo.IgnoreArmor)
		{
			List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (wornApparel[i].CheckPreAbsorbDamage(dinfo))
				{
					absorbed = true;
					if (this.pawn.Spawned && dinfo.CheckForJobOverride)
					{
						this.pawn.jobs.Notify_DamageTaken(dinfo);
					}
					return;
				}
			}
		}
		if (this.pawn.Spawned)
		{
			this.pawn.stances.Notify_DamageTaken(dinfo);
			this.pawn.stances.stunner.Notify_DamageApplied(dinfo);
		}
		if (this.pawn.RaceProps.IsFlesh && dinfo.Def.ExternalViolenceFor(this.pawn))
		{
			Pawn pawn2 = dinfo.Instigator as Pawn;
			if (pawn2 != null)
			{
				if (pawn2.HostileTo(this.pawn))
				{
					this.pawn.relations.canGetRescuedThought = true;
				}
				if (this.pawn.RaceProps.Humanlike && pawn2.RaceProps.Humanlike && this.pawn.needs.mood != null && (!pawn2.HostileTo(this.pawn) || (pawn2.Faction == homeFaction && pawn2.InMentalState)))
				{
					this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HarmedMe, pawn2);
				}
			}
			ThingDef thingDef = ((pawn2 != null && dinfo.Weapon != pawn2.def) ? dinfo.Weapon : null);
			TaleRecorder.RecordTale(TaleDefOf.Wounded, this.pawn, pawn2, thingDef);
		}
		absorbed = false;
	}

	public void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (ShouldBeDead())
		{
			if (!ShouldBeDeathrestingOrInComa())
			{
				if (!this.pawn.Destroyed)
				{
					this.pawn.Kill(dinfo);
				}
				return;
			}
			ForceDeathrestOrComa(dinfo, null);
		}
		if (dinfo.Def.additionalHediffs != null && (dinfo.Def.applyAdditionalHediffsIfHuntingForFood || !(dinfo.Instigator is Pawn { CurJob: not null } pawn) || pawn.CurJob.def != JobDefOf.PredatorHunt))
		{
			List<DamageDefAdditionalHediff> additionalHediffs = dinfo.Def.additionalHediffs;
			for (int i = 0; i < additionalHediffs.Count; i++)
			{
				DamageDefAdditionalHediff damageDefAdditionalHediff = additionalHediffs[i];
				if (damageDefAdditionalHediff.hediff == null)
				{
					continue;
				}
				float num = ((damageDefAdditionalHediff.severityFixed <= 0f) ? (totalDamageDealt * damageDefAdditionalHediff.severityPerDamageDealt) : damageDefAdditionalHediff.severityFixed);
				if (damageDefAdditionalHediff.victimSeverityScalingByInvBodySize)
				{
					num *= 1f / this.pawn.BodySize;
				}
				if (damageDefAdditionalHediff.victimSeverityScaling != null)
				{
					num *= (damageDefAdditionalHediff.inverseStatScaling ? Mathf.Max(1f - this.pawn.GetStatValue(damageDefAdditionalHediff.victimSeverityScaling), 0f) : this.pawn.GetStatValue(damageDefAdditionalHediff.victimSeverityScaling));
				}
				if (num >= 0f)
				{
					Hediff hediff = HediffMaker.MakeHediff(damageDefAdditionalHediff.hediff, this.pawn);
					hediff.Severity = num;
					AddHediff(hediff, null, dinfo);
					if (Dead)
					{
						return;
					}
				}
			}
		}
		for (int j = 0; j < hediffSet.hediffs.Count; j++)
		{
			hediffSet.hediffs[j].Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
		}
		if (this.pawn.Spawned && dinfo.CheckForJobOverride)
		{
			this.pawn.jobs.Notify_DamageTaken(dinfo);
		}
	}

	public void RestorePart(BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true)
	{
		if (part == null)
		{
			Log.Error("Tried to restore null body part.");
			return;
		}
		RestorePartRecursiveInt(part, diffException);
		hediffSet.DirtyCache();
		if (checkStateChange)
		{
			CheckForStateChange(null, null);
		}
	}

	private void RestorePartRecursiveInt(BodyPartRecord part, Hediff diffException = null)
	{
		List<Hediff> hediffs = hediffSet.hediffs;
		for (int num = hediffs.Count - 1; num >= 0; num--)
		{
			Hediff hediff = hediffs[num];
			if (hediff.Part == part && hediff != diffException && !hediff.def.keepOnBodyPartRestoration)
			{
				hediffs.RemoveAt(num);
				hediff.PostRemoved();
			}
		}
		for (int i = 0; i < part.parts.Count; i++)
		{
			RestorePartRecursiveInt(part.parts[i], diffException);
		}
	}

	public float FactorForDamage(DamageInfo dinfo)
	{
		return hediffSet.FactorForDamage(dinfo);
	}

	public void CheckForStateChange(DamageInfo? dinfo, Hediff hediff)
	{
		if (Dead || isBeingKilled)
		{
			return;
		}
		if (ModsConfig.BiotechActive && pawn.mechanitor != null)
		{
			pawn.mechanitor.Notify_HediffStateChange(hediff);
		}
		if (hediff != null && hediff.def.blocksSleeping && !pawn.Awake())
		{
			RestUtility.WakeUp(pawn);
		}
		if (hediff?.CurStage != null && hediff.CurStage.disabledWorkTags != WorkTags.None)
		{
			pawn.Notify_DisabledWorkTypesChanged();
		}
		if (pawn.Crawling && !CanCrawl && pawn.CurJob != null)
		{
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
		}
		else if (ShouldBeDead())
		{
			if (ShouldBeDeathrestingOrInComa())
			{
				ForceDeathrestOrComa(dinfo, hediff);
			}
			else if (!pawn.Destroyed)
			{
				pawn.Kill(dinfo, hediff);
			}
		}
		else if (!Downed)
		{
			if (ShouldBeDowned())
			{
				if (pawn.kindDef.forceDeathOnDowned)
				{
					pawn.Kill(dinfo);
					return;
				}
				if (!forceDowned && ((dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(pawn)) || (hediff != null && hediff.def.canApplyDodChanceForCapacityChanges)) && !pawn.IsWildMan() && !pawn.IsDeactivated() && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || !pawn.HostFaction.IsPlayer))
				{
					bool flag = (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasActiveGene(GeneDefOf.Deathless)) || hediffSet.HasPreventsDeath;
					float num = ((overrideDeathOnDownedChance >= 0f) ? overrideDeathOnDownedChance : ((pawn.IsMutant && pawn.mutant.Def.deathOnDownedChance >= 0f) ? pawn.mutant.Def.deathOnDownedChance : ((flag && pawn.Faction == Faction.OfPlayer) ? 0f : (pawn.kindDef.overrideDeathOnDownedChance.HasValue ? pawn.kindDef.overrideDeathOnDownedChance.Value : ((ModsConfig.AnomalyActive && pawn.Faction == Faction.OfEntities && pawn.MapHeld != null) ? HealthTuning.DeathOnDownedChance_EntityFromThreatCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(pawn.MapHeld)) : (pawn.RaceProps.Animal ? 0.5f : ((!pawn.RaceProps.IsMechanoid) ? ((Find.Storyteller.difficulty.unwaveringPrisoners ? HealthTuning.DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve : HealthTuning.DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve_WaveringPrisoners).Evaluate(StorytellerUtilityPopulation.PopulationIntent) * Find.Storyteller.difficulty.enemyDeathOnDownedChanceFactor) : 1f)))))));
					if (Rand.Chance(num))
					{
						if (DebugViewSettings.logCauseOfDeath)
						{
							Log.Message("CauseOfDeath: chance on downed " + num.ToStringPercent());
						}
						if (flag && !pawn.Dead)
						{
							pawn.health.AddHediff(HediffDefOf.MissingBodyPart, pawn.health.hediffSet.GetBrain(), dinfo);
						}
						else
						{
							pawn.Kill(dinfo);
						}
						return;
					}
				}
				forceDowned = false;
				MakeDowned(dinfo, hediff);
				return;
			}
			if (!capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.jobs != null && pawn.CurJob != null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
				if (pawn.equipment != null && pawn.equipment.Primary != null)
				{
					if (pawn.kindDef.destroyGearOnDrop)
					{
						pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
					}
					else if (pawn.InContainerEnclosed)
					{
						pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.holdingOwner);
					}
					else if (pawn.SpawnedOrAnyParentSpawned)
					{
						pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var _, pawn.PositionHeld);
					}
					else if (pawn.IsCaravanMember())
					{
						ThingWithComps primary = pawn.equipment.Primary;
						pawn.equipment.Remove(primary);
						if (!pawn.inventory.innerContainer.TryAdd(primary))
						{
							primary.Destroy();
						}
					}
					else
					{
						pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
					}
				}
			}
		}
		else if (!ShouldBeDowned())
		{
			MakeUndowned(hediff);
		}
		if (Downed && couldCrawl && !CanCrawl && !pawn.InBed())
		{
			pawn.pather?.StopDead();
			pawn.jobs?.StopAll();
			pawn.GetLord()?.Notify_PawnDowned(pawn);
		}
		couldCrawl = CanCrawl;
	}

	private bool ShouldBeDeathrestingOrInComa()
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
		{
			return false;
		}
		return true;
	}

	public bool ShouldBeDowned()
	{
		if (!InPainShock && capacities.CanBeAwake && (capacities.CapableOf(PawnCapacityDefOf.Moving) || pawn.RaceProps.doesntMove))
		{
			return pawn.ageTracker.CurLifeStage.alwaysDowned;
		}
		return true;
	}

	public bool ShouldBeDead()
	{
		if (Dead)
		{
			return true;
		}
		if (hediffSet.HasPreventsDeath)
		{
			return false;
		}
		foreach (Hediff hediff in hediffSet.hediffs)
		{
			if (hediff.CauseDeathNow())
			{
				return true;
			}
		}
		if (ShouldBeDeadFromRequiredCapacity() != null)
		{
			return true;
		}
		if (PawnCapacityUtility.CalculatePartEfficiency(hediffSet, pawn.RaceProps.body.corePart) <= 0.0001f)
		{
			if (DebugViewSettings.logCauseOfDeath)
			{
				Log.Message("CauseOfDeath: zero efficiency of " + pawn.RaceProps.body.corePart.Label);
			}
			return true;
		}
		if (ShouldBeDeadFromLethalDamageThreshold())
		{
			return true;
		}
		return false;
	}

	public PawnCapacityDef ShouldBeDeadFromRequiredCapacity()
	{
		List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			PawnCapacityDef pawnCapacityDef = allDefsListForReading[i];
			if ((pawn.RaceProps.IsFlesh ? pawnCapacityDef.lethalFlesh : pawnCapacityDef.lethalMechanoids) && !capacities.CapableOf(pawnCapacityDef))
			{
				if (DebugViewSettings.logCauseOfDeath)
				{
					Log.Message("CauseOfDeath: no longer capable of " + pawnCapacityDef.defName);
				}
				return pawnCapacityDef;
			}
		}
		return null;
	}

	public bool ShouldBeDeadFromLethalDamageThreshold()
	{
		float num = 0f;
		for (int i = 0; i < hediffSet.hediffs.Count; i++)
		{
			if (hediffSet.hediffs[i] is Hediff_Injury)
			{
				num += hediffSet.hediffs[i].Severity;
			}
		}
		bool num2 = num >= LethalDamageThreshold;
		if (num2 && DebugViewSettings.logCauseOfDeath)
		{
			Log.Message($"CauseOfDeath: lethal damage {num} >= {LethalDamageThreshold}");
		}
		return num2;
	}

	public bool WouldLosePartAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
	{
		Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
		hediff.Severity = severity;
		return CheckPredicateAfterAddingHediff(hediff, () => hediffSet.PartIsMissing(part));
	}

	public bool WouldDieAfterAddingHediff(Hediff hediff)
	{
		if (Dead)
		{
			return true;
		}
		bool num = CheckPredicateAfterAddingHediff(hediff, ShouldBeDead);
		if (num && DebugViewSettings.logCauseOfDeath)
		{
			Log.Message($"CauseOfDeath: WouldDieAfterAddingHediff=true for {pawn.Name}");
		}
		return num;
	}

	public bool WouldDieAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
	{
		Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
		hediff.Severity = severity;
		return WouldDieAfterAddingHediff(hediff);
	}

	public bool WouldBeDownedAfterAddingHediff(Hediff hediff)
	{
		if (Dead)
		{
			return false;
		}
		return CheckPredicateAfterAddingHediff(hediff, ShouldBeDowned);
	}

	public bool WouldBeDownedAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
	{
		Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
		hediff.Severity = severity;
		return WouldBeDownedAfterAddingHediff(hediff);
	}

	public void SetDead()
	{
		if (Dead)
		{
			Log.Error($"{pawn} set dead while already dead.");
		}
		healthState = PawnHealthState.Dead;
	}

	private bool CheckPredicateAfterAddingHediff(Hediff hediff, Func<bool> pred)
	{
		HashSet<Hediff> missing = CalculateMissingPartHediffsFromInjury(hediff);
		hediffSet.hediffs.Add(hediff);
		if (missing != null)
		{
			hediffSet.hediffs.AddRange(missing);
		}
		hediffSet.DirtyCache();
		bool result = pred();
		if (missing != null)
		{
			hediffSet.hediffs.RemoveAll((Hediff x) => missing.Contains(x));
		}
		hediffSet.hediffs.Remove(hediff);
		hediffSet.DirtyCache();
		return result;
	}

	private HashSet<Hediff> CalculateMissingPartHediffsFromInjury(Hediff hediff)
	{
		HashSet<Hediff> missing = null;
		if (hediff.Part != null && hediff.Part != pawn.RaceProps.body.corePart && hediff.Severity >= hediffSet.GetPartHealth(hediff.Part))
		{
			missing = new HashSet<Hediff>();
			AddAllParts(hediff.Part);
		}
		return missing;
		void AddAllParts(BodyPartRecord part)
		{
			Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
			hediff_MissingPart.lastInjury = hediff.def;
			hediff_MissingPart.Part = part;
			missing.Add(hediff_MissingPart);
			foreach (BodyPartRecord part in part.parts)
			{
				AddAllParts(part);
			}
		}
	}

	private void ForceDeathrestOrComa(DamageInfo? dinfo, Hediff hediff)
	{
		if (pawn.CanDeathrest())
		{
			if (SanguophageUtility.TryStartDeathrest(pawn, DeathrestStartReason.LethalDamage))
			{
				GeneUtility.OffsetHemogen(pawn, -9999f);
			}
		}
		else
		{
			SanguophageUtility.TryStartRegenComa(pawn, DeathrestStartReason.LethalDamage);
		}
		if (!Downed)
		{
			forceDowned = true;
			MakeDowned(dinfo, hediff);
		}
	}

	private void MakeDowned(DamageInfo? dinfo, Hediff hediff)
	{
		if (Downed)
		{
			Log.Error($"{pawn} tried to do MakeDowned while already downed.");
			return;
		}
		if (pawn.guilt != null && pawn.GetLord()?.LordJob != null && pawn.GetLord().LordJob.GuiltyOnDowned)
		{
			pawn.guilt.Notify_Guilty();
		}
		healthState = PawnHealthState.Down;
		PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, dinfo, PawnDiedOrDownedThoughtsKind.Downed);
		if (pawn.InMentalState && pawn.MentalStateDef.recoverFromDowned)
		{
			pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
		}
		pawn.mindState.droppedWeapon = null;
		pawn.mindState.nextMoveOrderIsCrawlBreak = true;
		if (pawn.Spawned)
		{
			pawn.DropAndForbidEverything(keepInventoryAndEquipmentIfInBed: true, rememberPrimary: true);
			pawn.stances.CancelBusyStanceSoft();
		}
		if (!pawn.DutyActiveWhenDown(onlyInBed: true) && (!pawn.IsMutant || !pawn.health.CanCrawl || !pawn.mutant.Def.canAttackWhileCrawling))
		{
			pawn.ClearMind_NewTemp(ifLayingKeepLaying: true, clearInspiration: false, clearMentalState: false, wasDowned: true);
		}
		if (Current.ProgramState == ProgramState.Playing)
		{
			pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Incapped, dinfo);
		}
		if (pawn.Drafted)
		{
			pawn.drafter.Drafted = false;
		}
		PortraitsCache.SetDirty(pawn);
		GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		if (pawn.SpawnedOrAnyParentSpawned)
		{
			GenHostility.Notify_PawnLostForTutor(pawn, pawn.MapHeld);
		}
		if (pawn.RaceProps.Humanlike && !pawn.IsSubhuman && Current.ProgramState == ProgramState.Playing && pawn.SpawnedOrAnyParentSpawned)
		{
			if (pawn.HostileTo(Faction.OfPlayer))
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.Capturing, pawn, OpportunityType.Important);
			}
			if (pawn.Faction == Faction.OfPlayer)
			{
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.Rescuing, pawn, OpportunityType.Critical);
			}
		}
		if (dinfo?.Instigator is Pawn instigator)
		{
			RecordsUtility.Notify_PawnDowned(pawn, instigator);
		}
		if (pawn.Spawned && (hediff == null || hediff.def.recordDownedTale))
		{
			TaleRecorder.RecordTale(TaleDefOf.Downed, pawn, dinfo?.Instigator as Pawn, dinfo?.Weapon);
			Find.BattleLog.Add(new BattleLogEntry_StateTransition(pawn, RulePackDefOf.Transition_Downed, dinfo?.Instigator as Pawn, hediff, dinfo?.HitPart));
		}
		Find.Storyteller.Notify_PawnEvent(pawn, AdaptationEvent.Downed, dinfo);
		pawn.mechanitor?.Notify_Downed();
		pawn.mutant?.Notify_Downed();
		foreach (Hediff hediff2 in hediffSet.hediffs)
		{
			hediff2.Notify_Downed();
		}
		pawn.Notify_Downed();
		pawn.GetLord()?.Notify_PawnDowned(pawn);
		pawn.flight?.ForceLand();
	}

	private void MakeUndowned(Hediff hediff)
	{
		if (!Downed)
		{
			Log.Error($"{pawn} tried to do MakeUndowned when already undowned.");
			return;
		}
		pawn.pather?.StopDead();
		pawn.jobs?.CheckForJobOverride();
		healthState = PawnHealthState.Mobile;
		if (PawnUtility.ShouldSendNotificationAbout(pawn) && (hediff == null || hediff.def != HediffDefOf.Deathrest))
		{
			Messages.Message("MessageNoLongerDowned".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);
		}
		PortraitsCache.SetDirty(pawn);
		GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		if (pawn.guest != null)
		{
			pawn.guest.Notify_PawnUndowned();
		}
		pawn.GetLord()?.Notify_PawnUndowned(pawn);
	}

	public void NotifyPlayerOfKilled(DamageInfo? dinfo, Hediff hediff, Caravan caravan)
	{
		TaggedString diedLetterText = HealthUtility.GetDiedLetterText(pawn, dinfo, hediff);
		Quest quest = null;
		if (pawn.IsBorrowedByAnyFaction())
		{
			foreach (QuestPart_LendColonistsToFaction item in QuestUtility.GetAllQuestPartsOfType<QuestPart_LendColonistsToFaction>())
			{
				if (item.LentColonistsListForReading.Contains(pawn))
				{
					diedLetterText += "\n\n" + "LentColonistDied".Translate(pawn.Named("PAWN"), item.lendColonistsToFaction.Named("FACTION"));
					quest = item.quest;
					break;
				}
			}
		}
		diedLetterText = diedLetterText.AdjustedFor(pawn);
		if (pawn.Faction == Faction.OfPlayer)
		{
			TaggedString label = "Death".Translate() + ": " + pawn.LabelShortCap;
			if (caravan != null)
			{
				Messages.Message("MessageCaravanDeathCorpseAddedToInventory".Translate(pawn.Named("PAWN")), caravan, MessageTypeDefOf.PawnDeath);
			}
			Hediff_DeathRefusal firstHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
			if (pawn.Ideo != null && firstHediff == null)
			{
				foreach (Precept item2 in pawn.Ideo.PreceptsListForReading)
				{
					if (!string.IsNullOrWhiteSpace(item2.def.extraTextPawnDeathLetter))
					{
						diedLetterText += "\n\n" + item2.def.extraTextPawnDeathLetter.Formatted(pawn.Named("PAWN"));
					}
				}
			}
			if (firstHediff != null)
			{
				diedLetterText += "\n\n" + "SelfResurrectText".Translate(pawn.Named("PAWN"));
			}
			if (pawn.Name != null && !pawn.Name.Numerical && pawn.RaceProps.Animal)
			{
				label += " (" + pawn.KindLabel + ")";
			}
			pawn.relations?.CheckAppendBondedAnimalDiedInfo(ref diedLetterText, ref label);
			Find.LetterStack.ReceiveLetter(label, diedLetterText, LetterDefOf.Death, pawn, null, quest);
		}
		else
		{
			Messages.Message(diedLetterText, pawn, MessageTypeDefOf.PawnDeath);
		}
	}

	public void Notify_Resurrected(bool restoreMissingParts = true, float gettingScarsChance = 0f)
	{
		if (gettingScarsChance > 0f)
		{
			for (int i = 0; i < hediffSet.hediffs.Count; i++)
			{
				if (hediffSet.hediffs[i] is Hediff_Injury hediff_Injury && !hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(hediff_Injury.Part))
				{
					HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff_Injury.TryGetComp<HediffComp_GetsPermanent>();
					if (hediffComp_GetsPermanent != null && !hediffComp_GetsPermanent.IsPermanent && Rand.Chance(gettingScarsChance))
					{
						hediffComp_GetsPermanent.IsPermanent = true;
						hediff_Injury.Severity = Mathf.Min(hediff_Injury.Severity, Rand.RangeInclusive(2, 6));
					}
				}
			}
		}
		healthState = PawnHealthState.Mobile;
		hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x.TryGetComp<HediffComp_Immunizable>() != null);
		hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && (x.IsLethal || x.IsAnyStageLifeThreatening()));
		hediffSet.hediffs.RemoveAll((Hediff x) => x.def.forceRemoveOnResurrection);
		if (!pawn.RaceProps.IsMechanoid)
		{
			hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && !x.IsPermanent());
		}
		else
		{
			tmpMechInjuries.Clear();
			hediffSet.GetHediffs(ref tmpMechInjuries, (Hediff_Injury x) => x != null && x.def.everCurableByItem && !x.IsPermanent());
			if (tmpMechInjuries.Count > 0)
			{
				float num = tmpMechInjuries.Sum((Hediff_Injury x) => x.Severity) * 0.5f / (float)tmpMechInjuries.Count;
				for (int num2 = 0; num2 < tmpMechInjuries.Count; num2++)
				{
					tmpMechInjuries[num2].Severity -= num;
				}
				tmpMechInjuries.Clear();
			}
		}
		hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && x.IsPermanent() && hediffSet.GetPartHealth(x.Part) <= 0f);
		if (restoreMissingParts)
		{
			while (true)
			{
				Hediff_MissingPart hediff_MissingPart = hediffSet.GetMissingPartsCommonAncestors().FirstOrDefault((Hediff_MissingPart x) => !hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x.Part));
				if (hediff_MissingPart == null)
				{
					break;
				}
				RestorePart(hediff_MissingPart.Part, null, checkStateChange: false);
			}
		}
		for (int num3 = hediffSet.hediffs.Count - 1; num3 >= 0; num3--)
		{
			hediffSet.hediffs[num3].Notify_Resurrected();
		}
		hediffSet.DirtyCache();
		if (ShouldBeDead())
		{
			hediffSet.hediffs.RemoveAll((Hediff h) => !h.def.keepOnBodyPartRestoration);
		}
		Notify_HediffChanged(null);
	}

	public void HealthTick()
	{
		if (Dead)
		{
			return;
		}
		tmpRemovedHediffs.Clear();
		tmpHediffs.Clear();
		tmpHediffs.AddRange(hediffSet.hediffs);
		foreach (Hediff tmpHediff in tmpHediffs)
		{
			if (tmpRemovedHediffs.Contains(tmpHediff))
			{
				continue;
			}
			try
			{
				tmpHediff.Tick();
				tmpHediff.PostTick();
			}
			catch (Exception arg)
			{
				Log.Error($"Exception ticking hediff {tmpHediff.ToStringSafe()} for pawn {pawn.ToStringSafe()}. Removing hediff... Exception: {arg}");
				try
				{
					RemoveHediff(tmpHediff);
				}
				catch (Exception arg2)
				{
					Log.Error($"Error while removing hediff: {arg2}");
				}
			}
			if (Dead)
			{
				return;
			}
		}
		for (int num = hediffSet.hediffs.Count - 1; num >= 0; num--)
		{
			Hediff hediff = hediffSet.hediffs[num];
			if (hediff.ShouldRemove)
			{
				RemoveHediff(hediff);
			}
		}
	}

	public void HealthTickInterval(int delta)
	{
		if (Dead)
		{
			return;
		}
		tmpRemovedHediffs.Clear();
		tmpHediffs.Clear();
		tmpHediffs.AddRange(hediffSet.hediffs);
		foreach (Hediff tmpHediff in tmpHediffs)
		{
			if (tmpRemovedHediffs.Contains(tmpHediff))
			{
				continue;
			}
			try
			{
				tmpHediff.TickInterval(delta);
				tmpHediff.PostTickInterval(delta);
			}
			catch (Exception arg)
			{
				Log.Error($"Exception interval ticking hediff {tmpHediff.ToStringSafe()} for pawn {pawn.ToStringSafe()}. Removing hediff... Exception: {arg}");
				try
				{
					RemoveHediff(tmpHediff);
				}
				catch (Exception arg2)
				{
					Log.Error($"Error while removing hediff: {arg2}");
				}
			}
			if (Dead)
			{
				return;
			}
		}
		for (int num = hediffSet.hediffs.Count - 1; num >= 0; num--)
		{
			Hediff hediff = hediffSet.hediffs[num];
			if (hediff.ShouldRemove)
			{
				RemoveHediff(hediff);
			}
		}
		if (Dead)
		{
			return;
		}
		immunity.ImmunityHandlerTickInterval(delta);
		if (pawn.Spawned && pawn.Crawling && pawn.MapHeld.reservationManager.IsReservedAndRespected(pawn, pawn))
		{
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
		if ((pawn.RaceProps.IsFlesh || pawn.RaceProps.IsAnomalyEntity) && pawn.IsHashIntervalTick(600, delta) && (pawn.needs.food == null || !pawn.needs.food.Starving))
		{
			bool flag = false;
			if (hediffSet.HasNaturallyHealingInjury())
			{
				float num2 = 8f;
				if (pawn.GetPosture() != PawnPosture.Standing)
				{
					num2 += 4f;
					Building_Bed building_Bed = pawn.CurrentBed();
					if (building_Bed != null)
					{
						num2 += building_Bed.def.building.bed_healPerDay;
					}
				}
				foreach (Hediff hediff3 in hediffSet.hediffs)
				{
					HediffStage curStage = hediff3.CurStage;
					if (curStage != null && curStage.naturalHealingFactor != -1f)
					{
						num2 *= curStage.naturalHealingFactor;
					}
				}
				hediffSet.GetHediffs(ref tmpHediffInjuries, (Hediff_Injury h) => h.CanHealNaturally());
				tmpHediffInjuries.RandomElement().Heal(num2 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
				flag = true;
			}
			if (hediffSet.HasTendedAndHealingInjury())
			{
				Need_Food food = pawn.needs.food;
				if (food == null || !food.Starving)
				{
					hediffSet.GetHediffs(ref tmpHediffInjuries, (Hediff_Injury h) => h.CanHealFromTending());
					Hediff_Injury hediff_Injury = tmpHediffInjuries.RandomElement();
					float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
					float num3 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
					hediff_Injury.Heal(8f * num3 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
					flag = true;
				}
			}
			if (flag && !HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessageFullyHealed".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);
			}
		}
		if ((pawn.RaceProps.IsFlesh || pawn.RaceProps.IsAnomalyEntity) && pawn.IsHashIntervalTick(15, delta) && ModsConfig.AnomalyActive && hediffSet.HasRegeneration)
		{
			float num4 = 0f;
			foreach (Hediff hediff4 in hediffSet.hediffs)
			{
				if (hediff4.CurStage != null)
				{
					num4 += hediff4.CurStage.regeneration;
				}
			}
			num4 *= 0.00025f;
			if (num4 > 0f)
			{
				hediffSet.GetHediffs(ref tmpHediffInjuries, (Hediff_Injury h) => true);
				foreach (Hediff_Injury tmpHediffInjury in tmpHediffInjuries)
				{
					float num5 = Mathf.Min(num4, tmpHediffInjury.Severity);
					num4 -= num5;
					tmpHediffInjury.Heal(num5);
					hediffSet.Notify_Regenerated(num5);
					if (num4 <= 0f)
					{
						break;
					}
				}
				if (num4 > 0f)
				{
					hediffSet.GetHediffs(ref tmpHediffMissing, (Hediff_MissingPart h) => h.Part.parent != null && !tmpHediffInjuries.Any((Hediff_Injury x) => x.Part == h.Part.parent) && hediffSet.GetFirstHediffMatchingPart<Hediff_MissingPart>(h.Part.parent) == null && hediffSet.GetFirstHediffMatchingPart<Hediff_AddedPart>(h.Part.parent) == null);
					using List<Hediff_MissingPart>.Enumerator enumerator3 = tmpHediffMissing.GetEnumerator();
					if (enumerator3.MoveNext())
					{
						Hediff_MissingPart current4 = enumerator3.Current;
						BodyPartRecord part = current4.Part;
						RemoveHediff(current4);
						Hediff hediff2 = AddHediff(HediffDefOf.Misc, part);
						float partHealth = hediffSet.GetPartHealth(part);
						hediff2.Severity = Mathf.Max(partHealth - 1f, partHealth * 0.9f);
						hediffSet.Notify_Regenerated(partHealth - hediff2.Severity);
					}
				}
			}
		}
		if (CanBleed && hediffSet.BleedRateTotal >= 0.1f && (pawn.Spawned || pawn.ParentHolder is Pawn_CarryTracker) && pawn.SpawnedOrAnyParentSpawned)
		{
			if (pawn.Crawling && pawn.Spawned)
			{
				if (!lastSmearDropPos.HasValue || Vector3.Distance(pawn.DrawPos, lastSmearDropPos.Value) > BloodFilthDropDistanceRangeFromBleedRate.LerpThroughRange(hediffSet.BleedRateTotal))
				{
					DropBloodSmear();
				}
			}
			else
			{
				lastSmearDropPos = null;
				float num6 = hediffSet.BleedRateTotal * pawn.BodySize;
				num6 = ((pawn.GetPosture() != PawnPosture.Standing) ? (num6 * 0.0004f) : (num6 * 0.004f));
				if (Rand.Chance(num6 * (float)delta))
				{
					DropBloodFilth();
				}
			}
		}
		if (!pawn.IsHashIntervalTick(60, delta))
		{
			return;
		}
		List<HediffGiverSetDef> hediffGiverSets = pawn.RaceProps.hediffGiverSets;
		if (hediffGiverSets != null)
		{
			for (int num7 = 0; num7 < hediffGiverSets.Count; num7++)
			{
				List<HediffGiver> hediffGivers = hediffGiverSets[num7].hediffGivers;
				for (int num8 = 0; num8 < hediffGivers.Count; num8++)
				{
					hediffGivers[num8].OnIntervalPassed(pawn, null);
					if (pawn.Dead)
					{
						return;
					}
				}
			}
		}
		if (pawn.story == null || pawn.IsWorldPawn())
		{
			return;
		}
		List<Trait> allTraits = pawn.story.traits.allTraits;
		for (int num9 = 0; num9 < allTraits.Count; num9++)
		{
			if (allTraits[num9].Suppressed)
			{
				continue;
			}
			TraitDegreeData currentData = allTraits[num9].CurrentData;
			if (!(currentData.randomDiseaseMtbDays > 0f) || !Rand.MTBEventOccurs(currentData.randomDiseaseMtbDays, 60000f, 60f))
			{
				continue;
			}
			BiomeDef biome = (pawn.Tile.Valid ? Find.WorldGrid[pawn.Tile].PrimaryBiome : DefDatabase<BiomeDef>.GetRandom());
			IncidentDef incidentDef = DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef d) => d.category == IncidentCategoryDefOf.DiseaseHuman).RandomElementByWeightWithFallback((IncidentDef d) => biome.CommonalityOfDisease(d));
			if (incidentDef == null)
			{
				continue;
			}
			string blockedInfo;
			List<Pawn> list = ((IncidentWorker_Disease)incidentDef.Worker).ApplyToPawns(Gen.YieldSingle(pawn), out blockedInfo);
			if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				if (list.Contains(pawn))
				{
					Find.LetterStack.ReceiveLetter("LetterLabelTraitDisease".Translate(incidentDef.diseaseIncident.label), "LetterTraitDisease".Translate(pawn.LabelCap, incidentDef.diseaseIncident.label, pawn.Named("PAWN")).AdjustedFor(pawn), LetterDefOf.NegativeEvent, pawn);
				}
				else if (!blockedInfo.NullOrEmpty())
				{
					Messages.Message(blockedInfo, pawn, MessageTypeDefOf.NeutralEvent);
				}
			}
		}
	}

	public bool HasHediffsNeedingTend(bool forAlert = false)
	{
		return hediffSet.HasTendableHediff(forAlert);
	}

	public bool HasHediffsNeedingTendByPlayer(bool forAlert = false)
	{
		if (HasHediffsNeedingTend(forAlert))
		{
			if (pawn.NonHumanlikeOrWildMan())
			{
				if (pawn.Faction == Faction.OfPlayer)
				{
					return true;
				}
				Building_Bed building_Bed = pawn.CurrentBed();
				if (building_Bed != null && building_Bed.Faction == Faction.OfPlayer)
				{
					return true;
				}
				if (pawn.IsOnHoldingPlatform)
				{
					return true;
				}
			}
			else
			{
				if (pawn.IsOnHoldingPlatform)
				{
					return true;
				}
				if ((pawn.Faction == Faction.OfPlayer && pawn.HostFaction == null) || pawn.HostFaction == Faction.OfPlayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void DropBloodFilth()
	{
		if ((pawn.Spawned || pawn.ParentHolder is Pawn_CarryTracker) && pawn.SpawnedOrAnyParentSpawned)
		{
			ThingDef thingDef = (pawn.IsMutant ? (pawn.mutant.Def.bloodDef ?? pawn.RaceProps.BloodDef) : pawn.RaceProps.BloodDef);
			if (thingDef != null)
			{
				FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, thingDef, pawn.LabelIndefinite());
			}
		}
	}

	public void DropBloodSmear()
	{
		ThingDef thingDef = (pawn.IsMutant ? (pawn.mutant.Def.bloodSmearDef ?? pawn.RaceProps.BloodSmearDef) : pawn.RaceProps.BloodSmearDef);
		if (thingDef == null)
		{
			lastSmearDropPos = pawn.DrawPos;
			return;
		}
		FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, thingDef, out var outFilth, pawn.LabelIndefinite(), FilthSourceFlags.None, shouldPropagate: false);
		if (outFilth != null)
		{
			float rotation = ((!lastSmearDropPos.HasValue) ? pawn.pather.lastMoveDirection : (lastSmearDropPos.Value - pawn.DrawPos).AngleFlat());
			outFilth.SetOverrideDrawPositionAndRotation(pawn.DrawPos.WithY(thingDef.Altitude), rotation);
			lastSmearDropPos = pawn.DrawPos;
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Hediff hediff in hediffSet.hediffs)
		{
			IEnumerable<Gizmo> gizmos = hediff.GetGizmos();
			if (gizmos == null || (Dead && !hediff.def.showGizmosOnCorpse))
			{
				continue;
			}
			foreach (Gizmo item in gizmos)
			{
				yield return item;
			}
		}
	}
}
