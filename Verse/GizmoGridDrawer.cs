using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GizmoGridDrawer
	{
		public static HashSet<KeyCode> drawnHotKeys = new HashSet<KeyCode>();

		private static float heightDrawn;

		private static int heightDrawnFrame;

		public static readonly Vector2 GizmoSpacing = new Vector2(5f, 14f);

		private static List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();

		private static List<Gizmo> firstGizmos = new List<Gizmo>();

		private static List<Gizmo> tmpAllGizmos = new List<Gizmo>();

		private static readonly Func<Gizmo, Gizmo, int> SortByOrder = (Gizmo lhs, Gizmo rhs) => lhs.order.CompareTo(rhs.order);

		public static float HeightDrawnRecently
		{
			get
			{
				if (Time.frameCount > heightDrawnFrame + 2)
				{
					return 0f;
				}
				return heightDrawn;
			}
		}

		public static void DrawGizmoGrid(IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo)
		{
			tmpAllGizmos.Clear();
			tmpAllGizmos.AddRange(gizmos);
			tmpAllGizmos.SortStable(SortByOrder);
			gizmoGroups.Clear();
			for (int i = 0; i < tmpAllGizmos.Count; i++)
			{
				Gizmo gizmo = tmpAllGizmos[i];
				bool flag = false;
				for (int j = 0; j < gizmoGroups.Count; j++)
				{
					if (gizmoGroups[j][0].GroupsWith(gizmo))
					{
						flag = true;
						gizmoGroups[j].Add(gizmo);
						gizmoGroups[j][0].MergeWith(gizmo);
						break;
					}
				}
				if (!flag)
				{
					List<Gizmo> list = SimplePool<List<Gizmo>>.Get();
					list.Add(gizmo);
					gizmoGroups.Add(list);
				}
			}
			firstGizmos.Clear();
			for (int k = 0; k < gizmoGroups.Count; k++)
			{
				List<Gizmo> list2 = gizmoGroups[k];
				Gizmo gizmo2 = null;
				for (int l = 0; l < list2.Count; l++)
				{
					if (!list2[l].disabled)
					{
						gizmo2 = list2[l];
						break;
					}
				}
				if (gizmo2 == null)
				{
					gizmo2 = list2.FirstOrDefault();
				}
				else
				{
					Command_Toggle command_Toggle = gizmo2 as Command_Toggle;
					if (command_Toggle != null)
					{
						if (!command_Toggle.activateIfAmbiguous && !command_Toggle.isActive())
						{
							for (int m = 0; m < list2.Count; m++)
							{
								Command_Toggle command_Toggle2 = list2[m] as Command_Toggle;
								if (command_Toggle2 != null && !command_Toggle2.disabled && command_Toggle2.isActive())
								{
									gizmo2 = list2[m];
									break;
								}
							}
						}
						if (command_Toggle.activateIfAmbiguous && command_Toggle.isActive())
						{
							for (int n = 0; n < list2.Count; n++)
							{
								Command_Toggle command_Toggle3 = list2[n] as Command_Toggle;
								if (command_Toggle3 != null && !command_Toggle3.disabled && !command_Toggle3.isActive())
								{
									gizmo2 = list2[n];
									break;
								}
							}
						}
					}
				}
				if (gizmo2 != null)
				{
					firstGizmos.Add(gizmo2);
				}
			}
			drawnHotKeys.Clear();
			float num = UI.screenWidth - 147;
			float maxWidth = num - startX;
			Text.Font = GameFont.Tiny;
			Vector2 topLeft = new Vector2(startX, (float)(UI.screenHeight - 35) - GizmoSpacing.y - 75f);
			mouseoverGizmo = null;
			Gizmo gizmo3 = null;
			Event ev = null;
			Gizmo gizmo4 = null;
			for (int num2 = 0; num2 < firstGizmos.Count; num2++)
			{
				Gizmo gizmo5 = firstGizmos[num2];
				if (gizmo5.Visible)
				{
					if (topLeft.x + gizmo5.GetWidth(maxWidth) > num)
					{
						topLeft.x = startX;
						topLeft.y -= 75f + GizmoSpacing.x;
					}
					heightDrawnFrame = Time.frameCount;
					heightDrawn = (float)UI.screenHeight - topLeft.y;
					GizmoResult gizmoResult = gizmo5.GizmoOnGUI(topLeft, maxWidth);
					if (gizmoResult.State == GizmoState.Interacted || (gizmoResult.State == GizmoState.OpenedFloatMenu && gizmo5.RightClickFloatMenuOptions.FirstOrDefault() == null))
					{
						ev = gizmoResult.InteractEvent;
						gizmo3 = gizmo5;
					}
					else if (gizmoResult.State == GizmoState.OpenedFloatMenu)
					{
						gizmo4 = gizmo5;
					}
					if ((int)gizmoResult.State >= 1)
					{
						mouseoverGizmo = gizmo5;
					}
					GenUI.AbsorbClicksInRect(new Rect(topLeft.x, topLeft.y, gizmo5.GetWidth(maxWidth), 75f + GizmoSpacing.y).ContractedBy(-12f));
					topLeft.x += gizmo5.GetWidth(maxWidth) + GizmoSpacing.x;
				}
			}
			if (gizmo3 != null)
			{
				List<Gizmo> list3 = FindMatchingGroup(gizmo3);
				for (int num3 = 0; num3 < list3.Count; num3++)
				{
					Gizmo gizmo6 = list3[num3];
					if (gizmo6 != gizmo3 && !gizmo6.disabled && gizmo3.InheritInteractionsFrom(gizmo6))
					{
						gizmo6.ProcessInput(ev);
					}
				}
				gizmo3.ProcessInput(ev);
				Event.current.Use();
			}
			else if (gizmo4 != null)
			{
				List<FloatMenuOption> list4 = new List<FloatMenuOption>();
				foreach (FloatMenuOption rightClickFloatMenuOption in gizmo4.RightClickFloatMenuOptions)
				{
					list4.Add(rightClickFloatMenuOption);
				}
				List<Gizmo> list5 = FindMatchingGroup(gizmo4);
				for (int num4 = 0; num4 < list5.Count; num4++)
				{
					Gizmo gizmo7 = list5[num4];
					if (gizmo7 == gizmo4 || gizmo7.disabled || !gizmo4.InheritFloatMenuInteractionsFrom(gizmo7))
					{
						continue;
					}
					foreach (FloatMenuOption rightClickFloatMenuOption2 in gizmo7.RightClickFloatMenuOptions)
					{
						FloatMenuOption floatMenuOption = null;
						for (int num5 = 0; num5 < list4.Count; num5++)
						{
							if (list4[num5].Label == rightClickFloatMenuOption2.Label)
							{
								floatMenuOption = list4[num5];
								break;
							}
						}
						if (floatMenuOption == null)
						{
							list4.Add(rightClickFloatMenuOption2);
						}
						else
						{
							if (rightClickFloatMenuOption2.Disabled)
							{
								continue;
							}
							if (!floatMenuOption.Disabled)
							{
								Action prevAction = floatMenuOption.action;
								Action localOptionAction = rightClickFloatMenuOption2.action;
								floatMenuOption.action = delegate
								{
									prevAction();
									localOptionAction();
								};
							}
							else if (floatMenuOption.Disabled)
							{
								list4[list4.IndexOf(floatMenuOption)] = rightClickFloatMenuOption2;
							}
						}
					}
				}
				Event.current.Use();
				if (list4.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list4));
				}
			}
			for (int num6 = 0; num6 < gizmoGroups.Count; num6++)
			{
				gizmoGroups[num6].Clear();
				SimplePool<List<Gizmo>>.Return(gizmoGroups[num6]);
			}
			gizmoGroups.Clear();
			firstGizmos.Clear();
			tmpAllGizmos.Clear();
			static List<Gizmo> FindMatchingGroup(Gizmo toMatch)
			{
				for (int num7 = 0; num7 < gizmoGroups.Count; num7++)
				{
					if (gizmoGroups[num7].Contains(toMatch))
					{
						return gizmoGroups[num7];
					}
				}
				return null;
			}
		}
	}
}
