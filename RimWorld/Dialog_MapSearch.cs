using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Dialog_MapSearch : Dialog_Search<Thing>
{
	private Map map;

	private static readonly Texture2D InventoryIcon = ContentFinder<Texture2D>.Get("UI/Widgets/SearchBar_Inventory");

	private static readonly Texture2D ContainerIcon = ContentFinder<Texture2D>.Get("UI/Widgets/SearchBar_Container");

	private List<Thing> tmpContents = new List<Thing>();

	protected override List<Thing> SearchSet => map.listerThings.AllThings;

	protected override bool ShouldClose
	{
		get
		{
			if (!WorldRendererUtility.WorldSelected)
			{
				return Find.CurrentMap != map;
			}
			return true;
		}
	}

	protected override TaggedString SearchLabel => "SearchTheMap".Translate();

	public Dialog_MapSearch(Map map)
	{
		this.map = map;
	}

	public override void PostOpen()
	{
		base.PostOpen();
		ThingListChangedCallbacks thingListChangedCallbacks = map.thingListChangedCallbacks;
		thingListChangedCallbacks.onThingAdded = (Action<Thing>)Delegate.Combine(thingListChangedCallbacks.onThingAdded, new Action<Thing>(TryAddElement));
		ThingListChangedCallbacks thingListChangedCallbacks2 = map.thingListChangedCallbacks;
		thingListChangedCallbacks2.onThingRemoved = (Action<Thing>)Delegate.Combine(thingListChangedCallbacks2.onThingRemoved, new Action<Thing>(TryRemoveElement));
	}

	public override void PostClose()
	{
		base.PostClose();
		ThingListChangedCallbacks thingListChangedCallbacks = map.thingListChangedCallbacks;
		thingListChangedCallbacks.onThingAdded = (Action<Thing>)Delegate.Remove(thingListChangedCallbacks.onThingAdded, new Action<Thing>(TryAddElement));
		ThingListChangedCallbacks thingListChangedCallbacks2 = map.thingListChangedCallbacks;
		thingListChangedCallbacks2.onThingRemoved = (Action<Thing>)Delegate.Remove(thingListChangedCallbacks2.onThingRemoved, new Action<Thing>(TryRemoveElement));
	}

	protected override void CheckAnyElementRemoved()
	{
		bool flag = false;
		for (int num = searchResults.Count - 1; num >= 0; num--)
		{
			Thing thing = searchResults.Values[num];
			if (thing == null || thing.Destroyed || thing.MapHeld != map || !ThingIsVisibleToPlayer(thing))
			{
				searchResults.RemoveAt(num);
				searchResultsSet.Remove(thing);
				flag = true;
			}
		}
		if (flag)
		{
			SetInitialSizeAndPosition();
		}
	}

	protected override void DoIcon(Thing element, Rect iconRect)
	{
		Widgets.ThingIcon(iconRect, element);
	}

	protected override void DoLabel(Thing element, Rect labelRect)
	{
		Widgets.Label(labelRect, element.LabelCap);
	}

	protected override void DoExtraIcon(Thing element, Rect iconRect)
	{
		if (element.ParentHolder?.ParentHolder is Pawn)
		{
			GUI.DrawTexture(iconRect.ContractedBy(2f), InventoryIcon);
		}
		else if (!(element.ParentHolder is Map))
		{
			GUI.DrawTexture(iconRect.ContractedBy(2f), ContainerIcon);
		}
	}

	protected override void ClikedOnElement(Thing element)
	{
		if (element == null || element.Destroyed || element.MapHeld != map)
		{
			return;
		}
		CameraJumper.TryJump(element);
		Find.Selector.ClearSelection();
		Thing obj = element;
		IThingHolder parentHolder = element.ParentHolder;
		if (parentHolder != null)
		{
			if (parentHolder.ParentHolder is Pawn { Corpse: var corpse } pawn)
			{
				obj = ((corpse == null) ? ((Thing)pawn) : ((Thing)corpse));
			}
			else if (parentHolder is Thing thing)
			{
				obj = thing;
			}
		}
		Find.Selector.Select(obj);
	}

	protected override bool ShouldSkipElement(Thing element)
	{
		if (element == null)
		{
			return true;
		}
		if (element is Corpse { Bugged: not false })
		{
			return true;
		}
		if (element is MinifiedThing { InnerThing: null })
		{
			return true;
		}
		return false;
	}

	protected override void OnHighlightUpdate(Thing element)
	{
		GenDraw.DrawArrowPointingAt(element.PositionHeld.ToVector3Shifted());
	}

	protected override void SetInitialSizeAndPosition()
	{
		scrollHeight = (float)searchResults.Count * 26f;
		Vector2 initialSize = InitialSize;
		initialSize.y = Mathf.Clamp(initialSize.y + scrollHeight, InitialSize.y, (float)UI.screenHeight / 2f);
		windowRect = new Rect((float)UI.screenWidth - initialSize.x, (float)UI.screenHeight - initialSize.y - 35f, initialSize.x, initialSize.y).Rounded();
	}

	private bool CanAddThing(Thing thing)
	{
		if (!quickSearchWidget.filter.Text.NullOrEmpty() && thing != null && thing.def.selectable && !thing.Destroyed && thing.def.showInSearch && thing.MapHeld == map && (DebugSettings.searchIgnoresRestrictions || !thing.PositionHeld.Fogged(thing.MapHeld)) && !searchResultsSet.Contains(thing) && !(thing is Corpse { Bugged: not false }) && (TextMatch(thing.LabelNoCount.StripTags()) || TextMatch(thing.def.label.StripTags())))
		{
			return ThingIsVisibleToPlayer(thing);
		}
		return false;
	}

	private bool ThingIsVisibleToPlayer(Thing thing)
	{
		if (DebugSettings.searchIgnoresRestrictions)
		{
			return true;
		}
		if (thing is Pawn pawn && pawn.IsHiddenFromPlayer())
		{
			return false;
		}
		IThingHolder parentHolder = thing.ParentHolder;
		if (parentHolder != null)
		{
			if (parentHolder is Pawn_InventoryTracker pawn_InventoryTracker && pawn_InventoryTracker.pawn.IsHiddenFromPlayer())
			{
				return false;
			}
			if (parentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker && pawn_EquipmentTracker.pawn.IsHiddenFromPlayer())
			{
				return false;
			}
			if (parentHolder is Pawn_ApparelTracker pawn_ApparelTracker && pawn_ApparelTracker.pawn.IsHiddenFromPlayer())
			{
				return false;
			}
			if (parentHolder is Pawn_CarryTracker pawn_CarryTracker && pawn_CarryTracker.pawn.IsHiddenFromPlayer())
			{
				return false;
			}
		}
		return true;
	}

	protected override void TryAddElement(Thing element)
	{
		bool flag = false;
		foreach (Thing item in ContentsFromThing(element))
		{
			searchResults.Add(item.LabelNoParenthesis.ToLower(), item);
			searchResultsSet.Add(item);
			flag = true;
		}
		if (flag)
		{
			SetInitialSizeAndPosition();
		}
	}

	protected override void TryRemoveElement(Thing thing)
	{
		bool flag = false;
		foreach (Thing item in ContentsFromThing(thing))
		{
			int num = searchResults.IndexOfValue(item);
			if (num >= 0)
			{
				searchResults.RemoveAt(num);
				searchResultsSet.Remove(item);
				flag = true;
			}
		}
		if (flag)
		{
			SetInitialSizeAndPosition();
		}
	}

	private List<Thing> ContentsFromThing(Thing thing)
	{
		tmpContents.Clear();
		if (CanAddThing(thing))
		{
			tmpContents.Add(thing);
		}
		if (!thing.Faction.IsPlayerSafe() && !(thing is Corpse))
		{
			return tmpContents;
		}
		if (thing is Corpse { Bugged: false } corpse)
		{
			thing = corpse.InnerPawn;
		}
		if (thing is ISearchableContents { SearchableContents: { } searchableContents2 })
		{
			foreach (Thing item in (IEnumerable<Thing>)searchableContents2)
			{
				if (CanAddThing(item))
				{
					tmpContents.Add(item);
				}
			}
		}
		if (thing is Pawn pawn && (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsAnimal || pawn.Corpse != null))
		{
			if (pawn.equipment != null)
			{
				foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading)
				{
					if (CanAddThing(item2))
					{
						tmpContents.Add(item2);
					}
				}
			}
			if (pawn.apparel != null)
			{
				foreach (Apparel item3 in pawn.apparel.WornApparel)
				{
					if (CanAddThing(item3))
					{
						tmpContents.Add(item3);
					}
				}
			}
			if (pawn.inventory != null)
			{
				foreach (Thing item4 in pawn.inventory.innerContainer)
				{
					if (CanAddThing(item4))
					{
						tmpContents.Add(item4);
					}
				}
			}
		}
		if (thing is ThingWithComps thingWithComps)
		{
			foreach (ThingComp allComp in thingWithComps.AllComps)
			{
				if (!(allComp is ISearchableContents { SearchableContents: { } searchableContents4 }))
				{
					continue;
				}
				foreach (Thing item5 in (IEnumerable<Thing>)searchableContents4)
				{
					if (CanAddThing(item5))
					{
						tmpContents.Add(item5);
					}
				}
			}
		}
		return tmpContents;
	}
}
