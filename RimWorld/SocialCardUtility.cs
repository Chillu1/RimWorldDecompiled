using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class SocialCardUtility
{
	private class CachedSocialTabEntry
	{
		public Pawn otherPawn;

		public int opinionOfOtherPawn;

		public int opinionOfMe;

		public List<PawnRelationDef> relations = new List<PawnRelationDef>();

		public PregnancyApproach? pregnancyApproach;
	}

	private class CachedSocialTabEntryComparer : IComparer<CachedSocialTabEntry>
	{
		public int Compare(CachedSocialTabEntry a, CachedSocialTabEntry b)
		{
			bool flag = a.relations.Any();
			bool flag2 = b.relations.Any();
			if (flag != flag2)
			{
				return flag2.CompareTo(flag);
			}
			if (flag && flag2)
			{
				float num = float.MinValue;
				for (int i = 0; i < a.relations.Count; i++)
				{
					if (a.relations[i].importance > num)
					{
						num = a.relations[i].importance;
					}
				}
				float num2 = float.MinValue;
				for (int j = 0; j < b.relations.Count; j++)
				{
					if (b.relations[j].importance > num2)
					{
						num2 = b.relations[j].importance;
					}
				}
				if (num != num2)
				{
					return num2.CompareTo(num);
				}
			}
			if (a.opinionOfOtherPawn != b.opinionOfOtherPawn)
			{
				return b.opinionOfOtherPawn.CompareTo(a.opinionOfOtherPawn);
			}
			return a.otherPawn.thingIDNumber.CompareTo(b.otherPawn.thingIDNumber);
		}
	}

	private static Vector2 listScrollPosition = Vector2.zero;

	private static Vector2 ideoExposureScrollPosition = Vector2.zero;

	private static float listScrollViewHeight = 0f;

	private static bool showAllRelations;

	private static List<CachedSocialTabEntry> cachedEntries = new List<CachedSocialTabEntry>();

	private static List<Precept_Role> cachedRoles = new List<Precept_Role>();

	private static Pawn cachedForPawn;

	private const float TopPadding = 15f;

	private const float TopPaddingDevMode = 20f;

	private static readonly Color RelationLabelColor = new Color(0.6f, 0.6f, 0.6f);

	private static readonly Color PawnLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

	private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private const float RowTopPadding = 3f;

	private const float RowLeftRightPadding = 5f;

	private const float IconSize = 32f;

	private static CachedSocialTabEntryComparer CachedEntriesComparer = new CachedSocialTabEntryComparer();

	private static readonly Texture2D BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Green);

	private static readonly Vector2 RoleChangeButtonSize = new Vector2(115f, 28f);

	private const float IdeoExposurePctWidth = 40f;

	private const float IdeoExposureWidth = 93f;

	private const float IdeoExposureHeight = 36f;

	private const float IdeoExposureMaxRowsBeforeScroll = 2.5f;

	private const float IdeoExposureHighlightMargin = 4f;

	private const float IdeoExposureDetailSpace = 30f;

	private static float ideoChooseAge = -1f;

	private static HashSet<Pawn> tmpToCache = new HashSet<Pawn>();

	private static List<Pawn> pawnsForSocialInfoTmp = new List<Pawn>();

	public static List<Pair<Trait, MemeDef>> tmpAgreeableMemeTraitPairs = new List<Pair<Trait, MemeDef>>();

	public static List<Pair<Trait, MemeDef>> tmpDisagreeableMemeTraitPairs = new List<Pair<Trait, MemeDef>>();

	private static float IdeoligionChooseAge
	{
		get
		{
			if (ideoChooseAge == -1f)
			{
				ideoChooseAge = ThingDefOf.Human.race.lifeStageAges.First((LifeStageAge lsa) => lsa.def == LifeStageDefOf.HumanlikeChild).minAge;
			}
			return ideoChooseAge;
		}
	}

	public static void DrawSocialCard(Rect rect, Pawn pawn)
	{
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Small;
		float num = (Prefs.DevMode ? 20f : 15f);
		Rect rect2 = new Rect(0f, num, rect.width, rect.height - num).ContractedBy(10f);
		Rect rect3 = new Rect(0f, 5f, rect.width, 40f);
		Rect rect4 = new Rect(0f, 40f, rect.width, 40f);
		Rect rect5 = rect2;
		Rect rect6 = rect2;
		if (ModsConfig.IdeologyActive && !pawn.Dead && pawn.Ideo != null)
		{
			if (!pawn.Ideo.classicMode)
			{
				rect5.yMin += 40f;
				DrawPawnCertainty(pawn, rect3);
			}
			else
			{
				rect4.y = rect3.y;
			}
			rect5.yMin += 45f;
			Precept_Role precept_Role = pawn.Ideo?.GetRole(pawn);
			string label = ((precept_Role != null) ? precept_Role.LabelForPawn(pawn) : ((string)"NoRoleAssigned".Translate()));
			DrawPawnRole(pawn, precept_Role, label, rect4);
			DrawPawnRoleSelection(pawn, rect4);
		}
		rect5.height *= 0.63f;
		rect6.y = rect5.yMax + 17f;
		rect6.yMax = rect2.yMax;
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		Widgets.DrawLineHorizontal(0f, (rect5.yMax + rect6.y) / 2f, rect.width);
		GUI.color = Color.white;
		if (Prefs.DevMode && !pawn.Dead)
		{
			DrawDebugOptions(new Rect(5f, rect5.yMin - 20f, rect.width, 22f), pawn);
		}
		DrawRelationsAndOpinions(rect5, pawn);
		InteractionCardUtility.DrawInteractionsLog(rect6, pawn, Find.PlayLog.AllEntries, 12);
		Widgets.EndGroup();
	}

	private static void CheckRecache(Pawn selPawnForSocialInfo)
	{
		if (cachedForPawn != selPawnForSocialInfo || Time.frameCount % 20 == 0)
		{
			Recache(selPawnForSocialInfo);
		}
	}

	private static void Recache(Pawn selPawnForSocialInfo)
	{
		cachedForPawn = selPawnForSocialInfo;
		tmpToCache.Clear();
		foreach (Pawn relatedPawn in selPawnForSocialInfo.relations.RelatedPawns)
		{
			if (ShouldShowPawnRelations(relatedPawn, selPawnForSocialInfo))
			{
				RecacheEntry(relatedPawn, selPawnForSocialInfo);
				tmpToCache.Add(relatedPawn);
			}
		}
		List<Pawn> list = PawnsForSocialInfo(selPawnForSocialInfo);
		for (int i = 0; i < list.Count; i++)
		{
			Pawn pawn = list[i];
			if (!tmpToCache.Contains(pawn))
			{
				RecacheEntry(pawn, selPawnForSocialInfo);
				tmpToCache.Add(pawn);
			}
		}
		cachedEntries.RemoveAll((CachedSocialTabEntry x) => !tmpToCache.Contains(x.otherPawn));
		cachedEntries.Sort(CachedEntriesComparer);
		cachedRoles.Clear();
		if (selPawnForSocialInfo.Ideo != null)
		{
			cachedRoles.AddRange(RitualUtility.AllRolesForPawn(selPawnForSocialInfo));
			cachedRoles.SortBy((Precept_Role x) => x.def.displayOrderInImpact);
		}
	}

	private static bool ShouldShowPawnRelations(Pawn pawn, Pawn selPawnForSocialInfo)
	{
		if (showAllRelations)
		{
			return true;
		}
		if ((pawn.RaceProps.Animal && pawn.Dead && pawn.Corpse == null) || pawn.Name == null || pawn.Name.Numerical)
		{
			return false;
		}
		if (pawn.relations.hidePawnRelations || selPawnForSocialInfo.relations.hidePawnRelations)
		{
			return false;
		}
		if (pawn.relations.everSeenByPlayer)
		{
			return true;
		}
		return false;
	}

	public static List<Pawn> PawnsForSocialInfo(Pawn pawn)
	{
		pawnsForSocialInfoTmp.Clear();
		List<Pawn> list = null;
		if (pawn.MapHeld != null)
		{
			list = pawn.MapHeld.mapPawns.AllPawns;
		}
		else if (pawn.IsCaravanMember())
		{
			list = pawn.GetCaravan().PawnsListForReading;
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn2 = list[i];
				if (pawn2.RaceProps.Humanlike && pawn2 != pawn && ShouldShowPawnRelations(pawn2, pawn) && (pawn2.relations.OpinionOf(pawn) != 0 || pawn.relations.OpinionOf(pawn2) != 0))
				{
					pawnsForSocialInfoTmp.AddUnique(pawn2);
				}
			}
		}
		return pawnsForSocialInfoTmp;
	}

	private static void RecacheEntry(Pawn pawn, Pawn selPawnForSocialInfo, int? opinionOfMe = null, int? opinionOfOtherPawn = null)
	{
		bool flag = false;
		foreach (CachedSocialTabEntry cachedEntry in cachedEntries)
		{
			if (cachedEntry.otherPawn == pawn)
			{
				RecacheEntryInt(cachedEntry, selPawnForSocialInfo, opinionOfMe, opinionOfOtherPawn);
				flag = true;
			}
		}
		if (!flag)
		{
			CachedSocialTabEntry cachedSocialTabEntry = new CachedSocialTabEntry();
			cachedSocialTabEntry.otherPawn = pawn;
			RecacheEntryInt(cachedSocialTabEntry, selPawnForSocialInfo, opinionOfMe, opinionOfOtherPawn);
			cachedEntries.Add(cachedSocialTabEntry);
		}
	}

	private static void RecacheEntryInt(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo, int? opinionOfMe = null, int? opinionOfOtherPawn = null)
	{
		entry.opinionOfMe = (opinionOfMe.HasValue ? opinionOfMe.Value : entry.otherPawn.relations.OpinionOf(selPawnForSocialInfo));
		entry.opinionOfOtherPawn = (opinionOfOtherPawn.HasValue ? opinionOfOtherPawn.Value : selPawnForSocialInfo.relations.OpinionOf(entry.otherPawn));
		entry.relations.Clear();
		bool flag = false;
		foreach (PawnRelationDef relation in selPawnForSocialInfo.GetRelations(entry.otherPawn))
		{
			entry.relations.Add(relation);
			if (LovePartnerRelationUtility.IsLovePartnerRelation(relation))
			{
				flag = true;
			}
			if ((selPawnForSocialInfo.IsMutant && selPawnForSocialInfo.mutant.Def.incapableOfSocialInteractions) || (entry.otherPawn.IsMutant && entry.otherPawn.mutant.Def.incapableOfSocialInteractions))
			{
				flag = false;
			}
		}
		entry.relations.Sort((PawnRelationDef a, PawnRelationDef b) => b.importance.CompareTo(a.importance));
		if (Current.ProgramState == ProgramState.Playing && ModsConfig.BiotechActive && flag)
		{
			entry.pregnancyApproach = selPawnForSocialInfo.relations.GetPregnancyApproachForPartner(entry.otherPawn);
		}
		else
		{
			entry.pregnancyApproach = null;
		}
	}

	public static void DrawPawnCertainty(Pawn pawn, Rect rect)
	{
		tmpAgreeableMemeTraitPairs.Clear();
		tmpDisagreeableMemeTraitPairs.Clear();
		float num = rect.x + 17f;
		Rect rect2 = new Rect(num, rect.y + rect.height / 2f - 16f, 32f, 32f);
		pawn.Ideo.DrawIcon(rect2);
		num += 42f;
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect3 = new Rect(num, rect.y, rect.width / 2f - num, rect.height);
		Widgets.Label(rect3, pawn.Ideo.name.Truncate(rect3.width));
		Text.Anchor = TextAnchor.UpperLeft;
		num += rect3.width + 10f;
		Rect rect4 = new Rect(rect2.x, rect.y + rect.height / 2f - 16f, 0f, 32f);
		Rect rect5 = new Rect(num, rect.y + rect.height / 2f - 16f, rect.width - num - 26f, 32f);
		rect4.xMax = rect5.xMax;
		if (Mouse.IsOver(rect4))
		{
			Widgets.DrawHighlight(rect4);
			string text = pawn.ideo.CertaintyChangePerDay.ToStringPercent();
			if (pawn.ideo.CertaintyChangePerDay >= 0f)
			{
				text = "+" + text;
			}
			TaggedString tip = "CertaintyInIdeo".Translate(pawn.Named("PAWN"), pawn.Ideo.Named("IDEO")) + ": " + pawn.ideo.Certainty.ToStringPercent() + "\n\n" + "CertaintyChangePerDay".Translate() + ": " + text + "\n\n";
			float statValue = pawn.GetStatValue(StatDefOf.CertaintyLossFactor);
			if (statValue != 1f)
			{
				tip += StatDefOf.CertaintyLossFactor.LabelCap + ": " + statValue.ToStringPercent();
				foreach (Trait allTrait in pawn.story.traits.allTraits)
				{
					if (!allTrait.Suppressed)
					{
						float num2 = allTrait.MultiplierOfStat(StatDefOf.CertaintyLossFactor);
						if (num2 != 1f)
						{
							tip += "\n -  " + "AbilityIdeoConvertBreakdownTrait".Translate(allTrait.LabelCap.Named("TRAIT")) + ": x" + num2.ToStringPercent();
						}
					}
				}
				foreach (Precept item in pawn.Ideo.PreceptsListForReading)
				{
					if (item.def.statFactors != null)
					{
						float statFactorFromList = item.def.statFactors.GetStatFactorFromList(StatDefOf.CertaintyLossFactor);
						if (statFactorFromList != 1f)
						{
							tip += "\n -  " + "AbilityIdeoConvertBreakdownPercept".Translate(item.LabelCap.Named("PRECEPT")) + ": x" + statFactorFromList.ToStringPercent();
						}
					}
				}
				tip += "\n\n";
			}
			string text2 = ConversionUtility.GetCertaintyReductionFactorsDescription(pawn).Resolve();
			if (!text2.NullOrEmpty())
			{
				tip += text2 + "\n\n";
			}
			if (pawn.story.traits != null)
			{
				string text3 = string.Empty;
				foreach (Trait allTrait2 in pawn.story.traits.allTraits)
				{
					if (allTrait2.Suppressed)
					{
						continue;
					}
					List<MemeDef> affectedMemes = allTrait2.CurrentData.GetAffectedMemes(allTrait2.def, agreeable: true);
					for (int i = 0; i < affectedMemes.Count; i++)
					{
						if (pawn.Ideo.HasMeme(affectedMemes[i]))
						{
							tmpAgreeableMemeTraitPairs.Add(new Pair<Trait, MemeDef>(allTrait2, affectedMemes[i]));
						}
					}
					List<MemeDef> affectedMemes2 = allTrait2.CurrentData.GetAffectedMemes(allTrait2.def, agreeable: false);
					for (int j = 0; j < affectedMemes2.Count; j++)
					{
						if (pawn.Ideo.HasMeme(affectedMemes2[j]))
						{
							tmpDisagreeableMemeTraitPairs.Add(new Pair<Trait, MemeDef>(allTrait2, affectedMemes2[j]));
						}
					}
				}
				tmpAgreeableMemeTraitPairs.OrderBy((Pair<Trait, MemeDef> x) => x.First.Label);
				tmpDisagreeableMemeTraitPairs.OrderBy((Pair<Trait, MemeDef> x) => x.First.Label);
				if (tmpAgreeableMemeTraitPairs.Any())
				{
					text3 += "ConversionWeakerDueToTraitAgreements".Translate() + ":\n" + tmpAgreeableMemeTraitPairs.Select((Pair<Trait, MemeDef> x) => x.First.LabelCap + "/" + x.Second.LabelCap).ToLineList("  - ");
				}
				if (tmpDisagreeableMemeTraitPairs.Any())
				{
					if (!text3.NullOrEmpty())
					{
						text3 += "\n\n";
					}
					text3 += "ConversionStrongerDueToTraitAgreements".Translate() + ":\n" + tmpDisagreeableMemeTraitPairs.Select((Pair<Trait, MemeDef> x) => x.First.LabelCap + "/" + x.Second.LabelCap).ToLineList("  - ");
				}
				if (!text3.NullOrEmpty())
				{
					tip += text3 + "\n\n";
				}
			}
			string text4 = "MoodChangeRate".Translate() + ": ";
			foreach (CurvePoint point in ConversionTuning.CertaintyPerDayByMoodCurve.Points)
			{
				string text5 = point.y.ToStringPercent();
				if (point.y >= 0f)
				{
					text5 = "+" + text5;
				}
				text4 += "\n -  " + "Mood".Translate() + " " + point.x.ToStringPercent() + ": " + "PerDay".Translate(text5);
			}
			tip += text4.Colorize(Color.grey);
			TooltipHandler.TipRegion(rect4, () => tip.Resolve(), 10218219);
		}
		if (Widgets.ButtonInvisible(rect4))
		{
			IdeoUIUtility.OpenIdeoInfo(pawn.ideo.Ideo);
		}
		Widgets.FillableBar(rect5.ContractedBy(4f), pawn.ideo.Certainty, BarFullTexHor);
	}

	public static void DrawPawnRole(Pawn pawn, Precept_Role role, string label, Rect rect, bool drawLine = true)
	{
		float num = rect.x + 17f;
		if (role != null)
		{
			float y = rect.y + rect.height / 2f - 16f;
			Rect outerRect = rect;
			outerRect.x = num;
			outerRect.y = y;
			outerRect.width = 32f;
			outerRect.height = 32f;
			GUI.color = role.ideo.Color;
			Widgets.DrawTextureFitted(outerRect, role.Icon, 1f);
			GUI.color = Color.white;
			num += 42f;
		}
		else
		{
			GUI.color = Color.gray;
		}
		Rect rect2 = new Rect(rect.x + 17f, rect.y + rect.height / 2f - 16f, rect.width - num - RoleChangeButtonSize.x, 32f);
		Rect rect3 = rect;
		rect3.xMin = num;
		Text.Anchor = TextAnchor.MiddleLeft;
		Widgets.Label(rect3, label);
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
		if (Mouse.IsOver(rect2))
		{
			string roleDesc = "RoleDesc".Translate().Resolve();
			if (role != null)
			{
				roleDesc = roleDesc + "\n\n" + role.LabelForPawn(pawn) + ": " + role.GetTip();
			}
			Widgets.DrawHighlight(rect2);
			TipSignal tip = new TipSignal(() => roleDesc, pawn.thingIDNumber * 39);
			TooltipHandler.TipRegion(rect2, tip);
		}
		if (drawLine)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.yMax, rect.width);
			GUI.color = Color.white;
		}
	}

	public static void DrawPawnRoleSelection(Pawn pawn, Rect rect)
	{
		if (!pawn.IsFreeNonSlaveColonist)
		{
			return;
		}
		Precept_Role precept_Role = pawn.Ideo?.GetRole(pawn);
		Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
		Precept_Ritual roleChangeRitual = (Precept_Ritual)(pawn.Ideo?.GetPrecept(PreceptDefOf.RoleChange));
		TargetInfo ritualTarget = roleChangeRitual.targetFilter.BestTarget(pawn, TargetInfo.Invalid);
		bool flag = cachedRoles.Any() && pawn.Ideo != null;
		if (!flag)
		{
			GUI.color = Color.gray;
		}
		float y = rect.y + rect.height / 2f - 14f;
		Rect rect2 = new Rect(rect.width - 150f, y, RoleChangeButtonSize.x, RoleChangeButtonSize.y);
		rect2.xMax = rect.width - 26f - 4f;
		if (Widgets.ButtonText(rect2, "ChooseRole".Translate() + "...", drawBackground: true, doMouseoverSound: true, flag))
		{
			if (ritualTarget.IsValid)
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				if (precept_Role != null)
				{
					list.Add(new FloatMenuOption("RemoveCurrentRole".Translate(), delegate
					{
						Dialog_BeginRitual dialog_BeginRitual = (Dialog_BeginRitual)roleChangeRitual.GetRitualBeginWindow(ritualTarget, null, null, pawn, new Dictionary<string, Pawn> { { "role_changer", pawn } });
						dialog_BeginRitual.SetRoleToChangeTo(null);
						Find.WindowStack.Add(dialog_BeginRitual);
					}, Widgets.PlaceholderIconTex, Color.white));
				}
				foreach (Precept_Role cachedRole in cachedRoles)
				{
					Precept_Role newRole = cachedRole;
					if (newRole != precept_Role && newRole.Active && newRole.RequirementsMet(pawn) && (!newRole.def.leaderRole || pawn.Ideo == primaryIdeo))
					{
						string text = newRole.LabelForPawn(pawn).CapitalizeFirst();
						if (!pawn.Ideo.classicMode)
						{
							text = text + " (" + newRole.def.label + ")";
						}
						list.Add(new FloatMenuOption(text, delegate
						{
							Dialog_BeginRitual dialog_BeginRitual = (Dialog_BeginRitual)roleChangeRitual.GetRitualBeginWindow(ritualTarget, null, null, pawn, new Dictionary<string, Pawn> { { "role_changer", pawn } });
							dialog_BeginRitual.SetRoleToChangeTo(newRole);
							Find.WindowStack.Add(dialog_BeginRitual);
						}, newRole.Icon, newRole.ideo.Color, MenuOptionPriority.Default, DrawTooltip)
						{
							orderInPriority = newRole.def.displayOrderInImpact
						});
					}
					void DrawTooltip(Rect r)
					{
						TipSignal tip = new TipSignal(() => newRole.GetTip(), pawn.thingIDNumber * 39);
						TooltipHandler.TipRegion(r, tip);
					}
				}
				foreach (Precept_Role cachedRole2 in cachedRoles)
				{
					if ((cachedRole2 != precept_Role && !cachedRole2.RequirementsMet(pawn)) || !cachedRole2.Active)
					{
						string text2 = cachedRole2.LabelForPawn(pawn) + " (" + cachedRole2.def.label + ")";
						if (cachedRole2.ChosenPawnSingle() != null)
						{
							text2 = text2 + ": " + cachedRole2.ChosenPawnSingle().LabelShort;
						}
						else if (!cachedRole2.RequirementsMet(pawn))
						{
							text2 = text2 + ": " + cachedRole2.GetFirstUnmetRequirement(pawn).GetLabel(cachedRole2).CapitalizeFirst();
						}
						else if (!cachedRole2.Active && cachedRole2.def.activationBelieverCount > cachedRole2.ideo.ColonistBelieverCountCached)
						{
							text2 += ": " + "InactiveRoleRequiresMoreBelievers".Translate(cachedRole2.def.activationBelieverCount, cachedRole2.ideo.memberName, cachedRole2.ideo.ColonistBelieverCountCached).CapitalizeFirst();
						}
						list.Add(new FloatMenuOption(text2, null, cachedRole2.Icon, cachedRole2.ideo.Color)
						{
							orderInPriority = cachedRole2.def.displayOrderInImpact
						});
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			else
			{
				Messages.Message((Find.IdeoManager.classicMode ? "AbilityDisabledNoRitualSpot" : "AbilityDisabledNoAltarIdeogramOrRitualsSpot").Translate(), pawn, MessageTypeDefOf.RejectInput);
			}
		}
		GUI.color = Color.white;
	}

	private static void DrawIdeoExposure(Pawn baby, float rectWidth, out float heightOffset)
	{
		List<Pawn_IdeoTracker.IdeoExposureWeight> babyIdeoExposureSorted = baby.ideo.BabyIdeoExposureSorted;
		float babyIdeoExposureTotal = baby.ideo.BabyIdeoExposureTotal;
		int num = babyIdeoExposureSorted?.Count ?? 0;
		float num2 = rectWidth - 20f - 18f;
		int num3 = Mathf.FloorToInt(num2 / 93f);
		int num4 = Mathf.FloorToInt((num - 1) / num3) + 1;
		float num5 = ((num == 0) ? 90f : (Mathf.Min(num4, 2.5f) * 36f + 10f));
		Rect rect = new Rect(10f, 10f, num2, 25f + num5 + 10f);
		GUI.BeginGroup(rect);
		float curY = 0f;
		Widgets.ListSeparator(ref curY, num2, "IdeoExposureSectionHeader".Translate().CapitalizeFirst());
		curY += 4f;
		TextAnchor anchor = Text.Anchor;
		Rect rect2 = new Rect(0f, curY, num2, num5);
		if (num > 0)
		{
			float width = (((float)num4 > 2.5f) ? (num2 - 16f) : num2);
			Widgets.BeginScrollView(viewRect: new Rect(0f, curY, width, (float)num4 * 36f + 10f), outRect: rect2, scrollPosition: ref ideoExposureScrollPosition);
			Text.Anchor = TextAnchor.MiddleCenter;
			int num6 = 0;
			foreach (Pawn_IdeoTracker.IdeoExposureWeight item in babyIdeoExposureSorted)
			{
				if (num6 > 0 && num6 % num3 == 0)
				{
					curY += 36f;
				}
				DrawIdeoExposureItem(baby, item.ideo, rect, item.exposure, babyIdeoExposureTotal, num6 % num3, curY);
				num6++;
			}
			Widgets.EndScrollView();
		}
		else
		{
			GameFont font = Text.Font;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, "IdeoExposureNoExposure".Translate(baby.Named("BABY")));
			Text.Font = GameFont.Tiny;
			rect2.yMin += 30f;
			Widgets.Label(rect2, "IdeoExposureNoExposureDetail".Translate(baby.Named("BABY"), IdeoligionChooseAge.ToStringDecimalIfSmall().Named("CHILDAGE")).CapitalizeFirst());
			Text.Font = font;
		}
		Text.Anchor = anchor;
		GUI.EndGroup();
		heightOffset = rect.height + rect.yMin;
	}

	private static void DrawIdeoExposureItem(Pawn baby, Ideo ideo, Rect rect, float ideoExposure, float totalExposure, int rowIndex, float yOff)
	{
		float num = rect.x + 93f * (float)rowIndex;
		float f = Mathf.Clamp01(ideoExposure / totalExposure);
		ideo.DrawIcon(new Rect(num, rect.y + yOff, 32f, 32f).ContractedBy(4f));
		Widgets.Label(new Rect(num + 32f + 4f, rect.y + yOff, 40f, 32f), f.ToStringPercent());
		Rect rect2 = new Rect(num, rect.y + yOff, 80f, 32f);
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
			TaggedString taggedString = "IdeoExposurePointsTooltipJoinLikelihood".Translate(baby.Named("BABY"), f.ToStringPercent().Named("PERCENT"), ideo.Named("IDEO")) + "\n\n" + "IdeoExposurePointsTooltipJoinDescription".Translate(baby.Named("BABY"), IdeoligionChooseAge.ToStringDecimalIfSmall().Named("CHILDAGE")) + "\n\n" + "IdeoExposurePointsTooltipExposureDescription".Translate(baby.Named("BABY")) + "\n\n" + "IdeoExposurePointsTooltipIdeoExposure".Translate(ideo.Named("IDEO")) + ": " + ideoExposure.ToStringDecimalIfSmall() + "\n" + "IdeoExposurePointsTooltipTotalExposure".Translate() + ": " + totalExposure.ToStringDecimalIfSmall();
			TooltipHandler.TipRegion(rect2, taggedString);
			if (Widgets.ButtonInvisible(rect2))
			{
				IdeoUIUtility.OpenIdeoInfo(ideo);
			}
		}
	}

	public static bool AnyRelations(Pawn selPawnForSocialInfo)
	{
		CheckRecache(selPawnForSocialInfo);
		return cachedEntries.Any();
	}

	public static void DrawRelationsAndOpinions(Rect rect, Pawn selPawnForSocialInfo)
	{
		CheckRecache(selPawnForSocialInfo);
		if (Current.ProgramState != ProgramState.Playing)
		{
			showAllRelations = false;
		}
		Widgets.BeginGroup(rect);
		Text.Font = GameFont.Small;
		GUI.color = Color.white;
		bool flag = CanDrawTryRomance(selPawnForSocialInfo);
		float num = (flag ? (RoleChangeButtonSize.y + 10f) : 0f);
		Rect outRect = new Rect(0f, 0f, rect.width, rect.height - num);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, listScrollViewHeight);
		Rect rect2 = rect;
		if (viewRect.height > outRect.height)
		{
			rect2.width -= 16f;
		}
		Widgets.BeginScrollView(outRect, ref listScrollPosition, viewRect);
		float num2 = 0f;
		float y = listScrollPosition.y;
		float num3 = listScrollPosition.y + outRect.height;
		for (int i = 0; i < cachedEntries.Count; i++)
		{
			float rowHeight = GetRowHeight(cachedEntries[i], rect2.width, selPawnForSocialInfo);
			if (num2 > y - rowHeight && num2 < num3)
			{
				DrawPawnRow(num2, rect2.width, cachedEntries[i], selPawnForSocialInfo);
			}
			num2 += rowHeight;
		}
		if (Event.current.type == EventType.Layout)
		{
			listScrollViewHeight = num2 + 30f;
		}
		Widgets.EndScrollView();
		if (flag)
		{
			DrawTryRomance(new Rect(rect.width - 150f + 10f, rect.height - RoleChangeButtonSize.y, RoleChangeButtonSize.x, RoleChangeButtonSize.y), selPawnForSocialInfo);
		}
		Widgets.EndGroup();
		GUI.color = Color.white;
	}

	private static bool CanDrawTryRomance(Pawn pawn)
	{
		if (ModsConfig.BiotechActive && pawn.ageTracker.AgeBiologicalYearsFloat >= 16f && pawn.Spawned)
		{
			return pawn.IsFreeColonist;
		}
		return false;
	}

	private static void DrawTryRomance(Rect buttonRect, Pawn pawn)
	{
		Color color = GUI.color;
		bool isTryRomanceOnCooldown = pawn.relations.IsTryRomanceOnCooldown;
		AcceptanceReport acceptanceReport = RelationsUtility.RomanceEligible(pawn, initiator: true, forOpinionExplanation: false);
		List<FloatMenuOption> list = (acceptanceReport.Accepted ? RomanceOptions(pawn) : null);
		GUI.color = ((!acceptanceReport.Accepted || list.NullOrEmpty() || isTryRomanceOnCooldown) ? ColoredText.SubtleGrayColor : Color.white);
		if (Widgets.ButtonText(buttonRect, "TryRomanceButtonLabel".Translate() + "..."))
		{
			if (isTryRomanceOnCooldown)
			{
				int numTicks = pawn.relations.romanceEnableTick - Find.TickManager.TicksGame;
				Messages.Message("CantRomanceInitiateMessageCooldown".Translate(pawn, numTicks.ToStringTicksToPeriod()), MessageTypeDefOf.RejectInput, historical: false);
				return;
			}
			if (!acceptanceReport.Accepted)
			{
				if (!acceptanceReport.Reason.NullOrEmpty())
				{
					Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
				}
				return;
			}
			if (list.NullOrEmpty())
			{
				Messages.Message("TryRomanceNoOptsMessage".Translate(pawn), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}
		GUI.color = color;
	}

	private static List<FloatMenuOption> RomanceOptions(Pawn romancer)
	{
		List<(float, FloatMenuOption)> list = new List<(float, FloatMenuOption)>();
		List<FloatMenuOption> list2 = new List<FloatMenuOption>();
		foreach (Pawn item in romancer.Map.mapPawns.FreeColonistsSpawned)
		{
			if (RelationsUtility.RomanceOption(romancer, item, out var option, out var chance))
			{
				list.Add((chance, option));
			}
			else if (option != null)
			{
				list2.Add(option);
			}
		}
		return (from pair in list
			orderby pair.Item1 descending
			select pair.Item2).Concat(list2.OrderBy((FloatMenuOption opt) => opt.Label)).ToList();
	}

	private static void DrawPawnRow(float y, float width, CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
	{
		float rowHeight = GetRowHeight(entry, width, selPawnForSocialInfo);
		Rect rect = new Rect(0f, y, width, rowHeight);
		Pawn otherPawn = entry.otherPawn;
		if (Mouse.IsOver(rect))
		{
			GUI.color = HighlightColor;
			GUI.DrawTexture(rect, TexUI.HighlightTex);
		}
		Rect rect2 = rect;
		if (entry.pregnancyApproach.HasValue)
		{
			rect2.xMax -= 28f;
		}
		if (Mouse.IsOver(rect2))
		{
			TooltipHandler.TipRegion(rect2, () => GetPawnRowTooltip(entry, selPawnForSocialInfo), entry.otherPawn.thingIDNumber * 13 + selPawnForSocialInfo.thingIDNumber);
		}
		if (Widgets.ButtonInvisible(rect2))
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (otherPawn.Dead)
				{
					Messages.Message("MessageCantSelectDeadPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (otherPawn.SpawnedOrAnyParentSpawned || otherPawn.IsCaravanMember())
				{
					CameraJumper.TryJumpAndSelect(otherPawn);
				}
				else
				{
					Messages.Message("MessageCantSelectOffMapPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			else if (Find.GameInitData.startingAndOptionalPawns.Contains(otherPawn))
			{
				Page_ConfigureStartingPawns page_ConfigureStartingPawns = Find.WindowStack.WindowOfType<Page_ConfigureStartingPawns>();
				if (page_ConfigureStartingPawns != null)
				{
					page_ConfigureStartingPawns.SelectPawn(otherPawn);
					SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
				}
			}
		}
		CalculateColumnsWidths(width, out var relationsWidth, out var pawnLabelWidth, out var myOpinionWidth, out var hisOpinionWidth, out var pawnSituationLabelWidth);
		if (Current.ProgramState != ProgramState.Playing)
		{
			DrawPawnAndRelationLabel(rect: new Rect(5f, y + 3f, width - myOpinionWidth - hisOpinionWidth - 5f, rowHeight - 3f), entry: entry, pawn: otherPawn, selPawnForSocialInfo: selPawnForSocialInfo);
			Rect rect3 = new Rect(width - myOpinionWidth - hisOpinionWidth, y + 3f, myOpinionWidth, rowHeight - 3f);
			DrawMyOpinion(entry, rect3, selPawnForSocialInfo);
			DrawHisOpinion(rect: new Rect(rect3.xMax, y + 3f, hisOpinionWidth, rowHeight - 3f), entry: entry, selPawnForSocialInfo: selPawnForSocialInfo);
			return;
		}
		if (entry.pregnancyApproach.HasValue)
		{
			pawnSituationLabelWidth -= 28f;
		}
		Rect rect4 = new Rect(5f, y + 3f, relationsWidth, rowHeight - 3f);
		DrawRelationLabel(entry, rect4, selPawnForSocialInfo);
		Rect rect5 = new Rect(rect4.xMax, y + 3f, pawnLabelWidth, rowHeight - 3f);
		DrawPawnLabel(otherPawn, rect5);
		Rect rect6 = new Rect(rect5.xMax, y + 3f, myOpinionWidth, rowHeight - 3f);
		DrawMyOpinion(entry, rect6, selPawnForSocialInfo);
		Rect rect7 = new Rect(rect6.xMax, y + 3f, hisOpinionWidth, rowHeight - 3f);
		DrawHisOpinion(entry, rect7, selPawnForSocialInfo);
		Rect rect8 = new Rect(rect7.xMax, y + 3f, pawnSituationLabelWidth, rowHeight - 3f);
		DrawPawnSituationLabel(entry.otherPawn, rect8, selPawnForSocialInfo);
		if (entry.pregnancyApproach.HasValue)
		{
			DrawPregnancyApproach(entry, new Rect(rect8.xMax + 4f, y + 3f, 24f, 24f), selPawnForSocialInfo);
		}
	}

	private static float GetRowHeight(CachedSocialTabEntry entry, float rowWidth, Pawn selPawnForSocialInfo)
	{
		CalculateColumnsWidths(rowWidth, out var relationsWidth, out var pawnLabelWidth, out var myOpinionWidth, out var hisOpinionWidth, out var _);
		float a = 0f;
		if (Current.ProgramState != ProgramState.Playing)
		{
			float width = rowWidth - hisOpinionWidth - myOpinionWidth - 10f;
			a = Text.CalcHeight(GetRelationsString(entry, selPawnForSocialInfo) + ": " + GetPawnLabel(entry.otherPawn), width);
		}
		else
		{
			a = Mathf.Max(a, Text.CalcHeight(GetRelationsString(entry, selPawnForSocialInfo), relationsWidth));
			a = Mathf.Max(a, Text.CalcHeight(GetPawnLabel(entry.otherPawn), pawnLabelWidth));
		}
		return a + 3f;
	}

	private static void CalculateColumnsWidths(float rowWidth, out float relationsWidth, out float pawnLabelWidth, out float myOpinionWidth, out float hisOpinionWidth, out float pawnSituationLabelWidth)
	{
		float num = rowWidth - 10f;
		relationsWidth = num * 0.23f;
		pawnLabelWidth = num * 0.41f;
		myOpinionWidth = num * 0.075f;
		hisOpinionWidth = num * 0.085f;
		pawnSituationLabelWidth = num * 0.2f;
		if (myOpinionWidth < 25f)
		{
			pawnLabelWidth -= 25f - myOpinionWidth;
			myOpinionWidth = 25f;
		}
		if (hisOpinionWidth < 35f)
		{
			pawnLabelWidth -= 35f - hisOpinionWidth;
			hisOpinionWidth = 35f;
		}
	}

	private static void DrawRelationLabel(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
	{
		string relationsString = GetRelationsString(entry, selPawnForSocialInfo);
		if (!relationsString.NullOrEmpty())
		{
			GUI.color = RelationLabelColor;
			Widgets.Label(rect, relationsString);
		}
	}

	private static void DrawPawnLabel(Pawn pawn, Rect rect)
	{
		GUI.color = PawnLabelColor;
		Widgets.Label(rect, GetPawnLabel(pawn));
	}

	private static void DrawPawnAndRelationLabel(CachedSocialTabEntry entry, Pawn pawn, Rect rect, Pawn selPawnForSocialInfo)
	{
		Widgets.Label(rect, (GetRelationsString(entry, selPawnForSocialInfo) + ": ").Colorize(RelationLabelColor) + GetPawnLabel(pawn).Colorize(PawnLabelColor));
	}

	private static void DrawMyOpinion(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
	{
		if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
		{
			int opinionOfOtherPawn = entry.opinionOfOtherPawn;
			GUI.color = OpinionLabelColor(opinionOfOtherPawn);
			Widgets.Label(rect, opinionOfOtherPawn.ToStringWithSign());
		}
	}

	private static void DrawHisOpinion(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
	{
		if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
		{
			int opinionOfMe = entry.opinionOfMe;
			Color color = OpinionLabelColor(opinionOfMe);
			GUI.color = new Color(color.r, color.g, color.b, 0.4f);
			Widgets.Label(rect, "(" + opinionOfMe.ToStringWithSign() + ")");
		}
	}

	private static void DrawPawnSituationLabel(Pawn pawn, Rect rect, Pawn selPawnForSocialInfo)
	{
		GUI.color = Color.gray;
		string label = GetPawnSituationLabel(pawn, selPawnForSocialInfo).Truncate(rect.width);
		Widgets.Label(rect, label);
	}

	private static void DrawPregnancyApproach(CachedSocialTabEntry entry, Rect rect, Pawn selPawnForSocialInfo)
	{
		if (!entry.pregnancyApproach.HasValue || !Find.Storyteller.difficulty.ChildrenAllowed)
		{
			return;
		}
		GUI.color = Color.white;
		PregnancyApproach value = entry.pregnancyApproach.Value;
		AcceptanceReport acceptanceReport = PregnancyUtility.CanEverProduceChild(selPawnForSocialInfo, entry.otherPawn);
		if (selPawnForSocialInfo.IsWorldPawn())
		{
			acceptanceReport = "PawnIsAway".Translate(selPawnForSocialInfo.Named("PAWN"));
		}
		if (!acceptanceReport.Accepted)
		{
			GUI.color = Color.grey;
		}
		GUI.DrawTexture(rect, value.GetIcon());
		GUI.color = Color.white;
		if (Widgets.ButtonInvisible(rect))
		{
			if (acceptanceReport.Accepted)
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (PregnancyApproach value2 in Enum.GetValues(typeof(PregnancyApproach)))
				{
					PregnancyApproach rmLocal = value2;
					list.Add(new FloatMenuOption(rmLocal.GetDescription(), delegate
					{
						selPawnForSocialInfo.relations.SetPregnancyApproach(entry.otherPawn, rmLocal);
					}, value2.GetIcon(), Color.white));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			else
			{
				Messages.Message("PregnancyNotPossible".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
			}
		}
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, acceptanceReport ? ("PregnancyApproach".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n" + value.GetDescription() + "\n\n" + "ClickToChangePregnancyApproach".Translate().Colorize(ColoredText.SubtleGrayColor)) : ("PregnancyNotPossible".Translate().Resolve() + ": " + acceptanceReport.Reason.CapitalizeFirst()));
		}
	}

	private static Color OpinionLabelColor(int opinion)
	{
		if (Mathf.Abs(opinion) < 10)
		{
			return Color.gray;
		}
		if (opinion < 0)
		{
			return ColorLibrary.RedReadable;
		}
		return Color.green;
	}

	private static string GetPawnLabel(Pawn pawn)
	{
		if (pawn.Name != null)
		{
			return pawn.Name.ToStringFull;
		}
		return pawn.LabelCapNoCount;
	}

	public static string GetPawnSituationLabel(Pawn pawn, Pawn fromPOV)
	{
		if (pawn.Dead)
		{
			return "Dead".Translate();
		}
		if (pawn.Destroyed)
		{
			return "Missing".Translate();
		}
		if (PawnUtility.IsKidnappedPawn(pawn))
		{
			return "Kidnapped".Translate();
		}
		QuestPart_LendColonistsToFaction questPart_LendColonistsToFaction = QuestUtility.GetAllQuestPartsOfType<QuestPart_LendColonistsToFaction>().FirstOrDefault((QuestPart_LendColonistsToFaction p) => p.LentColonistsListForReading.Contains(pawn));
		if (questPart_LendColonistsToFaction != null)
		{
			return "Lent".Translate(questPart_LendColonistsToFaction.lendColonistsToFaction.Named("FACTION"), questPart_LendColonistsToFaction.ReturnPawnsInDurationTicks.ToStringTicksToDays("0.0")).Resolve();
		}
		if (pawn.kindDef == PawnKindDefOf.Slave)
		{
			return "Slave".Translate().CapitalizeFirst();
		}
		if (PawnUtility.IsFactionLeader(pawn))
		{
			return "FactionLeader".Translate();
		}
		Faction faction = pawn.Faction;
		if (faction != fromPOV.Faction)
		{
			if (faction == null || fromPOV.Faction == null)
			{
				return "Neutral".Translate();
			}
			return faction.RelationKindWith(fromPOV.Faction) switch
			{
				FactionRelationKind.Hostile => "Hostile".Translate() + ", " + faction.Name, 
				FactionRelationKind.Neutral => "Neutral".Translate() + ", " + faction.Name, 
				FactionRelationKind.Ally => "Ally".Translate() + ", " + faction.Name, 
				_ => "", 
			};
		}
		return "";
	}

	private static string GetRelationsString(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
	{
		string text = "";
		if (entry.relations.Count == 0)
		{
			if (entry.opinionOfOtherPawn < -20)
			{
				return "Rival".Translate();
			}
			if (entry.opinionOfOtherPawn > 20)
			{
				return "Friend".Translate();
			}
			return "Acquaintance".Translate();
		}
		for (int i = 0; i < entry.relations.Count; i++)
		{
			PawnRelationDef pawnRelationDef = entry.relations[i];
			text = (text.NullOrEmpty() ? pawnRelationDef.GetGenderSpecificLabelCap(entry.otherPawn) : (text + ", " + pawnRelationDef.GetGenderSpecificLabel(entry.otherPawn)));
		}
		return text;
	}

	private static string GetPawnRowTooltip(CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (entry.otherPawn.RaceProps.Humanlike && selPawnForSocialInfo.RaceProps.Humanlike)
		{
			stringBuilder.AppendLine(selPawnForSocialInfo.relations.OpinionExplanation(entry.otherPawn));
			stringBuilder.AppendLine();
			string text = RomanceExplanation(selPawnForSocialInfo, entry.otherPawn);
			if (!text.NullOrEmpty())
			{
				stringBuilder.AppendLine(text);
			}
			stringBuilder.Append(("SomeonesOpinionOfMe".Translate(entry.otherPawn.LabelShort) + ": ").Colorize(ColoredText.TipSectionTitleColor));
			stringBuilder.Append(entry.opinionOfMe.ToStringWithSign());
		}
		else
		{
			stringBuilder.Append(entry.otherPawn.LabelCapNoCount);
			string pawnSituationLabel = GetPawnSituationLabel(entry.otherPawn, selPawnForSocialInfo);
			if (!pawnSituationLabel.NullOrEmpty())
			{
				stringBuilder.AppendLine(" (" + pawnSituationLabel + ")");
			}
			else
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(" - " + GetRelationsString(entry, selPawnForSocialInfo));
		}
		if (Prefs.DevMode)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("(debug) Compatibility: " + selPawnForSocialInfo.relations.CompatibilityWith(entry.otherPawn).ToString("F2"));
			stringBuilder.Append("(debug) RomanceChanceFactor: " + selPawnForSocialInfo.relations.SecondaryRomanceChanceFactor(entry.otherPawn).ToString("F2"));
		}
		return stringBuilder.ToString();
	}

	private static string RomanceExplanation(Pawn romancer, Pawn romanceTarget)
	{
		if (!CanDrawTryRomance(romancer))
		{
			return null;
		}
		AcceptanceReport acceptanceReport = RelationsUtility.RomanceEligiblePair(romancer, romanceTarget, forOpinionExplanation: true);
		if (!acceptanceReport.Accepted && acceptanceReport.Reason.NullOrEmpty())
		{
			return null;
		}
		if (!acceptanceReport.Accepted)
		{
			return "RomanceChanceCant".Translate() + (" (" + acceptanceReport.Reason + ")\n");
		}
		StringBuilder stringBuilder = new StringBuilder();
		float f = InteractionWorker_RomanceAttempt.SuccessChance(romancer, romanceTarget, 1f);
		stringBuilder.AppendLine(("RomanceChance".Translate() + (": " + f.ToStringPercent())).Colorize(ColoredText.TipSectionTitleColor));
		stringBuilder.Append(InteractionWorker_RomanceAttempt.RomanceFactors(romancer, romanceTarget));
		return stringBuilder.ToString();
	}

	private static void DrawDebugOptions(Rect rect, Pawn pawn)
	{
		Widgets.BeginGroup(rect);
		Widgets.CheckboxLabeled(new Rect(0f, 0f, 145f, 22f), "DEV: AllRelations", ref showAllRelations);
		Widgets.EndGroup();
	}

	public static void ClearCaches()
	{
		cachedForPawn = null;
		cachedEntries.Clear();
		cachedRoles.Clear();
		tmpToCache.Clear();
		tmpDisagreeableMemeTraitPairs.Clear();
		tmpAgreeableMemeTraitPairs.Clear();
		pawnsForSocialInfoTmp.Clear();
	}
}
