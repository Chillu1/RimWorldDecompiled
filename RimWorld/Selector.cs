using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Selector
	{
		public DragBox dragBox = new DragBox();

		private List<object> selected = new List<object>();

		private static List<string> cantTakeReasons = new List<string>();

		private const float PawnSelectRadius = 1f;

		private const int MaxNumSelected = 200;

		private bool ShiftIsHeld
		{
			get
			{
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					return Input.GetKey(KeyCode.RightShift);
				}
				return true;
			}
		}

		public List<object> SelectedObjects => selected;

		public List<object> SelectedObjectsListForReading => selected;

		public Thing SingleSelectedThing
		{
			get
			{
				if (selected.Count != 1)
				{
					return null;
				}
				if (selected[0] is Thing)
				{
					return (Thing)selected[0];
				}
				return null;
			}
		}

		public object FirstSelectedObject
		{
			get
			{
				if (selected.Count == 0)
				{
					return null;
				}
				return selected[0];
			}
		}

		public object SingleSelectedObject
		{
			get
			{
				if (selected.Count != 1)
				{
					return null;
				}
				return selected[0];
			}
		}

		public int NumSelected => selected.Count;

		public Zone SelectedZone
		{
			get
			{
				if (selected.Count == 0)
				{
					return null;
				}
				return selected[0] as Zone;
			}
			set
			{
				ClearSelection();
				if (value != null)
				{
					Select(value);
				}
			}
		}

		public void SelectorOnGUI()
		{
			HandleMapClicks();
			if (KeyBindingDefOf.Cancel.KeyDownEvent && selected.Count > 0)
			{
				ClearSelection();
				Event.current.Use();
			}
			if (NumSelected > 0 && Find.MainTabsRoot.OpenTab == null && !WorldRendererUtility.WorldRenderedNow)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Inspect, playSound: false);
			}
		}

		private void HandleMapClicks()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (Event.current.clickCount == 1)
					{
						dragBox.active = true;
						dragBox.start = UI.MouseMapPosition();
					}
					if (Event.current.clickCount == 2)
					{
						SelectAllMatchingObjectUnderMouseOnScreen();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && selected.Count > 0)
				{
					if (selected.Count == 1 && selected[0] is Pawn)
					{
						FloatMenuMakerMap.TryMakeFloatMenu((Pawn)selected[0]);
					}
					else
					{
						cantTakeReasons.Clear();
						for (int i = 0; i < selected.Count; i++)
						{
							Pawn pawn = selected[i] as Pawn;
							if (pawn != null)
							{
								MassTakeFirstAutoTakeableOption_NewTemp(pawn, UI.MouseCell(), out string cantTakeReason);
								if (cantTakeReason != null)
								{
									cantTakeReasons.Add(cantTakeReason);
								}
							}
						}
						if (cantTakeReasons.Count == selected.Count)
						{
							FloatMenu window = new FloatMenu((from r in cantTakeReasons.Distinct()
								select new FloatMenuOption(r, null)).ToList());
							Find.WindowStack.Add(window);
						}
					}
					Event.current.Use();
				}
			}
			if (Event.current.rawType != EventType.MouseUp)
			{
				return;
			}
			if (Event.current.button == 0 && dragBox.active)
			{
				dragBox.active = false;
				if (!dragBox.IsValid)
				{
					SelectUnderMouse();
				}
				else
				{
					SelectInsideDragBox();
				}
			}
			Event.current.Use();
		}

		public bool IsSelected(object obj)
		{
			return selected.Contains(obj);
		}

		public void ClearSelection()
		{
			SelectionDrawer.Clear();
			selected.Clear();
		}

		public void Deselect(object obj)
		{
			if (selected.Contains(obj))
			{
				selected.Remove(obj);
			}
		}

		public void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
		{
			if (obj == null)
			{
				Log.Error("Cannot select null.");
				return;
			}
			Thing thing = obj as Thing;
			if (thing == null && !(obj is Zone))
			{
				Log.Error(string.Concat("Tried to select ", obj, " which is neither a Thing nor a Zone."));
				return;
			}
			if (thing != null && thing.Destroyed)
			{
				Log.Error("Cannot select destroyed thing.");
				return;
			}
			Pawn pawn = obj as Pawn;
			if (pawn != null && pawn.IsWorldPawn())
			{
				Log.Error("Cannot select world pawns.");
				return;
			}
			if (forceDesignatorDeselect)
			{
				Find.DesignatorManager.Deselect();
			}
			if (SelectedZone != null && !(obj is Zone))
			{
				ClearSelection();
			}
			if (obj is Zone && SelectedZone == null)
			{
				ClearSelection();
			}
			Map map = (thing != null) ? thing.Map : ((Zone)obj).Map;
			for (int num = selected.Count - 1; num >= 0; num--)
			{
				Thing thing2 = selected[num] as Thing;
				if (((thing2 != null) ? thing2.Map : ((Zone)selected[num]).Map) != map)
				{
					Deselect(selected[num]);
				}
			}
			if (selected.Count < 200 && !IsSelected(obj))
			{
				if (map != Find.CurrentMap)
				{
					Current.Game.CurrentMap = map;
					SoundDefOf.MapSelected.PlayOneShotOnCamera();
					IntVec3 cell = thing?.Position ?? ((Zone)obj).Cells[0];
					Find.CameraDriver.JumpToCurrentMapLoc(cell);
				}
				if (playSound)
				{
					PlaySelectionSoundFor(obj);
				}
				selected.Add(obj);
				SelectionDrawer.Notify_Selected(obj);
			}
		}

		public void Notify_DialogOpened()
		{
			dragBox.active = false;
		}

		private void PlaySelectionSoundFor(object obj)
		{
			if (obj is Pawn && ((Pawn)obj).Faction == Faction.OfPlayer && ((Pawn)obj).RaceProps.Humanlike)
			{
				SoundDefOf.ColonistSelected.PlayOneShotOnCamera();
			}
			else if (obj is Thing || obj is Zone)
			{
				SoundDefOf.ThingSelected.PlayOneShotOnCamera();
			}
			else
			{
				Log.Warning("Can't determine selection sound for " + obj);
			}
		}

		private void SelectInsideDragBox()
		{
			if (!ShiftIsHeld)
			{
				ClearSelection();
			}
			bool selectedSomething = false;
			List<Thing> list = Find.ColonistBar.MapColonistsOrCorpsesInScreenRect(dragBox.ScreenRect);
			for (int i = 0; i < list.Count; i++)
			{
				selectedSomething = true;
				Select(list[i]);
			}
			if (selectedSomething)
			{
				return;
			}
			List<Caravan> list2 = Find.ColonistBar.CaravanMembersCaravansInScreenRect(dragBox.ScreenRect);
			for (int j = 0; j < list2.Count; j++)
			{
				if (!selectedSomething)
				{
					CameraJumper.TryJumpAndSelect(list2[j]);
					selectedSomething = true;
				}
				else
				{
					Find.WorldSelector.Select(list2[j]);
				}
			}
			if (selectedSomething)
			{
				return;
			}
			List<Thing> boxThings = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(dragBox.ScreenRect).ToList();
			Func<Predicate<Thing>, bool> func = delegate(Predicate<Thing> predicate)
			{
				foreach (Thing item in boxThings.Where((Thing t) => predicate(t)))
				{
					Select(item);
					selectedSomething = true;
				}
				return selectedSomething;
			};
			Predicate<Thing> arg = (Thing t) => t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike && t.Faction == Faction.OfPlayer;
			if (func(arg))
			{
				return;
			}
			Predicate<Thing> arg2 = (Thing t) => t.def.category == ThingCategory.Pawn && ((Pawn)t).RaceProps.Humanlike;
			if (func(arg2))
			{
				return;
			}
			Predicate<Thing> arg3 = (Thing t) => t.def.CountAsResource;
			if (func(arg3))
			{
				return;
			}
			Predicate<Thing> arg4 = (Thing t) => t.def.category == ThingCategory.Pawn;
			if (func(arg4) || func((Thing t) => t.def.selectable))
			{
				return;
			}
			foreach (Zone item2 in ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(dragBox.ScreenRect).ToList())
			{
				selectedSomething = true;
				Select(item2);
			}
			if (!selectedSomething)
			{
				SelectUnderMouse();
			}
		}

		private IEnumerable<object> SelectableObjectsUnderMouse()
		{
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			Thing thing = Find.ColonistBar.ColonistOrCorpseAt(mousePositionOnUIInverted);
			if (thing != null && thing.Spawned)
			{
				yield return thing;
			}
			else
			{
				if (!UI.MouseCell().InBounds(Find.CurrentMap))
				{
					yield break;
				}
				TargetingParameters targetingParameters = new TargetingParameters();
				targetingParameters.mustBeSelectable = true;
				targetingParameters.canTargetPawns = true;
				targetingParameters.canTargetBuildings = true;
				targetingParameters.canTargetItems = true;
				targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
				List<Thing> selectableList = GenUI.ThingsUnderMouse(UI.MouseMapPosition(), 1f, targetingParameters);
				if (selectableList.Count > 0 && selectableList[0] is Pawn && (selectableList[0].DrawPos - UI.MouseMapPosition()).MagnitudeHorizontal() < 0.4f)
				{
					for (int num = selectableList.Count - 1; num >= 0; num--)
					{
						Thing thing2 = selectableList[num];
						if (thing2.def.category == ThingCategory.Pawn && (thing2.DrawPos - UI.MouseMapPosition()).MagnitudeHorizontal() > 0.4f)
						{
							selectableList.Remove(thing2);
						}
					}
				}
				for (int i = 0; i < selectableList.Count; i++)
				{
					yield return selectableList[i];
				}
				Zone zone = Find.CurrentMap.zoneManager.ZoneAt(UI.MouseCell());
				if (zone != null)
				{
					yield return zone;
				}
			}
		}

		public static IEnumerable<object> SelectableObjectsAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (ThingSelectionUtility.SelectableByMapClick(thing))
				{
					yield return thing;
				}
			}
			Zone zone = map.zoneManager.ZoneAt(c);
			if (zone != null)
			{
				yield return zone;
			}
		}

		private void SelectUnderMouse()
		{
			Caravan caravan = Find.ColonistBar.CaravanMemberCaravanAt(UI.MousePositionOnUIInverted);
			if (caravan != null)
			{
				CameraJumper.TryJumpAndSelect(caravan);
				return;
			}
			Thing thing = Find.ColonistBar.ColonistOrCorpseAt(UI.MousePositionOnUIInverted);
			if (thing != null && !thing.Spawned)
			{
				CameraJumper.TryJump(thing);
				return;
			}
			List<object> list = SelectableObjectsUnderMouse().ToList();
			if (list.Count == 0)
			{
				if (!ShiftIsHeld)
				{
					ClearSelection();
				}
			}
			else if (list.Count == 1)
			{
				object obj2 = list[0];
				if (!ShiftIsHeld)
				{
					ClearSelection();
					Select(obj2);
				}
				else if (!selected.Contains(obj2))
				{
					Select(obj2);
				}
				else
				{
					Deselect(obj2);
				}
			}
			else
			{
				if (list.Count <= 1)
				{
					return;
				}
				object obj3 = list.Where((object obj) => selected.Contains(obj)).FirstOrDefault();
				if (obj3 != null)
				{
					if (!ShiftIsHeld)
					{
						int num = list.IndexOf(obj3) + 1;
						if (num >= list.Count)
						{
							num -= list.Count;
						}
						ClearSelection();
						Select(list[num]);
						return;
					}
					foreach (object item in list)
					{
						if (selected.Contains(item))
						{
							Deselect(item);
						}
					}
				}
				else
				{
					if (!ShiftIsHeld)
					{
						ClearSelection();
					}
					Select(list[0]);
				}
			}
		}

		public void SelectNextAt(IntVec3 c, Map map)
		{
			if (SelectedObjects.Count() != 1)
			{
				Log.Error("Cannot select next at with < or > 1 selected.");
				return;
			}
			List<object> list = SelectableObjectsAt(c, map).ToList();
			int num = list.IndexOf(SingleSelectedThing) + 1;
			if (num >= list.Count)
			{
				num -= list.Count;
			}
			ClearSelection();
			Select(list[num]);
		}

		private void SelectAllMatchingObjectUnderMouseOnScreen()
		{
			List<object> list = SelectableObjectsUnderMouse().ToList();
			if (list.Count == 0)
			{
				return;
			}
			Thing clickedThing = list.FirstOrDefault((object o) => o is Pawn && ((Pawn)o).Faction == Faction.OfPlayer && !((Pawn)o).IsPrisoner) as Thing;
			clickedThing = (list.FirstOrDefault((object o) => o is Pawn) as Thing);
			if (clickedThing == null)
			{
				clickedThing = (list.Where((object o) => o is Thing && !((Thing)o).def.neverMultiSelect).FirstOrDefault() as Thing);
			}
			Rect rect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
			if (clickedThing == null)
			{
				if (list.FirstOrDefault((object o) => o is Zone && ((Zone)o).IsMultiselectable) == null)
				{
					return;
				}
				foreach (Zone item in ThingSelectionUtility.MultiSelectableZonesInScreenRectDistinct(rect))
				{
					if (!IsSelected(item))
					{
						Select(item);
					}
				}
				return;
			}
			IEnumerable<Thing> enumerable = ThingSelectionUtility.MultiSelectableThingsInScreenRectDistinct(rect);
			Predicate<Thing> predicate = delegate(Thing t)
			{
				if (t.def != clickedThing.GetInnerIfMinified().def || t.Faction != clickedThing.Faction || IsSelected(t))
				{
					return false;
				}
				Pawn pawn = clickedThing as Pawn;
				if (pawn != null)
				{
					Pawn pawn2 = t as Pawn;
					if (pawn2.RaceProps != pawn.RaceProps)
					{
						return false;
					}
					if (pawn2.HostFaction != pawn.HostFaction)
					{
						return false;
					}
				}
				return true;
			};
			foreach (Thing item2 in (IEnumerable)enumerable)
			{
				if (predicate(item2.GetInnerIfMinified()))
				{
					Select(item2);
				}
			}
		}

		[Obsolete("Obsolete, only used to avoid error when patching")]
		private static void MassTakeFirstAutoTakeableOption(Pawn pawn, IntVec3 dest)
		{
			MassTakeFirstAutoTakeableOption_NewTemp(pawn, dest, out string _);
		}

		private static void MassTakeFirstAutoTakeableOption_NewTemp(Pawn pawn, IntVec3 dest, out string cantTakeReason)
		{
			FloatMenuOption floatMenuOption = null;
			cantTakeReason = null;
			foreach (FloatMenuOption item in FloatMenuMakerMap.ChoicesAtFor(dest.ToVector3Shifted(), pawn))
			{
				if (item.Disabled || !item.autoTakeable)
				{
					cantTakeReason = item.Label;
				}
				else if (floatMenuOption == null || item.autoTakeablePriority > floatMenuOption.autoTakeablePriority)
				{
					floatMenuOption = item;
				}
			}
			floatMenuOption?.Chosen(colonistOrdering: true, null);
		}
	}
}
