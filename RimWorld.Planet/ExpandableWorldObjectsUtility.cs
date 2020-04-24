using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class ExpandableWorldObjectsUtility
	{
		private static float transitionPct;

		private static float expandMoreTransitionPct;

		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		private const float WorldObjectIconSize = 30f;

		private const float ExpandMoreWorldObjectIconSizeFactor = 1.35f;

		private const float TransitionSpeed = 3f;

		private const float ExpandMoreTransitionSpeed = 4f;

		public static float TransitionPct
		{
			get
			{
				if (!Find.PlaySettings.showExpandingIcons)
				{
					return 0f;
				}
				return transitionPct;
			}
		}

		public static float ExpandMoreTransitionPct
		{
			get
			{
				if (!Find.PlaySettings.showExpandingIcons)
				{
					return 0f;
				}
				return expandMoreTransitionPct;
			}
		}

		public static void ExpandableWorldObjectsUpdate()
		{
			float num = Time.deltaTime * 3f;
			if ((int)Find.WorldCameraDriver.CurrentZoom <= 0)
			{
				transitionPct -= num;
			}
			else
			{
				transitionPct += num;
			}
			transitionPct = Mathf.Clamp01(transitionPct);
			float num2 = Time.deltaTime * 4f;
			if ((int)Find.WorldCameraDriver.CurrentZoom <= 2)
			{
				expandMoreTransitionPct -= num2;
			}
			else
			{
				expandMoreTransitionPct += num2;
			}
			expandMoreTransitionPct = Mathf.Clamp01(expandMoreTransitionPct);
		}

		public static void ExpandableWorldObjectsOnGUI()
		{
			if (TransitionPct == 0f)
			{
				return;
			}
			tmpWorldObjects.Clear();
			tmpWorldObjects.AddRange(Find.WorldObjects.AllWorldObjects);
			SortByExpandingIconPriority(tmpWorldObjects);
			WorldTargeter worldTargeter = Find.WorldTargeter;
			List<WorldObject> worldObjectsUnderMouse = null;
			if (worldTargeter.IsTargeting)
			{
				worldObjectsUnderMouse = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			}
			for (int i = 0; i < tmpWorldObjects.Count; i++)
			{
				WorldObject worldObject = tmpWorldObjects[i];
				if (worldObject.def.expandingIcon && !worldObject.HiddenBehindTerrainNow())
				{
					Color expandingIconColor = worldObject.ExpandingIconColor;
					expandingIconColor.a = TransitionPct;
					if (worldTargeter.IsTargetedNow(worldObject, worldObjectsUnderMouse))
					{
						float num = GenMath.LerpDouble(-1f, 1f, 0.7f, 1f, Mathf.Sin(Time.time * 8f));
						expandingIconColor.r *= num;
						expandingIconColor.g *= num;
						expandingIconColor.b *= num;
					}
					GUI.color = expandingIconColor;
					GUI.DrawTexture(ExpandedIconScreenRect(worldObject), worldObject.ExpandingIcon);
				}
			}
			tmpWorldObjects.Clear();
			GUI.color = Color.white;
		}

		public static Rect ExpandedIconScreenRect(WorldObject o)
		{
			Vector2 vector = o.ScreenPos();
			float num = (!o.ExpandMore) ? 30f : Mathf.Lerp(30f, 40.5f, ExpandMoreTransitionPct);
			return new Rect(vector.x - num / 2f, vector.y - num / 2f, num, num);
		}

		public static bool IsExpanded(WorldObject o)
		{
			if (TransitionPct > 0.5f)
			{
				return o.def.expandingIcon;
			}
			return false;
		}

		public static void GetExpandedWorldObjectUnderMouse(Vector2 mousePos, List<WorldObject> outList)
		{
			outList.Clear();
			Vector2 point = mousePos;
			point.y = (float)UI.screenHeight - point.y;
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				WorldObject worldObject = allWorldObjects[i];
				if (IsExpanded(worldObject) && ExpandedIconScreenRect(worldObject).Contains(point) && !worldObject.HiddenBehindTerrainNow())
				{
					outList.Add(worldObject);
				}
			}
			SortByExpandingIconPriority(outList);
			outList.Reverse();
		}

		private static void SortByExpandingIconPriority(List<WorldObject> worldObjects)
		{
			worldObjects.SortBy(delegate(WorldObject x)
			{
				float num = x.ExpandingIconPriority;
				if (x.Faction != null && x.Faction.IsPlayer)
				{
					num += 0.001f;
				}
				return num;
			}, (WorldObject x) => x.ID);
		}
	}
}
