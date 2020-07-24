using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Designator_Place : Designator
	{
		protected Rot4 placingRot = Rot4.North;

		protected static float middleMouseDownTime;

		private const float RotButSize = 64f;

		private const float RotButSpacing = 10f;

		public static readonly Color CanPlaceColor = new Color(0.5f, 1f, 0.6f, 0.4f);

		public static readonly Color CannotPlaceColor = new Color(1f, 0f, 0f, 0.4f);

		private static List<Thing> tmpThings = new List<Thing>();

		public abstract BuildableDef PlacingDef
		{
			get;
		}

		public Designator_Place()
		{
			soundDragSustain = SoundDefOf.Designate_DragBuilding;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_PlaceBuilding;
		}

		public override void DrawMouseAttachments()
		{
			base.DrawMouseAttachments();
			Map currentMap = Find.CurrentMap;
			ThingDef thingDef;
			if (currentMap == null || (thingDef = (PlacingDef as ThingDef)) == null || thingDef.displayNumbersBetweenSameDefDistRange.max <= 0f)
			{
				return;
			}
			IntVec3 intVec = UI.MouseCell();
			tmpThings.Clear();
			tmpThings.AddRange(currentMap.listerThings.ThingsOfDef(thingDef));
			tmpThings.AddRange(currentMap.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint));
			foreach (Thing tmpThing in tmpThings)
			{
				if ((tmpThing.def == thingDef || tmpThing.def.entityDefToBuild == thingDef) && (tmpThing.Position.x == intVec.x || tmpThing.Position.z == intVec.z) && CanDrawNumbersBetween(tmpThing, thingDef, intVec, tmpThing.Position, currentMap))
				{
					IntVec3 intVec2 = tmpThing.Position - intVec;
					intVec2.x = Mathf.Abs(intVec2.x) + 1;
					intVec2.z = Mathf.Abs(intVec2.z) + 1;
					if (intVec2.x >= 3)
					{
						Vector2 screenPos = (tmpThing.Position.ToUIPosition() + intVec.ToUIPosition()) / 2f;
						screenPos.y = tmpThing.Position.ToUIPosition().y;
						Color textColor = thingDef.displayNumbersBetweenSameDefDistRange.Includes(intVec2.x) ? Color.white : Color.red;
						Widgets.DrawNumberOnMap(screenPos, intVec2.x, textColor);
					}
					if (intVec2.z >= 3)
					{
						Vector2 screenPos2 = (tmpThing.Position.ToUIPosition() + intVec.ToUIPosition()) / 2f;
						screenPos2.x = tmpThing.Position.ToUIPosition().x;
						Color textColor2 = thingDef.displayNumbersBetweenSameDefDistRange.Includes(intVec2.z) ? Color.white : Color.red;
						Widgets.DrawNumberOnMap(screenPos2, intVec2.z, textColor2);
					}
				}
			}
			tmpThings.Clear();
		}

		protected virtual bool CanDrawNumbersBetween(Thing thing, ThingDef def, IntVec3 a, IntVec3 b, Map map)
		{
			return !GenThing.CloserThingBetween(def, a, b, map);
		}

		public override void DoExtraGuiControls(float leftX, float bottomY)
		{
			ThingDef thingDef = PlacingDef as ThingDef;
			if (thingDef == null || !thingDef.rotatable)
			{
				return;
			}
			Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
			Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
			{
				RotationDirection rotationDirection = RotationDirection.None;
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Medium;
				Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
				if (Widgets.ButtonImage(rect, TexUI.RotLeftTex))
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
					rotationDirection = RotationDirection.Counterclockwise;
					Event.current.Use();
				}
				Widgets.Label(rect, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
				Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
				if (Widgets.ButtonImage(rect2, TexUI.RotRightTex))
				{
					SoundDefOf.DragSlider.PlayOneShotOnCamera();
					rotationDirection = RotationDirection.Clockwise;
					Event.current.Use();
				}
				Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
				if (rotationDirection != 0)
				{
					placingRot.Rotate(rotationDirection);
				}
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			});
		}

		public override void SelectedProcessInput(Event ev)
		{
			base.SelectedProcessInput(ev);
			ThingDef thingDef = PlacingDef as ThingDef;
			if (thingDef != null && thingDef.rotatable)
			{
				HandleRotationShortcuts();
			}
		}

		public override void SelectedUpdate()
		{
			GenDraw.DrawNoBuildEdgeLines();
			IntVec3 intVec = UI.MouseCell();
			if (ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted) || !intVec.InBounds(base.Map))
			{
				return;
			}
			if (PlacingDef is TerrainDef)
			{
				GenUI.RenderMouseoverBracket();
				return;
			}
			Color ghostCol = (!CanDesignateCell(intVec).Accepted) ? CannotPlaceColor : CanPlaceColor;
			DrawGhost(ghostCol);
			if (CanDesignateCell(intVec).Accepted && PlacingDef.specialDisplayRadius > 0.01f)
			{
				GenDraw.DrawRadiusRing(intVec, PlacingDef.specialDisplayRadius);
			}
			GenDraw.DrawInteractionCell((ThingDef)PlacingDef, intVec, placingRot);
		}

		protected virtual void DrawGhost(Color ghostCol)
		{
			ThingDef def;
			if ((def = (PlacingDef as ThingDef)) != null)
			{
				MeditationUtility.DrawMeditationFociAffectedByBuildingOverlay(base.Map, def, Faction.OfPlayer, UI.MouseCell(), placingRot);
			}
			GhostDrawer.DrawGhostThing(UI.MouseCell(), placingRot, (ThingDef)PlacingDef, null, ghostCol, AltitudeLayer.Blueprint);
		}

		private void HandleRotationShortcuts()
		{
			RotationDirection rotationDirection = RotationDirection.None;
			if (Event.current.button == 2)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
					middleMouseDownTime = Time.realtimeSinceStartup;
				}
				if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
				{
					rotationDirection = RotationDirection.Clockwise;
				}
			}
			if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Clockwise;
			}
			if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Counterclockwise;
			}
			if (rotationDirection == RotationDirection.Clockwise)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				placingRot.Rotate(RotationDirection.Clockwise);
			}
			if (rotationDirection == RotationDirection.Counterclockwise)
			{
				SoundDefOf.DragSlider.PlayOneShotOnCamera();
				placingRot.Rotate(RotationDirection.Counterclockwise);
			}
		}

		public override void Selected()
		{
			placingRot = PlacingDef.defaultPlacingRot;
		}
	}
}
