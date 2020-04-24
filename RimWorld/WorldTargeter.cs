using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class WorldTargeter
	{
		private Func<GlobalTargetInfo, bool> action;

		private bool canTargetTiles;

		private Texture2D mouseAttachment;

		public bool closeWorldTabWhenFinished;

		private Action onUpdate;

		private Func<GlobalTargetInfo, string> extraLabelGetter;

		private const float BaseFeedbackTexSize = 0.8f;

		public bool IsTargeting => action != null;

		public void BeginTargeting(Func<GlobalTargetInfo, bool> action, bool canTargetTiles, Texture2D mouseAttachment = null, bool closeWorldTabWhenFinished = false, Action onUpdate = null, Func<GlobalTargetInfo, string> extraLabelGetter = null)
		{
			this.action = action;
			this.canTargetTiles = canTargetTiles;
			this.mouseAttachment = mouseAttachment;
			this.closeWorldTabWhenFinished = closeWorldTabWhenFinished;
			this.onUpdate = onUpdate;
			this.extraLabelGetter = extraLabelGetter;
		}

		public void StopTargeting()
		{
			if (closeWorldTabWhenFinished)
			{
				CameraJumper.TryHideWorld();
			}
			action = null;
			canTargetTiles = false;
			mouseAttachment = null;
			closeWorldTabWhenFinished = false;
			onUpdate = null;
			extraLabelGetter = null;
		}

		public void ProcessInputEvents()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0 && IsTargeting)
				{
					GlobalTargetInfo arg = CurrentTargetUnderMouse();
					if (action(arg))
					{
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
						StopTargeting();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && IsTargeting)
				{
					SoundDefOf.CancelMode.PlayOneShotOnCamera();
					StopTargeting();
					Event.current.Use();
				}
			}
			if (KeyBindingDefOf.Cancel.KeyDownEvent && IsTargeting)
			{
				SoundDefOf.CancelMode.PlayOneShotOnCamera();
				StopTargeting();
				Event.current.Use();
			}
		}

		public void TargeterOnGUI()
		{
			if (!IsTargeting || Mouse.IsInputBlockedNow)
			{
				return;
			}
			Vector2 mousePosition = Event.current.mousePosition;
			Texture2D image = mouseAttachment ?? TexCommand.Attack;
			Rect position = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
			GUI.DrawTexture(position, image);
			if (extraLabelGetter != null)
			{
				GUI.color = Color.white;
				string text = extraLabelGetter(CurrentTargetUnderMouse());
				if (!text.NullOrEmpty())
				{
					Color color = GUI.color;
					GUI.color = Color.white;
					Rect rect = new Rect(position.xMax, position.y, 9999f, 100f);
					Vector2 vector = Text.CalcSize(text);
					GUI.DrawTexture(new Rect(rect.x - vector.x * 0.1f, rect.y, vector.x * 1.2f, vector.y), TexUI.GrayTextBG);
					GUI.color = color;
					Widgets.Label(rect, text);
				}
				GUI.color = Color.white;
			}
		}

		public void TargeterUpdate()
		{
			if (IsTargeting)
			{
				Vector3 pos = Vector3.zero;
				GlobalTargetInfo globalTargetInfo = CurrentTargetUnderMouse();
				if (globalTargetInfo.HasWorldObject)
				{
					pos = globalTargetInfo.WorldObject.DrawPos;
				}
				else if (globalTargetInfo.Tile >= 0)
				{
					pos = Find.WorldGrid.GetTileCenter(globalTargetInfo.Tile);
				}
				if (globalTargetInfo.IsValid && !Mouse.IsInputBlockedNow)
				{
					WorldRendererUtility.DrawQuadTangentialToPlanet(pos, 0.8f * Find.WorldGrid.averageTileSize, 0.018f, WorldMaterials.CurTargetingMat);
				}
				if (onUpdate != null)
				{
					onUpdate();
				}
			}
		}

		public bool IsTargetedNow(WorldObject o, List<WorldObject> worldObjectsUnderMouse = null)
		{
			if (!IsTargeting)
			{
				return false;
			}
			if (worldObjectsUnderMouse == null)
			{
				worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			}
			if (worldObjectsUnderMouse.Any())
			{
				return o == worldObjectsUnderMouse[0];
			}
			return false;
		}

		private GlobalTargetInfo CurrentTargetUnderMouse()
		{
			if (!IsTargeting)
			{
				return GlobalTargetInfo.Invalid;
			}
			List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			if (list.Any())
			{
				return list[0];
			}
			if (canTargetTiles)
			{
				int num = GenWorld.MouseTile();
				if (num >= 0)
				{
					return new GlobalTargetInfo(num);
				}
				return GlobalTargetInfo.Invalid;
			}
			return GlobalTargetInfo.Invalid;
		}
	}
}
