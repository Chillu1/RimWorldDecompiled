using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Steam;

namespace Verse;

public static class GizmoGridDrawer
{
	public static HashSet<KeyCode> drawnHotKeys = new HashSet<KeyCode>();

	public static Func<Gizmo, bool> customActivator;

	private static float heightDrawn;

	private static int heightDrawnFrame;

	private static readonly Vector2 GizmoSpacing = new Vector2(5f, 14f);

	private const float GizmoStartX = 14f;

	private static List<object> objList = new List<object>();

	private static List<Gizmo> gizmoList = new List<Gizmo>();

	private static int cacheFrame;

	private static List<object> tmpObjectCacheList = new List<object>();

	private static List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();

	private static List<Gizmo> firstGizmos = new List<Gizmo>();

	private static List<Command> shrinkableCommands = new List<Command>();

	private static List<Gizmo> tmpAllGizmos = new List<Gizmo>();

	private static readonly Func<Gizmo, Gizmo, int> SortByOrder = (Gizmo lhs, Gizmo rhs) => lhs.Order.CompareTo(rhs.Order);

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

	public static void DrawGizmoGridFor(IEnumerable<object> selectedObjects, out Gizmo mouseoverGizmo)
	{
		mouseoverGizmo = null;
		ISelectable obj = null;
		if (Find.ScreenshotModeHandler.Active)
		{
			return;
		}
		try
		{
			bool flag = true;
			int frameCount = Time.frameCount;
			if (cacheFrame == frameCount)
			{
				tmpObjectCacheList.Clear();
				tmpObjectCacheList.AddRange(selectedObjects);
				if (objList.Count == tmpObjectCacheList.Count)
				{
					for (int i = 0; i < objList.Count; i++)
					{
						if (tmpObjectCacheList[i] != objList[i])
						{
							flag = false;
							break;
						}
					}
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				cacheFrame = frameCount;
				objList.Clear();
				objList.AddRange(selectedObjects);
				gizmoList.Clear();
				for (int j = 0; j < objList.Count; j++)
				{
					if (objList[j] is ISelectable selectable)
					{
						gizmoList.AddRange(selectable.GetGizmos());
					}
					if (objList[j] is Gizmo item)
					{
						gizmoList.Add(item);
					}
				}
				for (int k = 0; k < objList.Count; k++)
				{
					if (!(objList[k] is Thing t))
					{
						continue;
					}
					List<Designator> allDesignators = Find.ReverseDesignatorDatabase.AllDesignators;
					for (int l = 0; l < allDesignators.Count; l++)
					{
						Command_Action command_Action = allDesignators[l].CreateReverseDesignationGizmo(t);
						if (command_Action != null)
						{
							gizmoList.Add(command_Action);
						}
					}
				}
			}
			float num = 14f;
			IInspectPane inspectPane = Find.WindowStack.WindowOfType<IInspectPane>();
			if (inspectPane != null)
			{
				num += InspectPaneUtility.PaneWidthFor(inspectPane);
			}
			DrawGizmoGrid(gizmoList, num, out mouseoverGizmo, null, null, null, objList.Count > 1);
		}
		catch (Exception arg)
		{
			Log.ErrorOnce($"{arg} currentSelectable: {obj.ToStringSafe()}", 3427734);
		}
	}

	public static void DrawGizmoGrid(IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo, Func<Gizmo, bool> customActivatorFunc = null, Func<Gizmo, bool> highlightFunc = null, Func<Gizmo, bool> lowlightFunc = null, bool multipleSelected = false)
	{
		if (Event.current.type == EventType.Layout)
		{
			mouseoverGizmo = null;
			return;
		}
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
		shrinkableCommands.Clear();
		float num = UI.screenWidth - 147;
		float num2 = (float)(UI.screenHeight - 35) - GizmoSpacing.y - 75f;
		if (SteamDeck.IsSteamDeck && SteamDeck.KeyboardShowing && Find.MainTabsRoot.OpenTab == MainButtonDefOf.Architect && ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).QuickSearchWidgetFocused)
		{
			num2 -= 335f;
		}
		Vector2 vector = new Vector2(startX, num2);
		float maxWidth = num - startX;
		int num3 = 0;
		for (int k = 0; k < gizmoGroups.Count; k++)
		{
			List<Gizmo> list2 = gizmoGroups[k];
			Gizmo gizmo2 = null;
			for (int l = 0; l < list2.Count; l++)
			{
				if (!list2[l].Disabled)
				{
					gizmo2 = list2[l];
					break;
				}
			}
			if (gizmo2 == null)
			{
				gizmo2 = list2.FirstOrDefault();
			}
			else if (gizmo2 is Command_Toggle command_Toggle)
			{
				if (!command_Toggle.activateIfAmbiguous && !command_Toggle.isActive())
				{
					for (int m = 0; m < list2.Count; m++)
					{
						if (list2[m] is Command_Toggle { Disabled: false } command_Toggle2 && command_Toggle2.isActive())
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
						if (list2[n] is Command_Toggle { Disabled: false } command_Toggle3 && !command_Toggle3.isActive())
						{
							gizmo2 = list2[n];
							break;
						}
					}
				}
			}
			if (gizmo2 != null)
			{
				if (gizmo2 is Command_Ability command_Ability)
				{
					command_Ability.GroupAbilityCommands(list2);
				}
				if (gizmo2 is Command { shrinkable: not false, Visible: not false } command)
				{
					shrinkableCommands.Add(command);
				}
				if (vector.x + gizmo2.GetWidth(maxWidth) > num)
				{
					vector.x = startX;
					vector.y -= 75f + GizmoSpacing.y;
					num3++;
				}
				vector.x += gizmo2.GetWidth(maxWidth) + GizmoSpacing.x;
				firstGizmos.Add(gizmo2);
			}
		}
		if (num3 > 1 && shrinkableCommands.Count > 1)
		{
			for (int num4 = 0; num4 < shrinkableCommands.Count; num4++)
			{
				firstGizmos.Remove(shrinkableCommands[num4]);
			}
		}
		else
		{
			shrinkableCommands.Clear();
		}
		drawnHotKeys.Clear();
		customActivator = customActivatorFunc;
		Text.Font = GameFont.Tiny;
		Vector2 vector2 = new Vector2(startX, num2);
		mouseoverGizmo = null;
		Gizmo interactedGiz = null;
		Event interactedEvent = null;
		Gizmo floatMenuGiz = null;
		bool isFirst = true;
		for (int num5 = 0; num5 < firstGizmos.Count; num5++)
		{
			Gizmo gizmo3 = firstGizmos[num5];
			if (!gizmo3.Visible)
			{
				continue;
			}
			if (vector2.x + gizmo3.GetWidth(maxWidth) > num)
			{
				vector2.x = startX;
				vector2.y -= 75f + GizmoSpacing.y;
			}
			heightDrawnFrame = Time.frameCount;
			heightDrawn = (float)UI.screenHeight - vector2.y;
			bool multipleSelected2 = false;
			for (int num6 = 0; num6 < firstGizmos.Count; num6++)
			{
				if (num5 != num6 && firstGizmos[num5].ShowPawnDetailsWith(firstGizmos[num6]))
				{
					multipleSelected2 = true;
					break;
				}
			}
			GizmoResult result = gizmo3.GizmoOnGUI(parms: new GizmoRenderParms
			{
				highLight = (highlightFunc?.Invoke(gizmo3) ?? false),
				lowLight = (lowlightFunc?.Invoke(gizmo3) ?? false),
				isFirst = isFirst,
				multipleSelected = multipleSelected2
			}, topLeft: vector2, maxWidth: maxWidth);
			ProcessGizmoState(gizmo3, result, ref mouseoverGizmo);
			isFirst = false;
			GenUI.AbsorbClicksInRect(new Rect(vector2.x - 12f, vector2.y, gizmo3.GetWidth(maxWidth) + 12f, 75f + GizmoSpacing.y));
			vector2.x += gizmo3.GetWidth(maxWidth) + GizmoSpacing.x;
		}
		float x = vector2.x;
		int num7 = 0;
		for (int num8 = 0; num8 < shrinkableCommands.Count; num8++)
		{
			Command command2 = shrinkableCommands[num8];
			float getShrunkSize = command2.GetShrunkSize;
			if (vector2.x + getShrunkSize > num)
			{
				num7++;
				if (num7 > 1)
				{
					x = startX;
				}
				vector2.x = x;
				vector2.y -= getShrunkSize + 3f;
			}
			Vector2 topLeft = vector2;
			topLeft.y += getShrunkSize + 3f;
			heightDrawnFrame = Time.frameCount;
			heightDrawn = Mathf.Min(heightDrawn, (float)UI.screenHeight - topLeft.y);
			bool multipleSelected3 = false;
			for (int num9 = 0; num9 < shrinkableCommands.Count; num9++)
			{
				if (num8 != num9 && shrinkableCommands[num8].ShowPawnDetailsWith(shrinkableCommands[num9]))
				{
					multipleSelected3 = true;
					break;
				}
			}
			GizmoResult result2 = command2.GizmoOnGUIShrunk(parms: new GizmoRenderParms
			{
				highLight = (highlightFunc?.Invoke(command2) ?? false),
				lowLight = (lowlightFunc?.Invoke(command2) ?? false),
				isFirst = isFirst,
				multipleSelected = multipleSelected3
			}, topLeft: topLeft, size: getShrunkSize);
			ProcessGizmoState(command2, result2, ref mouseoverGizmo);
			isFirst = false;
			GenUI.AbsorbClicksInRect(new Rect(topLeft.x - 3f, topLeft.y, getShrunkSize + 3f, getShrunkSize + 3f));
			vector2.x += getShrunkSize + 3f;
		}
		if (interactedGiz != null)
		{
			List<Gizmo> list3 = FindMatchingGroup(interactedGiz);
			for (int num10 = 0; num10 < list3.Count; num10++)
			{
				Gizmo gizmo4 = list3[num10];
				if (gizmo4 != interactedGiz && !gizmo4.Disabled && interactedGiz.InheritInteractionsFrom(gizmo4))
				{
					gizmo4.ProcessInput(interactedEvent);
				}
			}
			interactedGiz.ProcessInput(interactedEvent);
			interactedGiz.ProcessGroupInput(interactedEvent, list3);
			Event.current.Use();
		}
		else if (floatMenuGiz != null)
		{
			List<FloatMenuOption> list4 = new List<FloatMenuOption>();
			foreach (FloatMenuOption rightClickFloatMenuOption in floatMenuGiz.RightClickFloatMenuOptions)
			{
				list4.Add(rightClickFloatMenuOption);
			}
			List<Gizmo> list5 = FindMatchingGroup(floatMenuGiz);
			for (int num11 = 0; num11 < list5.Count; num11++)
			{
				Gizmo gizmo5 = list5[num11];
				if (gizmo5 == floatMenuGiz || gizmo5.Disabled || !floatMenuGiz.InheritFloatMenuInteractionsFrom(gizmo5))
				{
					continue;
				}
				foreach (FloatMenuOption rightClickFloatMenuOption2 in gizmo5.RightClickFloatMenuOptions)
				{
					FloatMenuOption floatMenuOption = null;
					for (int num12 = 0; num12 < list4.Count; num12++)
					{
						if (list4[num12].Label == rightClickFloatMenuOption2.Label)
						{
							floatMenuOption = list4[num12];
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
		for (int num13 = 0; num13 < gizmoGroups.Count; num13++)
		{
			gizmoGroups[num13].Clear();
			SimplePool<List<Gizmo>>.Return(gizmoGroups[num13]);
		}
		gizmoGroups.Clear();
		firstGizmos.Clear();
		tmpAllGizmos.Clear();
		static List<Gizmo> FindMatchingGroup(Gizmo toMatch)
		{
			for (int num14 = 0; num14 < gizmoGroups.Count; num14++)
			{
				if (gizmoGroups[num14].Contains(toMatch))
				{
					return gizmoGroups[num14];
				}
			}
			return null;
		}
		void ProcessGizmoState(Gizmo giz, GizmoResult gizmoResult, ref Gizmo mouseoverGiz)
		{
			if (gizmoResult.State == GizmoState.Interacted || (gizmoResult.State == GizmoState.OpenedFloatMenu && giz.RightClickFloatMenuOptions.FirstOrDefault() == null))
			{
				interactedEvent = gizmoResult.InteractEvent;
				interactedGiz = giz;
			}
			else if (gizmoResult.State == GizmoState.OpenedFloatMenu)
			{
				floatMenuGiz = giz;
			}
			if ((int)gizmoResult.State >= 1)
			{
				mouseoverGiz = giz;
			}
		}
	}
}
