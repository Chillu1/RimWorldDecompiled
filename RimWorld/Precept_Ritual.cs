using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Grammar;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Precept_Ritual : Precept
	{
		public List<RitualObligation> activeObligations;

		public List<RitualObligation> completedObligations;

		public RulePackDef nameMaker;

		public List<RitualObligationTrigger> obligationTriggers = new List<RitualObligationTrigger>();

		public RitualOutcomeEffectWorker outcomeEffect;

		public RitualAttachableOutcomeEffectDef attachableOutcomeEffect;

		public RitualObligationTargetFilter obligationTargetFilter;

		public RitualTargetFilter targetFilter;

		public RitualBehaviorWorker behavior;

		public bool ritualOnlyForIdeoMembers;

		public bool isAnytime;

		public bool canBeAnytime;

		public bool allowOtherInstances;

		public bool playsIdeoMusic;

		public bool mergeGizmosForObligations;

		public bool canMergeGizmosFromDifferentIdeos = true;

		public int lastFinishedTick = -1;

		public int abilityOnCooldownUntilTick = -1;

		public bool ignoreExtremeTemperatures;

		public string ritualExpectedDesc;

		public string ritualExpectedDescNoAdjective;

		public string shortDescOverride;

		public string ritualExplanation;

		public string iconPathOverride;

		public string cancelIconPathOverride;

		public string patternGroupTag;

		public TechLevel minTechLevel;

		public TechLevel maxTechLevel;

		public bool generatedAttachedReward;

		public bool showIdeoIconsInDialog = true;

		public List<PlanetLayerDef> layerWhitelist;

		public List<PlanetLayerDef> layerBlacklist;

		public RitualPatternDef sourcePattern;

		private string shortDescOverrideCap;

		public const float RepeatQualityPenaltyMax = -0.95f;

		public const int RepeatPenaltyDurationDays = 20;

		public const float LowQualityWarningThreshold = 0.25f;

		private readonly IntVec2 DateIconSize = new IntVec2(28, 28);

		private Texture2D icon;

		private Texture2D cancelIcon;

		private string tipLabelCached;

		private static Texture2D dayRitualTex;

		private static Texture2D anytimeRitualTex;

		private static Texture2D eventRitualTex;

		private StringBuilder tmpTipExtraSb = new StringBuilder();

		private List<LordJob_Ritual> tmpActiveRituals = new List<LordJob_Ritual>();

		public float RepeatQualityPenalty => Mathf.Lerp(-0.95f, 0f, RepeatPenaltyProgress);

		public override string UIInfoFirstLine
		{
			get
			{
				if (!ShortDescOverrideCap.NullOrEmpty())
				{
					return ShortDescOverrideCap;
				}
				return def.LabelCap.Resolve();
			}
		}

		public override string UIInfoSecondLine => base.LabelCap;

		public bool RepeatPenaltyActive
		{
			get
			{
				if (isAnytime && lastFinishedTick != -1 && def.useRepeatPenalty)
				{
					return TicksSinceLastPerformed < 1200000;
				}
				return false;
			}
		}

		public int TicksSinceLastPerformed => GenTicks.TicksGame - lastFinishedTick;

		public float RepeatPenaltyProgress => (float)TicksSinceLastPerformed / 1200000f;

		public string RepeatPenaltyTimeLeft => (1200000 - TicksSinceLastPerformed).ToStringTicksToPeriod();

		public bool SupportsAttachableOutcomeEffect => outcomeEffect?.SupportsAttachableOutcomeEffect ?? false;

		public bool IsDateTriggered
		{
			get
			{
				if (obligationTriggers != null)
				{
					return obligationTriggers.Any((RitualObligationTrigger o) => o is RitualObligationTrigger_Date);
				}
				return false;
			}
		}

		public string ShortDescOverrideCap
		{
			get
			{
				if (shortDescOverrideCap.NullOrEmpty() && shortDescOverride != null)
				{
					shortDescOverrideCap = shortDescOverride.CapitalizeFirst();
				}
				return shortDescOverrideCap;
			}
		}

		public override Texture2D Icon
		{
			get
			{
				if (icon == null)
				{
					string text = iconPathOverride ?? def.iconPath;
					if (text != null)
					{
						icon = ContentFinder<Texture2D>.Get(text);
					}
					else
					{
						icon = ideo.Icon;
					}
				}
				return icon;
			}
		}

		public virtual Texture2D CancelIcon
		{
			get
			{
				if (cancelIcon == null)
				{
					string text = cancelIconPathOverride ?? def.iconPath;
					if (text != null)
					{
						cancelIcon = ContentFinder<Texture2D>.Get(text);
					}
					else
					{
						cancelIcon = ideo.Icon;
					}
				}
				return cancelIcon;
			}
		}

		public override string TipLabel
		{
			get
			{
				if (tipLabelCached == null)
				{
					tipLabelCached = ((!def.tipLabelOverride.NullOrEmpty()) ? def.tipLabelOverride : base.LabelCap);
					if (!ShortDescOverrideCap.NullOrEmpty() && ShortDescOverrideCap != tipLabelCached)
					{
						tipLabelCached = tipLabelCached + "\n" + ShortDescOverrideCap;
					}
					if (isAnytime)
					{
						tipLabelCached += "\n" + "RitualStartAnyTime".Translate();
					}
					else
					{
						RitualObligationTrigger_Date ritualObligationTrigger_Date = obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault();
						if (ritualObligationTrigger_Date != null)
						{
							tipLabelCached = tipLabelCached + "\n" + ritualObligationTrigger_Date.DateString;
						}
						else
						{
							tipLabelCached += "\n" + "RitualStartEvent".Translate();
						}
					}
				}
				return tipLabelCached;
			}
		}

		public override bool UsesGeneratedName => true;

		public override bool CanRegenerate => true;

		public static Texture2D DateRitualTex
		{
			get
			{
				if (dayRitualTex == null)
				{
					dayRitualTex = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/DateRitual");
				}
				return dayRitualTex;
			}
		}

		public static Texture2D AnytimeRitualTex
		{
			get
			{
				if (anytimeRitualTex == null)
				{
					anytimeRitualTex = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/AnytimeRitual");
				}
				return anytimeRitualTex;
			}
		}

		public static Texture2D EventRitualTex
		{
			get
			{
				if (eventRitualTex == null)
				{
					eventRitualTex = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/EventRitual");
				}
				return eventRitualTex;
			}
		}

		public TaggedString GetBeginRitualText(RitualObligation obligation = null)
		{
			string text = obligationTargetFilter?.LabelExtraPart(obligation) ?? "";
			if (!string.IsNullOrEmpty(def.ritualPatternBase.beginRitualOverride))
			{
				return new TaggedString(def.ritualPatternBase.beginRitualOverride).Formatted(Label, text);
			}
			if (text.NullOrEmpty() || mergeGizmosForObligations)
			{
				return "BeginRitual".Translate(Label);
			}
			return "BeginRitualFor".Translate(Label, text);
		}

		public override void ClearTipCache()
		{
			base.ClearTipCache();
			tipLabelCached = null;
		}

		public override string GetTip()
		{
			return TipMainPart() + TipExtraPart();
		}

		public string TipMainPart()
		{
			Precept.tmpCompsDesc.Clear();
			if (RepeatPenaltyActive)
			{
				float num = (float)Mathf.RoundToInt(RepeatPenaltyProgress * 20f * 10f) / 10f;
				float num2 = (float)Mathf.RoundToInt((1f - RepeatPenaltyProgress) * 20f * 10f) / 10f;
				Precept.tmpCompsDesc.AppendLine(ColorizeWarning("RitualRepeatPenaltyTip".Translate(20, num, RepeatQualityPenalty.ToStringPercent(), num2)));
				Precept.tmpCompsDesc.AppendLine();
			}
			if (!DescriptionForTip.NullOrEmpty())
			{
				string value = DescriptionForTip;
				if (behavior?.descriptionOverride != null)
				{
					value = behavior?.descriptionOverride;
				}
				Precept.tmpCompsDesc.Append(value);
			}
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				if (obligationTrigger.TriggerExtraDesc != null)
				{
					Precept.tmpCompsDesc.AppendLine();
					Precept.tmpCompsDesc.AppendInNewLine(obligationTrigger.TriggerExtraDesc);
				}
			}
			if (outcomeEffect != null)
			{
				StringBuilder stringBuilder = new StringBuilder(outcomeEffect.Description);
				if (!outcomeEffect.def.extraPredictedOutcomeDescriptions.NullOrEmpty())
				{
					foreach (string extraPredictedOutcomeDescription in outcomeEffect.def.extraPredictedOutcomeDescriptions)
					{
						stringBuilder.Append(" " + extraPredictedOutcomeDescription.Formatted(shortDescOverride ?? def.label));
					}
				}
				if (attachableOutcomeEffect != null)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendInNewLine(attachableOutcomeEffect.DescriptionForRitualValidated(this));
				}
				if (stringBuilder.Length > 0)
				{
					Precept.tmpCompsDesc.AppendLine();
					Precept.tmpCompsDesc.AppendInNewLine(stringBuilder.ToString());
				}
			}
			return Precept.tmpCompsDesc.ToString();
		}

		public string TipExtraPart()
		{
			tmpTipExtraSb.Clear();
			if (obligationTargetFilter != null && obligationTargetFilter.GetTargetInfos(null).Any())
			{
				tmpTipExtraSb.AppendLine();
				tmpTipExtraSb.AppendInNewLine(ColorizeDescTitle("RitualFocusObjects".Translate() + ":"));
				tmpTipExtraSb.AppendInNewLine(obligationTargetFilter.GetTargetInfos(null).ToLineList("  - ", capitalizeItems: true));
			}
			string text = RolesDescription();
			if (text != null)
			{
				tmpTipExtraSb.Append(ColorizeDescTitle("\n\n" + "RitualParticipatingRoles".Translate() + ":" + "\n"));
				tmpTipExtraSb.Append(text);
			}
			if (ideo.Fluid)
			{
				SimpleCurve developmentPointsOverOutcomeIndexCurveForRitual = IdeoDevelopmentUtility.GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ideo, this);
				if (developmentPointsOverOutcomeIndexCurveForRitual != null)
				{
					float y = developmentPointsOverOutcomeIndexCurveForRitual.MinBy((CurvePoint p) => p.y).y;
					float y2 = developmentPointsOverOutcomeIndexCurveForRitual.MaxBy((CurvePoint p) => p.y).y;
					tmpTipExtraSb.Append(ColorizeDescTitle("\n\n" + "RitualDevelopmentPoints".Translate() + ":" + "\n"));
					tmpTipExtraSb.Append("RitualDevelopmentPointsTip".Translate(y, y2));
				}
			}
			return tmpTipExtraSb.ToString();
		}

		public string RolesDescription()
		{
			if (behavior.def.roles.NullOrEmpty())
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (RitualRole role in behavior.def.roles)
			{
				Precept_Role precept_Role = role.FindInstance(ideo);
				TaggedString taggedString = role.LabelCap;
				if (precept_Role != null)
				{
					taggedString = precept_Role.LabelCap;
					if (precept_Role != null && precept_Role.def?.leaderRole == true)
					{
						Pawn pawn = precept_Role.ChosenPawnSingle();
						if (pawn != null)
						{
							taggedString = precept_Role.LabelForPawn(pawn).CapitalizeFirst();
						}
					}
				}
				stringBuilder.AppendInNewLine("  - " + taggedString + " (" + ((role.required && !role.substitutable) ? "Required".Translate().ToLower() : "Optional".Translate()) + ")");
			}
			return stringBuilder.ToString();
		}

		public override void Init(Ideo ideo, FactionDef generatingFor = null)
		{
			base.Init(ideo);
			RegenerateName();
		}

		public override void PostMake()
		{
			sourcePattern = def.ritualPatternBase;
		}

		public override string GenerateNameRaw()
		{
			if (nameMaker == null)
			{
				return def.label;
			}
			GrammarRequest request = new GrammarRequest
			{
				Includes = { nameMaker }
			};
			AddIdeoRulesTo(ref request);
			if (!request.Rules.Any())
			{
				return def.label;
			}
			if (ideo.culture.festivalNameMaker != null)
			{
				request.Includes.Add(ideo.culture.festivalNameMaker);
			}
			List<string> list = new List<string>();
			string text = GrammarResolver.Resolve("r_ritualName", request, null, forceLog: false, null, null, list, capitalizeFirstSentence: false);
			usesDefiniteArticle = !list.Contains("noArticle");
			if (def.capitalizeAsTitle)
			{
				text = GenText.CapitalizeAsTitle(text);
			}
			return text;
		}

		public void AddObligation(RitualObligation obligation)
		{
			if ((!ideo.ObligationsActive && !def.allowOptionalRitualObligations) || !ModLister.CheckIdeology("Ritual obligations"))
			{
				return;
			}
			bool flag = def.notifyPlayerOnOpportunity && (float)Find.TickManager.TicksGame >= 60000f * def.skipOpportunityLettersBeforeDay;
			foreach (RitualRole item in behavior.def.RequiredRoles())
			{
				Precept_Role precept_Role = item.FindInstance(ideo);
				if (precept_Role == null || precept_Role.Active)
				{
					continue;
				}
				if (flag)
				{
					RitualObligationTrigger_Date ritualObligationTrigger_Date = obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault();
					if (ritualObligationTrigger_Date != null)
					{
						Find.LetterStack.ReceiveLetter("LetterObligationRoleInactive".Translate(base.LabelCap), "LetterObligationRoleInactiveDateDesc".Translate(ritualObligationTrigger_Date.DateString, base.LabelCap, ideo.memberName, precept_Role.Named("ROLE")), LetterDefOf.NeutralEvent);
					}
				}
				return;
			}
			if (activeObligations == null)
			{
				activeObligations = new List<RitualObligation>();
			}
			if (completedObligations == null)
			{
				completedObligations = new List<RitualObligation>();
			}
			if (obligation.onlyForPawns == null)
			{
				obligation.onlyForPawns = new List<Pawn>();
			}
			foreach (Pawn item2 in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if (item2.Ideo == ideo)
				{
					obligation.onlyForPawns.Add(item2);
				}
			}
			activeObligations.Add(obligation);
			if (obligation.sendLetter && flag && def.notifyPlayerOnOpportunity)
			{
				LookTargets lookTargets = LookTargets.Invalid;
				TargetInfo firstValidTarget = obligation.FirstValidTarget;
				Map map = Find.AnyPlayerHomeMap;
				if (firstValidTarget.IsValid)
				{
					map = firstValidTarget.Map;
				}
				if (map != null)
				{
					IEnumerable<TargetInfo> targets = obligationTargetFilter.GetTargets(obligation, map);
					lookTargets = ((!targets.Any()) ? ((LookTargets)firstValidTarget) : ((LookTargets)targets.First()));
				}
				Find.LetterStack.ReceiveLetter(obligation.LetterLabel, obligation.LetterText.Resolve(), LetterDefOf.NeutralEvent, lookTargets);
			}
		}

		public void RemoveObligation(RitualObligation obligation, bool completed = false)
		{
			activeObligations.Remove(obligation);
			if (completed)
			{
				completedObligations.Add(obligation);
			}
		}

		public bool ShouldShowGizmo(TargetInfo target)
		{
			if (activeObligations != null)
			{
				for (int i = 0; i < activeObligations.Count; i++)
				{
					if (obligationTargetFilter.CanUseTarget(target, activeObligations[i]).ShouldShowGizmo)
					{
						return true;
					}
				}
			}
			if (obligationTriggers.FirstOrDefault((RitualObligationTrigger o) => o is RitualObligationTrigger_Date) != null || isAnytime)
			{
				return obligationTargetFilter.CanUseTarget(target, null).ShouldShowGizmo;
			}
			return false;
		}

		public RitualTargetUseReport CanUseTarget(TargetInfo target, RitualObligation obligation)
		{
			return obligationTargetFilter.CanUseTarget(target, obligation);
		}

		public override void Tick()
		{
			base.Tick();
			if (!activeObligations.NullOrEmpty())
			{
				for (int num = activeObligations.Count - 1; num >= 0; num--)
				{
					if (!activeObligations[num].StillValid || !obligationTargetFilter.ObligationTargetsValid(activeObligations[num]))
					{
						RemoveObligation(activeObligations[num]);
					}
				}
			}
			for (int i = 0; i < obligationTriggers.Count; i++)
			{
				try
				{
					obligationTriggers[i].Tick();
				}
				catch (Exception ex)
				{
					Log.Error("Error while ticking a ritual obligation trigger: " + ex);
				}
			}
		}

		public override IEnumerable<FloatMenuOption> EditFloatMenuOptions()
		{
			yield return EditFloatMenuOption();
		}

		public override void Notify_MemberDied(Pawn p)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberDied(p);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberDied(p);
			}
		}

		public override void Notify_MemberCorpseDestroyed(Pawn p)
		{
			if (!ModsConfig.IdeologyActive || Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberCorpseDestroyed(p);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberCorpseDestroyed(p);
			}
		}

		public override void Notify_MemberChangedFaction(Pawn p, Faction oldFaction, Faction newFaction)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberChangedFaction(p, oldFaction, newFaction);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberChangedFaction(p, oldFaction, newFaction);
			}
		}

		public override void Notify_MemberLost(Pawn pawn)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberLost(pawn);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberLost(pawn);
			}
		}

		public override void Notify_MemberGained(Pawn pawn)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberGained(pawn);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberGained(pawn);
			}
		}

		public override void Notify_MemberGenerated(Pawn pawn, bool newborn, bool ignoreApparel = false)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			base.Notify_MemberGenerated(pawn, newborn, ignoreApparel);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberGenerated(pawn);
			}
		}

		public override void Notify_MemberGuestStatusChanged(Pawn pawn)
		{
			base.Notify_MemberGuestStatusChanged(pawn);
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_MemberGuestStatusChanged(pawn);
			}
		}

		public override void Notify_GameStarted()
		{
			base.Notify_GameStarted();
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_GameStarted();
			}
		}

		public override void Notify_IdeoReformed()
		{
			base.Notify_IdeoReformed();
			foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
			{
				obligationTrigger.Notify_IdeoReformed();
			}
		}

		public IEnumerable<Gizmo> GetGizmoFor(TargetInfo t)
		{
			if (!allowOtherInstances)
			{
				foreach (LordJob_Ritual activeRitual in Find.IdeoManager.GetActiveRituals(t.Map))
				{
					if (activeRitual.Ritual == this)
					{
						yield break;
					}
				}
			}
			if (!activeObligations.NullOrEmpty())
			{
				foreach (RitualObligation activeObligation in activeObligations)
				{
					Command_Ritual command_Ritual = CommandForObligation(activeObligation);
					if (command_Ritual != null)
					{
						yield return command_Ritual;
						if (mergeGizmosForObligations)
						{
							break;
						}
					}
				}
				yield break;
			}
			if (isAnytime)
			{
				RitualTargetUseReport ritualTargetUseReport = CanUseTarget(t, null);
				Command_Ritual command_Ritual2 = new Command_Ritual(this, t);
				if (!ritualTargetUseReport.failReason.NullOrEmpty())
				{
					command_Ritual2.disabledReason = ritualTargetUseReport.failReason;
					command_Ritual2.Disabled = true;
				}
				yield return command_Ritual2;
				yield break;
			}
			RitualObligationTrigger ritualObligationTrigger = obligationTriggers?.FirstOrDefault((RitualObligationTrigger o) => o is RitualObligationTrigger_Date);
			if (ritualObligationTrigger != null)
			{
				RitualObligationTrigger_Date ritualObligationTrigger_Date = (RitualObligationTrigger_Date)ritualObligationTrigger;
				int num = ritualObligationTrigger_Date.OccursOnTick();
				int num2 = ritualObligationTrigger_Date.CurrentTickRelative();
				if (num2 > num)
				{
					num += 3600000;
				}
				Command_Ritual command_Ritual3 = new Command_Ritual(this, t);
				command_Ritual3.disabledReason = "DateRitualNoObligation".Translate(base.LabelCap, (num - num2).ToStringTicksToPeriod(), ritualObligationTrigger_Date.DateString).Resolve();
				command_Ritual3.Disabled = true;
				yield return command_Ritual3;
			}
			Command_Ritual CommandForObligation(RitualObligation obligation)
			{
				RitualTargetUseReport ritualTargetUseReport2 = CanUseTarget(t, obligation);
				if (ritualTargetUseReport2.canUse)
				{
					return new Command_Ritual(this, t, obligation);
				}
				if (!ritualTargetUseReport2.failReason.NullOrEmpty())
				{
					return new Command_Ritual(this, t, obligation)
					{
						disabledReason = ritualTargetUseReport2.failReason,
						Disabled = true
					};
				}
				return null;
			}
		}

		public void ShowRitualBeginWindow(TargetInfo targetInfo, RitualObligation forObligation = null, Pawn selectedPawn = null, Dictionary<string, Pawn> forcedForRole = null)
		{
			RitualObligation ritualObligation = forObligation;
			if (ritualObligation == null)
			{
				ritualObligation = ((activeObligations != null) ? activeObligations.FirstOrDefault((RitualObligation o) => obligationTargetFilter.CanUseTarget(targetInfo, o).canUse) : null);
			}
			Window ritualBeginWindow = GetRitualBeginWindow(targetInfo, ritualObligation, null, null, forcedForRole, selectedPawn);
			if (ritualBeginWindow != null)
			{
				Find.WindowStack.Add(ritualBeginWindow);
			}
		}

		public virtual Window GetRitualBeginWindow(TargetInfo targetInfo, RitualObligation obligation = null, Action onConfirm = null, Pawn organizer = null, Dictionary<string, Pawn> forcedForRole = null, Pawn selectedPawn = null)
		{
			string text = behavior.CanStartRitualNow(targetInfo, this, selectedPawn, forcedForRole);
			if (!string.IsNullOrEmpty(text))
			{
				Messages.Message(text, targetInfo, MessageTypeDefOf.RejectInput, historical: false);
			}
			List<string> list = new List<string>();
			if (outcomeEffect != null)
			{
				if (!outcomeEffect.def.extraInfoLines.NullOrEmpty())
				{
					foreach (string extraInfoLine in outcomeEffect.def.extraInfoLines)
					{
						list.Add(extraInfoLine);
					}
				}
				if (!outcomeEffect.def.extraPredictedOutcomeDescriptions.NullOrEmpty())
				{
					foreach (string extraPredictedOutcomeDescription in outcomeEffect.def.extraPredictedOutcomeDescriptions)
					{
						list.Add(extraPredictedOutcomeDescription.Formatted(shortDescOverride ?? def.label));
					}
				}
				if (attachableOutcomeEffect != null)
				{
					list.Add(attachableOutcomeEffect.DescriptionForRitualValidated(this, targetInfo.Map));
				}
			}
			string ritualLabel = Label.CapitalizeFirst();
			TargetInfo target = targetInfo;
			Map map = targetInfo.Map;
			Dialog_BeginRitual.ActionCallback action = delegate(RitualRoleAssignments assignments)
			{
				behavior.TryExecuteOn(targetInfo, organizer, this, obligation, assignments, playerForced: true);
				onConfirm?.Invoke();
				return true;
			};
			Pawn organizer2 = organizer;
			RitualObligation obligation2 = obligation;
			Dialog_BeginRitual.PawnFilter filter = delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
			{
				if (pawn.GetLord() != null)
				{
					return false;
				}
				if (pawn.RaceProps.Animal && !behavior.def.roles.Any((RitualRole r) => r.AppliesToPawn(pawn, out var _, targetInfo, null, null, null, skipReason: true)))
				{
					return false;
				}
				if (pawn.IsSubhuman)
				{
					return false;
				}
				return !ritualOnlyForIdeoMembers || def.allowSpectatorsFromOtherIdeos || pawn.Ideo == ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || pawn.RaceProps.Animal || (!forcedForRole.NullOrEmpty() && forcedForRole.ContainsValue(pawn));
			};
			string okButtonText = "Begin".Translate();
			List<Pawn> requiredPawns = ((organizer != null) ? new List<Pawn> { organizer } : null);
			List<string> extraInfoText = list;
			return new Dialog_BeginRitual(ritualLabel, this, target, map, action, organizer2, obligation2, filter, okButtonText, requiredPawns, forcedForRole, null, extraInfoText, selectedPawn);
		}

		public void Notify_CooldownFromAbilityStarted(int cooldown)
		{
			abilityOnCooldownUntilTick = Find.TickManager.TicksGame + cooldown;
		}

		public override void DrawIcon(Rect rect)
		{
			GUI.color = ideo.Color;
			GUI.DrawTexture(rect, Icon);
			GUI.color = Color.white;
		}

		public override void DrawPreceptBox(Rect preceptBox, IdeoEditMode editMode, bool forceHighlight = false)
		{
			base.DrawPreceptBox(preceptBox, editMode, forceHighlight);
			Rect position = new Rect(preceptBox.xMax - (float)DateIconSize.x, preceptBox.yMin, DateIconSize.x, DateIconSize.z);
			if (!isAnytime)
			{
				if (obligationTriggers.OfType<RitualObligationTrigger_Date>().FirstOrDefault() != null)
				{
					GUI.DrawTexture(position, DateRitualTex);
				}
				else
				{
					GUI.DrawTexture(position, EventRitualTex);
				}
			}
			else
			{
				GUI.DrawTexture(position, AnytimeRitualTex);
			}
		}

		public override void Notify_RecachedPrecepts()
		{
			tipCached = null;
		}

		public override bool TryGetLostByReformingWarning(out string warning)
		{
			warning = null;
			IEnumerable<LordJob_Ritual> activeRitualsForPrecept = RitualUtility.GetActiveRitualsForPrecept(this);
			if (activeRitualsForPrecept.Any())
			{
				foreach (LordJob_Ritual item in activeRitualsForPrecept)
				{
					if (!warning.NullOrEmpty())
					{
						warning += "\n\n";
					}
					warning += "ReformIdeoCancelRituals".Translate(item.RitualLabel);
				}
				return true;
			}
			return false;
		}

		public override void Notify_RemovedByReforming()
		{
			tmpActiveRituals.Clear();
			tmpActiveRituals.AddRange(RitualUtility.GetActiveRitualsForPrecept(this));
			for (int i = 0; i < tmpActiveRituals.Count; i++)
			{
				tmpActiveRituals[i].Cancel();
			}
			tmpActiveRituals.Clear();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref nameMaker, "nameMaker");
			Scribe_Values.Look(ref ritualOnlyForIdeoMembers, "ritualOnlyForIdeoMembers", defaultValue: false);
			Scribe_Values.Look(ref ritualExpectedDesc, "ritualExpectedDesc");
			Scribe_Values.Look(ref ritualExpectedDescNoAdjective, "ritualExpectedDescNoAdjective");
			Scribe_Values.Look(ref shortDescOverride, "shortDescOverride");
			Scribe_Values.Look(ref iconPathOverride, "iconPathOverride");
			Scribe_Values.Look(ref patternGroupTag, "patternGroupTag");
			Scribe_Values.Look(ref minTechLevel, "minTechLevel", TechLevel.Undefined);
			Scribe_Values.Look(ref maxTechLevel, "maxTechLevel", TechLevel.Undefined);
			Scribe_Values.Look(ref isAnytime, "isAnytime", defaultValue: false);
			Scribe_Values.Look(ref canBeAnytime, "canBeAnytime", defaultValue: false);
			Scribe_Values.Look(ref allowOtherInstances, "allowOtherInstances", defaultValue: false);
			Scribe_Values.Look(ref playsIdeoMusic, "playsIdeoMusic", defaultValue: false);
			Scribe_Values.Look(ref ritualExplanation, "ritualExplanation");
			Scribe_Values.Look(ref mergeGizmosForObligations, "mergeGizmosForObligations", defaultValue: false);
			Scribe_Values.Look(ref canMergeGizmosFromDifferentIdeos, "canMergeGizmosFromDifferentIdeos", defaultValue: true);
			Scribe_Values.Look(ref generatedAttachedReward, "generatedAttachedReward", defaultValue: false);
			Scribe_Defs.Look(ref sourcePattern, "sourcePattern");
			Scribe_Values.Look(ref ignoreExtremeTemperatures, "ignoreExtremeTemperatures", defaultValue: false);
			Scribe_Deep.Look(ref obligationTargetFilter, "obligationTargetFilter");
			Scribe_Deep.Look(ref targetFilter, "targetFilter");
			Scribe_Deep.Look(ref behavior, "behavior");
			Scribe_Collections.Look(ref obligationTriggers, "triggers", LookMode.Deep);
			Scribe_Defs.Look(ref attachableOutcomeEffect, "attachableOutcomeEffect");
			Scribe_Deep.Look(ref outcomeEffect, "outcomeEffect");
			Scribe_Collections.Look(ref layerWhitelist, "layerWhitelist", LookMode.Undefined);
			Scribe_Collections.Look(ref layerBlacklist, "layerBlacklist", LookMode.Undefined);
			if (!GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
			{
				Scribe_Collections.Look(ref activeObligations, "activeObligations", LookMode.Deep);
				Scribe_Collections.Look(ref completedObligations, "completedObligations", LookMode.Deep);
				Scribe_Values.Look(ref lastFinishedTick, "lastFinishedTick", 0);
				Scribe_Values.Look(ref abilityOnCooldownUntilTick, "abilityOnCooldownUntilTick", -1);
			}
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (!activeObligations.NullOrEmpty())
			{
				if (activeObligations.RemoveAll((RitualObligation t) => t == null) != 0)
				{
					Log.Warning("Some activeObligations were null.");
				}
				foreach (RitualObligation activeObligation in activeObligations)
				{
					activeObligation.precept = this;
				}
			}
			if (!obligationTriggers.NullOrEmpty())
			{
				if (obligationTriggers.RemoveAll((RitualObligationTrigger t) => t == null) != 0)
				{
					Log.Warning("Some obligationTriggers were null.");
				}
				foreach (RitualObligationTrigger obligationTrigger in obligationTriggers)
				{
					obligationTrigger.ritual = this;
				}
			}
			if (obligationTargetFilter != null)
			{
				obligationTargetFilter.parent = this;
			}
			if (attachableOutcomeEffect == null && !generatedAttachedReward && SupportsAttachableOutcomeEffect)
			{
				attachableOutcomeEffect = DefDatabase<RitualAttachableOutcomeEffectDef>.AllDefs.Where((RitualAttachableOutcomeEffectDef d) => d.CanAttachToRitual(this)).RandomElementWithFallback();
				generatedAttachedReward = true;
			}
			cancelIconPathOverride = sourcePattern.cancelIconPathOverride;
		}

		public override void CopyTo(Precept other)
		{
			base.CopyTo(other);
			Precept_Ritual precept_Ritual = (Precept_Ritual)other;
			if (activeObligations != null)
			{
				precept_Ritual.activeObligations = new List<RitualObligation>();
				foreach (RitualObligation activeObligation in activeObligations)
				{
					RitualObligation ritualObligation = new RitualObligation();
					activeObligation.CopyTo(ritualObligation);
					precept_Ritual.activeObligations.Add(ritualObligation);
				}
			}
			precept_Ritual.nameMaker = nameMaker;
			precept_Ritual.obligationTriggers.Clear();
			for (int i = 0; i < obligationTriggers.Count; i++)
			{
				RitualObligationTrigger ritualObligationTrigger = (RitualObligationTrigger)Activator.CreateInstance(obligationTriggers[i].GetType());
				obligationTriggers[i].CopyTo(ritualObligationTrigger);
				ritualObligationTrigger.ritual = precept_Ritual;
				precept_Ritual.obligationTriggers.Add(ritualObligationTrigger);
			}
			if (obligationTargetFilter != null)
			{
				precept_Ritual.obligationTargetFilter = obligationTargetFilter;
				precept_Ritual.obligationTargetFilter.parent = precept_Ritual;
			}
			precept_Ritual.outcomeEffect = outcomeEffect;
			precept_Ritual.attachableOutcomeEffect = attachableOutcomeEffect;
			precept_Ritual.targetFilter = targetFilter;
			precept_Ritual.behavior = behavior;
			precept_Ritual.ritualOnlyForIdeoMembers = ritualOnlyForIdeoMembers;
			precept_Ritual.isAnytime = isAnytime;
			precept_Ritual.canBeAnytime = canBeAnytime;
			precept_Ritual.playsIdeoMusic = playsIdeoMusic;
			precept_Ritual.mergeGizmosForObligations = mergeGizmosForObligations;
			precept_Ritual.canMergeGizmosFromDifferentIdeos = canMergeGizmosFromDifferentIdeos;
			precept_Ritual.lastFinishedTick = lastFinishedTick;
			precept_Ritual.abilityOnCooldownUntilTick = abilityOnCooldownUntilTick;
			precept_Ritual.ritualExpectedDesc = ritualExpectedDesc;
			precept_Ritual.ritualExpectedDescNoAdjective = ritualExpectedDescNoAdjective;
			precept_Ritual.shortDescOverride = shortDescOverride;
			precept_Ritual.ritualExplanation = ritualExplanation;
			precept_Ritual.iconPathOverride = iconPathOverride;
			precept_Ritual.patternGroupTag = patternGroupTag;
			precept_Ritual.minTechLevel = minTechLevel;
			precept_Ritual.maxTechLevel = maxTechLevel;
			precept_Ritual.generatedAttachedReward = generatedAttachedReward;
			precept_Ritual.sourcePattern = sourcePattern;
		}
	}
}
