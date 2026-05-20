using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public class WITab_Caravan_Health : WITab
{
	private Vector2 scrollPosition;

	private float scrollViewHeight;

	private Pawn specificHealthTabForPawn;

	private bool compactMode;

	private static List<PawnCapacityDef> capacitiesToDisplay = new List<PawnCapacityDef>();

	private const float RowHeight = 40f;

	private const float PawnLabelHeight = 18f;

	private const float PawnLabelColumnWidth = 100f;

	private const float SpaceAroundIcon = 4f;

	private const float PawnCapacityColumnWidth = 100f;

	private const float BeCarriedIfSickColumnWidth = 40f;

	private const float BeCarriedIfSickIconSize = 24f;

	private static readonly Texture2D BeCarriedIfSickIcon = ContentFinder<Texture2D>.Get("UI/Icons/CarrySick");

	private List<Pawn> Pawns => base.SelCaravan.PawnsListForReading;

	private List<PawnCapacityDef> CapacitiesToDisplay
	{
		get
		{
			capacitiesToDisplay.Clear();
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].showOnCaravanHealthTab)
				{
					capacitiesToDisplay.Add(allDefsListForReading[i]);
				}
			}
			capacitiesToDisplay.SortBy((PawnCapacityDef x) => x.listOrder);
			return capacitiesToDisplay;
		}
	}

	private float SpecificHealthTabWidth
	{
		get
		{
			EnsureSpecificHealthTabForPawnValid();
			if (specificHealthTabForPawn.DestroyedOrNull())
			{
				return 0f;
			}
			return 630f;
		}
	}

	public WITab_Caravan_Health()
	{
		labelKey = "TabCaravanHealth";
	}

	protected override void FillTab()
	{
		EnsureSpecificHealthTabForPawnValid();
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		float curY = 0f;
		Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
		DoColumnHeaders(ref curY);
		DoRows(ref curY, rect2, rect);
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}

	protected override void UpdateSize()
	{
		EnsureSpecificHealthTabForPawnValid();
		base.UpdateSize();
		size = GetRawSize(compactMode: false);
		if (size.x + SpecificHealthTabWidth > (float)UI.screenWidth)
		{
			compactMode = true;
			size = GetRawSize(compactMode: true);
		}
		else
		{
			compactMode = false;
		}
	}

	protected override void ExtraOnGUI()
	{
		EnsureSpecificHealthTabForPawnValid();
		base.ExtraOnGUI();
		Pawn localSpecificHealthTabForPawn = specificHealthTabForPawn;
		if (localSpecificHealthTabForPawn == null)
		{
			return;
		}
		Rect tabRect = base.TabRect;
		float specificHealthTabWidth = SpecificHealthTabWidth;
		Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificHealthTabWidth, tabRect.height);
		Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
		{
			if (!localSpecificHealthTabForPawn.DestroyedOrNull())
			{
				HealthCardUtility.DrawPawnHealthCard(new Rect(Vector2.zero, rect.size), localSpecificHealthTabForPawn, allowOperations: false, showBloodLoss: true, localSpecificHealthTabForPawn);
				if (Widgets.CloseButtonFor(rect.AtZero()))
				{
					specificHealthTabForPawn = null;
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			}
		});
	}

	private void DoColumnHeaders(ref float curY)
	{
		if (!compactMode)
		{
			float num = 135f;
			Text.Anchor = TextAnchor.UpperCenter;
			GUI.color = Widgets.SeparatorLabelColor;
			Widgets.Label(new Rect(num, 3f, 100f, 100f), "Pain".Translate());
			num += 100f;
			List<PawnCapacityDef> list = CapacitiesToDisplay;
			for (int i = 0; i < list.Count; i++)
			{
				Widgets.Label(new Rect(num, 3f, 100f, 100f), list[i].LabelCap.Truncate(100f));
				num += 100f;
			}
			Rect rect = new Rect(num + 8f, 0f, 24f, 24f);
			GUI.DrawTexture(rect, BeCarriedIfSickIcon);
			TooltipHandler.TipRegionByKey(rect, "BeCarriedIfSickTip");
			num += 40f;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}
	}

	private void DoRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
	{
		List<Pawn> pawns = Pawns;
		if (specificHealthTabForPawn != null && !pawns.Contains(specificHealthTabForPawn))
		{
			specificHealthTabForPawn = null;
		}
		bool flag = false;
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			if (pawn.IsColonist)
			{
				if (!flag)
				{
					Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
					flag = true;
				}
				DoRow(ref curY, scrollViewRect, scrollOutRect, pawn);
			}
		}
		bool flag2 = false;
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn2 = pawns[j];
			if (!pawn2.IsColonist)
			{
				if (!flag2)
				{
					Widgets.ListSeparator(ref curY, scrollViewRect.width, ModsConfig.BiotechActive ? "CaravanPrisonersAnimalsAndMechs".Translate() : "CaravanPrisonersAndAnimals".Translate());
					flag2 = true;
				}
				DoRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
			}
		}
	}

	private Vector2 GetRawSize(bool compactMode)
	{
		float num = 100f;
		if (!compactMode)
		{
			num += 100f;
			num += (float)CapacitiesToDisplay.Count * 100f;
			num += 40f;
		}
		Vector2 result = default(Vector2);
		result.x = 127f + num + 16f;
		result.y = Mathf.Min(550f, PaneTopY - 30f);
		return result;
	}

	private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
	{
		float num = scrollPosition.y - 40f;
		float num2 = scrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoRow(new Rect(0f, curY, viewRect.width, 40f), p);
		}
		curY += 40f;
	}

	private void DoRow(Rect rect, Pawn p)
	{
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		CaravanThingsTabUtility.DoAbandonButton(rect2, p, base.SelCaravan);
		rect2.width -= 24f;
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
		rect2.width -= 24f;
		CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, p, ref specificHealthTabForPawn);
		rect2.width -= 24f;
		if (Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
		}
		Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect3, p);
		Rect bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
		GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
		float num = bgRect.xMax;
		if (!compactMode)
		{
			if (p.RaceProps.IsFlesh)
			{
				Rect rect4 = new Rect(num, 0f, 100f, 40f);
				DoPain(rect4, p);
			}
			num += 100f;
			List<PawnCapacityDef> list = CapacitiesToDisplay;
			for (int i = 0; i < list.Count; i++)
			{
				Rect rect5 = new Rect(num, 0f, 100f, 40f);
				if ((p.RaceProps.Humanlike && !list[i].showOnHumanlikes) || (p.RaceProps.Animal && !list[i].showOnAnimals) || (p.RaceProps.IsAnomalyEntity && !list[i].showOnAnomalyEntities) || (p.RaceProps.IsMechanoid && !list[i].showOnMechanoids) || (p.RaceProps.IsDrone && !list[i].showOnDrones) || !PawnCapacityUtility.BodyCanEverDoCapacity(p.RaceProps.body, list[i]))
				{
					num += 100f;
					continue;
				}
				DoCapacity(rect5, p, list[i]);
				num += 100f;
			}
		}
		if (!compactMode)
		{
			Vector2 vector = new Vector2(num + 8f, 8f);
			Widgets.Checkbox(vector, ref p.health.beCarriedByCaravanIfSick, 24f, disabled: false, paintable: true);
			TooltipHandler.TipRegionByKey(new Rect(vector, new Vector2(24f, 24f)), "BeCarriedIfSickTip");
			num += 40f;
		}
		if (p.Downed && !p.ageTracker.CurLifeStage.alwaysDowned)
		{
			GUI.color = new Color(1f, 0f, 0f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
			GUI.color = Color.white;
		}
		Widgets.EndGroup();
	}

	private void DoPain(Rect rect, Pawn pawn)
	{
		Pair<string, Color> painLabel = HealthCardUtility.GetPainLabel(pawn);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		GUI.color = painLabel.Second;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect, painLabel.First);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect))
		{
			string painTip = HealthCardUtility.GetPainTip(pawn);
			TooltipHandler.TipRegion(rect, painTip);
		}
	}

	private void DoCapacity(Rect rect, Pawn pawn, PawnCapacityDef capacity)
	{
		Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, capacity);
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
		}
		GUI.color = efficiencyLabel.Second;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect, efficiencyLabel.First);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
		if (Mouse.IsOver(rect))
		{
			string pawnCapacityTip = HealthCardUtility.GetPawnCapacityTip(pawn, capacity);
			TooltipHandler.TipRegion(rect, pawnCapacityTip);
		}
	}

	public override void Notify_ClearingAllMapsMemory()
	{
		base.Notify_ClearingAllMapsMemory();
		specificHealthTabForPawn = null;
	}

	private void EnsureSpecificHealthTabForPawnValid()
	{
		if (specificHealthTabForPawn != null && (specificHealthTabForPawn.Destroyed || !base.SelCaravan.ContainsPawn(specificHealthTabForPawn)))
		{
			specificHealthTabForPawn = null;
		}
	}
}
