using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ITab_Pawn_Visitor : ITab
	{
		private const float CheckboxInterval = 30f;

		private const float CheckboxMargin = 50f;

		public ITab_Pawn_Visitor()
		{
			size = new Vector2(280f, 0f);
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
			bool isPrisonerOfColony = base.SelPawn.IsPrisonerOfColony;
			bool flag = base.SelPawn.IsWildMan();
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.maxOneColumn = true;
			listing_Standard.Begin(rect);
			Rect rect2 = listing_Standard.GetRect(28f);
			rect2.width = 140f;
			MedicalCareUtility.MedicalCareSetter(rect2, ref base.SelPawn.playerSettings.medCare);
			listing_Standard.Gap(4f);
			if (isPrisonerOfColony)
			{
				if (!flag)
				{
					Rect rect3 = listing_Standard.Label("RecruitmentDifficulty".Translate() + ": " + base.SelPawn.RecruitDifficulty(Faction.OfPlayer).ToStringPercent());
					if (base.SelPawn.royalty != null)
					{
						RoyalTitle title2 = base.SelPawn.royalty.MostSeniorTitle;
						if (title2 != null && Mouse.IsOver(rect3))
						{
							string valueString = title2.def.recruitmentDifficultyOffset.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Offset);
							TooltipHandler.TipRegion(rect3, () => "RecruitmentValueOffsetRoyal".Translate() + " (" + title2.def.GetLabelCapFor(base.SelPawn) + ")" + ": " + valueString, 947584751);
							Widgets.DrawHighlight(rect3);
						}
					}
					string value = RecruitUtility.RecruitChanceFactorForRecruitDifficulty(base.SelPawn, Faction.OfPlayer).ToStringPercent();
					string value2 = RecruitUtility.RecruitChanceFactorForMood(base.SelPawn).ToStringPercent();
					string text = base.SelPawn.RecruitChanceFinalByFaction(Faction.OfPlayer).ToStringPercent();
					Rect rect4 = listing_Standard.Label("RecruitmentChance".Translate() + ": " + text);
					if (Mouse.IsOver(rect4))
					{
						string recruitmentChanceTooltip = null;
						recruitmentChanceTooltip = "RecruitmentChanceExplanation".Translate(value, value2, text);
						if (!base.SelPawn.guest.lastRecruiterName.NullOrEmpty())
						{
							recruitmentChanceTooltip += "RecruitmentChanceWithLastRecruiterExplanationPart".Translate().Formatted(value, value2, text, base.SelPawn.guest.lastRecruiterName, base.SelPawn.guest.lastRecruiterNegotiationAbilityFactor.ToStringPercent(), base.SelPawn.guest.lastRecruiterOpinionChanceFactor.ToStringPercent(), base.SelPawn.guest.hasOpinionOfLastRecruiter ? base.SelPawn.guest.lastRecruiterOpinion.ToStringWithSign() : "-", base.SelPawn.guest.lastRecruiterFinalChance.ToStringPercent(), base.SelPawn.guest.lastRecruiterResistanceReduce.ToString("0.0"));
							if (base.SelPawn.guest.lastRecruiterResistanceReduce > 0f)
							{
								recruitmentChanceTooltip += "RecruitmentLastRecruiterResistanceReduceExplanationPart".Translate(base.SelPawn.guest.lastRecruiterResistanceReduce.ToString("0.0"));
							}
						}
						TooltipHandler.TipRegion(rect4, () => recruitmentChanceTooltip, 947584753);
					}
					Widgets.DrawHighlightIfMouseover(rect4);
					Rect rect5 = listing_Standard.Label("RecruitmentResistance".Translate() + ": " + base.SelPawn.guest.resistance.ToString("F1"));
					if (base.SelPawn.royalty != null)
					{
						RoyalTitle title = base.SelPawn.royalty.MostSeniorTitle;
						if (title != null && Mouse.IsOver(rect5))
						{
							TooltipHandler.TipRegion(rect5, delegate
							{
								StringBuilder stringBuilder = new StringBuilder();
								if (title.def.recruitmentResistanceOffset != 1f)
								{
									stringBuilder.AppendLine("RecruitmentValueFactorRoyal".Translate() + " (" + title.def.GetLabelCapFor(base.SelPawn) + ")" + ": " + title.def.recruitmentResistanceFactor.ToStringPercent());
								}
								if (title.def.recruitmentResistanceOffset != 0f)
								{
									string t2 = title.def.recruitmentDifficultyOffset.ToStringByStyle(ToStringStyle.FloatMaxOne, ToStringNumberSense.Offset);
									stringBuilder.AppendLine("RecruitmentValueOffsetRoyal".Translate() + " (" + title.def.GetLabelCapFor(base.SelPawn) + ")" + ": " + t2);
								}
								return stringBuilder.ToString().TrimEndNewlines();
							}, 947584755);
							Widgets.DrawHighlight(rect5);
						}
					}
				}
				listing_Standard.Label("SlavePrice".Translate() + ": " + base.SelPawn.GetStatValue(StatDefOf.MarketValue).ToStringMoney());
				TaggedString t;
				if (base.SelPawn.Faction == null || base.SelPawn.Faction.IsPlayer || !base.SelPawn.Faction.CanChangeGoodwillFor(Faction.OfPlayer, 1))
				{
					t = "None".Translate();
				}
				else
				{
					bool isHealthy;
					int goodwillGainForPrisonerRelease = base.SelPawn.Faction.GetGoodwillGainForPrisonerRelease(base.SelPawn, out isHealthy);
					t = ((!isHealthy) ? ("None".Translate() + " (" + "UntendedInjury".Translate().ToLower() + ")") : (base.SelPawn.Faction.NameColored + " " + goodwillGainForPrisonerRelease.ToStringWithSign()));
				}
				TooltipHandler.TipRegionByKey(listing_Standard.Label("PrisonerReleasePotentialRelationGains".Translate() + ": " + t), "PrisonerReleaseRelationGainsDesc");
				if (base.SelPawn.guilt.IsGuilty)
				{
					listing_Standard.Label("ConsideredGuilty".Translate(base.SelPawn.guilt.TicksUntilInnocent.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor)));
				}
				int num = (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(base.SelPawn);
				string text2 = "PrisonBreakMTBDays".Translate() + ": ";
				text2 = ((!base.SelPawn.Awake()) ? ((string)(text2 + "NotWhileAsleep".Translate())) : ((num >= 0) ? (text2 + "PeriodDays".Translate(num).ToString().Colorize(ColoredText.DateTimeColor)) : ((string)(text2 + "Never".Translate()))));
				TooltipHandler.TipRegionByKey(listing_Standard.Label(text2), "PrisonBreakMTBDaysDescription");
				Rect rect6 = listing_Standard.GetRect(160f).Rounded();
				Widgets.DrawMenuSection(rect6);
				Rect position = rect6.ContractedBy(10f);
				GUI.BeginGroup(position);
				Rect rect7 = new Rect(0f, 0f, position.width, 30f);
				foreach (PrisonerInteractionModeDef item in DefDatabase<PrisonerInteractionModeDef>.AllDefs.OrderBy((PrisonerInteractionModeDef pim) => pim.listOrder))
				{
					if (flag && !item.allowOnWildMan)
					{
						continue;
					}
					if (Widgets.RadioButtonLabeled(rect7, item.LabelCap, base.SelPawn.guest.interactionMode == item))
					{
						base.SelPawn.guest.interactionMode = item;
						if (item == PrisonerInteractionModeDefOf.Execution && base.SelPawn.MapHeld != null && !ColonyHasAnyWardenCapableOfViolence(base.SelPawn.MapHeld))
						{
							Messages.Message("MessageCantDoExecutionBecauseNoWardenCapableOfViolence".Translate(), base.SelPawn, MessageTypeDefOf.CautionInput, historical: false);
						}
					}
					rect7.y += 28f;
				}
				GUI.EndGroup();
			}
			listing_Standard.End();
			size = new Vector2(280f, listing_Standard.CurHeight + 10f + 24f);
		}

		private bool ColonyHasAnyWardenCapableOfViolence(Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && !item.WorkTagIsDisabled(WorkTags.Violent))
				{
					return true;
				}
			}
			return false;
		}
	}
}
