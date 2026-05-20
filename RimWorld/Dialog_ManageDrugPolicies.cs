using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Dialog_ManageDrugPolicies : Dialog_ManagePolicies<DrugPolicy>
{
	private Vector2 scrollPosition;

	private const float DrugEntryRowHeight = 35f;

	private const float CellsPadding = 4f;

	private const float EntryLineYOffset = 2f;

	private const float InfoButtonSize = 16f;

	private const float HeaderHeight = 36f;

	private static readonly Texture2D IconForAddiction = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Addiction");

	private static readonly Texture2D IconForJoy = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Joy");

	private static readonly Texture2D IconScheduled = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Schedule");

	private const float UsageSpacing = 12f;

	protected override string TitleKey => "DrugPolicyTitle";

	protected override string TipKey => "DrugPolicyTip";

	protected override float OffsetHeaderY => 36f;

	public override Vector2 InitialSize => new Vector2(Mathf.Min(Screen.width - 50, 1300), 720f);

	public Dialog_ManageDrugPolicies(DrugPolicy policy)
		: base(policy)
	{
	}

	protected override DrugPolicy CreateNewPolicy()
	{
		return Current.Game.drugPolicyDatabase.MakeNewDrugPolicy();
	}

	protected override DrugPolicy GetDefaultPolicy()
	{
		return Current.Game.drugPolicyDatabase.DefaultDrugPolicy();
	}

	protected override void SetDefaultPolicy(DrugPolicy policy)
	{
		Current.Game.drugPolicyDatabase.SetDefault(policy);
	}

	protected override AcceptanceReport TryDeletePolicy(DrugPolicy policy)
	{
		return Current.Game.drugPolicyDatabase.TryDelete(policy);
	}

	protected override List<DrugPolicy> GetPolicies()
	{
		return Current.Game.drugPolicyDatabase.AllPolicies;
	}

	protected override void DoContentsRect(Rect rect)
	{
		DoPolicyConfigArea(rect);
	}

	public override void PostOpen()
	{
		base.PostOpen();
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
	}

	private void DoPolicyConfigArea(Rect rect)
	{
		Rect rect2 = rect;
		rect2.y -= OffsetHeaderY;
		rect2.height = 36f;
		Rect rect3 = rect;
		rect3.yMin = rect2.yMax;
		DoColumnLabels(rect2);
		Widgets.DrawMenuSection(rect3);
		rect3 = rect3.ContractedBy(1f);
		if (base.SelectedPolicy.Count == 0)
		{
			GUI.color = Color.grey;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect3, "NoDrugs".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			return;
		}
		float height = (float)base.SelectedPolicy.Count * 35f;
		Rect viewRect = new Rect(0f, 0f, rect3.width, height);
		Widgets.AdjustRectsForScrollView(rect, ref rect3, ref viewRect);
		Widgets.BeginScrollView(rect3, ref scrollPosition, viewRect);
		DrugPolicy selectedPolicy = base.SelectedPolicy;
		for (int i = 0; i < selectedPolicy.Count; i++)
		{
			Rect rect4 = new Rect(0f, (float)i * 35f, viewRect.width, 35f);
			DoEntryRow(rect4, selectedPolicy[i], i);
		}
		Widgets.EndScrollView();
	}

	private void CalculateColumnsWidths(Rect rect, out float addictionWidth, out float allowJoyWidth, out float scheduledWidth, out float drugIconWidth, out float drugNameWidth, out float frequencyWidth, out float moodThresholdWidth, out float joyThresholdWidth, out float takeToInventoryWidth)
	{
		float num = rect.width - 27f - 108f;
		drugIconWidth = 27f;
		drugNameWidth = num * 0.25f;
		addictionWidth = 36f;
		allowJoyWidth = 36f;
		scheduledWidth = 36f;
		frequencyWidth = num * 0.3f;
		moodThresholdWidth = num * 0.15f;
		joyThresholdWidth = num * 0.15f;
		takeToInventoryWidth = num * 0.15f;
	}

	private void DoColumnLabels(Rect rect)
	{
		rect.width -= 16f;
		CalculateColumnsWidths(rect, out var addictionWidth, out var allowJoyWidth, out var scheduledWidth, out var drugIconWidth, out var drugNameWidth, out var frequencyWidth, out var moodThresholdWidth, out var joyThresholdWidth, out var takeToInventoryWidth);
		float x = rect.x;
		Text.Anchor = TextAnchor.LowerCenter;
		Rect rect2 = new Rect(x + 4f, rect.y, drugNameWidth + drugIconWidth, rect.height);
		Widgets.Label(rect2, "DrugColumnLabel".Translate());
		TooltipHandler.TipRegionByKey(rect2, "DrugNameColumnDesc");
		x += drugNameWidth + drugIconWidth;
		Rect rect3 = new Rect(x, rect.y, takeToInventoryWidth, rect.height);
		Widgets.Label(rect3, "TakeToInventoryColumnLabel".Translate());
		TooltipHandler.TipRegionByKey(rect3, "TakeToInventoryColumnDesc");
		x += takeToInventoryWidth;
		Rect rect4 = new Rect(x, rect.yMax - 24f, 24f, 24f);
		GUI.DrawTexture(rect4, IconForAddiction);
		TooltipHandler.TipRegionByKey(rect4, "DrugUsageTipForAddiction");
		x += addictionWidth;
		Rect rect5 = new Rect(x, rect.yMax - 24f, 24f, 24f);
		GUI.DrawTexture(rect5, IconForJoy);
		TooltipHandler.TipRegionByKey(rect5, "DrugUsageTipForJoy");
		x += allowJoyWidth;
		Rect rect6 = new Rect(x, rect.yMax - 24f, 24f, 24f);
		GUI.DrawTexture(rect6, IconScheduled);
		TooltipHandler.TipRegionByKey(rect6, "DrugUsageTipScheduled");
		x += scheduledWidth;
		Text.Anchor = TextAnchor.LowerCenter;
		Rect rect7 = new Rect(x, rect.y, frequencyWidth, rect.height);
		Widgets.Label(rect7, "FrequencyColumnLabel".Translate());
		TooltipHandler.TipRegionByKey(rect7, "FrequencyColumnDesc");
		x += frequencyWidth;
		Rect rect8 = new Rect(x, rect.y, moodThresholdWidth, rect.height);
		Widgets.Label(rect8, "MoodThresholdColumnLabel".Translate());
		TooltipHandler.TipRegionByKey(rect8, "MoodThresholdColumnDesc");
		x += moodThresholdWidth;
		Rect rect9 = new Rect(x, rect.y, joyThresholdWidth, rect.height);
		Widgets.Label(rect9, "JoyThresholdColumnLabel".Translate());
		TooltipHandler.TipRegionByKey(rect9, "JoyThresholdColumnDesc");
		x += joyThresholdWidth;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	private void DoEntryRow(Rect rect, DrugPolicyEntry entry, int index)
	{
		CalculateColumnsWidths(rect, out var addictionWidth, out var allowJoyWidth, out var scheduledWidth, out var drugIconWidth, out var drugNameWidth, out var frequencyWidth, out var moodThresholdWidth, out var joyThresholdWidth, out var takeToInventoryWidth);
		if (index % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Text.Anchor = TextAnchor.MiddleLeft;
		float x = rect.x;
		float num = (rect.height - drugIconWidth) / 2f;
		Widgets.ThingIcon(new Rect(x + 5f, rect.y + num, drugIconWidth, drugIconWidth), entry.drug);
		x += drugIconWidth;
		Widgets.Label(new Rect(x, rect.y, drugNameWidth, rect.height).ContractedBy(4f), entry.drug.LabelCap);
		Widgets.InfoCardButton(new Rect
		{
			x = x + Text.CalcSize(entry.drug.LabelCap).x + 5f,
			y = rect.y + 8f,
			width = 16f,
			height = 16f
		}, entry.drug);
		x += drugNameWidth;
		Widgets.TextFieldNumeric(new Rect(x, rect.y, takeToInventoryWidth, rect.height).ContractedBy(4f), ref entry.takeToInventory, ref entry.takeToInventoryTempBuffer, 0f, PawnUtility.GetMaxAllowedToPickUp(entry.drug));
		x += takeToInventoryWidth;
		if (entry.drug.IsAddictiveDrug)
		{
			Widgets.Checkbox(x, rect.y + 2f, ref entry.allowedForAddiction, 24f, disabled: false, paintable: true);
		}
		x += addictionWidth;
		if (entry.drug.IsPleasureDrug)
		{
			Widgets.Checkbox(x, rect.y + 2f, ref entry.allowedForJoy, 24f, disabled: false, paintable: true);
		}
		x += allowJoyWidth;
		Widgets.Checkbox(x, rect.y + 2f, ref entry.allowScheduled, 24f, disabled: false, paintable: true);
		x += scheduledWidth;
		if (entry.allowScheduled)
		{
			entry.daysFrequency = Widgets.FrequencyHorizontalSlider(new Rect(x, rect.y + 2f, frequencyWidth, rect.height).ContractedBy(4f), entry.daysFrequency, 0.1f, 25f, roundToInt: true);
			x += frequencyWidth;
			entry.onlyIfMoodBelow = Widgets.HorizontalSlider(label: (!(entry.onlyIfMoodBelow < 1f)) ? ((string)"NoDrugUseRequirement".Translate()) : entry.onlyIfMoodBelow.ToStringPercent(), rect: new Rect(x, rect.y + 2f, moodThresholdWidth, rect.height).ContractedBy(4f), value: entry.onlyIfMoodBelow, min: 0.01f, max: 1f, middleAlignment: true);
			x += moodThresholdWidth;
			entry.onlyIfJoyBelow = Widgets.HorizontalSlider(label: (!(entry.onlyIfJoyBelow < 1f)) ? ((string)"NoDrugUseRequirement".Translate()) : entry.onlyIfJoyBelow.ToStringPercent(), rect: new Rect(x, rect.y + 2f, joyThresholdWidth, rect.height).ContractedBy(4f), value: entry.onlyIfJoyBelow, min: 0.01f, max: 1f, middleAlignment: true);
			x += joyThresholdWidth;
		}
		else
		{
			x += frequencyWidth + moodThresholdWidth + joyThresholdWidth;
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}
}
