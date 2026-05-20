using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class Bill_Production : Bill, IRenameable
{
	public BillRepeatModeDef repeatMode = BillRepeatModeDefOf.RepeatCount;

	public int repeatCount = 1;

	private BillStoreModeDef storeMode = BillStoreModeDefOf.BestStockpile;

	private ISlotGroup storeGroup;

	private string playerCustomName;

	public int targetCount = 10;

	public bool pauseWhenSatisfied;

	public int unpauseWhenYouHave = 5;

	public bool includeEquipped;

	public bool includeTainted;

	public FloatRange hpRange = FloatRange.ZeroToOne;

	public QualityRange qualityRange = QualityRange.All;

	public bool limitToAllowedStuff;

	private ISlotGroup includeGroup;

	public bool paused;

	protected override string StatusString
	{
		get
		{
			if (paused)
			{
				return " " + "Paused".Translate().CapitalizeFirst();
			}
			return "";
		}
	}

	protected override float StatusLineMinHeight
	{
		get
		{
			if (!CanUnpause())
			{
				return 0f;
			}
			return 24f;
		}
	}

	public string RepeatInfoText
	{
		get
		{
			if (repeatMode == BillRepeatModeDefOf.Forever)
			{
				return "Forever".Translate();
			}
			if (repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				return repeatCount + "x";
			}
			if (repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				return recipe.WorkerCounter.CountProducts(this) + "/" + targetCount;
			}
			throw new InvalidOperationException();
		}
	}

	public string RenamableLabel
	{
		get
		{
			return playerCustomName ?? BaseLabel;
		}
		set
		{
			playerCustomName = value;
		}
	}

	public string BaseLabel => base.LabelCap;

	public string InspectLabel => RenamableLabel;

	public override string LabelCap => RenamableLabel;

	public Bill_Production()
	{
	}

	public Bill_Production(RecipeDef recipe, Precept_ThingStyle precept = null)
		: base(recipe, precept)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref repeatMode, "repeatMode");
		Scribe_Values.Look(ref repeatCount, "repeatCount", 0);
		Scribe_Defs.Look(ref storeMode, "storeMode");
		Scribe_Values.Look(ref playerCustomName, "playerCustomName");
		Scribe_Values.Look(ref targetCount, "targetCount", 0);
		Scribe_Values.Look(ref pauseWhenSatisfied, "pauseWhenSatisfied", defaultValue: false);
		Scribe_Values.Look(ref unpauseWhenYouHave, "unpauseWhenYouHave", 0);
		Scribe_Values.Look(ref includeEquipped, "includeEquipped", defaultValue: false);
		Scribe_Values.Look(ref includeTainted, "includeTainted", defaultValue: false);
		Scribe_Values.Look(ref hpRange, "hpRange", FloatRange.ZeroToOne);
		Scribe_Values.Look(ref qualityRange, "qualityRange", QualityRange.All);
		Scribe_Values.Look(ref limitToAllowedStuff, "limitToAllowedStuff", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			SaveSlotReferencable(storeGroup, "storeGroup");
			SaveSlotReferencable(includeGroup, "includeGroup");
		}
		else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.LoadingVars)
		{
			LoadSlotReferencable(ref storeGroup, "storeGroup");
			LoadSlotReferencable(ref includeGroup, "includeGroup");
		}
		Scribe_Values.Look(ref paused, "paused", defaultValue: false);
		if (repeatMode == null)
		{
			repeatMode = BillRepeatModeDefOf.RepeatCount;
		}
		if (storeMode == null)
		{
			storeMode = BillStoreModeDefOf.BestStockpile;
		}
	}

	private static void SaveSlotReferencable(ISlotGroup slot, string key)
	{
		ILoadReferenceable refee = null;
		if (slot is ILoadReferenceable loadReferenceable)
		{
			refee = loadReferenceable;
		}
		else if (slot is SlotGroup { parent: ILoadReferenceable parent })
		{
			refee = parent;
		}
		Scribe_References.Look(ref refee, key);
	}

	private static void LoadSlotReferencable(ref ISlotGroup slot, string key)
	{
		ILoadReferenceable refee = null;
		Scribe_References.Look(ref refee, key);
		if (refee is ISlotGroup slotGroup)
		{
			slot = slotGroup;
		}
		else if (refee is ISlotGroupParent slotGroupParent)
		{
			slot = slotGroupParent.GetSlotGroup();
		}
	}

	public override BillStoreModeDef GetStoreMode()
	{
		return storeMode;
	}

	public override ISlotGroup GetSlotGroup()
	{
		return storeGroup;
	}

	public void SetIncludeGroup(ISlotGroup group)
	{
		includeGroup = group;
	}

	public ISlotGroup GetIncludeSlotGroup()
	{
		return includeGroup;
	}

	public override void SetStoreMode(BillStoreModeDef mode, ISlotGroup group = null)
	{
		storeGroup = group;
		storeMode = mode;
		if (storeMode == BillStoreModeDefOf.SpecificStockpile != (group != null))
		{
			Log.ErrorOnce("Inconsistent bill StoreMode data set", 75645354);
		}
	}

	public override bool ShouldDoNow()
	{
		if (repeatMode != BillRepeatModeDefOf.TargetCount)
		{
			paused = false;
		}
		if (suspended)
		{
			return false;
		}
		if (repeatMode == BillRepeatModeDefOf.Forever)
		{
			return true;
		}
		if (repeatMode == BillRepeatModeDefOf.RepeatCount)
		{
			return repeatCount > 0;
		}
		if (repeatMode == BillRepeatModeDefOf.TargetCount)
		{
			int num = recipe.WorkerCounter.CountProducts(this);
			if (pauseWhenSatisfied && num >= targetCount)
			{
				paused = true;
			}
			if (num <= unpauseWhenYouHave || !pauseWhenSatisfied)
			{
				paused = false;
			}
			if (paused)
			{
				return false;
			}
			return num < targetCount;
		}
		throw new InvalidOperationException();
	}

	public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
	{
		if (repeatMode == BillRepeatModeDefOf.RepeatCount)
		{
			if (repeatCount > 0)
			{
				repeatCount--;
			}
			if (repeatCount == 0)
			{
				Messages.Message("MessageBillComplete".Translate(LabelCap), (Thing)billStack.billGiver, MessageTypeDefOf.TaskCompletion);
			}
		}
		recipe.Worker.Notify_IterationCompleted(billDoer, ingredients);
	}

	protected virtual Window GetBillDialog()
	{
		return new Dialog_BillConfig(this, ((Thing)billStack.billGiver).Position);
	}

	protected override void DoConfigInterface(Rect baseRect, Color baseColor)
	{
		Rect rect = new Rect(28f, 32f, 100f, 30f);
		GUI.color = new Color(1f, 1f, 1f, 0.65f);
		Widgets.Label(rect, RepeatInfoText);
		GUI.color = baseColor;
		WidgetRow widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp);
		if (widgetRow.ButtonText("Details".Translate() + "..."))
		{
			Find.WindowStack.Add(GetBillDialog());
		}
		if (widgetRow.ButtonText(repeatMode.LabelCap.Resolve().PadRight(20)))
		{
			BillRepeatModeUtility.MakeConfigFloatMenu(this);
		}
		if (widgetRow.ButtonIcon(TexButton.Plus))
		{
			if (repeatMode == BillRepeatModeDefOf.Forever)
			{
				repeatMode = BillRepeatModeDefOf.RepeatCount;
				repeatCount = 1;
			}
			else if (repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				int num = recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
				targetCount += num;
				unpauseWhenYouHave += num;
			}
			else if (repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				repeatCount += GenUI.CurrentAdjustmentMultiplier();
			}
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			if (TutorSystem.TutorialMode && repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				TutorSystem.Notify_Event(recipe.defName + "-RepeatCountSetTo-" + repeatCount);
			}
		}
		if (widgetRow.ButtonIcon(TexButton.Minus))
		{
			if (repeatMode == BillRepeatModeDefOf.Forever)
			{
				repeatMode = BillRepeatModeDefOf.RepeatCount;
				repeatCount = 1;
			}
			else if (repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				int num2 = recipe.targetCountAdjustment * GenUI.CurrentAdjustmentMultiplier();
				targetCount = Mathf.Max(0, targetCount - num2);
				unpauseWhenYouHave = Mathf.Max(0, unpauseWhenYouHave - num2);
			}
			else if (repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				repeatCount = Mathf.Max(0, repeatCount - GenUI.CurrentAdjustmentMultiplier());
			}
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
			if (TutorSystem.TutorialMode && repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				TutorSystem.Notify_Event(recipe.defName + "-RepeatCountSetTo-" + repeatCount);
			}
		}
	}

	private bool CanUnpause()
	{
		if (repeatMode == BillRepeatModeDefOf.TargetCount && paused && pauseWhenSatisfied)
		{
			return recipe.WorkerCounter.CountProducts(this) < targetCount;
		}
		return false;
	}

	public override void DoStatusLineInterface(Rect rect)
	{
		if (paused && new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenUp).ButtonText("Unpause".Translate()))
		{
			paused = false;
		}
	}

	public override void ValidateSettings()
	{
		base.ValidateSettings();
		ValidateGroup(ref storeGroup);
		ValidateGroup(ref includeGroup);
		if (storeGroup == null && storeMode == BillStoreModeDefOf.SpecificStockpile)
		{
			SetStoreMode(BillStoreModeDefOf.DropOnFloor);
		}
	}

	private void ValidateGroup(ref ISlotGroup slot)
	{
		if (slot == null)
		{
			return;
		}
		if (slot is SlotGroup slotGroup)
		{
			if (slotGroup.parent is Zone_Stockpile zone && !IsZoneValid(zone))
			{
				slot = null;
			}
			else if (slotGroup.parent is Building_Storage storage && !IsBuildingValid(storage))
			{
				slot = null;
			}
		}
		else if (slot is StorageGroup storageGroup && !IsStorageGroupValid(storageGroup))
		{
			slot = null;
		}
	}

	private bool IsZoneValid(Zone_Stockpile zone)
	{
		if (zone == null)
		{
			return false;
		}
		int id = zone.ID;
		if (!base.Map.zoneManager.AllZones.Contains(zone))
		{
			zone = base.Map.zoneManager.AllZones.FirstOrDefault((Zone x) => x.ID == id) as Zone_Stockpile;
		}
		if (zone == null)
		{
			return false;
		}
		if (!zone.zoneManager.AllZones.Contains(zone))
		{
			if (this != BillUtility.Clipboard)
			{
				Messages.Message("MessageBillValidationIncludeZoneDeleted".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), zone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
			}
			return false;
		}
		if (base.Map != null && !base.Map.zoneManager.AllZones.Contains(zone))
		{
			if (this != BillUtility.Clipboard)
			{
				Messages.Message("MessageBillValidationIncludeZoneUnavailable".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), zone.label), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
			}
			return false;
		}
		return true;
	}

	private bool IsBuildingValid(Building_Storage storage)
	{
		if (storage == null)
		{
			return true;
		}
		if (base.Map != null && !base.Map.haulDestinationManager.AllGroups.Contains(storage.slotGroup))
		{
			if (this != BillUtility.Clipboard)
			{
				Messages.Message("MessageBillValidationIncludeBuildingDeleted".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), storage.LabelCap), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
			}
			return false;
		}
		return true;
	}

	private bool IsStorageGroupValid(StorageGroup group)
	{
		if (group == null)
		{
			return true;
		}
		if (base.Map != null && !base.Map.storageGroups.HasStorageGroup(group))
		{
			if (this != BillUtility.Clipboard)
			{
				Messages.Message("MessageBillValidationIncludeStorageGroupDeleted".Translate(LabelCap, billStack.billGiver.LabelShort.CapitalizeFirst(), group.RenamableLabel), billStack.billGiver as Thing, MessageTypeDefOf.NegativeEvent);
			}
			return false;
		}
		return true;
	}

	public override Bill Clone()
	{
		Bill_Production obj = (Bill_Production)base.Clone();
		obj.repeatMode = repeatMode;
		obj.repeatCount = repeatCount;
		obj.storeMode = storeMode;
		obj.storeGroup = storeGroup;
		obj.playerCustomName = playerCustomName;
		obj.targetCount = targetCount;
		obj.pauseWhenSatisfied = pauseWhenSatisfied;
		obj.unpauseWhenYouHave = unpauseWhenYouHave;
		obj.includeEquipped = includeEquipped;
		obj.includeTainted = includeTainted;
		obj.includeGroup = includeGroup;
		obj.hpRange = hpRange;
		obj.qualityRange = qualityRange;
		obj.limitToAllowedStuff = limitToAllowedStuff;
		obj.paused = paused;
		return obj;
	}
}
