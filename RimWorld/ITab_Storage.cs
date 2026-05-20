using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ITab_Storage : ITab
{
	private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

	private static readonly Vector2 WinSize = new Vector2(300f, 480f);

	protected virtual IStoreSettingsParent SelStoreSettingsParent
	{
		get
		{
			if (base.SelObject is Thing t)
			{
				IStoreSettingsParent thingOrThingCompStoreSettingsParent = GetThingOrThingCompStoreSettingsParent(t);
				if (thingOrThingCompStoreSettingsParent != null)
				{
					return thingOrThingCompStoreSettingsParent;
				}
				return null;
			}
			if (base.AllSelObjects.Count > 1)
			{
				bool flag = true;
				StorageGroup storageGroup = (base.AllSelObjects.First() as IStorageGroupMember)?.Group;
				if (storageGroup != null)
				{
					foreach (object allSelObject in base.AllSelObjects)
					{
						if (!(allSelObject is IStorageGroupMember storageGroupMember))
						{
							flag = false;
							break;
						}
						if (storageGroupMember.Group != storageGroup)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return (base.AllSelObjects.First() as IStorageGroupMember)?.Group;
					}
				}
				return null;
			}
			return base.SelObject as IStoreSettingsParent;
		}
	}

	public override bool IsVisible
	{
		get
		{
			if (base.SelObject != null)
			{
				if (base.SelObject is Thing { Faction: not null } thing && thing.Faction != Faction.OfPlayer)
				{
					return false;
				}
			}
			else
			{
				if (base.AllSelObjects.Count <= 1)
				{
					return false;
				}
				foreach (object allSelObject in base.AllSelObjects)
				{
					if (allSelObject is Thing { Faction: not null } thing2 && thing2.Faction != Faction.OfPlayer)
					{
						return false;
					}
				}
			}
			return SelStoreSettingsParent?.StorageTabVisible ?? false;
		}
	}

	protected virtual bool IsPrioritySettingVisible => true;

	private float TopAreaHeight => IsPrioritySettingVisible ? 35 : 20;

	public ITab_Storage()
	{
		size = WinSize;
		labelKey = "TabStorage";
		tutorTag = "Storage";
	}

	public override void OnOpen()
	{
		base.OnOpen();
		thingFilterState.quickSearch.Reset();
	}

	protected override void FillTab()
	{
		IStoreSettingsParent storeSettingsParent = SelStoreSettingsParent;
		StorageSettings settings = storeSettingsParent.GetStoreSettings();
		Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
		Widgets.BeginGroup(rect);
		if (IsPrioritySettingVisible)
		{
			Text.Font = GameFont.Small;
			Rect rect2 = new Rect(0f, 0f, 160f, TopAreaHeight - 6f);
			if (Widgets.ButtonText(rect2, "Priority".Translate() + ": " + settings.Priority.Label().CapitalizeFirst()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (StoragePriority value in Enum.GetValues(typeof(StoragePriority)))
				{
					if (value != StoragePriority.Unstored)
					{
						StoragePriority localPr = value;
						list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
						{
							settings.Priority = localPr;
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			UIHighlighter.HighlightOpportunity(rect2, "StoragePriority");
		}
		ThingFilter parentFilter = null;
		if (storeSettingsParent.GetParentStoreSettings() != null)
		{
			parentFilter = storeSettingsParent.GetParentStoreSettings().filter;
		}
		Rect rect3 = new Rect(0f, TopAreaHeight, rect.width, rect.height - TopAreaHeight);
		Bill[] first = (from b in BillUtility.GlobalBills()
			where b is Bill_Production && b.GetSlotGroup() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStore((Bill_Production)b, b.GetSlotGroup())
			select b).ToArray();
		ThingFilterUI.DoThingFilterConfigWindow(rect3, thingFilterState, settings.filter, parentFilter, 8, null, HiddenSpecialThingFilters());
		Bill[] second = (from b in BillUtility.GlobalBills()
			where b is Bill_Production && b.GetSlotGroup() == storeSettingsParent && b.recipe.WorkerCounter.CanPossiblyStore((Bill_Production)b, b.GetSlotGroup())
			select b).ToArray();
		foreach (Bill item in first.Except(second))
		{
			Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(item.LabelCap, item.billStack.billGiver.LabelShort.CapitalizeFirst(), SlotGroup.GetGroupLabel(item.GetSlotGroup())), item.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
		}
		PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
		Widgets.EndGroup();
	}

	protected IStoreSettingsParent GetThingOrThingCompStoreSettingsParent(Thing t)
	{
		if (t is IStoreSettingsParent result)
		{
			return result;
		}
		if (t is ThingWithComps { AllComps: var allComps })
		{
			for (int i = 0; i < allComps.Count; i++)
			{
				if (allComps[i] is IStoreSettingsParent result2)
				{
					return result2;
				}
			}
		}
		return null;
	}

	public override void Notify_ClickOutsideWindow()
	{
		base.Notify_ClickOutsideWindow();
		thingFilterState.quickSearch.Unfocus();
	}

	private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
	{
		if (ModsConfig.IdeologyActive)
		{
			yield return SpecialThingFilterDefOf.AllowVegetarian;
			yield return SpecialThingFilterDefOf.AllowCarnivore;
			yield return SpecialThingFilterDefOf.AllowCannibal;
			yield return SpecialThingFilterDefOf.AllowInsectMeat;
		}
	}
}
