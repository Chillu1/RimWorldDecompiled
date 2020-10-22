using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class ColonistBarColonistDrawer
	{
		private Dictionary<string, string> pawnLabelsCache = new Dictionary<string, string>();

		private static readonly Texture2D MoodBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.4f, 0.47f, 0.53f, 0.44f));

		private static readonly Texture2D DeadColonistTex = ContentFinder<Texture2D>.Get("UI/Misc/DeadColonist");

		private static readonly Texture2D Icon_FormingCaravan = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/FormingCaravan");

		private static readonly Texture2D Icon_MentalStateNonAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateNonAggro");

		private static readonly Texture2D Icon_MentalStateAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro");

		private static readonly Texture2D Icon_MedicalRest = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MedicalRest");

		private static readonly Texture2D Icon_Sleeping = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping");

		private static readonly Texture2D Icon_Fleeing = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Fleeing");

		private static readonly Texture2D Icon_Attacking = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Attacking");

		private static readonly Texture2D Icon_Idle = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Idle");

		private static readonly Texture2D Icon_Burning = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Burning");

		private static readonly Texture2D Icon_Inspired = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Inspired");

		public static readonly Vector2 PawnTextureSize = new Vector2(ColonistBar.BaseSize.x - 2f, 75f);

		public static readonly Vector3 PawnTextureCameraOffset = new Vector3(0f, 0f, 0.3f);

		public const float PawnTextureCameraZoom = 1.28205f;

		private const float PawnTextureHorizontalPadding = 1f;

		private const float BaseIconSize = 20f;

		private const float BaseGroupFrameMargin = 12f;

		public const float DoubleClickTime = 0.5f;

		private static Vector2[] bracketLocs = new Vector2[4];

		private ColonistBar ColonistBar => Find.ColonistBar;

		public void DrawColonist(Rect rect, Pawn colonist, Map pawnMap, bool highlight, bool reordering)
		{
			float alpha = ColonistBar.GetEntryRectAlpha(rect);
			ApplyEntryInAnotherMapAlphaFactor(pawnMap, ref alpha);
			if (reordering)
			{
				alpha *= 0.5f;
			}
			Color color2 = (GUI.color = new Color(1f, 1f, 1f, alpha));
			GUI.DrawTexture(rect, ColonistBar.BGTex);
			if (colonist.needs != null && colonist.needs.mood != null)
			{
				Rect position = rect.ContractedBy(2f);
				float num = position.height * colonist.needs.mood.CurLevelPercentage;
				position.yMin = position.yMax - num;
				position.height = num;
				GUI.DrawTexture(position, MoodBGTex);
			}
			if (highlight)
			{
				int thickness = ((rect.width <= 22f) ? 2 : 3);
				GUI.color = Color.white;
				Widgets.DrawBox(rect, thickness);
				GUI.color = color2;
			}
			Rect rect2 = rect.ContractedBy(-2f * ColonistBar.Scale);
			if ((colonist.Dead ? Find.Selector.SelectedObjects.Contains(colonist.Corpse) : Find.Selector.SelectedObjects.Contains(colonist)) && !WorldRendererUtility.WorldRenderedNow)
			{
				DrawSelectionOverlayOnGUI(colonist, rect2);
			}
			else if (WorldRendererUtility.WorldRenderedNow && colonist.IsCaravanMember() && Find.WorldSelector.IsSelected(colonist.GetCaravan()))
			{
				DrawCaravanSelectionOverlayOnGUI(colonist.GetCaravan(), rect2);
			}
			GUI.DrawTexture(GetPawnTextureRect(rect.position), PortraitsCache.Get(colonist, PawnTextureSize, PawnTextureCameraOffset, 1.28205f));
			GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);
			DrawIcons(rect, colonist);
			GUI.color = color2;
			if (colonist.Dead)
			{
				GUI.DrawTexture(rect, DeadColonistTex);
			}
			float num2 = 4f * ColonistBar.Scale;
			Vector2 pos = new Vector2(rect.center.x, rect.yMax - num2);
			GenMapUI.DrawPawnLabel(colonist, pos, alpha, rect.width + ColonistBar.SpaceBetweenColonistsHorizontal - 2f, pawnLabelsCache);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
		}

		private Rect GroupFrameRect(int group)
		{
			float num = 99999f;
			float num2 = 0f;
			float num3 = 0f;
			List<ColonistBar.Entry> entries = ColonistBar.Entries;
			List<Vector2> drawLocs = ColonistBar.DrawLocs;
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].group == group)
				{
					num = Mathf.Min(num, drawLocs[i].x);
					num2 = Mathf.Max(num2, drawLocs[i].x + ColonistBar.Size.x);
					num3 = Mathf.Max(num3, drawLocs[i].y + ColonistBar.Size.y);
				}
			}
			return new Rect(num, 0f, num2 - num, num3 - 0f).ContractedBy(-12f * ColonistBar.Scale);
		}

		public void DrawGroupFrame(int group)
		{
			Rect position = GroupFrameRect(group);
			Map map = ColonistBar.Entries.Find((ColonistBar.Entry x) => x.group == group).map;
			float num = ((map == null) ? ((!WorldRendererUtility.WorldRenderedNow) ? 0.75f : 1f) : ((map == Find.CurrentMap && !WorldRendererUtility.WorldRenderedNow) ? 1f : 0.75f));
			Widgets.DrawRectFast(position, new Color(0.5f, 0.5f, 0.5f, 0.4f * num));
		}

		private void ApplyEntryInAnotherMapAlphaFactor(Map map, ref float alpha)
		{
			if (map == null)
			{
				if (!WorldRendererUtility.WorldRenderedNow)
				{
					alpha = Mathf.Min(alpha, 0.4f);
				}
			}
			else if (map != Find.CurrentMap || WorldRendererUtility.WorldRenderedNow)
			{
				alpha = Mathf.Min(alpha, 0.4f);
			}
		}

		public void HandleClicks(Rect rect, Pawn colonist, int reorderableGroup, out bool reordering)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && Mouse.IsOver(rect))
			{
				Event.current.Use();
				CameraJumper.TryJump(colonist);
			}
			reordering = ReorderableWidget.Reorderable(reorderableGroup, rect, useRightButton: true);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
			{
				Event.current.Use();
			}
		}

		public void HandleGroupFrameClicks(int group)
		{
			Rect rect = GroupFrameRect(group);
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Mouse.IsOver(rect) && !ColonistBar.AnyColonistOrCorpseAt(UI.MousePositionOnUIInverted))
			{
				bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
				if ((!worldRenderedNow && !Find.Selector.dragBox.IsValidAndActive) || (worldRenderedNow && !Find.WorldSelector.dragBox.IsValidAndActive))
				{
					Find.Selector.dragBox.active = false;
					Find.WorldSelector.dragBox.active = false;
					ColonistBar.Entry entry = ColonistBar.Entries.Find((ColonistBar.Entry x) => x.group == group);
					Map map = entry.map;
					if (map == null)
					{
						if (WorldRendererUtility.WorldRenderedNow)
						{
							CameraJumper.TrySelect(entry.pawn);
						}
						else
						{
							CameraJumper.TryJumpAndSelect(entry.pawn);
						}
					}
					else
					{
						if (!CameraJumper.TryHideWorld() && Find.CurrentMap != map)
						{
							SoundDefOf.MapSelected.PlayOneShotOnCamera();
						}
						Current.Game.CurrentMap = map;
					}
				}
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
			{
				Event.current.Use();
			}
		}

		public void Notify_RecachedEntries()
		{
			pawnLabelsCache.Clear();
		}

		public Rect GetPawnTextureRect(Vector2 pos)
		{
			float x = pos.x;
			float y = pos.y;
			Vector2 vector = PawnTextureSize * ColonistBar.Scale;
			return new Rect(x + 1f, y - (vector.y - ColonistBar.Size.y) - 1f, vector.x, vector.y).ContractedBy(1f);
		}

		private void DrawIcons(Rect rect, Pawn colonist)
		{
			if (colonist.Dead)
			{
				return;
			}
			float num = 20f * ColonistBar.Scale;
			Vector2 pos = new Vector2(rect.x + 1f, rect.yMax - num - 1f);
			bool flag = false;
			if (colonist.CurJob != null)
			{
				JobDef def = colonist.CurJob.def;
				if (def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic)
				{
					flag = true;
				}
				else if (def == JobDefOf.Wait_Combat)
				{
					Stance_Busy stance_Busy = colonist.stances.curStance as Stance_Busy;
					if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
					{
						flag = true;
					}
				}
			}
			if (colonist.IsFormingCaravan())
			{
				DrawIcon(Icon_FormingCaravan, ref pos, "ActivityIconFormingCaravan".Translate());
			}
			if (colonist.InAggroMentalState)
			{
				DrawIcon(Icon_MentalStateAggro, ref pos, colonist.MentalStateDef.LabelCap);
			}
			else if (colonist.InMentalState)
			{
				DrawIcon(Icon_MentalStateNonAggro, ref pos, colonist.MentalStateDef.LabelCap);
			}
			else if (colonist.InBed() && colonist.CurrentBed().Medical)
			{
				DrawIcon(Icon_MedicalRest, ref pos, "ActivityIconMedicalRest".Translate());
			}
			else if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
			{
				DrawIcon(Icon_Sleeping, ref pos, "ActivityIconSleeping".Translate());
			}
			else if (colonist.CurJob != null && colonist.CurJob.def == JobDefOf.FleeAndCower)
			{
				DrawIcon(Icon_Fleeing, ref pos, "ActivityIconFleeing".Translate());
			}
			else if (flag)
			{
				DrawIcon(Icon_Attacking, ref pos, "ActivityIconAttacking".Translate());
			}
			else if (colonist.mindState.IsIdle && GenDate.DaysPassed >= 1)
			{
				DrawIcon(Icon_Idle, ref pos, "ActivityIconIdle".Translate());
			}
			if (colonist.IsBurning() && pos.x + num <= rect.xMax)
			{
				DrawIcon(Icon_Burning, ref pos, "ActivityIconBurning".Translate());
			}
			if (colonist.Inspired && pos.x + num <= rect.xMax)
			{
				DrawIcon(Icon_Inspired, ref pos, colonist.InspirationDef.LabelCap);
			}
		}

		private void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
		{
			float num = 20f * ColonistBar.Scale;
			Rect rect = new Rect(pos.x, pos.y, num, num);
			GUI.DrawTexture(rect, icon);
			TooltipHandler.TipRegion(rect, tooltip);
			pos.x += num;
		}

		private void DrawSelectionOverlayOnGUI(Pawn colonist, Rect rect)
		{
			Thing obj = colonist;
			if (colonist.Dead)
			{
				obj = colonist.Corpse;
			}
			float num = 0.4f * ColonistBar.Scale;
			SelectionDrawerUtility.CalculateSelectionBracketPositionsUI(textureSize: new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * num, (float)SelectionDrawerUtility.SelectedTexGUI.height * num), bracketLocs: bracketLocs, obj: obj, rect: rect, selectTimes: SelectionDrawer.SelectTimes, jumpDistanceFactor: 20f * ColonistBar.Scale);
			DrawSelectionOverlayOnGUI(bracketLocs, num);
		}

		private void DrawCaravanSelectionOverlayOnGUI(Caravan caravan, Rect rect)
		{
			float num = 0.4f * ColonistBar.Scale;
			SelectionDrawerUtility.CalculateSelectionBracketPositionsUI(textureSize: new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * num, (float)SelectionDrawerUtility.SelectedTexGUI.height * num), bracketLocs: bracketLocs, obj: caravan, rect: rect, selectTimes: WorldSelectionDrawer.SelectTimes, jumpDistanceFactor: 20f * ColonistBar.Scale);
			DrawSelectionOverlayOnGUI(bracketLocs, num);
		}

		private void DrawSelectionOverlayOnGUI(Vector2[] bracketLocs, float selectedTexScale)
		{
			int num = 90;
			for (int i = 0; i < 4; i++)
			{
				Widgets.DrawTextureRotated(bracketLocs[i], SelectionDrawerUtility.SelectedTexGUI, num, selectedTexScale);
				num += 90;
			}
		}
	}
}
