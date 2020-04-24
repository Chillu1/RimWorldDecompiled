using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
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

		public const int MainRectsY = 100;

		private const float MainRectsHeight = 355f;

		private const int ConfigRectTitlesHeight = 40;

		private const int FactionIconSize = 22;

		public static Vector2 BasePawnCardSize = new Vector2(480f, 455f);

		private const int MaxNameLength = 12;

		public const int MaxNickLength = 16;

		public const int MaxTitleLength = 25;

		public const int QuestLineHeight = 20;

		private static readonly Texture2D QuestIcon = ContentFinder<Texture2D>.Get("UI/Icons/Quest");

		public static readonly Color StackElementBackground = new Color(1f, 1f, 1f, 0.1f);

		private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

		private static readonly Color TitleCausedWorkTagDisableColor = new Color(0.67f, 0.84f, 0.9f);

		private static StringBuilder tmpInspectStrings = new StringBuilder();

		public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-.]*$");

		private const int QuestIconSize = 24;

		private const int QuestIconExtraPaddingLeft = -7;

		public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect))
		{
			bool creationMode = randomizeCallback != null;
			GUI.BeginGroup(creationMode ? creationRect : rect);
			Rect rect2 = new Rect(0f, 0f, 300f, 30f);
			NameTriple nameTriple = pawn.Name as NameTriple;
			if (creationMode && nameTriple != null)
			{
				Rect rect3 = new Rect(rect2);
				rect3.width *= 0.333f;
				Rect rect4 = new Rect(rect2);
				rect4.width *= 0.333f;
				rect4.x += rect4.width;
				Rect rect5 = new Rect(rect2);
				rect5.width *= 0.333f;
				rect5.x += rect4.width * 2f;
				string name = nameTriple.First;
				string name2 = nameTriple.Nick;
				string name3 = nameTriple.Last;
				DoNameInputRect(rect3, ref name, 12);
				if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				DoNameInputRect(rect4, ref name2, 16);
				GUI.color = Color.white;
				DoNameInputRect(rect5, ref name3, 12);
				if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
				{
					pawn.Name = new NameTriple(name, string.IsNullOrEmpty(name2) ? name : name2, name3);
				}
				TooltipHandler.TipRegionByKey(rect3, "FirstNameDesc");
				TooltipHandler.TipRegionByKey(rect4, "ShortIdentifierDesc");
				TooltipHandler.TipRegionByKey(rect5, "LastNameDesc");
			}
			else
			{
				rect2.width = 999f;
				Text.Font = GameFont.Medium;
				Widgets.Label(rect2, pawn.Name.ToStringFull);
				Text.Font = GameFont.Small;
			}
			if (randomizeCallback != null)
			{
				Rect rect6 = new Rect(creationRect.width - 24f - 100f, 0f, 100f, rect2.height);
				if (Widgets.ButtonText(rect6, "Randomize".Translate()))
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					randomizeCallback();
				}
				UIHighlighter.HighlightOpportunity(rect6, "RandomizePawn");
			}
			if (creationMode)
			{
				Widgets.InfoCardButton(creationRect.width - 24f, 0f, pawn);
			}
			else if (!pawn.health.Dead)
			{
				float num = PawnCardSize(pawn).x - 85f;
				if (pawn.IsFreeColonist && pawn.Spawned)
				{
					Rect rect7 = new Rect(num, 0f, 30f, 30f);
					if (Mouse.IsOver(rect7))
					{
						TooltipHandler.TipRegion(rect7, PawnBanishUtility.GetBanishButtonTip(pawn));
					}
					if (Widgets.ButtonImage(rect7, TexButton.Banish))
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
				if (pawn.IsColonist)
				{
					Rect rect8 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegionByKey(rect8, "RenameColonist");
					if (Widgets.ButtonImage(rect8, TexButton.Rename))
					{
						Find.WindowStack.Add(new Dialog_NamePawn(pawn));
					}
					num -= 40f;
				}
				if (pawn.IsFreeColonist && !pawn.IsQuestLodger() && pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
				{
					Rect rect9 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegionByKey(rect9, "RenounceTitle");
					if (Widgets.ButtonImage(rect9, TexButton.RenounceTitle))
					{
						FloatMenuUtility.MakeMenu(pawn.royalty.AllTitlesForReading, (RoyalTitle title) => "RenounceTitle".Translate() + ": " + "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()), (RoyalTitle title) => delegate
						{
							RoyalTitleUtility.FindLostAndGainedPermits(title.def, null, out List<RoyalTitlePermitDef> _, out List<RoyalTitlePermitDef> lostPermits);
							StringBuilder stringBuilder = new StringBuilder();
							if (lostPermits.Count > 0)
							{
								stringBuilder.AppendLine("RenounceTitleWillLoosePermits".Translate(pawn.Named("PAWN")) + ":");
								foreach (RoyalTitlePermitDef item2 in lostPermits)
								{
									stringBuilder.AppendLine("- " + item2.LabelCap + " (" + FirstTitleWithPermit(item2).GetLabelFor(pawn) + ")");
								}
								stringBuilder.AppendLine();
							}
							stringBuilder.AppendLine(title.faction.def.renounceTitleMessage);
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("RenounceTitleDescription".Translate(pawn.Named("PAWN"), "TitleOfFaction".Translate(title.def.GetLabelCapFor(pawn), title.faction.GetCallLabel()).Named("TITLE"), stringBuilder.ToString().TrimEndNewlines().Named("EFFECTS")), delegate
							{
								pawn.royalty.SetTitle(title.faction, null, grantRewards: false);
							}, destructive: true));
						});
					}
					num -= 40f;
				}
			}
			List<GenUI.AnonymousStackElement> stackElements = new List<GenUI.AnonymousStackElement>();
			Text.Font = GameFont.Small;
			string text = pawn.MainDesc(writeFaction: false);
			Vector2 vector = Text.CalcSize(text);
			Rect rect10 = new Rect(0f, 45f, vector.x + 5f, 24f);
			Widgets.Label(rect10, text);
			float height = Text.CalcHeight(text, rect10.width);
			Rect rect11 = new Rect(rect10.x, rect10.y, rect10.width, height);
			if (Mouse.IsOver(rect11))
			{
				TooltipHandler.TipRegion(rect11, () => pawn.ageTracker.AgeTooltipString, 6873641);
			}
			float num2 = 0f;
			if (pawn.Faction != null && !pawn.Faction.def.hidden)
			{
				float num3 = Text.CalcSize(pawn.Faction.Name).x + 22f + 15f;
				stackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Rect rect23 = new Rect(r.x, r.y, r.width, r.height);
						Color color6 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect23, BaseContent.WhiteTex);
						GUI.color = color6;
						Widgets.DrawHighlightIfMouseover(rect23);
						Rect rect24 = new Rect(r.x, r.y, r.width, r.height);
						Rect position4 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
						GUI.color = pawn.Faction.Color;
						GUI.DrawTexture(position4, pawn.Faction.def.FactionIcon);
						GUI.color = color6;
						Widgets.Label(new Rect(rect24.x + rect24.height + 5f, rect24.y, rect24.width - 10f, rect24.height), pawn.Faction.Name);
						if (Widgets.ButtonInvisible(rect23))
						{
							if (creationMode)
							{
								Find.WindowStack.Add(new Dialog_FactionDuringLanding());
							}
							else
							{
								Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
							}
						}
						if (Mouse.IsOver(rect23))
						{
							TipSignal tip6 = new TipSignal(() => "Faction".Translate() + "\n\n" + "FactionDesc".Translate(pawn.Named("PAWN")) + "\n\n" + "ClickToViewFactions".Translate(), pawn.Faction.loadID * 37);
							TooltipHandler.TipRegion(rect23, tip6);
						}
					},
					width = num3
				});
				num2 += num3;
			}
			bool flag = false;
			float num4 = rect.width - vector.x - 10f;
			tmpExtraFactions.Clear();
			QuestUtility.GetExtraFactionsFromQuestParts(pawn, tmpExtraFactions);
			foreach (ExtraFaction tmpExtraFaction in tmpExtraFactions)
			{
				ExtraFaction localExtraFaction = tmpExtraFaction;
				string factionName = localExtraFaction.faction.Name;
				bool drawExtraFactionIcon = localExtraFaction.factionType == ExtraFactionType.HomeFaction;
				float num5 = ElementWidth();
				if (flag || num2 + num5 >= num4)
				{
					factionName = "...";
					num5 = ElementWidth();
					flag = true;
				}
				stackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Rect rect20 = new Rect(r.x, r.y, r.width, r.height);
						Rect rect21 = drawExtraFactionIcon ? rect20 : r;
						Color color5 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(rect21, BaseContent.WhiteTex);
						GUI.color = color5;
						Widgets.DrawHighlightIfMouseover(rect21);
						if (drawExtraFactionIcon)
						{
							Rect rect22 = new Rect(r.x, r.y, r.width, r.height);
							Rect position3 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
							GUI.color = localExtraFaction.faction.Color;
							GUI.DrawTexture(position3, localExtraFaction.faction.def.FactionIcon);
							GUI.color = color5;
							Widgets.Label(new Rect(rect22.x + rect22.height + 5f, rect22.y, rect22.width - 10f, rect22.height), factionName);
						}
						else
						{
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), factionName);
						}
						if (Widgets.ButtonInvisible(rect20))
						{
							Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Factions);
						}
						if (Mouse.IsOver(rect21))
						{
							TipSignal tip5 = new TipSignal(() => localExtraFaction.factionType.GetLabel().CapitalizeFirst() + "\n\n" + "ExtraFactionDesc".Translate(pawn.Named("PAWN")) + "\n\n" + "ClickToViewFactions".Translate(), localExtraFaction.faction.loadID ^ 0x738AC053);
							TooltipHandler.TipRegion(rect21, tip5);
						}
					},
					width = num5
				});
				num2 += num5;
				float ElementWidth()
				{
					return Text.CalcSize(factionName).x + (float)(drawExtraFactionIcon ? 22 : 0) + 15f;
				}
			}
			GenUI.DrawElementStack(new Rect(vector.x + 10f, 45f, 999f, 24f), 22f, stackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false);
			stackElements.Clear();
			float curY = 72f;
			if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Count > 0)
			{
				foreach (RoyalTitle title2 in pawn.royalty.AllTitlesForReading)
				{
					RoyalTitle localTitle = title2;
					string titleLabel = localTitle.def.GetLabelCapFor(pawn) + " (" + pawn.royalty.GetFavor(localTitle.faction) + ")";
					stackElements.Add(new GenUI.AnonymousStackElement
					{
						drawer = delegate(Rect r)
						{
							Color color4 = GUI.color;
							Rect rect18 = new Rect(r.x, r.y, r.width + 22f, r.height);
							GUI.color = StackElementBackground;
							GUI.DrawTexture(rect18, BaseContent.WhiteTex);
							GUI.color = color4;
							int favor = pawn.royalty.GetFavor(localTitle.faction);
							if (Mouse.IsOver(rect18))
							{
								Widgets.DrawHighlight(rect18);
							}
							Rect rect19 = new Rect(r.x, r.y, r.width + 22f, r.height);
							Rect position2 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
							GUI.color = title2.faction.Color;
							GUI.DrawTexture(position2, localTitle.faction.def.FactionIcon);
							GUI.color = color4;
							Widgets.Label(new Rect(rect19.x + rect19.height + 5f, rect19.y, rect19.width - 10f, rect19.height), titleLabel);
							if (Widgets.ButtonInvisible(rect18))
							{
								Find.WindowStack.Add(new Dialog_InfoCard(localTitle.def, localTitle.faction));
							}
							if (Mouse.IsOver(rect18))
							{
								TipSignal tip4 = new TipSignal(() => GetTitleTipString(pawn, localTitle.faction, localTitle, favor), (int)curY * 37);
								TooltipHandler.TipRegion(rect18, tip4);
							}
						},
						width = Text.CalcSize(titleLabel).x + 15f
					});
				}
			}
			QuestUtility.AppendInspectStringsFromQuestParts(delegate(string str, Quest quest)
			{
				stackElements.Add(new GenUI.AnonymousStackElement
				{
					drawer = delegate(Rect r)
					{
						Color color3 = GUI.color;
						GUI.color = StackElementBackground;
						GUI.DrawTexture(r, BaseContent.WhiteTex);
						GUI.color = color3;
						DoQuestLine(r, str, quest);
					},
					width = GetQuestLineSize(str, quest).x
				});
			}, pawn, out int _);
			curY += GenUI.DrawElementStack(new Rect(0f, curY, rect.width - 5f, 50f), 22f, stackElements, delegate(Rect r, GenUI.AnonymousStackElement obj)
			{
				obj.drawer(r);
			}, (GenUI.AnonymousStackElement obj) => obj.width).height;
			if (stackElements.Any())
			{
				curY += 10f;
			}
			Rect leftRect = new Rect(0f, curY, 250f, 355f);
			Rect position = new Rect(leftRect.xMax, curY, 258f, 355f);
			GUI.BeginGroup(leftRect);
			curY = 0f;
			Pawn pawnLocal = pawn;
			List<Ability> abilities = (from a in pawn.abilities.abilities
				orderby a.def.level, a.def.EntropyGain
				select a).ToList();
			int numSections = abilities.Any() ? 5 : 4;
			float num6 = (float)Enum.GetValues(typeof(BackstorySlot)).Length * 22f;
			if (pawn.story != null && pawn.story.title != null)
			{
				num6 += 22f;
			}
			List<LeftRectSection> list = new List<LeftRectSection>();
			LeftRectSection item = new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num6),
				drawer = delegate(Rect sectionRect)
				{
					float num12 = sectionRect.y;
					Text.Font = GameFont.Small;
					foreach (BackstorySlot value6 in Enum.GetValues(typeof(BackstorySlot)))
					{
						Backstory backstory = pawn.story.GetBackstory(value6);
						if (backstory != null)
						{
							Rect rect14 = new Rect(sectionRect.x, num12, leftRect.width, 22f);
							if (Mouse.IsOver(rect14))
							{
								Widgets.DrawHighlight(rect14);
							}
							if (Mouse.IsOver(rect14))
							{
								TooltipHandler.TipRegion(rect14, backstory.FullDescriptionFor(pawn).Resolve());
							}
							Text.Anchor = TextAnchor.MiddleLeft;
							string str2 = (value6 == BackstorySlot.Adulthood) ? "Adulthood".Translate() : "Childhood".Translate();
							Widgets.Label(rect14, str2 + ":");
							Text.Anchor = TextAnchor.UpperLeft;
							Rect rect15 = new Rect(rect14);
							rect15.x += 90f;
							rect15.width -= 90f;
							string str3 = backstory.TitleCapFor(pawn.gender);
							Widgets.Label(rect15, str3.Truncate(rect15.width));
							num12 += rect14.height;
						}
					}
					if (pawn.story != null && pawn.story.title != null)
					{
						Rect rect16 = new Rect(sectionRect.x, num12, leftRect.width, 22f);
						Text.Anchor = TextAnchor.MiddleLeft;
						Widgets.Label(rect16, "Current".Translate() + ":");
						Text.Anchor = TextAnchor.UpperLeft;
						Rect rect17 = new Rect(rect16);
						rect17.x += 90f;
						rect17.width -= 90f;
						Widgets.Label(rect17, pawn.story.title);
						num12 += rect16.height;
					}
				}
			};
			list.Add(item);
			num6 = 30f;
			WorkTags disabledTags = pawn.CombinedDisabledWorkTags;
			List<WorkTags> disabledTagsList = WorkTagsFrom(disabledTags).ToList();
			bool allowWorkTagVerticalLayout = false;
			GenUI.StackElementWidthGetter<WorkTags> workTagWidthGetter = (WorkTags tag) => Text.CalcSize(tag.LabelTranslated().CapitalizeFirst()).x + 10f;
			if (disabledTags == WorkTags.None)
			{
				num6 += 22f;
			}
			else
			{
				disabledTagsList.Sort(delegate(WorkTags a, WorkTags b)
				{
					int num11 = GetWorkTypeDisableCauses(pawn, a).Any((object c) => c is RoyalTitleDef) ? 1 : (-1);
					int value5 = GetWorkTypeDisableCauses(pawn, b).Any((object c) => c is RoyalTitleDef) ? 1 : (-1);
					return num11.CompareTo(value5);
				});
				num6 += GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, disabledTagsList, delegate
				{
				}, workTagWidthGetter, 4f, 5f, allowOrderOptimization: false).height;
				num6 += 12f;
				allowWorkTagVerticalLayout = (GenUI.DrawElementStackVertical(new Rect(0f, 0f, rect.width, leftRect.height / (float)numSections), 22f, disabledTagsList, delegate
				{
				}, workTagWidthGetter).width <= leftRect.width);
			}
			item = new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num6),
				drawer = delegate(Rect sectionRect)
				{
					Text.Font = GameFont.Medium;
					float currentY3 = sectionRect.y;
					Widgets.Label(new Rect(sectionRect.x, currentY3, 200f, 30f), "IncapableOf".Translate(pawn));
					currentY3 += 30f;
					Text.Font = GameFont.Small;
					if (disabledTags == WorkTags.None)
					{
						GUI.color = Color.gray;
						Rect rect13 = new Rect(sectionRect.x, currentY3, leftRect.width, 24f);
						if (Mouse.IsOver(rect13))
						{
							Widgets.DrawHighlight(rect13);
						}
						Widgets.Label(rect13, "None".Translate());
						TooltipHandler.TipRegionByKey(rect13, "None");
					}
					else
					{
						GenUI.StackElementDrawer<WorkTags> drawer = delegate(Rect r, WorkTags tag)
						{
							Color color2 = GUI.color;
							GUI.color = StackElementBackground;
							GUI.DrawTexture(r, BaseContent.WhiteTex);
							GUI.color = color2;
							GUI.color = GetDisabledWorkTagLabelColor(pawn, tag);
							if (Mouse.IsOver(r))
							{
								Widgets.DrawHighlight(r);
							}
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), tag.LabelTranslated().CapitalizeFirst());
							if (Mouse.IsOver(r))
							{
								TooltipHandler.TipRegion(tip: new TipSignal(() => GetWorkTypeDisabledCausedBy(pawnLocal, tag) + "\n" + GetWorkTypesDisabledByWorkTag(tag), (int)currentY3 * 32), rect: r);
							}
						};
						if (allowWorkTagVerticalLayout)
						{
							GenUI.DrawElementStackVertical(new Rect(sectionRect.x, currentY3, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter);
						}
						else
						{
							GenUI.DrawElementStack(new Rect(sectionRect.x, currentY3, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, disabledTagsList, drawer, workTagWidthGetter, 5f);
						}
					}
					GUI.color = Color.white;
				}
			};
			list.Add(item);
			num6 = 30f;
			List<Trait> traits = pawn.story.traits.allTraits;
			num6 = ((traits != null && traits.Count != 0) ? (num6 + GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 22f, pawn.story.traits.allTraits, delegate
			{
			}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f).height) : (num6 + 22f));
			num6 += 12f;
			item = new LeftRectSection
			{
				rect = new Rect(0f, 0f, leftRect.width, num6),
				drawer = delegate(Rect sectionRect)
				{
					Text.Font = GameFont.Medium;
					float currentY2 = sectionRect.y;
					Widgets.Label(new Rect(sectionRect.x, currentY2, 200f, 30f), "Traits".Translate());
					currentY2 += 30f;
					Text.Font = GameFont.Small;
					if (traits == null || traits.Count == 0)
					{
						GUI.color = Color.gray;
						Rect rect12 = new Rect(sectionRect.x, currentY2, leftRect.width, 24f);
						if (Mouse.IsOver(rect12))
						{
							Widgets.DrawHighlight(rect12);
						}
						Widgets.Label(rect12, "None".Translate());
						currentY2 += rect12.height + 2f;
						TooltipHandler.TipRegionByKey(rect12, "None");
					}
					else
					{
						GenUI.DrawElementStack(new Rect(sectionRect.x, currentY2, leftRect.width - 5f, leftRect.height / (float)numSections), 22f, pawn.story.traits.allTraits, delegate(Rect r, Trait trait)
						{
							Color color = GUI.color;
							GUI.color = StackElementBackground;
							GUI.DrawTexture(r, BaseContent.WhiteTex);
							GUI.color = color;
							if (Mouse.IsOver(r))
							{
								Widgets.DrawHighlight(r);
							}
							Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
							if (Mouse.IsOver(r))
							{
								TooltipHandler.TipRegion(tip: new TipSignal(() => trait.TipString(pawn), (int)currentY2 * 37), rect: r);
							}
						}, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f);
					}
				}
			};
			list.Add(item);
			if (abilities.Any())
			{
				num6 = 30f;
				num6 += GenUI.DrawElementStack(new Rect(0f, 0f, leftRect.width - 5f, leftRect.height), 32f, abilities, delegate
				{
				}, (Ability abil) => 32f).height;
				item = new LeftRectSection
				{
					rect = new Rect(0f, 0f, leftRect.width, num6),
					drawer = delegate(Rect sectionRect)
					{
						Text.Font = GameFont.Medium;
						float currentY = sectionRect.y;
						Widgets.Label(new Rect(sectionRect.x, currentY, 200f, 30f), "Abilities".Translate(pawn));
						currentY += 30f;
						Text.Font = GameFont.Small;
						GenUI.DrawElementStack(new Rect(sectionRect.x, currentY, leftRect.width - 5f, leftRect.height), 32f, abilities, delegate(Rect r, Ability abil)
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
								TipSignal tip = new TipSignal(() => abil.def.GetTooltip() + "\n\n" + "ClickToLearnMore".Translate(), (int)currentY * 37);
								TooltipHandler.TipRegion(r, tip);
							}
						}, (Ability abil) => 32f);
						GUI.color = Color.white;
					}
				};
				list.Add(item);
			}
			float num7 = leftRect.height / (float)list.Count;
			float num8 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				LeftRectSection value = list[i];
				if (value.rect.height > num7)
				{
					num8 += value.rect.height - num7;
					value.calculatedSize = value.rect.height;
				}
				else
				{
					value.calculatedSize = num7;
				}
				list[i] = value;
			}
			bool flag2 = false;
			float num9 = 0f;
			if (num8 > 0f)
			{
				LeftRectSection value2 = list[0];
				float num10 = value2.rect.height + 12f;
				num8 -= value2.calculatedSize - num10;
				value2.calculatedSize = num10;
				list[0] = value2;
			}
			while (num8 > 0f)
			{
				bool flag3 = true;
				for (int j = 0; j < list.Count; j++)
				{
					LeftRectSection value3 = list[j];
					if (value3.calculatedSize - value3.rect.height > 0f)
					{
						value3.calculatedSize -= 1f;
						num8 -= 1f;
						flag3 = false;
					}
					list[j] = value3;
				}
				if (!flag3)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					LeftRectSection value4 = list[k];
					if (k > 0)
					{
						value4.calculatedSize = Mathf.Max(value4.rect.height, num7);
					}
					else
					{
						value4.calculatedSize = value4.rect.height + 22f;
					}
					num9 += value4.calculatedSize;
					list[k] = value4;
				}
				flag2 = true;
				break;
			}
			if (flag2)
			{
				Widgets.BeginScrollView(new Rect(0f, 0f, leftRect.width, leftRect.height), ref leftRectScrollPos, new Rect(0f, 0f, leftRect.width - 16f, num9));
			}
			curY = 0f;
			for (int l = 0; l < list.Count; l++)
			{
				LeftRectSection leftRectSection = list[l];
				leftRectSection.drawer(new Rect(0f, curY, leftRect.width, leftRectSection.rect.height));
				curY += leftRectSection.calculatedSize;
			}
			if (flag2)
			{
				Widgets.EndScrollView();
			}
			GUI.EndGroup();
			GUI.BeginGroup(position);
			SkillUI.DrawSkillsOf(mode: (Current.ProgramState != ProgramState.Playing) ? SkillUI.SkillDrawMode.Menu : SkillUI.SkillDrawMode.Gameplay, p: pawn, offset: new Vector2(0f, 0f));
			GUI.EndGroup();
			GUI.EndGroup();
			RoyalTitleDef FirstTitleWithPermit(RoyalTitlePermitDef permitDef)
			{
				return title.faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.First((RoyalTitleDef t) => t.permits != null && t.permits.Contains(permitDef));
			}
		}

		private static string GetTitleTipString(Pawn pawn, Faction faction, RoyalTitle title, int favor)
		{
			RoyalTitleDef def = title.def;
			TaggedString t = "RoyalTitleTooltipHasTitle".Translate(pawn.Named("PAWN"), faction.Named("FACTION"), def.GetLabelCapFor(pawn).Named("TITLE"));
			t += "\n\n" + faction.def.royalFavorLabel.CapitalizeFirst() + ": " + favor.ToString();
			RoyalTitleDef nextTitle = def.GetNextTitle(faction);
			if (nextTitle != null)
			{
				t += "\n" + "RoyalTitleTooltipNextTitle".Translate() + ": " + nextTitle.GetLabelCapFor(pawn) + " (" + "RoyalTitleTooltipNextTitleFavorCost".Translate(nextTitle.favorCost.ToString(), faction.Named("FACTION")) + ")";
			}
			else
			{
				t += "\n" + "RoyalTitleTooltipFinalTitle".Translate();
			}
			Pawn heir = pawn.royalty.GetHeir(faction);
			if (heir != null)
			{
				t += "\n\n" + "RoyalTitleTooltipInheritance".Translate(pawn.Named("PAWN"), heir.Named("HEIR"));
				if (heir.Faction == null)
				{
					t += " " + "RoyalTitleTooltipHeirNoFaction".Translate(heir.Named("HEIR"));
				}
				else if (heir.Faction != faction)
				{
					t += " " + "RoyalTitleTooltipHeirDifferentFaction".Translate(heir.Named("HEIR"), heir.Faction.Named("FACTION"));
				}
			}
			else
			{
				t += "\n\n" + "RoyalTitleTooltipNoHeir".Translate(pawn.Named("PAWN"));
			}
			t += "\n\n" + (title.conceited ? "RoyalTitleTooltipConceited" : "RoyalTitleTooltipNonConceited").Translate(pawn.Named("PAWN"));
			t += "\n\n" + RoyalTitleUtility.GetTitleProgressionInfo(faction, pawn);
			return (t + ("\n\n" + "ClickToLearnMore".Translate())).Resolve();
		}

		private static List<object> GetWorkTypeDisableCauses(Pawn pawn, WorkTags workTag)
		{
			List<object> list = new List<object>();
			if (pawn.story != null && pawn.story.childhood != null && (pawn.story.childhood.workDisables & workTag) != 0)
			{
				list.Add(pawn.story.childhood);
			}
			if (pawn.story != null && pawn.story.adulthood != null && (pawn.story.adulthood.workDisables & workTag) != 0)
			{
				list.Add(pawn.story.adulthood);
			}
			if (pawn.health != null && pawn.health.hediffSet != null)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					HediffStage curStage = hediff.CurStage;
					if (curStage != null && (curStage.disabledWorkTags & workTag) != 0)
					{
						list.Add(hediff);
					}
				}
			}
			if (pawn.story.traits != null)
			{
				for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
				{
					Trait trait = pawn.story.traits.allTraits[i];
					if ((trait.def.disabledWorkTags & workTag) != 0)
					{
						list.Add(trait);
					}
				}
			}
			if (pawn.royalty != null)
			{
				foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
				{
					if (item.conceited && (item.def.disabledWorkTags & workTag) != 0)
					{
						list.Add(item);
					}
				}
				return list;
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

		private static string GetWorkTypeDisabledCausedBy(Pawn pawn, WorkTags workTag)
		{
			List<object> workTypeDisableCauses = GetWorkTypeDisableCauses(pawn, workTag);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (object item in workTypeDisableCauses)
			{
				if (item is Backstory)
				{
					stringBuilder.AppendLine("IncapableOfTooltipBackstory".Translate((item as Backstory).TitleFor(pawn.gender)));
				}
				else if (item is Trait)
				{
					stringBuilder.AppendLine("IncapableOfTooltipTrait".Translate((item as Trait).LabelCap));
				}
				else if (item is Hediff)
				{
					stringBuilder.AppendLine("IncapableOfTooltipHediff".Translate((item as Hediff).LabelCap));
				}
				else if (item is RoyalTitle)
				{
					stringBuilder.AppendLine("IncapableOfTooltipTitle".Translate((item as RoyalTitle).def.GetLabelFor(pawn)));
				}
			}
			return stringBuilder.ToString();
		}

		private static string GetWorkTypesDisabledByWorkTag(WorkTags workTag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("IncapableOfTooltipWorkTypes".Translate());
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
			QuestUtility.AppendInspectStringsFromQuestParts(tmpInspectStrings, pawn, out int count);
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
				if (allSelectedItem != 0)
				{
					yield return allSelectedItem;
				}
			}
		}

		private static Vector2 GetQuestLineSize(string line, Quest quest)
		{
			Vector2 vector = Text.CalcSize(line);
			return new Vector2(17f + vector.x + 10f, Mathf.Max(24f, vector.y));
		}

		private static void DoQuestLine(Rect rect, string line, Quest quest)
		{
			Rect rect2 = rect;
			rect2.xMin += 22f;
			rect2.height = Text.CalcSize(line).y;
			float x = Text.CalcSize(line).x;
			Rect rect3 = new Rect(rect.x, rect.y, Mathf.Min(x, rect2.width) + 24f + -7f, rect.height);
			Widgets.DrawHighlightIfMouseover(rect3);
			TooltipHandler.TipRegionByKey(rect3, "ClickToViewInQuestsTab");
			GUI.DrawTexture(new Rect(rect.x + -7f, rect.y - 2f, 24f, 24f), QuestIcon);
			Widgets.Label(rect2, line.Truncate(rect2.width));
			if (Widgets.ButtonInvisible(rect3))
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
			}
		}
	}
}
