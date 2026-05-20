using System;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class TilePicker
	{
		private static readonly Vector2 ButtonSize = new Vector2(150f, 38f);

		private const int Padding = 8;

		private const int BottomPanelYOffset = -50;

		private Func<PlanetTile, bool> validator;

		private bool allowEscape;

		private bool active;

		private Action<PlanetTile> tileChosen;

		private Action noTileChosen;

		private Action onGuiAction;

		private Action onUpdateAction;

		private string title;

		private bool showRandomButton = true;

		private bool selectTileBehindObject;

		private bool forGravship;

		private bool showNextButton = true;

		private bool canCancel;

		private PlanetTile closestLayerTile = PlanetTile.Invalid;

		private string noTileChosenMessage;

		public bool Active => active;

		public bool AllowEscape => allowEscape;

		public bool ForGravship => forGravship;

		public PlanetTile ClosestLayerTile => closestLayerTile;

		[Obsolete]
		public void StartTargeting(Func<PlanetTile, bool> validator, Action<PlanetTile> tileChosen, Action onGuiAction = null, Action onUpdateAction = null, bool allowEscape = true, Action noTileChosen = null, string title = null, bool showRandomButton = true, bool selectTileBehindObject = false, bool hideFormCaravanGizmo = false, bool canCancel = false, string noTileChosenMessage = null)
		{
			StartTargeting_NewTemp(validator, tileChosen, onGuiAction, onUpdateAction, allowEscape, noTileChosen, title, showRandomButton, selectTileBehindObject, hideFormCaravanGizmo, showNextButton: true, canCancel, noTileChosenMessage);
		}

		public void StartTargeting_NewTemp(Func<PlanetTile, bool> validator, Action<PlanetTile> tileChosen, Action onGuiAction = null, Action onUpdateAction = null, bool allowEscape = true, Action noTileChosen = null, string title = null, bool showRandomButton = true, bool selectTileBehindObject = false, bool hideFormCaravanGizmo = false, bool showNextButton = true, bool canCancel = false, string noTileChosenMessage = null)
		{
			this.validator = validator;
			this.allowEscape = allowEscape;
			this.noTileChosen = noTileChosen;
			this.tileChosen = tileChosen;
			this.title = title;
			this.showRandomButton = showRandomButton;
			this.onGuiAction = onGuiAction;
			this.onUpdateAction = onUpdateAction;
			this.selectTileBehindObject = selectTileBehindObject;
			forGravship = hideFormCaravanGizmo;
			this.showNextButton = showNextButton;
			this.canCancel = canCancel;
			this.noTileChosenMessage = noTileChosenMessage ?? ((string)"MustSelectStartingSite".Translate());
			Find.WorldSelector.ClearSelection();
			active = true;
		}

		public void StopTargeting()
		{
			if (active && noTileChosen != null)
			{
				noTileChosen();
			}
			StopTargetingInt();
		}

		private void StopTargetingInt()
		{
			forGravship = false;
			active = false;
			closestLayerTile = PlanetTile.Invalid;
		}

		public void TileSelectorOnGUI()
		{
			if (!title.NullOrEmpty())
			{
				Text.Font = GameFont.Medium;
				Vector2 vector = Text.CalcSize(title);
				Widgets.Label(new Rect((float)UI.screenWidth / 2f - vector.x / 2f, 4f, vector.x + 4f, vector.y), title);
				Text.Font = GameFont.Small;
			}
			onGuiAction?.Invoke();
			Vector2 buttonSize = ButtonSize;
			int num = 0;
			if (showNextButton)
			{
				num++;
			}
			if (showRandomButton)
			{
				num++;
			}
			if (canCancel)
			{
				num++;
			}
			int num2 = (num + 1) * 8;
			Rect rect = new Rect((float)UI.screenWidth / 2f - (float)num * buttonSize.x / 2f - (float)num2 / 2f, (float)UI.screenHeight - (buttonSize.y + 8f) + -50f, (float)num * buttonSize.x + (float)num2, buttonSize.y + 16f);
			Widgets.DrawWindowBackground(rect);
			float num3 = rect.x + 8f;
			if (canCancel)
			{
				if (Widgets.ButtonText(new Rect(num3, rect.y + 8f, buttonSize.x, buttonSize.y), "Cancel".Translate()))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					StopTargeting();
				}
				num3 += buttonSize.x + 2f + 8f;
			}
			if (showRandomButton)
			{
				if (Widgets.ButtonText(new Rect(num3, rect.y + 8f, buttonSize.x, buttonSize.y), "SelectRandomSite".Translate()))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
					Find.WorldInterface.SelectedTile = TileFinder.RandomStartingTile();
					Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(Find.WorldInterface.SelectedTile));
				}
				num3 += buttonSize.x + 2f + 8f;
			}
			if ((showNextButton && Widgets.ButtonText(new Rect(num3, rect.y + 8f, buttonSize.x, buttonSize.y), "Next".Translate())) || KeyBindingDefOf.Accept.KeyDownEvent)
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				PlanetTile selectedTile = Find.WorldInterface.SelectedTile;
				if (!selectedTile.Valid)
				{
					if (selectTileBehindObject)
					{
						WorldObject singleSelectedObject = Find.WorldSelector.SingleSelectedObject;
						if (singleSelectedObject != null && singleSelectedObject.Tile.Valid)
						{
							selectedTile = singleSelectedObject.Tile;
							if (!selectedTile.Valid)
							{
								Messages.Message(noTileChosenMessage, MessageTypeDefOf.RejectInput, historical: false);
							}
							else if (validator(selectedTile))
							{
								StopTargetingInt();
								tileChosen(selectedTile);
								Event.current.Use();
							}
						}
						else
						{
							Messages.Message(noTileChosenMessage, MessageTypeDefOf.RejectInput, historical: false);
						}
					}
					else
					{
						Messages.Message(noTileChosenMessage, MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				else if (validator(selectedTile))
				{
					StopTargetingInt();
					tileChosen(selectedTile);
					Event.current.Use();
				}
			}
			if (KeyBindingDefOf.Cancel.KeyDownEvent && Active && !allowEscape)
			{
				Event.current.Use();
			}
		}

		public void TileSelectorUpdate()
		{
			onUpdateAction?.Invoke();
		}
	}
}
