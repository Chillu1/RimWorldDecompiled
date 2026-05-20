using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class CharacterCardUtility
{
	private struct LeftRectSection
	{
		public Rect rect;

		public Action<Rect> drawer;

		public float calculatedSize;
	}

	private static Vector2 leftRectScrollPos = Vector2.zero;

	private static bool warnedChangingXenotypeWillRandomizePawn = false;

	private static Rect highlightRect;

	private const float NonArchiteBaselinerChance = 0.5f;

	public const int MainRectsY = 100;

	private const float MainRectsHeight = 355f;

	private const int ConfigRectTitlesHeight = 40;

	private const int FactionIconSize = 22;

	private const int IdeoIconSize = 22;

	private const int GenderIconSize = 22;

	private const float RowHeight = 22f;

	private const float LeftRectHeight = 250f;

	private const float RightRectHeight = 258f;

	public static Vector2 BasePawnCardSize = new Vector2(480f, 455f);

	private static readonly Color FavColorBoxColor = new Color(0.25f, 0.25f, 0.25f);

	public const int MaxNameLength = 12;

	public const int MaxNickLength = 16;

	public const int MaxTitleLength = 25;

	public const int QuestLineHeight = 20;

	public const float RandomizeButtonWidth = 200f;

	public const float HighlightMargin = 6f;

	private static readonly Texture2D QuestIcon = ContentFinder<Texture2D>.Get("UI/Icons/Quest");

	private static readonly Texture2D UnrecruitableIcon = ContentFinder<Texture2D>.Get("UI/Icons/UnwaveringlyLoyal");

	public static readonly Color StackElementBackground = new Color(1f, 1f, 1f, 0.1f);

	public static List<CustomXenotype> cachedCustomXenotypes;

	private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

	private static readonly Color TitleCausedWorkTagDisableColor = new Color(0.67f, 0.84f, 0.9f);

	private static List<GenUI.AnonymousStackElement> tmpStackElements = new List<GenUI.AnonymousStackElement>();

	private static float tmpMaxElementStackHeight = 0f;

	private static StringBuilder tmpInspectStrings = new StringBuilder();

	public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-.]*$");

	private const int QuestIconSize = 24;

	private static List<CustomXenotype> CustomXenotypes
	{
		get
		{
			if (cachedCustomXenotypes == null)
			{
				cachedCustomXenotypes = new List<CustomXenotype>();
				foreach (FileInfo item in GenFilePaths.AllCustomXenotypeFiles.OrderBy((FileInfo f) => f.LastWriteTime))
				{
					string filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
					PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
					{
						if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
						{
							cachedCustomXenotypes.Add(xenotype);
						}
					}, skipOnMismatch: true);
				}
			}
			return cachedCustomXenotypes;
		}
	}

	public static List<CustomXenotype> CustomXenotypesForReading => CustomXenotypes;

	public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect), bool showName = true)
	{
		bool flag = randomizeCallback != null;
		Rect rect2 = (flag ? creationRect : rect);
		Widgets.BeginGroup(rect2);
		Rect rect3 = new Rect(0f, 0f, 300f, showName ? 30 : 0);
		if (showName)
		{
			if (flag && pawn.Name is NameTriple nameTriple)
			{
				Rect rect4 = new Rect(rect3);
				rect4.width *= 0.333f;
				Rect rect5 = new Rect(rect3);
				rect5.width *= 0.333f;
				rect5.x += rect5.width;
				Rect rect6 = new Rect(rect3);
				rect6.width *= 0.333f;
				rect6.x += rect5.width * 2f;
				string name = nameTriple.First;
				string name2 = nameTriple.Nick;
				string name3 = nameTriple.Last;
				DoNameInputRect(rect4, ref name, 12);
				if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				DoNameInputRect(rect5, ref name2, 16);
				GUI.color = Color.white;
				DoNameInputRect(rect6, ref name3, 12);
				if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
				{
					pawn.Name = new NameTriple(name, string.IsNullOrEmpty(name2) ? name : name2, name3);
				}
				TooltipHandler.TipRegionByKey(rect4, "FirstNameDesc");
				TooltipHandler.TipRegionByKey(rect5, "ShortIdentifierDesc");
				TooltipHandler.TipRegionByKey(rect6, "LastNameDesc");
			}
			else
			{
				rect3.width = 999f;
				Text.Font = GameFont.Medium;
				string text = pawn.Name.ToStringFull.CapitalizeFirst();
				Widgets.Label(rect3, text);
				if (pawn.guilt != null && pawn.guilt.IsGuilty)
				{
					float x = Text.CalcSize(text).x;
					Rect rect7 = new Rect(x + 10f, 0f, 32f, 32f);
					GUI.DrawTexture(rect7, TexUI.GuiltyTex);
					TooltipHandler.TipRegion(rect7, () => pawn.guilt.Tip, 6321623);
				}
				Text.Font = GameFont.Small;
			}
		}
		bool allowsChildSelection = ScenarioUtility.AllowsChildSelection(Find.Scenario);
		if (ModsConfig.BiotechActive && flag)
		{
			Widgets.DrawHighlight(highlightRect.ExpandedBy(6f));
		}
		if (flag)
		{
			Rect rect8 = new Rect(creationRect.width - 200f - 6f, 6f, 200f, rect3.height);
			if (Widgets.ButtonText(rect8, "Randomize".Translate()))
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				randomizeCallback();
			}
			UIHighlighter.HighlightOpportunity(rect8, "RandomizePawn");
			if (ModsConfig.BiotechActive)
			{
				LifestageAndXenotypeOptions(pawn, rect8, flag, allowsChildSelection, randomizeCallback);
			}
		}
		if (flag)
		{
			Widgets.InfoCardButton(rect3.xMax + 4f, (rect3.height - 24f) / 2f, pawn);
		}
		else if (!pawn.health.Dead)
		{
			float num = PawnCardSize(pawn).x - 85f;
			if (pawn.IsFreeColonist && pawn.Spawned && !pawn.IsQuestLodger() && showName)
			{
				Rect rect9 = new Rect(num, 0f, 30f, 30f);
				if (Mouse.IsOver(rect9))
				{
					TooltipHandler.TipRegion(rect9, PawnBanishUtility.GetBanishButtonTip(pawn));
				}
				if (Widgets.ButtonImage(rect9, TexButton.Banish))
				{
					if (pawn.Downed)
					{
						Messages.Message("MessageCantBanishDownedPawn".Translate(pawn.LabelShort, pawn).AdjustedFor(pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn);
					}
				}
				num -= 40f;
			}
			if ((pawn.IsColonist || pawn.IsColonySubhuman || DebugSettings.ShowDevGizmos) && showName)
			{
				Rect rect10 = new Rect(num, 0f, 30f, 30f);
				TooltipHandler.TipRegionByKey(rect10, "RenameColonist");
				if (Widgets.ButtonImage(rect10, TexButton.Rename))
				{
					Find.WindowStack.Add(pawn.NamePawnDialog());
				}
				num -= 40f;
			}
			if (pawn.IsFreeColonist && !pawn.IsQuestLodger() && pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
			{
				Rect rect11 = new Rect(num, 0f, 30f, 30f);
				TooltipHandler.TipRegionByKey(rect11, "RenounceTitle");
				if (Widgets.ButtonImage(rect11, TexButton.RenounceTitle))
				{
					FloatMenuUtility.MakeMenu(pawn.royalty.AllTitlesForReading, (RoyalTitle title) => "RenounceTitle".Translate() + ": " + "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()), delegate(RoyalTitle title)
					{
						return delegate
						{
							List<FactionPermit> list = pawn.royalty.PermitsFromFaction(title.faction);
							RoyalTitleUtility.FindLostAndGainedPermits(title.def, null, out var _, out var lostPermits);
							StringBuilder stringBuilder = new StringBuilder();
							if (lostPermits.Count > 0 || list.Count > 0)
							{
								stringBuilder.AppendLine("RenounceTitleWillLoosePermits".Translate(pawn.Named("PAWN")) + ":");
								foreach (RoyalTitlePermitDef item in lostPermits)
								{
									stringBuilder.AppendLine("- " + item.LabelCap + " (" + FirstTitleWithPermit(item).GetLabelFor(pawn) + ")");
								}
								foreach (FactionPermit item2 in list)
								{
									stringBuilder.AppendLine("- " + item2.Permit.LabelCap + " (" + item2.Title.GetLabelFor(pawn) + ")");
								}
								stringBuilder.AppendLine();
							}
							int permitPoints = pawn.royalty.GetPermitPoints(title.faction);
							if (permitPoints > 0)
							{
								stringBuilder.AppendLineTagged("RenounceTitleWillLosePermitPoints".Translate(pawn.Named("PAWN"), permitPoints.Named("POINTS"), title.faction.Named("FACTION")));
							}
							if (pawn.abilities.abilities.Any())
							{
								stringBuilder.AppendLine();
								stringBuilder.AppendLineTagged("RenounceTitleWillKeepPsylinkLevels".Translate(pawn.Named("PAWN")));
							}
							if (!title.faction.def.renounceTitleMessage.NullOrEmpty())
							{
								stringBuilder.AppendLine();
								stringBuilder.AppendLine(title.faction.def.renounceTitleMessage);
							}
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("RenounceTitleDescription".Translate(pawn.Named("PAWN"), "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()).Named("TITLE"), stringBuilder.ToString().TrimEndNewlines().Named("EFFECTS")), delegate
							{
								pawn.royalty.SetTitle(title.faction, null, grantRewards: false);
								pawn.royalty.ResetPermitsAndPoints(title.faction, title.def);
							}, destructive: true));
						};
						RoyalTitleDef FirstTitleWithPermit(RoyalTitlePermitDef permitDef)
						{
							return title.faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First((RoyalTitleDef t) => t.permits != null && t.permits.Contains(permitDef));
						}
					});
				}
				num -= 40f;
			}
			if (pawn.guilt != null && pawn.guilt.IsGuilty && pawn.IsFreeColonist && !pawn.IsQuestLodger())
			{
				Rect rect12 = new Rect(num + 5f, 0f, 30f, 30f);
				TooltipHandler.TipRegionByKey(rect12, "ExecuteColonist");
				if (Widgets.ButtonImage(rect12, TexButton.ExecuteColonist))
				{
					pawn.guilt.awaitingExecution = !pawn.guilt.awaitingExecution;
					if (pawn.guilt.awaitingExecution)
					{
						Messages.Message("MessageColonistMarkedForExecution".Translate(pawn), pawn, MessageTypeDefOf.SilentInput, historical: false);
					}
				}
				if (pawn.guilt.awaitingExecution)
				{
					Rect position = default(Rect);
					position.x += rect12.x + 22f;
					position.width = 15f;
					position.height = 15f;
					GUI.DrawTexture(position, Widgets.CheckboxOnTex);
				}
			}
		}
		float num2 = rect3.height + 10f;
		float num3 = num2;
		num2 = DoTopStack(pawn, rect, flag, num2);
		if (num2 - num3 < 78f)
		{
			num2 += 15f;
		}
		Rect leftRect = new Rect(0f, num2, 250f, rect2.height - num2);
		DoLeftSection(rect, leftRect, pawn);
		Rect rect13 = new Rect(leftRect.xMax, num2, 258f, rect2.height - num2);
		Widgets.BeginGroup(rect13);
		SkillUI.DrawSkillsOf(mode: (Current.ProgramState != ProgramState.Playing) ? SkillUI.SkillDrawMode.Menu : SkillUI.SkillDrawMode.Gameplay, p: pawn, offset: Vector2.zero, container: rect13);
		Widgets.EndGroup();
		Widgets.EndGroup();
	}

	private static string GetTitleTipString(Pawn pawn, Faction faction, RoyalTitle title, int favor)
	{
		RoyalTitleDef def = title.def;
		TaggedString taggedString = "RoyalTitleTooltipHasTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), def.GetLabelCapFor(pawn).Named("TITLE"));
		taggedString += "\n\n" + faction.def.royalFavorLabel.CapitalizeFirst() + ": " + favor;
		RoyalTitleDef nextTitle = def.GetNextTitle(faction);
		if (nextTitle != null)
		{
			taggedString += "\n" + "RoyalTitleTooltipNextTitle".Translate() + ": " + nextTitle.GetLabelCapFor(pawn) + " (" + "RoyalTitleTooltipNextTitleFavorCost".Translate(nextTitle.favorCost.ToString(), faction.Named("FACTION")) + ")";
		}
		else
		{
			taggedString += "\n" + "RoyalTitleTooltipFinalTitle".Translate();
		}
		if (title.def.canBeInherited)
		{
			Pawn heir = pawn.royalty.GetHeir(faction);
			if (heir != null)
			{
				taggedString += "\n\n" + "RoyalTitleTooltipInheritance".Translate(pawn.Named("PAWN"), heir.Named("HEIR"));
				if (heir.Faction == null)
				{
					taggedString += " " + "RoyalTitleTooltipHeirNoFaction".Translate(heir.Named("HEIR"));
				}
				else if (heir.Faction != faction)
				{
					taggedString += " " + "RoyalTitleTooltipHeirDifferentFaction".Translate(heir.Named("HEIR"), heir.Faction.Named("FACTION"));
				}
			}
			else
			{
				taggedString += "\n\n" + "RoyalTitleTooltipNoHeir".Translate(pawn.Named("PAWN"));
			}
		}
		else
		{
			taggedString += "\n\n" + "LetterRoyalTitleCantBeInherited".Translate(title.def.Named("TITLE")).CapitalizeFirst() + " " + "LetterRoyalTitleNoHeir".Translate(pawn.Named("PAWN"));
		}
		taggedString += "\n\n" + (title.conceited ? "RoyalTitleTooltipConceited" : "RoyalTitleTooltipNonConceited").Translate(pawn.Named("PAWN"));
		taggedString += "\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(faction, pawn);
		return (taggedString + ("\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor))).Resolve();
	}

	private static List<object> GetWorkTypeDisableCauses(Pawn pawn, WorkTags workTag)
	{
		List<object> list = new List<object>();
		if (pawn.story != null && pawn.story.Childhood != null && (pawn.story.Childhood.workDisables & workTag) != WorkTags.None)
		{
			list.Add(pawn.story.Childhood);
		}
		if (pawn.story != null && pawn.story.Adulthood != null && (pawn.story.Adulthood.workDisables & workTag) != WorkTags.None)
		{
			list.Add(pawn.story.Adulthood);
		}
		if (pawn.health != null && pawn.health.hediffSet != null)
		{
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				HediffStage curStage = hediff.CurStage;
				if (curStage != null && (curStage.disabledWorkTags & workTag) != WorkTags.None)
				{
					list.Add(hediff);
				}
			}
		}
		if (pawn.story.traits != null)
		{
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				if (!pawn.story.traits.allTraits[i].Suppressed)
				{
					Trait trait = pawn.story.traits.allTraits[i];
					if ((trait.def.disabledWorkTags & workTag) != WorkTags.None)
					{
						list.Add(trait);
					}
				}
			}
		}
		if (pawn.royalty != null)
		{
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				if (item.conceited && (item.def.disabledWorkTags & workTag) != WorkTags.None)
				{
					list.Add(item);
				}
			}
		}
		if (ModsConfig.IdeologyActive && pawn.Ideo != null)
		{
			Precept_Role role = pawn.Ideo.GetRole(pawn);
			if (role != null && (role.def.roleDisabledWorkTags & workTag) != WorkTags.None)
			{
				list.Add(role);
			}
		}
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			foreach (Gene item2 in pawn.genes.GenesListForReading)
			{
				if (item2.Active && (item2.def.disabledWorkTags & workTag) != WorkTags.None)
				{
					list.Add(item2);
				}
			}
		}
		if (ModsConfig.AnomalyActive && pawn.IsMutant && pawn.mutant.IsPassive)
		{
			list.Add(pawn.mutant.Def);
		}
		foreach (QuestPart_WorkDisabled item3 in QuestUtility.GetWorkDisabledQuestPart(pawn))
		{
			if ((item3.disabledWorkTags & workTag) != WorkTags.None && !list.Contains(item3.quest))
			{
				list.Add(item3.quest);
			}
		}
		return list;
	}

	private static Color GetDisabledWorkTagLabelColor(Pawn pawn, WorkTags workTag)
	{
		foreach (object workTypeDisableCause in GetWorkTypeDisableCauses(pawn, workTag))
		{
			if (workTypeDisableCause is RoyalTitleDef)
			{
				return TitleCausedWorkTagDisableColor;
			}
		}
		return Color.white;
	}

	private static void LifestageAndXenotypeOptions(Pawn pawn, Rect randomizeRect, bool creationMode, bool allowsChildSelection, Action randomizeCallback)
	{
		highlightRect = randomizeRect;
		highlightRect.yMax += randomizeRect.height + Text.LineHeight + 8f;
		int startingPawnIndex = StartingPawnUtility.PawnIndex(pawn);
		float width = (randomizeRect.width - 4f) / 2f;
		float x = randomizeRect.x;
		Rect rect = new Rect(x, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
		x += rect.width + 4f;
		Text.Anchor = TextAnchor.MiddleCenter;
		Rect rect2 = rect;
		rect2.y += rect.height + 4f;
		rect2.height = Text.LineHeight;
		Widgets.Label(rect2, pawn.DevelopmentalStage.ToString().Translate().CapitalizeFirst());
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect3 = new Rect(rect.x, rect.y, rect.width, rect2.yMax - rect.yMin);
		if (Mouse.IsOver(rect3))
		{
			Widgets.DrawHighlight(rect3);
			if (Find.WindowStack.FloatMenu == null)
			{
				TaggedString taggedString = GetLabel().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "DevelopmentalAgeSelectionDesc".Translate();
				if (!allowsChildSelection)
				{
					taggedString += "\n\n" + "MessageDevelopmentalStageSelectionDisabledByScenario".Translate().Colorize(ColorLibrary.RedReadable);
				}
				TooltipHandler.TipRegion(rect3, taggedString.Resolve());
			}
		}
		DevelopmentalStage developmentalStage = DevelopmentalStage.Baby | DevelopmentalStage.Child | DevelopmentalStage.Adult;
		if (ModsConfig.AnomalyActive && pawn.IsMutant)
		{
			developmentalStage = pawn.mutant.Def.allowedDevelopmentalStages;
		}
		if (Widgets.ButtonImageWithBG(rect, GetDevelopmentalStageIcon(), new Vector2(22f, 22f)) && TutorSystem.AllowAction("ChangeDevelopmentStage"))
		{
			if (allowsChildSelection)
			{
				int index = startingPawnIndex;
				PawnGenerationRequest existing = StartingPawnUtility.GetGenerationRequest(index);
				List<FloatMenuOption> options = new List<FloatMenuOption>
				{
					new FloatMenuOption("Adult".Translate().CapitalizeFirst(), (!developmentalStage.Has(DevelopmentalStage.Adult)) ? null : ((Action)delegate
					{
						if (!existing.AllowedDevelopmentalStages.Has(DevelopmentalStage.Adult))
						{
							existing.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
							existing.AllowDowned = false;
							StartingPawnUtility.SetGenerationRequest(index, existing);
							randomizeCallback();
						}
					}), DevelopmentalStageExtensions.AdultTex.Texture, Color.white),
					new FloatMenuOption("Child".Translate().CapitalizeFirst(), (!developmentalStage.Has(DevelopmentalStage.Child)) ? null : ((Action)delegate
					{
						if (!existing.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child))
						{
							existing.AllowedDevelopmentalStages = DevelopmentalStage.Child;
							existing.AllowDowned = false;
							StartingPawnUtility.SetGenerationRequest(index, existing);
							randomizeCallback();
						}
					}), DevelopmentalStageExtensions.ChildTex.Texture, Color.white),
					new FloatMenuOption("Baby".Translate().CapitalizeFirst(), (!developmentalStage.Has(DevelopmentalStage.Baby)) ? null : ((Action)delegate
					{
						if (!existing.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
						{
							existing.AllowedDevelopmentalStages = DevelopmentalStage.Baby;
							existing.AllowDowned = true;
							StartingPawnUtility.SetGenerationRequest(index, existing);
							randomizeCallback();
						}
					}), DevelopmentalStageExtensions.BabyTex.Texture, Color.white)
				};
				Find.WindowStack.Add(new FloatMenu(options));
			}
			else
			{
				Messages.Message("MessageDevelopmentalStageSelectionDisabledByScenario".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		Rect rect4 = new Rect(x, randomizeRect.y + randomizeRect.height + 4f, width, randomizeRect.height);
		Text.Anchor = TextAnchor.MiddleCenter;
		Rect rect5 = rect4;
		rect5.y += rect4.height + 4f;
		rect5.height = Text.LineHeight;
		Widgets.Label(rect5, GetXenotypeLabel(startingPawnIndex).Truncate(rect5.width));
		Text.Anchor = TextAnchor.UpperLeft;
		Rect rect6 = new Rect(rect4.x, rect4.y, rect4.width, rect5.yMax - rect4.yMin);
		if (Mouse.IsOver(rect6))
		{
			Widgets.DrawHighlight(rect6);
			if (Find.WindowStack.FloatMenu == null)
			{
				TooltipHandler.TipRegion(rect6, GetXenotypeLabel(startingPawnIndex).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "XenotypeSelectionDesc".Translate());
			}
		}
		if (!Widgets.ButtonImageWithBG(rect4, GetXenotypeIcon(startingPawnIndex), new Vector2(22f, 22f)) || !TutorSystem.AllowAction("ChangeXenotype"))
		{
			return;
		}
		int index2 = startingPawnIndex;
		List<FloatMenuOption> list = new List<FloatMenuOption>
		{
			new FloatMenuOption("AnyNonArchite".Translate().CapitalizeFirst(), delegate
			{
				List<XenotypeDef> allowedXenotypes = DefDatabase<XenotypeDef>.AllDefs.Where((XenotypeDef xenotypeDef) => !xenotypeDef.Archite && xenotypeDef != XenotypeDefOf.Baseliner).ToList();
				SetupGenerationRequest(index2, null, null, allowedXenotypes, 0.5f, (PawnGenerationRequest pawnGenerationRequest) => pawnGenerationRequest.ForcedXenotype != null || pawnGenerationRequest.ForcedCustomXenotype != null, randomizeCallback, randomize: false);
			}),
			new FloatMenuOption("XenotypeEditor".Translate() + "...", delegate
			{
				Find.WindowStack.Add(new Dialog_CreateXenotype(index2, delegate
				{
					cachedCustomXenotypes = null;
					randomizeCallback();
				}));
			})
		};
		foreach (XenotypeDef item in DefDatabase<XenotypeDef>.AllDefs.OrderBy((XenotypeDef xenotypeDef) => 0f - xenotypeDef.displayPriority))
		{
			XenotypeDef xenotype = item;
			list.Add(new FloatMenuOption(xenotype.LabelCap, delegate
			{
				SetupGenerationRequest(index2, xenotype, null, null, 0f, (PawnGenerationRequest req) => XenotypeValidator(req, xenotype), randomizeCallback);
			}, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, delegate(Rect r)
			{
				TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description);
			}, null, 24f, (Rect r) => Widgets.InfoCardButton(r.x, r.y + 3f, xenotype) ? true : false, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
		}
		foreach (CustomXenotype customXenotype in CustomXenotypes)
		{
			CustomXenotype customInner = customXenotype;
			list.Add(new FloatMenuOption(customInner.name.CapitalizeFirst() + " (" + "Custom".Translate() + ")", delegate
			{
				SetupGenerationRequest(index2, null, customInner, null, 0f, (PawnGenerationRequest req) => CustomXenotypeValidator(req, customInner), randomizeCallback);
			}, customInner.IconDef.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, null, null, 24f, delegate(Rect r)
			{
				if (Widgets.ButtonImage(new Rect(r.x, r.y + (r.height - r.width) / 2f, r.width, r.width), TexButton.Delete, GUI.color))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(customInner.name.CapitalizeFirst()), delegate
					{
						string path = GenFilePaths.AbsFilePathForXenotype(customInner.name);
						if (File.Exists(path))
						{
							File.Delete(path);
							cachedCustomXenotypes = null;
						}
					}, destructive: true));
					return true;
				}
				return false;
			}, null, playSelectionSound: true, 0, HorizontalJustification.Left, extraPartRightJustified: true));
		}
		Find.WindowStack.Add(new FloatMenu(list));
		static bool CustomXenotypeValidator(PawnGenerationRequest req, CustomXenotype customXenotype)
		{
			if (TutorSystem.TutorialMode && req.MustBeCapableOfViolence && customXenotype.genes.Any((GeneDef g) => g.disabledWorkTags.HasFlag(WorkTags.Violent)))
			{
				Messages.Message("MessageStartingPawnCapableOfViolence".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			return req.ForcedCustomXenotype != customXenotype;
		}
		Texture2D GetDevelopmentalStageIcon()
		{
			return StartingPawnUtility.GetGenerationRequest(startingPawnIndex).AllowedDevelopmentalStages.Icon().Texture;
		}
		string GetLabel()
		{
			PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
			if (generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Baby))
			{
				return "Baby".Translate();
			}
			if (generationRequest.AllowedDevelopmentalStages.Has(DevelopmentalStage.Child))
			{
				return "Child".Translate();
			}
			return "Adult".Translate();
		}
		static bool XenotypeValidator(PawnGenerationRequest req, XenotypeDef xenotypeDef)
		{
			if (TutorSystem.TutorialMode && req.MustBeCapableOfViolence && xenotypeDef.AllGenes.Any((GeneDef g) => g.disabledWorkTags.HasFlag(WorkTags.Violent)))
			{
				Messages.Message("MessageStartingPawnCapableOfViolence".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			return req.ForcedXenotype != xenotypeDef;
		}
	}

	private static void SetupGenerationRequest(int index, XenotypeDef xenotype, CustomXenotype customXenotype, List<XenotypeDef> allowedXenotypes, float forceBaselinerChance, Func<PawnGenerationRequest, bool> validator, Action randomizeCallback, bool randomize = true)
	{
		PawnGenerationRequest existing = StartingPawnUtility.GetGenerationRequest(index);
		if (!validator(existing))
		{
			return;
		}
		if (!warnedChangingXenotypeWillRandomizePawn && randomize)
		{
			Find.WindowStack.Add(new Dialog_MessageBox("WarnChangingXenotypeWillRandomizePawn".Translate(), "Yes".Translate(), delegate
			{
				warnedChangingXenotypeWillRandomizePawn = true;
				existing.ForcedXenotype = xenotype;
				existing.ForcedCustomXenotype = customXenotype;
				existing.AllowedXenotypes = allowedXenotypes;
				existing.ForceBaselinerChance = forceBaselinerChance;
				StartingPawnUtility.SetGenerationRequest(index, existing);
				randomizeCallback();
			}, "No".Translate()));
		}
		else
		{
			existing.ForcedXenotype = xenotype;
			existing.ForcedCustomXenotype = customXenotype;
			existing.AllowedXenotypes = allowedXenotypes;
			existing.ForceBaselinerChance = forceBaselinerChance;
			StartingPawnUtility.SetGenerationRequest(index, existing);
			if (randomize)
			{
				randomizeCallback();
			}
		}
	}

	private static string GetXenotypeLabel(int startingPawnIndex)
	{
		PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
		if (generationRequest.ForcedCustomXenotype != null)
		{
			return generationRequest.ForcedCustomXenotype.name.CapitalizeFirst();
		}
		if (generationRequest.ForcedXenotype != null)
		{
			return generationRequest.ForcedXenotype.LabelCap;
		}
		return "AnyLower".Translate().CapitalizeFirst();
	}

	private static Texture2D GetXenotypeIcon(int startingPawnIndex)
	{
		PawnGenerationRequest generationRequest = StartingPawnUtility.GetGenerationRequest(startingPawnIndex);
		if (generationRequest.ForcedXenotype != null)
		{
			return generationRequest.ForcedXenotype.Icon;
		}
		if (generationRequest.ForcedCustomXenotype != null)
		{
			return generationRequest.ForcedCustomXenotype.IconDef.Icon;
		}
		return GeneUtility.UniqueXenotypeTex.Texture;
	}

	private static float DoTopStack(Pawn pawn, Rect rect, bool creationMode, float curY)
	{
		tmpStackElements.Clear();
		float num = rect.width - 10f;
		float width = (creationMode ? (num - 20f - Page_ConfigureStartingPawns.PawnPortraitSize.x) : num);
		Text.Font = GameFont.Small;
		bool flag = ModsConfig.BiotechActive && creationMode;
		string mainDesc = pawn.MainDesc(writeFaction: false, !flag);
		if (flag)
		{
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					GUI.DrawTexture(r, pawn.gender.GetIcon());
					if (Mouse.IsOver(r))
					{
						TooltipHandler.TipRegion(r, () => pawn.gender.GetLabel(pawn.AnimalOrWildMan()).CapitalizeFirst(), 7594764);
					}
				},
				width = 22f
			});
		}
		tmpStackElements.Add(new GenUI.AnonymousStackElement
		{
			drawer = delegate(Rect r)
			{
				Widgets.Label(r, mainDesc);
				if (Mouse.IsOver(r))
				{
					TooltipHandler.TipRegion(r, () => pawn.ageTracker.AgeTooltipString, 6873641);
				}
			},
			width = Text.CalcSize(mainDesc).x + 5f
		});
		if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.GenesListForReading.Any())
		{
			float num2 = 22f;
			num2 += Text.CalcSize(pawn.genes.XenotypeLabelCap).x + 14f;
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
					GUI.color = StackElementBackground;
					GUI.DrawTexture(rect2, BaseContent.WhiteTex);
					GUI.color = Color.white;
					if (Mouse.IsOver(rect2))
					{
						Widgets.DrawHighlight(rect2);
					}
					Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
					GUI.color = XenotypeDef.IconColor;
					GUI.DrawTexture(position, pawn.genes.XenotypeIcon);
					GUI.color = Color.white;
					Widgets.Label(new Rect(r.x + 22f + 5f, r.y, r.width + 22f - 1f, r.height), pawn.genes.XenotypeLabelCap);
					if (Mouse.IsOver(r))
					{
						TooltipHandler.TipRegion(r, () => ("Xenotype".Translate() + ": " + pawn.genes.XenotypeLabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + pawn.genes.XenotypeDescShort + "\n\n" + "ViewGenesDesc".Translate(pawn.Named("PAWN")).ToString().StripTags()
							.Colorize(ColoredText.SubtleGrayColor), 883938493);
					}
					if (Widgets.ButtonInvisible(r))
					{
						if (Current.ProgramState == ProgramState.Playing && Find.WindowStack.WindowOfType<Dialog_InfoCard>() == null && Find.WindowStack.WindowOfType<Dialog_GrowthMomentChoices>() == null)
						{
							InspectPaneUtility.OpenTab(typeof(ITab_Genes));
						}
						else
						{
							Find.WindowStack.Add(new Dialog_ViewGenes(pawn));
						}
					}
				},
				width = num2
			});
			curY += GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height + 4f;
			tmpStackElements.Clear();
		}
		if (pawn.Faction != null && !pawn.Faction.Hidden)
		{
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
					Color color = GUI.color;
					GUI.color = StackElementBackground;
					GUI.DrawTexture(rect2, BaseContent.WhiteTex);
					GUI.color = color;
					Widgets.DrawHighlightIfMouseover(rect2);
					Rect rect3 = new Rect(r.x, r.y, r.width, r.height);
					Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
					GUI.color = pawn.Faction.Color;
					GUI.DrawTexture(position, pawn.Faction.def.FactionIcon);
					GUI.color = color;
					Widgets.Label(new Rect(rect3.x + rect3.height + 5f, rect3.y, rect3.width - 10f, rect3.height), pawn.Faction.Name);
					if (Widgets.ButtonInvisible(rect2))
					{
						if (creationMode || Find.WindowStack.AnyWindowAbsorbingAllInput)
						{
							Find.WindowStack.Add(new Dialog_FactionDuringLanding());
						}
						else
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
						}
					}
					if (Mouse.IsOver(rect2))
					{
						string text = "Faction".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "FactionDesc".Translate(pawn.Named("PAWN")).Resolve() + "\n\n" + "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor);
						TipSignal tip = new TipSignal(text, pawn.Faction.loadID * 37);
						TooltipHandler.TipRegion(rect2, tip);
					}
				},
				width = Text.CalcSize(pawn.Faction.Name).x + 22f + 15f
			});
		}
		tmpExtraFactions.Clear();
		QuestUtility.GetExtraFactionsFromQuestParts(pawn, tmpExtraFactions);
		GuestUtility.GetExtraFactionsFromGuestStatus(pawn, tmpExtraFactions);
		foreach (ExtraFaction tmpExtraFaction in tmpExtraFactions)
		{
			if (pawn.Faction == tmpExtraFaction.faction)
			{
				continue;
			}
			ExtraFaction localExtraFaction = tmpExtraFaction;
			string factionName = localExtraFaction.faction.Name;
			bool drawExtraFactionIcon = localExtraFaction.factionType == ExtraFactionType.HomeFaction || localExtraFaction.factionType == ExtraFactionType.MiniFaction;
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
					Rect rect3 = (drawExtraFactionIcon ? rect2 : r);
					Color color = GUI.color;
					GUI.color = StackElementBackground;
					GUI.DrawTexture(rect3, BaseContent.WhiteTex);
					GUI.color = color;
					Widgets.DrawHighlightIfMouseover(rect3);
					if (drawExtraFactionIcon)
					{
						Rect rect4 = new Rect(r.x, r.y, r.width, r.height);
						Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = localExtraFaction.faction.Color;
						GUI.DrawTexture(position, localExtraFaction.faction.def.FactionIcon);
						GUI.color = color;
						Widgets.Label(new Rect(rect4.x + rect4.height + 5f, rect4.y, rect4.width - 10f, rect4.height), factionName);
					}
					else
					{
						Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), factionName);
					}
					if (Widgets.ButtonInvisible(rect2))
					{
						Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
					}
					if (Mouse.IsOver(rect3))
					{
						TipSignal tip = new TipSignal((localExtraFaction.factionType.GetLabel().CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "ExtraFactionDesc".Translate(pawn.Named("PAWN")) + "\n\n" + "ClickToViewFactions".Translate().Colorize(ColoredText.SubtleGrayColor)).Resolve(), localExtraFaction.faction.loadID ^ 0x738AC053);
						TooltipHandler.TipRegion(rect3, tip);
					}
				},
				width = Text.CalcSize(factionName).x + (float)(drawExtraFactionIcon ? 22 : 0) + 15f
			});
		}
		if (!Find.IdeoManager.classicMode && pawn.Ideo != null && ModsConfig.IdeologyActive)
		{
			float width2 = Text.CalcSize(pawn.Ideo.name).x + 22f + 15f;
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					GUI.color = StackElementBackground;
					GUI.DrawTexture(r, BaseContent.WhiteTex);
					GUI.color = Color.white;
					IdeoUIUtility.DrawIdeoPlate(r, pawn.Ideo, pawn);
				},
				width = width2
			});
		}
		if (ModsConfig.IdeologyActive)
		{
			Precept_Role role = pawn.Ideo?.GetRole(pawn);
			if (role != null)
			{
				string roleLabel = role.LabelForPawn(pawn);
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Color color = GUI.color;
						Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect2, BaseContent.WhiteTex);
						GUI.color = color;
						if (Mouse.IsOver(rect2))
						{
							Widgets.DrawHighlight(rect2);
						}
						Rect rect3 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
						Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = pawn.Ideo.Color;
						GUI.DrawTexture(position, role.Icon);
						GUI.color = Color.white;
						Widgets.Label(new Rect(rect3.x + 22f + 5f, rect3.y, rect3.width - 10f, rect3.height), roleLabel);
						if (Widgets.ButtonInvisible(rect2))
						{
							InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Social));
						}
						if (Mouse.IsOver(rect2))
						{
							TipSignal tip = new TipSignal(() => role.GetTip(), (int)curY * 39);
							TooltipHandler.TipRegion(rect2, tip);
						}
					},
					width = Text.CalcSize(roleLabel).x + 22f + 14f
				});
			}
		}
		int count;
		if (pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
		{
			foreach (RoyalTitle title in pawn.royalty.AllTitlesInEffectForReading)
			{
				RoyalTitle localTitle = title;
				string labelCapFor = localTitle.def.GetLabelCapFor(pawn);
				count = pawn.royalty.GetFavor(localTitle.faction);
				string titleLabel = labelCapFor + " (" + count + ")";
				tmpStackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Color color = GUI.color;
						Rect rect2 = new Rect(r.x, r.y, r.width, r.height);
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect2, BaseContent.WhiteTex);
						GUI.color = color;
						int favor = pawn.royalty.GetFavor(localTitle.faction);
						if (Mouse.IsOver(rect2))
						{
							Widgets.DrawHighlight(rect2);
						}
						Rect rect3 = new Rect(r.x, r.y, r.width + 22f + 9f, r.height);
						Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = title.faction.Color;
						GUI.DrawTexture(position, localTitle.faction.def.FactionIcon);
						GUI.color = color;
						Widgets.Label(new Rect(rect3.x + 22f + 5f, rect3.y, rect3.width - 10f, rect3.height), titleLabel);
						if (Widgets.ButtonInvisible(rect2))
						{
							Find.WindowStack.Add(new Dialog_InfoCard(localTitle.def, localTitle.faction, pawn));
						}
						if (Mouse.IsOver(rect2))
						{
							TipSignal tip = new TipSignal(() => GetTitleTipString(pawn, localTitle.faction, localTitle, favor), (int)curY * 37);
							TooltipHandler.TipRegion(rect2, tip);
						}
					},
					width = Text.CalcSize(titleLabel).x + 22f + 14f
				});
			}
		}
		if (ModsConfig.IdeologyActive && !pawn.DevelopmentalStage.Baby() && pawn.story?.favoriteColor != null)
		{
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					string orIdeoColor = string.Empty;
					if (pawn.Ideo != null && !pawn.Ideo.classicMode)
					{
						orIdeoColor = "OrIdeoColor".Translate(pawn.Named("PAWN"));
					}
					Widgets.DrawRectFast(r, pawn.story.favoriteColor.color);
					GUI.color = FavColorBoxColor;
					Widgets.DrawBox(r);
					GUI.color = Color.white;
					TooltipHandler.TipRegion(r, () => "FavoriteColorTooltip".Translate(pawn.Named("PAWN"), pawn.story.favoriteColor.label.Named("COLOR"), 0.6f.ToStringPercent().Named("PERCENTAGE"), orIdeoColor.Named("ORIDEO")).Resolve(), 837472764);
				},
				width = 22f
			});
		}
		if (pawn.guest != null && !pawn.guest.Recruitable)
		{
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Color color = GUI.color;
					GUI.color = StackElementBackground;
					GUI.DrawTexture(r, BaseContent.WhiteTex);
					GUI.color = color;
					GUI.DrawTexture(r, UnrecruitableIcon);
					if (Mouse.IsOver(r))
					{
						Widgets.DrawHighlight(r);
						TooltipHandler.TipRegion(r, () => "Unrecruitable".Translate().AsTipTitle().CapitalizeFirst() + "\n\n" + "UnrecruitableDesc".Translate(pawn.Named("PAWN")).Resolve(), 15877733);
					}
				},
				width = 22f
			});
		}
		bool drawMinimized = tmpMaxElementStackHeight > 44f;
		QuestUtility.AppendInspectStringsFromQuestParts(delegate(string str, Quest quest)
		{
			tmpStackElements.Add(new GenUI.AnonymousStackElement
			{
				drawer = delegate(Rect r)
				{
					Color color = GUI.color;
					GUI.color = StackElementBackground;
					GUI.DrawTexture(r, BaseContent.WhiteTex);
					GUI.color = color;
					DoQuestLine(r, str, quest, drawMinimized);
				},
				width = GetQuestLineSize(str, quest, drawMinimized).x
			});
		}, pawn, out count);
		float height = GenUI.DrawElementStack(new Rect(0f, curY, width, 50f), 22f, tmpStackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
		{
			obj.drawer(r);
		}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height;
		tmpMaxElementStackHeight = Mathf.Max(height, tmpMaxElementStackHeight);
		curY += height;
		if (tmpStackElements.Any())
		{
			curY += 10f;
		}
		return curY;
	}

	private static void DoLeftSection(Rect rect, Rect leftRect, Pawn pawn)
	{
		Widgets.BeginGroup(leftRect);
		float num = 0f;
		Pawn pawnLocal = pawn;
		List<Ability> abilities = (from a in pawn.abilities.AllAbilitiesForReading
			where a.def.showOnCharacterCard
			orderby a.def.level, a.def.EntropyGain
			select a).ToList();
		int numSections = (abilities.Any() ? 5 : 4);
		float num2 = (float)Enum.GetValues(typeof(BackstorySlot)).Length * 22f;
		float stackHeight = 0f;
		if (pawn.story != null && pawn.story.title != null)
		{
			num2 += 22f;
		}
		List<LeftRectSection> list = new List<LeftRectSection>();
		list.Add(new LeftRectSection
		{
			rect = new Rect(0f, 0f, leftRect.width, num2),
			drawer = delegate(Rect sectionRect)
			{
				float num11 = sectionRect.y;
				Text.Font = GameFont.Small;
				foreach (BackstorySlot value6 in Enum.GetValues(typeof(BackstorySlot)))
				{
					BackstoryDef backstory = pawn.story.GetBackstory(value6);
					if (backstory != null)
					{
						Rect rect5 = new Rect(sectionRect.x, num11, leftRect.width, 22f);
						Text.Anchor = TextAnchor.MiddleLeft;
						Widgets.Label(rect5, (value6 == BackstorySlot.Adulthood) ? "Adulthood".Translate() : "Childhood".Translate());
						Text.Anchor = TextAnchor.UpperLeft;
						string text = backstory.TitleCapFor(pawn.gender);
						Rect rect6 = new Rect(rect5);
						rect6.x += 90f;
						rect6.width = Text.CalcSize(text).x + 10f;
						Color color = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect6, BaseContent.WhiteTex);
						GUI.color = color;
						Text.Anchor = TextAnchor.MiddleCenter;
						Widgets.Label(rect6, text.Truncate(rect6.width));
						Text.Anchor = TextAnchor.UpperLeft;
						if (Mouse.IsOver(rect6))
						{
							Widgets.DrawHighlight(rect6);
						}
						if (Mouse.IsOver(rect6))
						{
							TooltipHandler.TipRegion(rect6, backstory.FullDescriptionFor(pawn).Resolve());
						}
						num11 += rect5.height + 4f;
					}
				}
				if (pawn.story != null && pawn.story.title != null)
				{
					Rect rect7 = new Rect(sectionRect.x, num11, leftRect.width, 22f);
					Text.Anchor = TextAnchor.MiddleLeft;
					Widgets.Label(rect7, "BackstoryTitle".Translate() + ":");
					Text.Anchor = TextAnchor.UpperLeft;
					Rect rect8 = new Rect(rect7);
					rect8.x += 90f;
					rect8.width -= 90f;
					Widgets.Label(rect8, pawn.story.title);
					num11 += rect7.height;
				}
			}
		});
		num2 = 30f;
		List<Trait> traits = pawn.story.traits.allTraits;
		if (traits == null || traits.Count == 0)
		{
			num2 += 22f;
			stackHeight = 22f;
		}
		else
		{
			Rect rect2 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, pawn.story.traits.TraitsSorted, delegate
			{
			}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f, 4f, 5f, allowOrderOptimization: false);
			num2 += rect2.height;
			stackHeight = rect2.height;
		}
		list.Add(new LeftRectSection
		{
			rect = new Rect(0f, 0f, leftRect.width, num2),
			drawer = delegate(Rect sectionRect)
			{
				float currentY = sectionRect.y;
				Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 30f), "Traits".Translate().AsTipTitle());
				currentY += 24f;
				if (traits == null || traits.Count == 0)
				{
					Color color = GUI.color;
					GUI.color = Color.gray;
					Rect rect5 = new Rect(sectionRect.x, currentY, leftRect.width, 24f);
					if (Mouse.IsOver(rect5))
					{
						Widgets.DrawHighlight(rect5);
					}
					Widgets.Label(rect5, pawn.DevelopmentalStage.Baby() ? "TraitsDevelopLaterBaby".Translate() : "None".Translate());
					currentY += rect5.height + 2f;
					TooltipHandler.TipRegionByKey(rect5, "None");
					GUI.color = color;
				}
				else
				{
					GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, stackHeight), 22f, pawn.story.traits.TraitsSorted, delegate(Rect r, Trait trait)
					{
						Color color2 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = color2;
						if (Mouse.IsOver(r))
						{
							Widgets.DrawHighlight(r);
						}
						if (trait.Suppressed)
						{
							GUI.color = ColoredText.SubtleGrayColor;
						}
						else if (trait.sourceGene != null)
						{
							GUI.color = ColoredText.GeneColor;
						}
						Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
						GUI.color = Color.white;
						if (Mouse.IsOver(r))
						{
							Trait trLocal = trait;
							TooltipHandler.TipRegion(tip: new TipSignal(() => trLocal.TipString(pawn), (int)currentY * 37), rect: r);
						}
					}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f, 4f, 5f, allowOrderOptimization: false);
				}
			}
		});
		num2 = 30f;
		WorkTags disabledTags = pawn.CombinedDisabledWorkTags;
		List<WorkTags> disabledTagsList = WorkTagsFrom(disabledTags).ToList();
		bool allowWorkTagVerticalLayout = false;
		GenUI.StackElementWidthGetter<WorkTags> workTagWidthGetter = (WorkTags tag) => Text.CalcSize(tag.LabelTranslated().CapitalizeFirst()).x + 10f;
		if (disabledTags == WorkTags.None)
		{
			num2 += 22f;
		}
		else
		{
			disabledTagsList.Sort(delegate(WorkTags a, WorkTags b)
			{
				int num11 = (GetWorkTypeDisableCauses(pawn, a).Any((object c) => c is RoyalTitleDef) ? 1 : (-1));
				int value5 = (GetWorkTypeDisableCauses(pawn, b).Any((object c) => c is RoyalTitleDef) ? 1 : (-1));
				return num11.CompareTo(value5);
			});
			Rect rect3 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, disabledTagsList, delegate
			{
			}, workTagWidthGetter, 4f, 5f, allowOrderOptimization: false);
			num2 += rect3.height;
			stackHeight = rect3.height;
			num2 += 12f;
			allowWorkTagVerticalLayout = GenUI.DrawElementStackVertical(new Rect(0f, 0f, rect.width, stackHeight), 22f, disabledTagsList, delegate
			{
			}, workTagWidthGetter).width <= leftRect.width;
		}
		list.Add(new LeftRectSection
		{
			rect = new Rect(0f, 0f, leftRect.width, num2),
			drawer = delegate(Rect sectionRect)
			{
				float currentY = sectionRect.y;
				Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 24f), "IncapableOf".Translate(pawn).AsTipTitle());
				currentY += 24f;
				if (disabledTags == WorkTags.None)
				{
					GUI.color = Color.gray;
					Rect rect5 = new Rect(sectionRect.x, currentY, leftRect.width, 24f);
					if (Mouse.IsOver(rect5))
					{
						Widgets.DrawHighlight(rect5);
					}
					Widgets.Label(rect5, "None".Translate());
					TooltipHandler.TipRegionByKey(rect5, "None");
				}
				else
				{
					GenUI.StackElementDrawer<WorkTags> drawer = delegate(Rect r, WorkTags tag)
					{
						Color color = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = color;
						GUI.color = GetDisabledWorkTagLabelColor(pawn, tag);
						if (Mouse.IsOver(r))
						{
							Widgets.DrawHighlight(r);
						}
						Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), tag.LabelTranslated().CapitalizeFirst());
						if (Mouse.IsOver(r))
						{
							WorkTags tagLocal = tag;
							TooltipHandler.TipRegion(tip: new TipSignal(() => GetWorkTypeDisabledCausedBy(pawnLocal, tagLocal) + "\n" + GetWorkTypesDisabledByWorkTag(tagLocal), (int)currentY * 32), rect: r);
						}
					};
					if (allowWorkTagVerticalLayout)
					{
						GenUI.DrawElementStackVertical(new Rect(sectionRect.x, currentY, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter);
					}
					else
					{
						GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter, 5f);
					}
				}
				GUI.color = Color.white;
			}
		});
		if (abilities.Any())
		{
			num2 = 30f;
			Rect rect4 = GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 32f, abilities, delegate
			{
			}, (Ability abil) => 32f);
			num2 += rect4.height;
			stackHeight = rect4.height;
			list.Add(new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num2),
				drawer = delegate(Rect sectionRect)
				{
					float currentY = sectionRect.y;
					Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 24f), "Abilities".Translate(pawn).AsTipTitle());
					currentY += 24f;
					GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, stackHeight), 32f, abilities, delegate(Rect r, Ability abil)
					{
						GUI.DrawTexture(r, BaseContent.ClearTex);
						if (Mouse.IsOver(r))
						{
							Widgets.DrawHighlight(r);
						}
						if (Widgets.ButtonImage(r, abil.def.uiIcon, doMouseoverSound: false))
						{
							Find.WindowStack.Add(new Dialog_InfoCard(abil.def));
						}
						if (Mouse.IsOver(r))
						{
							Ability abilCapture = abil;
							TipSignal tip = new TipSignal(() => abilCapture.Tooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor), (int)currentY * 37);
							TooltipHandler.TipRegion(r, tip);
						}
					}, (Ability abil) => 32f);
					GUI.color = Color.white;
				}
			});
		}
		else
		{
			num2 += 12f;
		}
		float num3 = leftRect.height / (float)list.Count;
		float num4 = 0f;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			LeftRectSection value = list[num5];
			if (value.rect.height > num3)
			{
				num4 += value.rect.height - num3;
				value.calculatedSize = value.rect.height;
			}
			else
			{
				value.calculatedSize = num3;
			}
			list[num5] = value;
		}
		bool flag = false;
		float num6 = 0f;
		if (num4 > 0f)
		{
			LeftRectSection value2 = list[0];
			float num7 = value2.rect.height + 12f;
			num4 -= value2.calculatedSize - num7;
			value2.calculatedSize = num7;
			list[0] = value2;
		}
		while (num4 > 0f)
		{
			bool flag2 = true;
			for (int num8 = 0; num8 < list.Count; num8++)
			{
				LeftRectSection value3 = list[num8];
				if (value3.calculatedSize - value3.rect.height > 0f)
				{
					value3.calculatedSize -= 1f;
					num4 -= 1f;
					flag2 = false;
				}
				list[num8] = value3;
			}
			if (!flag2)
			{
				continue;
			}
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				LeftRectSection value4 = list[num9];
				if (num9 > 0)
				{
					value4.calculatedSize = Mathf.Max(value4.rect.height, num3);
				}
				else
				{
					value4.calculatedSize = value4.rect.height + 22f;
				}
				num6 += value4.calculatedSize;
				list[num9] = value4;
			}
			flag = true;
			break;
		}
		if (flag)
		{
			Widgets.BeginScrollView(new Rect(0f, 0f, leftRect.width, leftRect.height), ref leftRectScrollPos, new Rect(0f, 0f, leftRect.width - 16f, num6));
		}
		num = 0f;
		for (int num10 = 0; num10 < list.Count; num10++)
		{
			LeftRectSection leftRectSection = list[num10];
			leftRectSection.drawer(new Rect(0f, num, leftRect.width - 5f, leftRectSection.rect.height));
			num += leftRectSection.calculatedSize;
		}
		if (flag)
		{
			Widgets.EndScrollView();
		}
		Widgets.EndGroup();
	}

	private static string GetWorkTypeDisabledCausedBy(Pawn pawn, WorkTags workTag)
	{
		List<object> workTypeDisableCauses = GetWorkTypeDisableCauses(pawn, workTag);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (object item in workTypeDisableCauses)
		{
			if (item is BackstoryDef backstoryDef)
			{
				stringBuilder.AppendLine("IncapableOfTooltipBackstory".Translate() + ": " + backstoryDef.TitleFor(pawn.gender).CapitalizeFirst());
			}
			else if (item is Trait trait)
			{
				stringBuilder.AppendLine("IncapableOfTooltipTrait".Translate() + ": " + trait.LabelCap);
			}
			else if (item is Hediff hediff)
			{
				stringBuilder.AppendLine("IncapableOfTooltipHediff".Translate() + ": " + hediff.LabelCap);
			}
			else if (item is RoyalTitle royalTitle)
			{
				stringBuilder.AppendLine("IncapableOfTooltipTitle".Translate() + ": " + royalTitle.def.GetLabelFor(pawn));
			}
			else if (item is Quest quest)
			{
				stringBuilder.AppendLine("IncapableOfTooltipQuest".Translate() + ": " + quest.name);
			}
			else if (item is Precept_Role precept_Role)
			{
				stringBuilder.AppendLine("IncapableOfTooltipRole".Translate() + ": " + precept_Role.LabelForPawn(pawn));
			}
			else if (item is Gene gene)
			{
				stringBuilder.AppendLine("IncapableOfTooltipGene".Translate() + ": " + gene.LabelCap);
			}
			else if (item is MutantDef mutantDef)
			{
				stringBuilder.AppendLine("IncapableOfTooltipMutant".Translate() + ": " + mutantDef.LabelCap);
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetWorkTypesDisabledByWorkTag(WorkTags workTag)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("IncapableOfTooltipWorkTypes".Translate().Colorize(ColoredText.TipSectionTitleColor));
		foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
		{
			if ((allDef.workTags & workTag) > WorkTags.None)
			{
				stringBuilder.Append("- ");
				stringBuilder.AppendLine(allDef.pawnLabel);
			}
		}
		return stringBuilder.ToString();
	}

	public static Vector2 PawnCardSize(Pawn pawn)
	{
		Vector2 basePawnCardSize = BasePawnCardSize;
		tmpInspectStrings.Length = 0;
		QuestUtility.AppendInspectStringsFromQuestParts(tmpInspectStrings, pawn, out var count);
		if (count >= 2)
		{
			basePawnCardSize.y += (count - 1) * 20;
		}
		return basePawnCardSize;
	}

	public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
	{
		string text = Widgets.TextField(rect, name);
		if (text.Length <= maxLength && ValidNameRegex.IsMatch(text))
		{
			name = text;
		}
	}

	private static IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
	{
		foreach (WorkTags allSelectedItem in tags.GetAllSelectedItems<WorkTags>())
		{
			if (allSelectedItem != WorkTags.None)
			{
				yield return allSelectedItem;
			}
		}
	}

	private static Vector2 GetQuestLineSize(string line, Quest quest, bool drawMinimized)
	{
		if (drawMinimized)
		{
			return new Vector2(24f, 24f);
		}
		Vector2 vector = Text.CalcSize(line);
		return new Vector2(24f + vector.x + 10f, Mathf.Max(24f, vector.y));
	}

	private static void DoQuestLine(Rect rect, string line, Quest quest, bool drawMinimized)
	{
		Rect rect2 = rect;
		rect2.xMin += 29f;
		rect2.height = Text.CalcSize(line).y;
		float x = Text.CalcSize(line).x;
		Rect rect3 = new Rect(rect.x, rect.y, Mathf.Min(x, rect2.width) + 24f + 5f, rect.height);
		if (!quest.hidden)
		{
			Widgets.DrawHighlightIfMouseover(rect3);
			if (drawMinimized)
			{
				TooltipHandler.TipRegion(rect3, line + "\n\n" + "ClickToViewInQuestsTab".Translate());
			}
			else
			{
				TooltipHandler.TipRegionByKey(rect3, "ClickToViewInQuestsTab");
			}
		}
		GUI.DrawTexture(new Rect(rect.x, rect.y - 1f, 24f, 24f), QuestIcon);
		if (!drawMinimized)
		{
			Widgets.Label(rect2, line.Truncate(rect2.width));
		}
		if (!quest.hidden && Widgets.ButtonInvisible(rect3))
		{
			Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
			((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
		}
	}
}
