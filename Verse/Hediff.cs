using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class Hediff : IExposable, ILoadReferenceable
{
	public HediffDef def;

	public int ageTicks;

	public int tickAdded = -1;

	private BodyPartRecord part;

	public string sourceLabel;

	public ThingDef sourceDef;

	public BodyPartGroupDef sourceBodyPartGroup;

	public string sourceToolLabel;

	public HediffDef sourceHediffDef;

	public int loadID = -1;

	protected float severityInt;

	private bool recordedTale;

	protected bool causesNoPain;

	private bool visible;

	public WeakReference<LogEntry> combatLogEntry;

	public string combatLogText;

	public int temp_partIndexToSetLater = -1;

	public bool canBeThreateningToPart = true;

	private List<Ability> abilities;

	[Unsaved(false)]
	public Pawn pawn;

	private static StringBuilder tipSb = new StringBuilder();

	public virtual string LabelBase => CurStage?.overrideLabel ?? def.label;

	public string LabelBaseCap => LabelBase.CapitalizeFirst(def);

	public virtual string Label
	{
		get
		{
			string labelInBrackets = LabelInBrackets;
			return LabelBase + (labelInBrackets.NullOrEmpty() ? "" : (" (" + labelInBrackets + ")"));
		}
	}

	public string LabelCap => Label.CapitalizeFirst(def);

	public virtual Color LabelColor => def.defaultLabelColor;

	public virtual string LabelInBrackets
	{
		get
		{
			if (CurStage != null && !CurStage.label.NullOrEmpty())
			{
				return CurStage.label;
			}
			return null;
		}
	}

	public virtual string SeverityLabel
	{
		get
		{
			if (!IsLethal && !def.alwaysShowSeverity)
			{
				return null;
			}
			return (Severity / Mathf.Abs(def.lethalSeverity)).ToStringPercent();
		}
	}

	public virtual int UIGroupKey => Label.GetHashCode();

	public virtual string TipStringExtra
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (StatDrawEntry item in HediffStatsUtility.SpecialDisplayStats(CurStage, this))
			{
				if (item.ShouldDisplay())
				{
					stringBuilder.Append("  - " + item.LabelCap + ": " + item.ValueString);
					if (CurStage?.statOffsetEffectMultiplier != null)
					{
						stringBuilder.Append($" x {CurStage.statOffsetEffectMultiplier.LabelCap}");
					}
					else if (CurStage?.statFactorEffectMultiplier != null)
					{
						stringBuilder.Append($" x {CurStage.statFactorEffectMultiplier.LabelCap}");
					}
					stringBuilder.AppendLine();
				}
			}
			if (ModsConfig.AnomalyActive && !def.aptitudes.NullOrEmpty())
			{
				stringBuilder.AppendLine(def.aptitudes.Select((Aptitude x) => x.skill.LabelCap.ToString() + " " + x.level.ToStringWithSign()).ToLineList("  - ", capitalizeItems: true));
			}
			HediffStage stage = CurStage;
			if (stage != null)
			{
				if (!stage.enablesNeeds.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine(("AddsNeeds".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
					stringBuilder.AppendLine(stage.enablesNeeds.Select((NeedDef x) => x.LabelCap.ToString()).ToLineList("  - "));
				}
				if (!stage.disablesNeeds.NullOrEmpty())
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine(("DisabledNeedsLabel".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
					stringBuilder.AppendLine(stage.disablesNeeds.Select((NeedDef x) => x.LabelCap.ToString()).ToLineList("  - "));
				}
				if (stage.disabledWorkTags != WorkTags.None)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine(("DisabledWorkLabel".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
					IEnumerable<string> items = from x in DefDatabase<WorkTypeDef>.AllDefsListForReading
						where (stage.disabledWorkTags & x.workTags) != 0
						select x.labelShort;
					stringBuilder.Append("  - " + items.ToCommaList().CapitalizeFirst());
				}
			}
			if (def.CompProps<HediffCompProperties_GiveLovinMTBFactor>() != null)
			{
				stringBuilder.AppendLine("  - " + "IncreasesChanceOfLovin".Translate());
			}
			return stringBuilder.ToString();
		}
	}

	public virtual HediffStage CurStage
	{
		get
		{
			if (!def.stages.NullOrEmpty())
			{
				return def.stages[CurStageIndex];
			}
			return null;
		}
	}

	public virtual bool ShouldRemove => Severity <= 0f;

	public virtual bool Visible
	{
		get
		{
			if (!visible && CurStage != null)
			{
				return CurStage.becomeVisible;
			}
			return true;
		}
	}

	public virtual float BleedRate => 0f;

	public virtual float BleedRateScaled => BleedRate / pawn.HealthScale;

	public bool Bleeding => BleedRate > 1E-05f;

	public virtual float PainOffset
	{
		get
		{
			if (CurStage != null && !causesNoPain)
			{
				return CurStage.painOffset;
			}
			return 0f;
		}
	}

	public virtual float PainFactor => CurStage?.painFactor ?? 1f;

	public List<PawnCapacityModifier> CapMods => CurStage?.capMods;

	public virtual float SummaryHealthPercentImpact => 0f;

	public virtual float TendPriority
	{
		get
		{
			float a = 0f;
			HediffStage curStage = CurStage;
			if (curStage != null && curStage.lifeThreatening)
			{
				a = Mathf.Max(a, 1f);
			}
			a = Mathf.Max(a, BleedRate * 1.5f);
			HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
			if (hediffComp_TendDuration != null && hediffComp_TendDuration.TProps.severityPerDayTended < 0f)
			{
				a = Mathf.Max(a, 0.025f);
			}
			return a;
		}
	}

	public virtual TextureAndColor StateIcon => TextureAndColor.None;

	public virtual int CurStageIndex => def.StageAtSeverity(Severity);

	public virtual float Severity
	{
		get
		{
			return severityInt;
		}
		set
		{
			bool flag = false;
			if (IsLethal && value >= def.lethalSeverity)
			{
				value = def.lethalSeverity;
				flag = true;
			}
			bool flag2 = this is Hediff_Injury && value > severityInt && Mathf.RoundToInt(value) != Mathf.RoundToInt(severityInt);
			int curStageIndex = CurStageIndex;
			severityInt = Mathf.Clamp(value, def.minSeverity, def.maxSeverity);
			if (CurStageIndex != curStageIndex)
			{
				OnStageIndexChanged(CurStageIndex);
			}
			if ((CurStageIndex != curStageIndex || flag || flag2) && pawn.health.hediffSet.hediffs.Contains(this))
			{
				pawn.health.Notify_HediffChanged(this);
				if (!pawn.Dead && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
				}
			}
		}
	}

	public BodyPartRecord Part
	{
		get
		{
			return part;
		}
		set
		{
			if (pawn == null && value != null)
			{
				Log.Error("Hediff: Cannot set Part without setting pawn first.");
			}
			else
			{
				part = value;
			}
		}
	}

	public bool IsLethal
	{
		get
		{
			if (def.lethalSeverity > 0f)
			{
				return canBeThreateningToPart;
			}
			return false;
		}
	}

	public bool IsCurrentlyLifeThreatening => IsStageLifeThreatening(CurStage);

	public List<Ability> AllAbilitiesForReading
	{
		get
		{
			if (abilities == null && !def.abilities.NullOrEmpty())
			{
				abilities = new List<Ability>();
				foreach (AbilityDef ability in def.abilities)
				{
					abilities.Add(AbilityUtility.MakeAbility(ability, pawn));
				}
			}
			return abilities;
		}
	}

	public virtual string Description => def.Description;

	public virtual bool TendableNow(bool ignoreTimer = false)
	{
		if (!def.tendable || Severity <= 0f || this.FullyImmune() || !Visible || this.IsPermanent() || !pawn.RaceProps.IsFlesh || (this is Hediff_Injury && !pawn.health.CanBleed))
		{
			return false;
		}
		if (!ignoreTimer)
		{
			HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
			if (hediffComp_TendDuration != null && !hediffComp_TendDuration.AllowTend)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsStageLifeThreatening(HediffStage stage)
	{
		if (stage == null)
		{
			return false;
		}
		if (stage.lifeThreatening)
		{
			return canBeThreateningToPart;
		}
		return false;
	}

	public bool IsAnyStageLifeThreatening()
	{
		if (def.stages == null || !canBeThreateningToPart)
		{
			return false;
		}
		for (int i = 0; i < def.stages.Count; i++)
		{
			if (def.stages[i].lifeThreatening)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanEverKill()
	{
		if (!IsLethal)
		{
			return IsAnyStageLifeThreatening();
		}
		return true;
	}

	public void SetVisible()
	{
		visible = true;
	}

	protected virtual void OnStageIndexChanged(int stageIndex)
	{
		if (CurStage.pctConditionalThoughtsNullified > 0f || CurStage.pctAllThoughtNullification > 0f)
		{
			pawn.health.hediffSet.CacheThoughtsNullified();
		}
	}

	public virtual void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving && combatLogEntry != null)
		{
			LogEntry target = combatLogEntry.Target;
			if (target == null || !Current.Game.battleLog.IsEntryActive(target))
			{
				combatLogEntry = null;
			}
		}
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
		Scribe_Values.Look(ref tickAdded, "tickAdded", 0);
		Scribe_Values.Look(ref visible, "visible", defaultValue: false);
		Scribe_Values.Look(ref severityInt, "severity", 0f);
		Scribe_Values.Look(ref recordedTale, "recordedTale", defaultValue: false);
		Scribe_Values.Look(ref causesNoPain, "causesNoPain", defaultValue: false);
		Scribe_Values.Look(ref combatLogText, "combatLogText");
		Scribe_Values.Look(ref canBeThreateningToPart, "canBeThreateningToPart", defaultValue: false);
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref sourceDef, "source");
		Scribe_Defs.Look(ref sourceHediffDef, "sourceHediffDef");
		Scribe_Defs.Look(ref sourceBodyPartGroup, "sourceBodyPartGroup");
		Scribe_Values.Look(ref sourceLabel, "sourceLabel");
		Scribe_Values.Look(ref sourceToolLabel, "sourceToolLabel");
		Scribe_BodyParts.Look(ref part, "part");
		Scribe_References.Look(ref combatLogEntry, "combatLogEntry");
		Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && abilities != null)
		{
			foreach (Ability ability in abilities)
			{
				ability.pawn = pawn;
				ability.verb.caster = pawn;
			}
		}
		BackCompatibility.PostExposeData(this);
	}

	public virtual void Tick()
	{
	}

	public virtual void TickInterval(int delta)
	{
		ageTicks += delta;
		if (def.hediffGivers != null && pawn.IsHashIntervalTick(60, delta))
		{
			for (int i = 0; i < def.hediffGivers.Count; i++)
			{
				def.hediffGivers[i].OnIntervalPassed(pawn, this);
			}
		}
		if (Visible && !visible)
		{
			visible = true;
			if (def.taleOnVisible != null)
			{
				TaleRecorder.RecordTale(def.taleOnVisible, pawn, def);
			}
		}
		HediffStage curStage = CurStage;
		if (curStage == null)
		{
			return;
		}
		if (curStage.hediffGivers != null && pawn.IsHashIntervalTick(60, delta))
		{
			for (int j = 0; j < curStage.hediffGivers.Count; j++)
			{
				curStage.hediffGivers[j].OnIntervalPassed(pawn, this);
			}
		}
		if (curStage.mentalStateGivers != null && pawn.IsHashIntervalTick(60, delta) && !pawn.InMentalState)
		{
			for (int k = 0; k < curStage.mentalStateGivers.Count; k++)
			{
				MentalStateGiver mentalStateGiver = curStage.mentalStateGivers[k];
				if (Rand.MTBEventOccurs(mentalStateGiver.mtbDays, 60000f, 60f))
				{
					pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateGiver.mentalState, "MentalStateReason_Hediff".Translate(Label));
				}
			}
		}
		if (curStage.mentalBreakMtbDays > 0f && pawn.IsHashIntervalTick(60, delta) && !pawn.InMentalState && !pawn.Downed && Rand.MTBEventOccurs(curStage.mentalBreakMtbDays, 60000f, 60f))
		{
			TryDoRandomMentalBreak();
		}
		if (curStage.vomitMtbDays > 0f && pawn.IsHashIntervalTick(600, delta) && Rand.MTBEventOccurs(curStage.vomitMtbDays, 60000f, 600f) && pawn.Spawned && pawn.Awake() && pawn.RaceProps.IsFlesh)
		{
			pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
		}
		if (curStage.forgetMemoryThoughtMtbDays > 0f && pawn.needs?.mood != null && pawn.IsHashIntervalTick(400, delta) && Rand.MTBEventOccurs(curStage.forgetMemoryThoughtMtbDays, 60000f, 400f) && pawn.needs.mood.thoughts.memories.Memories.TryRandomElement(out var result))
		{
			pawn.needs.mood.thoughts.memories.RemoveMemory(result);
		}
		if (!recordedTale && curStage.tale != null)
		{
			TaleRecorder.RecordTale(curStage.tale, pawn);
			recordedTale = true;
		}
		if (curStage.destroyPart && Part != null && Part != pawn.RaceProps.body.corePart)
		{
			pawn.health.AddHediff(HediffDefOf.MissingBodyPart, Part);
		}
		if (curStage.deathMtbDays > 0f && pawn.IsHashIntervalTick(200, delta) && Rand.MTBEventOccurs(curStage.deathMtbDays, 60000f, 200f))
		{
			DoMTBDeath();
		}
	}

	private void DoMTBDeath()
	{
		HediffStage curStage = CurStage;
		if (!curStage.mtbDeathDestroysBrain && ModsConfig.BiotechActive)
		{
			Pawn_GeneTracker genes = pawn.genes;
			if (genes != null && genes.HasActiveGene(GeneDefOf.Deathless))
			{
				return;
			}
		}
		pawn.Kill(null, this);
		if (curStage.mtbDeathDestroysBrain)
		{
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (brain != null)
			{
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, brain);
				pawn.health.AddHediff(hediff, brain);
			}
		}
	}

	private void TryDoRandomMentalBreak()
	{
		HediffStage curStage = CurStage;
		if (curStage != null)
		{
			TaggedString taggedString = "MentalStateReason_Hediff".Translate(Label);
			if (!curStage.mentalBreakExplanation.NullOrEmpty())
			{
				taggedString += "\n\n" + curStage.mentalBreakExplanation.Formatted(pawn.Named("PAWN"));
			}
			MentalBreakDef result;
			if (pawn.NonHumanlikeOrWildMan())
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, taggedString);
			}
			else if (DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef x) => x.Worker.BreakCanOccur(pawn) && (curStage.allowedMentalBreakIntensities == null || curStage.allowedMentalBreakIntensities.Contains(x.intensity))).TryRandomElementByWeight((MentalBreakDef x) => x.Worker.CommonalityFor(pawn), out result))
			{
				result.Worker.TryStart(pawn, taggedString.Resolve(), causedByMood: false);
			}
		}
	}

	public virtual void PostMake()
	{
		Severity = Mathf.Max(Severity, def.initialSeverity);
		causesNoPain = Rand.Value < def.chanceToCauseNoPain;
		if (def.onlyLifeThreateningTo == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < def.onlyLifeThreateningTo.Count; i++)
		{
			if (Part.def == def.onlyLifeThreateningTo[i])
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			canBeThreateningToPart = false;
		}
	}

	public virtual void PostAdd(DamageInfo? dinfo)
	{
		tickAdded = Find.TickManager.TicksGame;
		if (!def.abilities.NullOrEmpty())
		{
			pawn.abilities?.Notify_TemporaryAbilitiesChanged();
		}
		if (!def.removeWithTags.NullOrEmpty())
		{
			for (int num = pawn.health.hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				Hediff hediff = pawn.health.hediffSet.hediffs[num];
				if (hediff != this && !hediff.def.tags.NullOrEmpty())
				{
					for (int i = 0; i < def.removeWithTags.Count; i++)
					{
						if (hediff.def.tags.Contains(def.removeWithTags[i]))
						{
							pawn.health.RemoveHediff(hediff);
							break;
						}
					}
				}
			}
		}
		if (!def.aptitudes.NullOrEmpty())
		{
			pawn.skills.DirtyAptitudes();
		}
		if (def.clearsEgo)
		{
			pawn.everLostEgo = true;
		}
	}

	public virtual void PreRemoved()
	{
	}

	public virtual void PostRemoved()
	{
		HediffStage curStage = CurStage;
		if (!pawn.Dead && def.chemicalNeed != null)
		{
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
		else if (curStage != null && !pawn.Dead && (!curStage.disablesNeeds.NullOrEmpty() || !curStage.enablesNeeds.NullOrEmpty()))
		{
			pawn.needs?.AddOrRemoveNeedsAsAppropriate();
		}
		if (!def.abilities.NullOrEmpty())
		{
			pawn.abilities?.Notify_TemporaryAbilitiesChanged();
		}
		if (!def.aptitudes.NullOrEmpty())
		{
			pawn.skills.DirtyAptitudes();
		}
	}

	public virtual void PostTick()
	{
	}

	public virtual void PostTickInterval(int delta)
	{
	}

	public virtual void Tended(float quality, float maxQuality, int batchPosition = 0)
	{
	}

	public virtual void Heal(float amount)
	{
		if (!(amount <= 0f))
		{
			Severity -= amount;
			pawn.health.Notify_HediffChanged(this);
		}
	}

	public virtual void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
	{
	}

	public virtual bool TryMergeWith(Hediff other)
	{
		if (other == null || other.def != def || other.Part != Part)
		{
			return false;
		}
		Severity += other.Severity;
		ageTicks = 0;
		return true;
	}

	public virtual bool CauseDeathNow()
	{
		if (IsLethal)
		{
			bool num = Severity >= def.lethalSeverity;
			if (num && DebugViewSettings.logCauseOfDeath)
			{
				Log.Message("CauseOfDeath: lethal severity exceeded " + Severity + " >= " + def.lethalSeverity);
			}
			return num;
		}
		return false;
	}

	public virtual void Notify_Downed()
	{
	}

	public virtual void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
	}

	public virtual void Notify_PawnKilled()
	{
	}

	public virtual void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
	}

	public virtual void Notify_PawnDamagedThing(Thing thing, DamageInfo dinfo, DamageWorker.DamageResult result)
	{
	}

	public virtual void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo targets)
	{
	}

	public virtual void Notify_EntropyGained(float baseAmount, float finalAmount, Thing source = null)
	{
	}

	public virtual void Notify_RelationAdded(Pawn otherPawn, PawnRelationDef relationDef)
	{
	}

	public virtual void Notify_ImplantUsed(string violationSourceName, float detectionChance, int violationSourceLevel = -1)
	{
	}

	public virtual void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
	{
	}

	public virtual void Notify_IngestedThing(Thing thing, int amount)
	{
	}

	public virtual void Notify_Resurrected()
	{
	}

	public virtual void Notify_PawnCorpseSpawned()
	{
	}

	public virtual void Notify_PawnCorpseDestroyed()
	{
	}

	public virtual void Notify_Regenerated(float hp)
	{
	}

	public virtual void Notify_SurgicallyRemoved(Pawn surgeon)
	{
		if (def.HasDefinedGraphicProperties || def.forceRenderTreeRecache)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public virtual void Notify_SurgicallyReplaced(Pawn surgeon)
	{
		if (def.HasDefinedGraphicProperties || def.forceRenderTreeRecache)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public virtual void Notify_Spawned()
	{
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		return null;
	}

	public virtual string GetInspectString()
	{
		return def.inspectString ?? string.Empty;
	}

	public virtual string GetTooltip(Pawn pawn, bool showHediffsDebugInfo)
	{
		tipSb.Clear();
		HediffStage curStage = CurStage;
		if (!LabelCap.NullOrEmpty())
		{
			tipSb.AppendTagged(LabelCap.Colorize(ColoredText.TipSectionTitleColor));
		}
		string severityLabel = SeverityLabel;
		if (!severityLabel.NullOrEmpty())
		{
			tipSb.Append(": ").Append(severityLabel);
		}
		tipSb.AppendLine();
		if (!def.overrideTooltip.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLineTagged(def.overrideTooltip.Formatted(pawn.Named("PAWN")));
		}
		else if (curStage != null && !curStage.overrideTooltip.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLineTagged(curStage.overrideTooltip.Formatted(pawn.Named("PAWN")));
		}
		else
		{
			string description = Description;
			if (!description.NullOrEmpty())
			{
				tipSb.AppendLine().AppendLine(description);
			}
		}
		if (!def.extraTooltip.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLineTagged(def.extraTooltip.Formatted(pawn.Named("PAWN")));
		}
		if (curStage != null && !curStage.extraTooltip.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLineTagged(curStage.extraTooltip.Formatted(pawn.Named("PAWN")));
		}
		string tipStringExtra = TipStringExtra;
		if (!tipStringExtra.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLine(tipStringExtra.TrimEndNewlines());
		}
		if (HealthCardUtility.GetCombatLogInfo(Gen.YieldSingle(this), out var taggedString, out var _) && !taggedString.NullOrEmpty())
		{
			tipSb.AppendLine().AppendLineTagged(("Cause".Translate() + ": " + taggedString).Colorize(ColoredText.SubtleGrayColor));
		}
		if (showHediffsDebugInfo && !DebugString().NullOrEmpty() && !DebugString().NullOrEmpty())
		{
			tipSb.AppendLine().AppendLine(DebugString().TrimEndNewlines());
		}
		return tipSb.ToString().TrimEnd();
	}

	public virtual void CopyFrom(Hediff other)
	{
		ageTicks = other.ageTicks;
		sourceLabel = other.sourceLabel;
		sourceDef = other.sourceDef;
		sourceBodyPartGroup = other.sourceBodyPartGroup;
		severityInt = other.severityInt;
	}

	public virtual void PostDebugAdd()
	{
	}

	public virtual string DebugString()
	{
		string text = "";
		if (!Visible)
		{
			text += "hidden\n";
		}
		text = text + "severity: " + Severity.ToString("F3") + ((Severity >= def.maxSeverity) ? " (reached max)" : "");
		if (TendableNow())
		{
			text = text + "\ntend priority: " + TendPriority;
		}
		return text.Indented();
	}

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
	{
		foreach (StatDrawEntry item in def.SpecialDisplayStats(req))
		{
			yield return item;
		}
	}

	public override string ToString()
	{
		return "(" + (def?.defName ?? GetType().Name) + ((part != null) ? (" " + part.Label) : "") + " ticksSinceCreation=" + ageTicks + ")";
	}

	public string GetUniqueLoadID()
	{
		return "Hediff_" + loadID;
	}
}
