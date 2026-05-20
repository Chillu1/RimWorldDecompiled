using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

public class WITab_Caravan_Gear : WITab
{
	private Vector2 leftPaneScrollPosition;

	private float leftPaneScrollViewHeight;

	private Vector2 rightPaneScrollPosition;

	private float rightPaneScrollViewHeight;

	private Thing draggedItem;

	private Vector2 draggedItemPosOffset;

	private bool droppedDraggedItem;

	private float leftPaneWidth;

	private float rightPaneWidth;

	private const float PawnRowHeight = 40f;

	private const float ItemRowHeight = 30f;

	private const float PawnLabelHeight = 18f;

	private const float PawnLabelColumnWidth = 100f;

	private const float GearLabelColumnWidth = 250f;

	private const float SpaceAroundIcon = 4f;

	private const float EquippedGearColumnWidth = 250f;

	private const float EquippedGearIconSize = 32f;

	private static List<Apparel> tmpApparel = new List<Apparel>();

	private static List<ThingWithComps> tmpExistingEquipment = new List<ThingWithComps>();

	private static List<Apparel> tmpExistingApparel = new List<Apparel>();

	private List<Pawn> Pawns => base.SelCaravan.PawnsListForReading;

	public WITab_Caravan_Gear()
	{
		labelKey = "TabCaravanGear";
	}

	protected override void UpdateSize()
	{
		base.UpdateSize();
		leftPaneWidth = 469f;
		rightPaneWidth = 345f;
		size.x = leftPaneWidth + rightPaneWidth;
		size.y = Mathf.Min(550f, PaneTopY - 30f);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		draggedItem = null;
	}

	protected override void FillTab()
	{
		Text.Font = GameFont.Small;
		CheckDraggedItemStillValid();
		CheckDropDraggedItem();
		Rect rect = new Rect(0f, 0f, leftPaneWidth, size.y);
		Widgets.BeginGroup(rect);
		DoLeftPane();
		Widgets.EndGroup();
		Widgets.BeginGroup(new Rect(rect.xMax, 0f, rightPaneWidth, size.y));
		DoRightPane();
		Widgets.EndGroup();
		if (draggedItem != null && droppedDraggedItem)
		{
			droppedDraggedItem = false;
			draggedItem = null;
		}
	}

	private void DoLeftPane()
	{
		Rect rect = new Rect(0f, 0f, leftPaneWidth, size.y).ContractedBy(10f);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, leftPaneScrollViewHeight);
		float curY = 0f;
		Widgets.BeginScrollView(rect, ref leftPaneScrollPosition, rect2);
		DoPawnRows(ref curY, rect2, rect);
		if (Event.current.type == EventType.Layout)
		{
			leftPaneScrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}

	private void DoRightPane()
	{
		Rect rect = new Rect(0f, 0f, rightPaneWidth, size.y).ContractedBy(10f);
		Rect rect2 = new Rect(0f, 0f, rect.width - 16f, rightPaneScrollViewHeight);
		if (draggedItem != null && rect.Contains(Event.current.mousePosition) && CurrentWearerOf(draggedItem) != null)
		{
			Widgets.DrawHighlight(rect);
			if (droppedDraggedItem)
			{
				MoveDraggedItemToInventory();
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			}
		}
		float curY = 0f;
		Widgets.BeginScrollView(rect, ref rightPaneScrollPosition, rect2);
		DoInventoryRows(ref curY, rect2, rect);
		if (Event.current.type == EventType.Layout)
		{
			rightPaneScrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}

	protected override void ExtraOnGUI()
	{
		base.ExtraOnGUI();
		if (draggedItem != null)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			Rect rect = new Rect(mousePosition.x - draggedItemPosOffset.x, mousePosition.y - draggedItemPosOffset.y, 32f, 32f);
			Find.WindowStack.ImmediateWindow(1283641090, rect, WindowLayer.Super, delegate
			{
				if (draggedItem != null)
				{
					Widgets.ThingIcon(rect.AtZero(), draggedItem);
				}
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}
		CheckDropDraggedItem();
	}

	private void DoPawnRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
	{
		List<Pawn> pawns = Pawns;
		Text.Font = GameFont.Tiny;
		GUI.color = Color.gray;
		Widgets.Label(new Rect(135f, curY + 6f, 200f, 100f), "DragToRearrange".Translate());
		GUI.color = Color.white;
		Text.Font = GameFont.Small;
		Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanColonists".Translate());
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			if (pawn.IsColonist)
			{
				DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn);
			}
		}
		bool flag = false;
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn2 = pawns[j];
			if (pawn2.IsPrisoner)
			{
				if (!flag)
				{
					Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanPrisoners".Translate());
					flag = true;
				}
				DoPawnRow(ref curY, scrollViewRect, scrollOutRect, pawn2);
			}
		}
	}

	private void DoPawnRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn p)
	{
		float num = leftPaneScrollPosition.y - 40f;
		float num2 = leftPaneScrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoPawnRow(new Rect(0f, curY, viewRect.width, 40f), p);
		}
		curY += 40f;
	}

	private void DoPawnRow(Rect rect, Pawn p)
	{
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		CaravanThingsTabUtility.DoAbandonButton(rect2, p, base.SelCaravan);
		rect2.width -= 24f;
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, p);
		rect2.width -= 24f;
		bool flag = draggedItem != null && rect2.Contains(Event.current.mousePosition) && CurrentWearerOf(draggedItem) != p;
		if ((Mouse.IsOver(rect2) && draggedItem == null) || flag)
		{
			Widgets.DrawHighlight(rect2);
		}
		if (flag && droppedDraggedItem)
		{
			TryEquipDraggedItem(p);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
		}
		Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect3, p);
		Rect bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
		GenMapUI.DrawPawnLabel(p, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
		float curX = bgRect.xMax;
		if (p.equipment != null)
		{
			List<ThingWithComps> allEquipmentListForReading = p.equipment.AllEquipmentListForReading;
			for (int i = 0; i < allEquipmentListForReading.Count; i++)
			{
				DoEquippedGear(allEquipmentListForReading[i], p, ref curX);
			}
		}
		if (p.apparel != null)
		{
			tmpApparel.Clear();
			tmpApparel.AddRange(p.apparel.WornApparel);
			tmpApparel.SortBy((Apparel x) => x.def.apparel.LastLayer.drawOrder, (Apparel x) => 0f - x.def.apparel.HumanBodyCoverage);
			for (int num = 0; num < tmpApparel.Count; num++)
			{
				DoEquippedGear(tmpApparel[num], p, ref curX);
			}
		}
		if (p.Downed && !p.ageTracker.CurLifeStage.alwaysDowned)
		{
			GUI.color = new Color(1f, 0f, 0f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
			GUI.color = Color.white;
		}
		Widgets.EndGroup();
	}

	private void DoInventoryRows(ref float curY, Rect scrollViewRect, Rect scrollOutRect)
	{
		List<Thing> list = CaravanInventoryUtility.AllInventoryItems(base.SelCaravan);
		Widgets.ListSeparator(ref curY, scrollViewRect.width, "CaravanWeaponsAndApparel".Translate());
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (IsVisibleWeapon(thing.def))
			{
				if (!flag)
				{
					flag = true;
				}
				DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing);
			}
		}
		bool flag2 = false;
		for (int j = 0; j < list.Count; j++)
		{
			Thing thing2 = list[j];
			if (thing2.def.IsApparel)
			{
				if (!flag2)
				{
					flag2 = true;
				}
				DoInventoryRow(ref curY, scrollViewRect, scrollOutRect, thing2);
			}
		}
		if (!flag && !flag2)
		{
			Widgets.NoneLabel(ref curY, scrollViewRect.width);
		}
	}

	private void DoInventoryRow(ref float curY, Rect viewRect, Rect scrollOutRect, Thing t)
	{
		float num = rightPaneScrollPosition.y - 30f;
		float num2 = rightPaneScrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoInventoryRow(new Rect(0f, curY, viewRect.width, 30f), t);
		}
		curY += 30f;
	}

	private void DoInventoryRow(Rect rect, Thing t)
	{
		Widgets.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, t);
		rect2.width -= 24f;
		if (draggedItem == null && Mouse.IsOver(rect2))
		{
			Widgets.DrawHighlight(rect2);
		}
		float num = ((t == draggedItem) ? 0.5f : 1f);
		Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect3, t, num);
		GUI.color = new Color(1f, 1f, 1f, num);
		Rect rect4 = new Rect(rect3.xMax + 4f, 0f, 250f, 30f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Widgets.Label(rect4, t.LabelCap);
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		GUI.color = Color.white;
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect2))
		{
			draggedItem = t;
			droppedDraggedItem = false;
			draggedItemPosOffset = new Vector2(16f, 16f);
			Event.current.Use();
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
		Widgets.EndGroup();
	}

	private void DoEquippedGear(Thing t, Pawn p, ref float curX)
	{
		Rect rect = new Rect(curX, 4f, 32f, 32f);
		bool flag = Mouse.IsOver(rect);
		Widgets.ThingIcon(alpha: (t == draggedItem) ? 0.2f : ((!flag || draggedItem != null) ? 1f : 0.75f), rect: rect, thing: t);
		curX += 32f;
		if (Mouse.IsOver(rect))
		{
			TooltipHandler.TipRegion(rect, t.LabelCap);
		}
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && flag)
		{
			draggedItem = t;
			droppedDraggedItem = false;
			draggedItemPosOffset = Event.current.mousePosition - rect.position;
			Event.current.Use();
			SoundDefOf.Click.PlayOneShotOnCamera();
		}
	}

	private void CheckDraggedItemStillValid()
	{
		if (draggedItem != null)
		{
			if (draggedItem.Destroyed)
			{
				draggedItem = null;
			}
			else if (CurrentWearerOf(draggedItem) == null && !CaravanInventoryUtility.AllInventoryItems(base.SelCaravan).Contains(draggedItem))
			{
				draggedItem = null;
			}
		}
	}

	private void CheckDropDraggedItem()
	{
		if (draggedItem != null && (Event.current.type == EventType.MouseUp || Event.current.rawType == EventType.MouseUp))
		{
			droppedDraggedItem = true;
		}
	}

	private bool IsVisibleWeapon(ThingDef t)
	{
		if (t.IsWeapon && t != ThingDefOf.WoodLog)
		{
			return t != ThingDefOf.Beer;
		}
		return false;
	}

	private Pawn CurrentWearerOf(Thing t)
	{
		IThingHolder parentHolder = t.ParentHolder;
		if (parentHolder is Pawn_EquipmentTracker || parentHolder is Pawn_ApparelTracker)
		{
			return (Pawn)parentHolder.ParentHolder;
		}
		return null;
	}

	private void MoveDraggedItemToInventory()
	{
		droppedDraggedItem = false;
		if (draggedItem is Apparel apparel && CurrentWearerOf(apparel) != null && CurrentWearerOf(apparel).apparel.IsLocked(apparel))
		{
			Messages.Message("MessageCantUnequipLockedApparel".Translate(), CurrentWearerOf(apparel), MessageTypeDefOf.RejectInput, historical: false);
			draggedItem = null;
			return;
		}
		Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(draggedItem, Pawns, null);
		if (pawn != null)
		{
			draggedItem.holdingOwner.TryTransferToContainer(draggedItem, pawn.inventory.innerContainer, 1);
		}
		else
		{
			Log.Warning("Could not find any pawn to move " + draggedItem?.ToString() + " to.");
		}
		draggedItem = null;
	}

	private void TryEquipDraggedItem(Pawn p)
	{
		droppedDraggedItem = false;
		if (!EquipmentUtility.CanEquip(draggedItem, p, out var cantReason))
		{
			Messages.Message("MessageCantEquipCustom".Translate(cantReason.CapitalizeFirst()), p, MessageTypeDefOf.RejectInput, historical: false);
			draggedItem = null;
			return;
		}
		if (draggedItem.def.IsWeapon)
		{
			if (p.guest.IsPrisoner)
			{
				Messages.Message("MessageCantEquipCustom".Translate("MessagePrisonerCannotEquipWeapon".Translate(p.Named("PAWN"))), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			if (p.WorkTagIsDisabled(WorkTags.Violent))
			{
				Messages.Message("MessageCantEquipIncapableOfViolence".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			if (p.WorkTagIsDisabled(WorkTags.Shooting) && draggedItem.def.IsRangedWeapon)
			{
				Messages.Message("MessageCantEquipIncapableOfShooting".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				Messages.Message("MessageCantEquipIncapableOfManipulation".Translate(), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
		}
		Apparel apparel = draggedItem as Apparel;
		ThingWithComps thingWithComps = draggedItem as ThingWithComps;
		if (apparel != null && p.apparel != null)
		{
			if (!ApparelUtility.HasPartsToWear(p, apparel.def))
			{
				Messages.Message("MessageCantWearApparelMissingBodyParts".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			if (CurrentWearerOf(apparel) != null && CurrentWearerOf(apparel).apparel.IsLocked(apparel))
			{
				Messages.Message("MessageCantUnequipLockedApparel".Translate(), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			if (p.apparel.WouldReplaceLockedApparel(apparel))
			{
				Messages.Message("MessageWouldReplaceLockedApparel".Translate(p.LabelShort, p), p, MessageTypeDefOf.RejectInput, historical: false);
				draggedItem = null;
				return;
			}
			tmpExistingApparel.Clear();
			tmpExistingApparel.AddRange(p.apparel.WornApparel);
			for (int i = 0; i < tmpExistingApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(apparel.def, tmpExistingApparel[i].def, p.RaceProps.body))
				{
					p.apparel.Remove(tmpExistingApparel[i]);
					Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(tmpExistingApparel[i], Pawns, null);
					if (pawn != null)
					{
						pawn.inventory.innerContainer.TryAdd(tmpExistingApparel[i]);
						continue;
					}
					Log.Warning("Could not find any pawn to move " + tmpExistingApparel[i]?.ToString() + " to.");
					tmpExistingApparel[i].Destroy();
				}
			}
			p.apparel.Wear((Apparel)apparel.SplitOff(1), dropReplacedApparel: false);
			if (p.outfits != null)
			{
				p.outfits.forcedHandler.SetForced(apparel, forced: true);
			}
		}
		else if (thingWithComps != null && p.equipment != null)
		{
			string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(draggedItem, p);
			if (!personaWeaponConfirmationText.NullOrEmpty())
			{
				_ = draggedItem;
				Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate
				{
					AddEquipment();
				}, "No".Translate()));
				draggedItem = null;
				return;
			}
			AddEquipment();
		}
		else
		{
			Log.Warning("Could not make " + p?.ToString() + " equip or wear " + draggedItem);
		}
		draggedItem = null;
		void AddEquipment()
		{
			tmpExistingEquipment.Clear();
			tmpExistingEquipment.AddRange(p.equipment.AllEquipmentListForReading);
			for (int j = 0; j < tmpExistingEquipment.Count; j++)
			{
				p.equipment.Remove(tmpExistingEquipment[j]);
				Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(tmpExistingEquipment[j], Pawns, null);
				if (pawn2 != null)
				{
					pawn2.inventory.innerContainer.TryAdd(tmpExistingEquipment[j]);
				}
				else
				{
					Log.Warning("Could not find any pawn to move " + tmpExistingEquipment[j]?.ToString() + " to.");
					tmpExistingEquipment[j].Destroy();
				}
			}
			p.equipment.AddEquipment((ThingWithComps)thingWithComps.SplitOff(1));
		}
	}
}
