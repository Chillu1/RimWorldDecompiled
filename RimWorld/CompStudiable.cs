using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompStudiable : ThingComp
{
	private static readonly CachedTexture StudyToggleIcon = new CachedTexture("UI/Icons/Study");

	public int lastStudiedTick = -9999999;

	public bool studyEnabled = true;

	public float studyPoints;

	public float anomalyKnowledgeGained;

	public int studyInteractions;

	private CompStudyUnlocks compStudyUnlocks;

	private CompActivity compActivity;

	private static StringBuilder knowledgeSb = new StringBuilder();

	public CompProperties_Studiable Props => (CompProperties_Studiable)props;

	public Pawn Pawn => parent as Pawn;

	public float ProgressPercent
	{
		get
		{
			if (!Props.Completable)
			{
				return 0f;
			}
			return studyPoints / Props.studyAmountToComplete;
		}
	}

	public float AdjustedAnomalyKnowledgePerStudy => AdjustedAnomalyKnowledge() / 5f;

	public bool Completed
	{
		get
		{
			if (Props.Completable)
			{
				return ProgressPercent >= 1f;
			}
			return false;
		}
	}

	public int TicksTilNextStudy => lastStudiedTick + Props.frequencyTicks - Find.TickManager.TicksGame;

	private bool IsMutant
	{
		get
		{
			if (Pawn != null)
			{
				return Pawn.IsMutant;
			}
			return false;
		}
	}

	public virtual float AnomalyKnowledge
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return 0f;
			}
			if (KnowledgeCategory == null)
			{
				return 0f;
			}
			float num = 0f;
			CompStudyUnlocks obj = compStudyUnlocks;
			if (obj != null && obj.StudyKnowledgeAmount.HasValue)
			{
				num = compStudyUnlocks.StudyKnowledgeAmount.Value;
			}
			else if (Props.anomalyKnowledge.HasValue)
			{
				num = Props.anomalyKnowledge.Value;
			}
			else if (Pawn != null)
			{
				num = Pawn.RaceProps.anomalyKnowledge;
			}
			else
			{
				Debug.LogError($"Thing {parent} has studiable comp but has no overriden anomaly knowledge amount, ensure this is on a pawn with setup race props vars, or provides a knowledge amount value in this comp.");
			}
			CompStudyUnlocks obj2 = compStudyUnlocks;
			if (obj2 != null && obj2.StudyKnowledgeAmount.HasValue)
			{
				num = compStudyUnlocks.StudyKnowledgeAmount.Value;
			}
			if (IsMutant)
			{
				num += Pawn.mutant.Def.anomalyKnowledgeOffset;
			}
			return num;
		}
	}

	public virtual KnowledgeCategoryDef KnowledgeCategory
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return null;
			}
			if (IsMutant)
			{
				return Pawn.mutant.Def.knowledgeCategory;
			}
			if (Props.knowledgeCategory != null)
			{
				return Props.knowledgeCategory;
			}
			return Pawn?.RaceProps.knowledgeCategory;
		}
	}

	public bool RequiresHoldingPlatform
	{
		get
		{
			if (!IsMutant)
			{
				return Props.requiresHoldingPlatform;
			}
			return true;
		}
	}

	public bool RequiresImprisonment
	{
		get
		{
			if (!IsMutant)
			{
				return Props.requiresImprisonment;
			}
			return false;
		}
	}

	public bool Deactivated
	{
		get
		{
			if (Props.canBeActivityDeactivated)
			{
				return CompActivity.Deactivated;
			}
			return false;
		}
	}

	private bool DisplayStudyInfoOnInspectPane
	{
		get
		{
			if (parent.Destroyed)
			{
				return false;
			}
			if (Completed)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && Find.Anomaly.HighestLevelReached < Props.minMonolithLevelForStudy)
			{
				return false;
			}
			return true;
		}
	}

	private CompActivity CompActivity => compActivity ?? (compActivity = parent.GetComp<CompActivity>());

	public override void PostPostMake()
	{
		base.PostPostMake();
		studyEnabled = Props.studyEnabledByDefault;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		compStudyUnlocks = parent.TryGetComp<CompStudyUnlocks>();
		Find.StudyManager.UpdateStudiableCache(parent, parent.Map ?? (parent.ParentHolder as Thing)?.Map);
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		Find.StudyManager.UpdateStudiableCache(parent, map);
		base.PostDeSpawn(map, mode);
	}

	public void SetStudyEnabled(bool enabled)
	{
		studyEnabled = enabled;
	}

	public bool CurrentlyStudiable()
	{
		if (!EverStudiable(out var _))
		{
			return false;
		}
		if (!studyEnabled)
		{
			return false;
		}
		if (Props.frequencyTicks > 0 && TicksTilNextStudy > 0)
		{
			return false;
		}
		if (parent is Pawn pawn)
		{
			if (RequiresHoldingPlatform)
			{
				CompHoldingPlatformTarget compHoldingPlatformTarget = pawn.TryGetComp<CompHoldingPlatformTarget>();
				if (compHoldingPlatformTarget == null || !compHoldingPlatformTarget.CanStudy)
				{
					return false;
				}
			}
			if (RequiresImprisonment)
			{
				if (pawn.Downed || !pawn.Awake())
				{
					return false;
				}
				if (pawn.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Study))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool EverStudiable()
	{
		string reason;
		return EverStudiable(out reason);
	}

	public bool EverStudiable(out string reason)
	{
		reason = null;
		if (!StudyUnlocked())
		{
			return false;
		}
		if (!EverStudiableCached(out reason))
		{
			return false;
		}
		return true;
	}

	public bool StudyUnlocked()
	{
		if (parent.Destroyed)
		{
			return false;
		}
		if (Deactivated)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && Find.Anomaly.HighestLevelReached < Props.minMonolithLevelForStudy && Find.Anomaly.GenerateMonolith)
		{
			return false;
		}
		return true;
	}

	public bool EverStudiableCached()
	{
		string reason;
		return EverStudiableCached(out reason);
	}

	public bool EverStudiableCached(out string reason)
	{
		reason = null;
		if (parent == null)
		{
			return false;
		}
		if (parent is Pawn pawn)
		{
			if (RequiresHoldingPlatform && !(parent.ParentHolder is Building_HoldingPlatform))
			{
				reason = "RequiresHoldingPlatform".Translate();
				return false;
			}
			if (RequiresImprisonment && ((!pawn.Inhumanized() && !pawn.kindDef.studiableAsPrisoner) || !pawn.IsPrisonerOfColony))
			{
				reason = "RequiresImprisonment".Translate();
				return false;
			}
		}
		return true;
	}

	public virtual void Study(Pawn studier, float studyAmount, float anomalyKnowledgeAmount = 0f)
	{
		bool completed = Completed;
		studyAmount *= Find.Storyteller.difficulty.researchSpeedFactor;
		studyAmount *= studier.GetStatValue(StatDefOf.ResearchSpeed);
		anomalyKnowledgeGained += anomalyKnowledgeAmount;
		Find.StudyManager.Study(parent, studier, studyAmount);
		if (ModsConfig.AnomalyActive && anomalyKnowledgeAmount > 0f)
		{
			Find.StudyManager.StudyAnomaly(parent, studier, anomalyKnowledgeAmount, KnowledgeCategory);
		}
		studier?.skills.Learn(SkillDefOf.Intellectual, 0.1f);
		if (!completed && Completed)
		{
			QuestUtility.SendQuestTargetSignals(parent.questTags, "Researched", parent.Named("SUBJECT"), studier.Named("STUDIER"));
			if (!Props.completedMessage.NullOrEmpty())
			{
				Messages.Message(Props.completedMessage, parent, MessageTypeDefOf.NeutralEvent);
			}
			if (studier != null && !Props.completedLetterText.NullOrEmpty() && !Props.completedLetterTitle.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(Props.completedLetterTitle.Formatted(studier.Named("STUDIER"), parent.Named("PARENT")), Props.completedLetterText.Formatted(studier.Named("STUDIER"), parent.Named("PARENT")), Props.completedLetterDef ?? LetterDefOf.NeutralEvent, new List<Thing> { parent, studier });
			}
		}
	}

	public void Notify_ActivityDeactivated()
	{
		if (Props.canBeActivityDeactivated)
		{
			studyEnabled = false;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		string reason;
		bool flag = EverStudiable(out reason);
		if (Props.showToggleGizmo && (flag || reason != null))
		{
			Command_Toggle command_Toggle = new Command_Toggle
			{
				defaultLabel = "CommandToggleStudy".Translate(),
				defaultDesc = "CommandToggleStudyDesc".Translate(),
				icon = StudyToggleIcon.Texture,
				isActive = () => studyEnabled,
				toggleAction = delegate
				{
					SetStudyEnabled(!studyEnabled);
				},
				hideIconIfDisabled = true
			};
			command_Toggle.tutorTag = "ToggleStudy";
			if (reason != null)
			{
				command_Toggle.Disable(reason);
			}
			yield return command_Toggle;
		}
		if (!DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		if (EverStudiable() && TicksTilNextStudy > 0)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "DEV: End study cooldown";
			command_Action.action = delegate
			{
				lastStudiedTick = Find.TickManager.TicksGame - Props.frequencyTicks;
			};
			yield return command_Action;
		}
		if (!Props.Completable || Completed)
		{
			yield break;
		}
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = "DEV: Complete study";
		command_Action2.action = delegate
		{
			int num = 100;
			while (!Completed && num > 0)
			{
				Study(parent.Map?.mapPawns?.FreeColonists?.RandomElement(), float.MaxValue);
				num--;
			}
		};
		yield return command_Action2;
	}

	private IEnumerable<Dialog_InfoCard.Hyperlink> GetRelatedQuestHyperlinks()
	{
		List<Quest> quests = Find.QuestManager.QuestsListForReading;
		for (int i = 0; i < quests.Count; i++)
		{
			if (quests[i].hidden || (quests[i].State != QuestState.Ongoing && quests[i].State != QuestState.NotYetAccepted))
			{
				continue;
			}
			List<QuestPart> partsListForReading = quests[i].PartsListForReading;
			for (int j = 0; j < partsListForReading.Count; j++)
			{
				if (partsListForReading[j] is QuestPart_RequirementsToAcceptThingStudied questPart_RequirementsToAcceptThingStudied && questPart_RequirementsToAcceptThingStudied.thing == parent)
				{
					yield return new Dialog_InfoCard.Hyperlink(quests[i]);
					break;
				}
			}
		}
	}

	private float AdjustedAnomalyKnowledge(StringBuilder sb = null)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return 0f;
		}
		float num = AnomalyKnowledge;
		sb?.AppendLine("StatsReport_BaseValue".Translate() + ": " + num.ToStringDecimalIfSmall());
		bool flag = false;
		if (parent.TryGetComp<CompHoldingPlatformTarget>(out var comp) && comp.CurrentlyHeldOnPlatform)
		{
			CompEntityHolder comp2 = comp.HeldPlatform.GetComp<CompEntityHolder>();
			float studyKnowledgeAmountMultiplier = ContainmentUtility.GetStudyKnowledgeAmountMultiplier(parent, comp2);
			if (!Mathf.Approximately(studyKnowledgeAmountMultiplier, 1f))
			{
				if (!flag)
				{
					sb?.AppendLine();
					flag = true;
				}
				num *= studyKnowledgeAmountMultiplier;
				sb?.AppendLine("FactorContainmentStrength".Translate() + " (" + StatDefOf.ContainmentStrength.Worker.ValueToString(comp2.ContainmentStrength, finalized: true) + ")" + ": x" + studyKnowledgeAmountMultiplier.ToStringPercent());
			}
			if (comp.HeldPlatform.HasAttachedElectroharvester)
			{
				if (!flag)
				{
					sb?.AppendLine();
					flag = true;
				}
				num *= 0.5f;
				sb?.AppendLine(ThingDefOf.Electroharvester.LabelCap + ": x" + 0.5f.ToStringPercent());
			}
		}
		if (parent.TryGetComp<CompActivity>(out var comp3))
		{
			if (!flag)
			{
				sb?.AppendLine();
				flag = true;
			}
			num *= comp3.ActivityResearchFactor;
			sb?.AppendLine("FactorActivity".Translate() + ": x" + comp3.ActivityResearchFactor.ToStringPercent());
		}
		if (!Mathf.Approximately(Props.knowledgeFactorOutdoors, 1f) && parent.IsOutside())
		{
			if (!flag)
			{
				sb?.AppendLine();
				flag = true;
			}
			num *= Props.knowledgeFactorOutdoors;
			sb?.AppendLine("Outdoors".Translate().CapitalizeFirst() + ": x" + Props.knowledgeFactorOutdoors.ToStringPercent());
		}
		if (!Mathf.Approximately(Find.Storyteller.difficulty.studyEfficiencyFactor, 1f))
		{
			if (!flag)
			{
				sb?.AppendLine();
				flag = true;
			}
			num *= Find.Storyteller.difficulty.studyEfficiencyFactor;
			sb?.AppendLine("DifficultyLevel".Translate() + ": x" + Find.Storyteller.difficulty.studyEfficiencyFactor.ToStringPercent());
		}
		sb?.AppendLine();
		sb?.AppendLine("StatsReport_FinalValue".Translate() + ": " + num.ToStringDecimalIfSmall());
		return num;
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
		if (enumerable != null)
		{
			foreach (StatDrawEntry item in enumerable)
			{
				yield return item;
			}
		}
		if (Props.Completable)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawn, "Study".Translate(), studyPoints + " / " + Props.studyAmountToComplete, "Stat_Studiable_Desc".Translate(), 3000, null, GetRelatedQuestHyperlinks(), forceUnfinalizedMode: false, overridesHideStats: true);
		}
		if (ModsConfig.AnomalyActive && AnomalyKnowledge > 0f && KnowledgeCategory != null && (!(parent is Pawn pawn) || IsMutant || RequiresHoldingPlatform || (RequiresImprisonment && (pawn.Inhumanized() || pawn.kindDef.studiableAsPrisoner))))
		{
			knowledgeSb.Clear();
			float f = AdjustedAnomalyKnowledge(knowledgeSb);
			yield return new StatDrawEntry(StatCategoryDefOf.Containment, "KnowledgeFromStudy".Translate(), "Stat_Knowledge".Translate(f.ToStringDecimalIfSmall(), KnowledgeCategory.label), "KnowledgeFromStudyDesc".Translate() + "\n\n" + knowledgeSb.ToString(), 2550, null, null, forceUnfinalizedMode: false, overridesHideStats: true);
			if (Props.frequencyTicks > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Containment, "StudyFrequency".Translate(), Props.frequencyTicks.ToStringTicksToPeriod(), "StudyFrequencyDesc".Translate(), 2545, null, null, forceUnfinalizedMode: false, overridesHideStats: true);
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref lastStudiedTick, "lastStudiedTick", 0);
		Scribe_Values.Look(ref studyEnabled, "studyEnabled", defaultValue: true);
		Scribe_Values.Look(ref studyPoints, "studiedAmount", 0f);
		Scribe_Values.Look(ref anomalyKnowledgeGained, "anomalyKnowledgeGained", 0f);
		Scribe_Values.Look(ref studyInteractions, "studyInteractions", 0);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			float value = 0f;
			Scribe_Values.Look(ref value, "progress", 0f);
			if (value > 0f)
			{
				float num = value / Props.studyAmountToComplete;
				studyPoints = Props.studyAmountToComplete * num;
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit && Find.StudyManager.backCompatStudyProgress.TryGetValue(parent.def, out var value2))
		{
			studyPoints = Props.studyAmountToComplete * value2;
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = string.Empty;
		if (!EverStudiable())
		{
			return text;
		}
		if (Props.Completable)
		{
			text = string.Concat(text, "StudyProgress".Translate() + ": " + studyPoints.ToString("F1") + " / ", Props.studyAmountToComplete.ToString());
			if (Completed)
			{
				text += " (" + "StudyCompleted".Translate() + ")";
			}
		}
		else if (!RequiresHoldingPlatform && !(parent is Pawn))
		{
			string inspectStringExtraFor = GetInspectStringExtraFor(parent);
			if (!inspectStringExtraFor.NullOrEmpty())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += inspectStringExtraFor;
			}
		}
		return text;
	}

	public static string GetInspectStringExtraFor(Thing thing)
	{
		CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
		if (compStudiable != null && compStudiable.DisplayStudyInfoOnInspectPane && compStudiable.EverStudiable())
		{
			TaggedString empty = TaggedString.Empty;
			if (ModsConfig.AnomalyActive && compStudiable.AnomalyKnowledge > 0f && compStudiable.KnowledgeCategory != null)
			{
				empty += "KnowledgeCategory".Translate() + ": " + compStudiable.KnowledgeCategory.LabelCap;
				if (compStudiable.TicksTilNextStudy <= 0)
				{
					empty += string.Format(" ({0})", "ReadyToStudy".Translate());
				}
				else if (compStudiable.TicksTilNextStudy > 0)
				{
					empty += string.Format(" ({0})", "CanBeStudiedInDuration".Translate(compStudiable.TicksTilNextStudy.ToStringTicksToPeriod()));
				}
				if (compStudiable.Props.knowledgeFactorOutdoors != 1f && compStudiable.parent.IsOutside())
				{
					empty += string.Format("\n{0} ({1})", "KnowledgeFactorOutdoors".Translate() + ": x" + compStudiable.Props.knowledgeFactorOutdoors.ToStringPercent(), "Outdoors".Translate());
				}
			}
			else if (compStudiable.TicksTilNextStudy <= 0)
			{
				empty += "ReadyToStudy".Translate().CapitalizeFirst();
			}
			else
			{
				empty += "CanBeStudiedInDuration".Translate(compStudiable.TicksTilNextStudy.ToStringTicksToPeriod());
			}
			return empty.Resolve();
		}
		return null;
	}
}
