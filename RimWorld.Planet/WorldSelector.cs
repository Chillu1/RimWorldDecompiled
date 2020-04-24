using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WorldSelector
	{
		public WorldDragBox dragBox = new WorldDragBox();

		private List<WorldObject> selected = new List<WorldObject>();

		public int selectedTile = -1;

		private const int MaxNumSelected = 80;

		private const float MaxDragBoxDiagonalToSelectTile = 30f;

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

		public List<WorldObject> SelectedObjects => selected;

		public WorldObject SingleSelectedObject
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

		public WorldObject FirstSelectedObject
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

		public int NumSelectedObjects => selected.Count;

		public bool AnyObjectOrTileSelected
		{
			get
			{
				if (NumSelectedObjects == 0)
				{
					return selectedTile >= 0;
				}
				return true;
			}
		}

		public void WorldSelectorOnGUI()
		{
			HandleWorldClicks();
			if (KeyBindingDefOf.Cancel.KeyDownEvent && selected.Count > 0)
			{
				ClearSelection();
				Event.current.Use();
			}
		}

		private void HandleWorldClicks()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (Event.current.clickCount == 1)
					{
						dragBox.active = true;
						dragBox.start = UI.MousePositionOnUIInverted;
					}
					if (Event.current.clickCount == 2)
					{
						SelectAllMatchingObjectUnderMouseOnScreen();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && selected.Count > 0)
				{
					if (selected.Count == 1 && selected[0] is Caravan)
					{
						Caravan caravan = (Caravan)selected[0];
						if (caravan.IsPlayerControlled && !FloatMenuMakerWorld.TryMakeFloatMenu(caravan))
						{
							AutoOrderToTile(caravan, GenWorld.MouseTile());
						}
					}
					else
					{
						for (int i = 0; i < selected.Count; i++)
						{
							Caravan caravan2 = selected[i] as Caravan;
							if (caravan2 != null && caravan2.IsPlayerControlled)
							{
								AutoOrderToTile(caravan2, GenWorld.MouseTile());
							}
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

		public bool IsSelected(WorldObject obj)
		{
			return selected.Contains(obj);
		}

		public void ClearSelection()
		{
			WorldSelectionDrawer.Clear();
			selected.Clear();
			selectedTile = -1;
		}

		public void Deselect(WorldObject obj)
		{
			if (selected.Contains(obj))
			{
				selected.Remove(obj);
			}
		}

		public void Select(WorldObject obj, bool playSound = true)
		{
			if (obj == null)
			{
				Log.Error("Cannot select null.");
				return;
			}
			selectedTile = -1;
			if (selected.Count < 80 && !IsSelected(obj))
			{
				if (playSound)
				{
					PlaySelectionSoundFor(obj);
				}
				selected.Add(obj);
				WorldSelectionDrawer.Notify_Selected(obj);
			}
		}

		public void Notify_DialogOpened()
		{
			dragBox.active = false;
		}

		private void PlaySelectionSoundFor(WorldObject obj)
		{
			SoundDefOf.ThingSelected.PlayOneShotOnCamera();
		}

		private void SelectInsideDragBox()
		{
			if (!ShiftIsHeld)
			{
				ClearSelection();
			}
			bool flag = false;
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Caravan> list = Find.ColonistBar.CaravanMembersCaravansInScreenRect(dragBox.ScreenRect);
				for (int i = 0; i < list.Count; i++)
				{
					flag = true;
					Select(list[i]);
				}
			}
			if (!flag && Current.ProgramState == ProgramState.Playing)
			{
				List<Thing> list2 = Find.ColonistBar.MapColonistsOrCorpsesInScreenRect(dragBox.ScreenRect);
				for (int j = 0; j < list2.Count; j++)
				{
					if (!flag)
					{
						CameraJumper.TryJumpAndSelect(list2[j]);
						flag = true;
					}
					else
					{
						Find.Selector.Select(list2[j]);
					}
				}
			}
			if (!flag)
			{
				List<WorldObject> list3 = WorldObjectSelectionUtility.MultiSelectableWorldObjectsInScreenRectDistinct(dragBox.ScreenRect).ToList();
				if (list3.Any((WorldObject x) => x is Caravan))
				{
					list3.RemoveAll((WorldObject x) => !(x is Caravan));
					if (list3.Any((WorldObject x) => x.Faction == Faction.OfPlayer))
					{
						list3.RemoveAll((WorldObject x) => x.Faction != Faction.OfPlayer);
					}
				}
				for (int k = 0; k < list3.Count; k++)
				{
					flag = true;
					Select(list3[k]);
				}
			}
			if (!flag)
			{
				bool canSelectTile = dragBox.Diagonal < 30f;
				SelectUnderMouse(canSelectTile);
			}
		}

		public IEnumerable<WorldObject> SelectableObjectsUnderMouse()
		{
			bool clickedDirectlyOnCaravan;
			bool usedColonistBar;
			return SelectableObjectsUnderMouse(out clickedDirectlyOnCaravan, out usedColonistBar);
		}

		public IEnumerable<WorldObject> SelectableObjectsUnderMouse(out bool clickedDirectlyOnCaravan, out bool usedColonistBar)
		{
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			if (Current.ProgramState == ProgramState.Playing)
			{
				Caravan caravan = Find.ColonistBar.CaravanMemberCaravanAt(mousePositionOnUIInverted);
				if (caravan != null)
				{
					clickedDirectlyOnCaravan = true;
					usedColonistBar = true;
					return Gen.YieldSingle((WorldObject)caravan);
				}
			}
			List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			clickedDirectlyOnCaravan = false;
			if (list.Count > 0 && list[0] is Caravan && list[0].DistanceToMouse(UI.MousePositionOnUI) < GenWorldUI.CaravanDirectClickRadius)
			{
				clickedDirectlyOnCaravan = true;
				for (int num = list.Count - 1; num >= 0; num--)
				{
					WorldObject worldObject = list[num];
					if (worldObject is Caravan && worldObject.DistanceToMouse(UI.MousePositionOnUI) > GenWorldUI.CaravanDirectClickRadius)
					{
						list.Remove(worldObject);
					}
				}
			}
			usedColonistBar = false;
			return list;
		}

		public static IEnumerable<WorldObject> SelectableObjectsAt(int tileID)
		{
			foreach (WorldObject item in Find.WorldObjects.ObjectsAt(tileID))
			{
				if (item.SelectableNow)
				{
					yield return item;
				}
			}
		}

		private void SelectUnderMouse(bool canSelectTile = true)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Thing thing = Find.ColonistBar.ColonistOrCorpseAt(UI.MousePositionOnUIInverted);
				Pawn pawn = thing as Pawn;
				if (thing != null && (pawn == null || !pawn.IsCaravanMember()))
				{
					if (thing.Spawned)
					{
						CameraJumper.TryJumpAndSelect(thing);
					}
					else
					{
						CameraJumper.TryJump(thing);
					}
					return;
				}
			}
			bool clickedDirectlyOnCaravan;
			bool usedColonistBar;
			List<WorldObject> list = SelectableObjectsUnderMouse(out clickedDirectlyOnCaravan, out usedColonistBar).ToList();
			if (usedColonistBar || (clickedDirectlyOnCaravan && list.Count >= 2))
			{
				canSelectTile = false;
			}
			if (list.Count == 0)
			{
				if (!ShiftIsHeld)
				{
					ClearSelection();
					if (canSelectTile)
					{
						selectedTile = GenWorld.MouseTile();
					}
				}
			}
			else if (list.Where((WorldObject obj) => selected.Contains(obj)).FirstOrDefault() != null)
			{
				if (!ShiftIsHeld)
				{
					int tile = canSelectTile ? GenWorld.MouseTile() : (-1);
					SelectFirstOrNextFrom(list, tile);
				}
				else
				{
					foreach (WorldObject item in list)
					{
						if (selected.Contains(item))
						{
							Deselect(item);
						}
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

		public void SelectFirstOrNextAt(int tileID)
		{
			SelectFirstOrNextFrom(SelectableObjectsAt(tileID).ToList(), tileID);
		}

		private void SelectAllMatchingObjectUnderMouseOnScreen()
		{
			List<WorldObject> list = SelectableObjectsUnderMouse().ToList();
			if (list.Count == 0)
			{
				return;
			}
			Type type = list[0].GetType();
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				if (!(type != allWorldObjects[i].GetType()) && (allWorldObjects[i] == list[0] || allWorldObjects[i].AllMatchingObjectsOnScreenMatchesWith(list[0])) && allWorldObjects[i].VisibleToCameraNow())
				{
					Select(allWorldObjects[i]);
				}
			}
		}

		private void AutoOrderToTile(Caravan c, int tile)
		{
			if (tile >= 0)
			{
				if (c.autoJoinable && CaravanExitMapUtility.AnyoneTryingToJoinCaravan(c))
				{
					CaravanExitMapUtility.OpenSomeoneTryingToJoinCaravanDialog(c, delegate
					{
						AutoOrderToTileNow(c, tile);
					});
				}
				else
				{
					AutoOrderToTileNow(c, tile);
				}
			}
		}

		private void AutoOrderToTileNow(Caravan c, int tile)
		{
			if (tile >= 0 && (tile != c.Tile || c.pather.Moving))
			{
				int num = CaravanUtility.BestGotoDestNear(tile, c);
				if (num >= 0)
				{
					c.pather.StartPath(num, null, repathImmediately: true);
					c.gotoMote.OrderedToTile(num);
					SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
				}
			}
		}

		private void SelectFirstOrNextFrom(List<WorldObject> objects, int tile)
		{
			int num = objects.FindIndex((WorldObject x) => selected.Contains(x));
			int num2 = -1;
			int num3 = -1;
			if (num != -1)
			{
				if (num == objects.Count - 1 || selected.Count >= 2)
				{
					if (selected.Count >= 2)
					{
						num3 = 0;
					}
					else if (tile >= 0)
					{
						num2 = tile;
					}
					else
					{
						num3 = 0;
					}
				}
				else
				{
					num3 = num + 1;
				}
			}
			else if (objects.Count == 0)
			{
				num2 = tile;
			}
			else
			{
				num3 = 0;
			}
			ClearSelection();
			if (num3 >= 0)
			{
				Select(objects[num3]);
			}
			selectedTile = num2;
		}
	}
}
