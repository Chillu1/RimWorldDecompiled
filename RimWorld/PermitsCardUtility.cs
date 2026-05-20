using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class PermitsCardUtility
{
	private static Vector2 rightScrollPosition;

	public static RoyalTitlePermitDef selectedPermit;

	public static Faction selectedFaction;

	private const float LeftRectPercent = 0.33f;

	private const float TitleHeight = 55f;

	private const float ReturnButtonWidth = 180f;

	private const float PermitOptionWidth = 200f;

	private const float PermitOptionHeight = 50f;

	private const float AcceptButtonHeight = 50f;

	private const float SwitchFactionsButtonSize = 32f;

	private const float LineWidthNotSelected = 2f;

	private const float LineWidthSelected = 4f;

	private const int ReturnPermitsCost = 8;

	private static readonly Vector2 PermitOptionSpacing = new Vector2(0.25f, 0.35f);

	private static readonly Texture2D SwitchFactionIcon = ContentFinder<Texture2D>.Get("UI/Icons/SwitchFaction");

	private static bool ShowSwitchFactionButton
	{
		get
		{
			int num = 0;
			foreach (Faction item in Find.FactionManager.AllFactionsVisible)
			{
				if (item.IsPlayer || item.def.permanentEnemy || item.temporary)
				{
					continue;
				}
				foreach (RoyalTitlePermitDef allDef in DefDatabase<RoyalTitlePermitDef>.AllDefs)
				{
					if (allDef.faction == item.def)
					{
						num++;
						break;
					}
				}
			}
			return num > 1;
		}
	}

	private static int TotalReturnPermitsCost(Pawn pawn)
	{
		int num = 8;
		List<FactionPermit> allFactionPermits = pawn.royalty.AllFactionPermits;
		for (int i = 0; i < allFactionPermits.Count; i++)
		{
			if (allFactionPermits[i].OnCooldown && allFactionPermits[i].Permit.royalAid != null)
			{
				num += allFactionPermits[i].Permit.royalAid.favorCost;
			}
		}
		return num;
	}

	public static void DrawRecordsCard(Rect rect, Pawn pawn)
	{
		if (!ModLister.CheckRoyalty("Permit"))
		{
			return;
		}
		rect.yMax -= 4f;
		if (ShowSwitchFactionButton)
		{
			Rect rect2 = new Rect(rect.x, rect.y, 32f, 32f);
			if (Widgets.ButtonImage(rect2, SwitchFactionIcon))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Faction item in Find.FactionManager.AllFactionsVisibleInViewOrder)
				{
					if (!item.IsPlayer && !item.def.permanentEnemy)
					{
						Faction localFaction = item;
						list.Add(new FloatMenuOption(localFaction.Name, delegate
						{
							selectedFaction = localFaction;
							selectedPermit = null;
						}, localFaction.def.FactionIcon, localFaction.Color));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			TooltipHandler.TipRegion(rect2, "SwitchFaction_Desc".Translate());
		}
		if (selectedFaction.def.HasRoyalTitles)
		{
			string label = "ReturnAllPermits".Translate();
			Rect rect3 = new Rect(rect.xMax - 180f, rect.y - 4f, 180f, 51f);
			int num = TotalReturnPermitsCost(pawn);
			if (Widgets.ButtonText(rect3, label))
			{
				if (!pawn.royalty.PermitsFromFaction(selectedFaction).Any())
				{
					Messages.Message("NoPermitsToReturn".Translate(pawn.Named("PAWN")), new LookTargets(pawn), MessageTypeDefOf.RejectInput, historical: false);
				}
				else if (pawn.royalty.GetFavor(selectedFaction) < num)
				{
					Messages.Message("NotEnoughFavor".Translate(num.Named("FAVORCOST"), selectedFaction.def.royalFavorLabel.Named("FAVOR"), pawn.Named("PAWN"), pawn.royalty.GetFavor(selectedFaction).Named("CURFAVOR")), MessageTypeDefOf.RejectInput);
				}
				else
				{
					string text = "ReturnAllPermits_Confirm".Translate(8.Named("BASEFAVORCOST"), num.Named("FAVORCOST"), selectedFaction.def.royalFavorLabel.Named("FAVOR"), selectedFaction.Named("FACTION"));
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
					{
						pawn.royalty.RefundPermits(8, selectedFaction);
					}, destructive: true));
				}
			}
			TooltipHandler.TipRegion(rect3, "ReturnAllPermits_Desc".Translate(8.Named("BASEFAVORCOST"), num.Named("FAVORCOST"), selectedFaction.def.royalFavorLabel.Named("FAVOR")));
		}
		RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
		Rect rect4 = new Rect(rect.xMax - 360f - 4f, rect.y - 4f, 360f, 55f);
		string text2 = string.Concat("CurrentTitle".Translate() + ": " + ((currentTitle != null) ? currentTitle.GetLabelFor(pawn).CapitalizeFirst() : ((string)"None".Translate())) + "\n" + "UnusedPermits".Translate() + ": ", pawn.royalty.GetPermitPoints(selectedFaction).ToString());
		if (!selectedFaction.def.royalFavorLabel.NullOrEmpty())
		{
			text2 = text2 + "\n" + selectedFaction.def.royalFavorLabel.CapitalizeFirst() + ": " + pawn.royalty.GetFavor(selectedFaction);
		}
		Widgets.Label(rect4, text2);
		rect.yMin += 55f;
		Rect rect5 = new Rect(rect);
		rect5.width *= 0.33f;
		DoLeftRect(rect5, pawn);
		Rect rect6 = new Rect(rect);
		rect6.xMin = rect5.xMax + 10f;
		DoRightRect(rect6, pawn);
	}

	private static void DoLeftRect(Rect rect, Pawn pawn)
	{
		float num = 0f;
		RoyalTitleDef currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
		Rect rect2 = new Rect(rect);
		Widgets.BeginGroup(rect2);
		if (selectedPermit != null)
		{
			Text.Font = GameFont.Medium;
			Rect rect3 = new Rect(0f, num, rect2.width, 0f);
			Widgets.LabelCacheHeight(ref rect3, selectedPermit.LabelCap);
			Text.Font = GameFont.Small;
			num += rect3.height;
			if (!selectedPermit.description.NullOrEmpty())
			{
				Rect rect4 = new Rect(0f, num, rect2.width, 0f);
				Widgets.LabelCacheHeight(ref rect4, selectedPermit.description);
				num += rect4.height + 16f;
			}
			Rect rect5 = new Rect(0f, num, rect2.width, 0f);
			string text = "Cooldown".Translate() + ": " + "PeriodDays".Translate(selectedPermit.cooldownDays);
			if (selectedPermit.royalAid != null && selectedPermit.royalAid.favorCost > 0 && !selectedFaction.def.royalFavorLabel.NullOrEmpty())
			{
				text = string.Concat(text, "\n" + "CooldownUseFavorCost".Translate(selectedFaction.def.royalFavorLabel.Named("HONOR")).CapitalizeFirst() + ": ", selectedPermit.royalAid.favorCost.ToString());
			}
			if (selectedPermit.minTitle != null)
			{
				text = text + "\n" + "RequiresTitle".Translate(selectedPermit.minTitle.GetLabelForBothGenders()).Resolve().Colorize((currentTitle != null && currentTitle.seniority >= selectedPermit.minTitle.seniority) ? Color.white : ColorLibrary.RedReadable);
			}
			if (selectedPermit.prerequisite != null)
			{
				text = text + "\n" + "UpgradeFrom".Translate(selectedPermit.prerequisite.LabelCap).Resolve().Colorize(PermitUnlocked(selectedPermit.prerequisite, pawn) ? Color.white : ColorLibrary.RedReadable);
			}
			Widgets.LabelCacheHeight(ref rect5, text);
			num += rect5.height + 4f;
			Rect rect6 = new Rect(0f, rect2.height - 50f, rect2.width, 50f);
			if (selectedPermit.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(selectedPermit, pawn) && Widgets.ButtonText(rect6, "AcceptPermit".Translate()))
			{
				SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
				pawn.royalty.AddPermit(selectedPermit, selectedFaction);
			}
		}
		Widgets.EndGroup();
	}

	private static void DoRightRect(Rect rect, Pawn pawn)
	{
		Widgets.DrawMenuSection(rect);
		if (selectedFaction == null)
		{
			return;
		}
		List<RoyalTitlePermitDef> allDefsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
		Rect outRect = rect.ContractedBy(10f);
		Rect rect2 = default(Rect);
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			RoyalTitlePermitDef permit = allDefsListForReading[i];
			if (CanDrawPermit(permit))
			{
				rect2.width = Mathf.Max(rect2.width, DrawPosition(permit).x + 200f + 26f);
				rect2.height = Mathf.Max(rect2.height, DrawPosition(permit).y + 50f + 26f);
			}
		}
		Widgets.BeginScrollView(outRect, ref rightScrollPosition, rect2);
		Widgets.BeginGroup(rect2.ContractedBy(10f));
		DrawLines();
		for (int j = 0; j < allDefsListForReading.Count; j++)
		{
			RoyalTitlePermitDef royalTitlePermitDef = allDefsListForReading[j];
			if (CanDrawPermit(royalTitlePermitDef))
			{
				Vector2 vector = DrawPosition(royalTitlePermitDef);
				Rect rect3 = new Rect(vector.x, vector.y, 200f, 50f);
				Color color = Widgets.NormalOptionColor;
				Color bgColor = (PermitUnlocked(royalTitlePermitDef, pawn) ? TexUI.OldFinishedResearchColor : TexUI.AvailResearchColor);
				Color borderColor;
				if (selectedPermit == royalTitlePermitDef)
				{
					borderColor = TexUI.HighlightBorderResearchColor;
					bgColor += TexUI.HighlightBgResearchColor;
				}
				else
				{
					borderColor = TexUI.DefaultBorderResearchColor;
				}
				if (!royalTitlePermitDef.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(royalTitlePermitDef, pawn))
				{
					color = Color.red;
				}
				if (Widgets.CustomButtonText(ref rect3, string.Empty, bgColor, color, borderColor))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					selectedPermit = royalTitlePermitDef;
				}
				TextAnchor anchor = Text.Anchor;
				Color color2 = GUI.color;
				GUI.color = color;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, royalTitlePermitDef.LabelCap);
				GUI.color = color2;
				Text.Anchor = anchor;
			}
		}
		Widgets.EndGroup();
		Widgets.EndScrollView();
	}

	private static void DrawLines()
	{
		Vector2 start = default(Vector2);
		Vector2 end = default(Vector2);
		List<RoyalTitlePermitDef> allDefsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < allDefsListForReading.Count; j++)
			{
				RoyalTitlePermitDef royalTitlePermitDef = allDefsListForReading[j];
				if (!CanDrawPermit(royalTitlePermitDef))
				{
					continue;
				}
				Vector2 vector = DrawPosition(royalTitlePermitDef);
				start.x = vector.x;
				start.y = vector.y + 25f;
				RoyalTitlePermitDef prerequisite = royalTitlePermitDef.prerequisite;
				if (prerequisite != null)
				{
					Vector2 vector2 = DrawPosition(prerequisite);
					end.x = vector2.x + 200f;
					end.y = vector2.y + 25f;
					if ((i == 1 && selectedPermit == royalTitlePermitDef) || selectedPermit == prerequisite)
					{
						Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
					}
					else if (i == 0)
					{
						Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
					}
				}
			}
		}
	}

	private static bool PermitUnlocked(RoyalTitlePermitDef permit, Pawn pawn)
	{
		if (pawn.royalty.HasPermit(permit, selectedFaction))
		{
			return true;
		}
		List<FactionPermit> allFactionPermits = pawn.royalty.AllFactionPermits;
		for (int i = 0; i < allFactionPermits.Count; i++)
		{
			if (allFactionPermits[i].Permit.prerequisite == permit && allFactionPermits[i].Faction == selectedFaction)
			{
				return true;
			}
		}
		return false;
	}

	private static Vector2 DrawPosition(RoyalTitlePermitDef permit)
	{
		Vector2 vector = new Vector2(permit.uiPosition.x * 200f, permit.uiPosition.y * 50f);
		return vector + vector * PermitOptionSpacing;
	}

	private static bool CanDrawPermit(RoyalTitlePermitDef permit)
	{
		if (permit.permitPointCost > 0)
		{
			if (permit.faction != null)
			{
				return permit.faction == selectedFaction.def;
			}
			return true;
		}
		return false;
	}
}
