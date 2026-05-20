using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ContainingSelectionUtility
{
	private static readonly Dictionary<string, TaggedString> labelCache = new Dictionary<string, TaggedString>();

	public static Gizmo SelectCarriedThingGizmo(Thing container, Thing carriedThing)
	{
		if (container == null || carriedThing == null)
		{
			return null;
		}
		if (!CanSelect(carriedThing, container))
		{
			return null;
		}
		string label = GizmoLabel(container, carriedThing);
		string description = GizmoDescription(container, carriedThing);
		return CreateSelectGizmo(label, description, carriedThing, carriedThing);
	}

	public static Gizmo SelectContainingThingGizmo(Thing containedThing)
	{
		if (containedThing.ParentHolder is Corpse)
		{
			return null;
		}
		if (containedThing.ParentHolder is Pawn_CarryTracker pawn_CarryTracker)
		{
			return CreateSelectGizmo("CommandSelectCarryingPawn", "CommandSelectCarryingPawnDesc", pawn_CarryTracker.pawn);
		}
		if (containedThing.ParentHolder is Thing thingToSelect)
		{
			return CreateSelectGizmo("CommandSelectContainerThing", "CommandSelectContainerThingDesc", thingToSelect);
		}
		if (containedThing.ParentHolder is ThingComp thingComp)
		{
			return CreateSelectGizmo("CommandSelectContainerThing", "CommandSelectContainerThingDesc", thingComp.parent);
		}
		if (containedThing.Spawned && containedThing.def.category == ThingCategory.Item)
		{
			List<Thing> thingList = containedThing.Position.GetThingList(containedThing.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Building_Storage building_Storage)
				{
					return CreateSelectStorageGizmo("CommandSelectContainerThing".Translate(building_Storage), "CommandSelectContainerThingDesc".Translate(), building_Storage, building_Storage);
				}
			}
		}
		return null;
	}

	public static IEnumerable<Thing> SelectableContainedThings(Thing container)
	{
		if (container is Pawn pawn)
		{
			Thing thing = pawn.carryTracker?.CarriedThing;
			if (thing != null && CanSelect(thing, container))
			{
				yield return thing;
			}
		}
		else
		{
			if (container is MinifiedThing || !container.Faction.IsPlayerSafe())
			{
				yield break;
			}
			if (container is IThingHolder thingHolder)
			{
				foreach (Thing item in (IEnumerable<Thing>)thingHolder.GetDirectlyHeldThings())
				{
					if (CanSelect(item, container))
					{
						yield return item;
					}
				}
			}
			else
			{
				if (!(container is ThingWithComps thingWithComps))
				{
					yield break;
				}
				foreach (ThingComp allComp in thingWithComps.AllComps)
				{
					if (!(allComp is IThingHolder thingHolder2))
					{
						continue;
					}
					foreach (Thing item2 in (IEnumerable<Thing>)thingHolder2.GetDirectlyHeldThings())
					{
						if (CanSelect(item2, container))
						{
							yield return item2;
						}
					}
				}
			}
		}
	}

	private static bool CanSelect(Thing carriedThing, Thing container)
	{
		if (!carriedThing.def.selectable)
		{
			return false;
		}
		if (carriedThing is Pawn)
		{
			return container.def.containedPawnsSelectable;
		}
		return container.def.containedItemsSelectable;
	}

	private static Gizmo CreateSelectGizmo(string label, string description, Thing thingToSelect, Thing iconThing = null)
	{
		if (!thingToSelect.def.selectable)
		{
			return null;
		}
		float scale = 1f;
		float angle = 0f;
		Vector2 iconProportions = Vector2.one;
		Color color = Color.white;
		Texture icon;
		if (!(iconThing is Pawn pawn) || !(pawn.def.uiIcon == null))
		{
			icon = ((iconThing == null) ? ((thingToSelect is Pawn) ? TexCommand.SelectCarriedPawn : TexCommand.SelectCarriedThing) : Widgets.GetIconFor(iconThing, new Vector2(75f, 75f), iconThing.def.defaultPlacingRot, stackOfOne: true, out scale, out angle, out iconProportions, out color, out var _));
		}
		else
		{
			Vector2 size = new Vector2(75f, 75f);
			Rot4 south = Rot4.South;
			PawnHealthState? healthStateOverride = PawnHealthState.Mobile;
			icon = PortraitsCache.Get(pawn, size, south, default(Vector3), 1f, supersample: true, compensateForUIScale: true, renderHeadgear: true, renderClothes: true, null, null, stylingStation: false, healthStateOverride);
		}
		return new Command_SelectStorage
		{
			defaultLabel = label.Translate(thingToSelect).TruncateHeight(75f, 37.5f, labelCache),
			defaultDesc = description.Translate(),
			icon = icon,
			iconDrawScale = scale * 0.85f,
			iconAngle = angle,
			defaultIconColor = color,
			iconProportions = iconProportions,
			action = delegate
			{
				Find.Selector.ClearSelection();
				Find.Selector.Select(thingToSelect);
			}
		};
	}

	public static Gizmo CreateSelectStorageGizmo(string label, string description, Thing thingToSelect, Thing iconThing = null, bool groupable = true)
	{
		if (!thingToSelect.def.selectable)
		{
			return null;
		}
		float scale;
		float angle;
		Vector2 iconProportions;
		Color color;
		Material material;
		Texture iconFor = Widgets.GetIconFor(iconThing, new Vector2(75f, 75f), iconThing.def.defaultPlacingRot, stackOfOne: false, out scale, out angle, out iconProportions, out color, out material);
		Text.Font = GameFont.Tiny;
		Command_SelectStorage result = new Command_SelectStorage
		{
			defaultLabel = label.TruncateHeight(75f, 37.5f),
			defaultDesc = description,
			icon = iconFor,
			iconDrawScale = scale * 0.85f,
			iconAngle = angle,
			defaultIconColor = color,
			iconProportions = iconProportions,
			groupable = groupable,
			groupKey = thingToSelect.thingIDNumber,
			action = delegate
			{
				Find.Selector.ClearSelection();
				Find.Selector.Select(thingToSelect);
			}
		};
		Text.Font = GameFont.Small;
		return result;
	}

	private static string GizmoLabel(Thing container, Thing carriedThing)
	{
		if (container is Pawn)
		{
			if (carriedThing is Pawn)
			{
				return "CommandSelectCarriedPawn";
			}
			return "CommandSelectCarriedThing";
		}
		if (carriedThing is Pawn)
		{
			return "CommandSelectContainedPawn";
		}
		return "CommandSelectContainedThing";
	}

	private static string GizmoDescription(Thing container, Thing carriedThing)
	{
		if (container is Pawn)
		{
			if (carriedThing is Pawn)
			{
				return "CommandSelectCarriedPawnDesc";
			}
			return "CommandSelectCarriedThingDesc";
		}
		if (carriedThing is Pawn)
		{
			return "CommandSelectContainedPawnDesc";
		}
		return "CommandSelectContainedThingDesc";
	}
}
