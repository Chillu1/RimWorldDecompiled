using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LudeonTK;

[StaticConstructorOnStartup]
public class Dialog_DevPalette : Window_Dev
{
	private Vector2 windowPosition;

	private static List<DebugActionNode> cachedNodes;

	private int reorderableGroupID = -1;

	private Dictionary<string, string> nameCache = new Dictionary<string, string>();

	private const string Title = "Dev palette";

	private const float ButtonSize = 24f;

	private const float ButtonSize_Small = 22f;

	private const string NoActionDesc = "<i>To add commands here, open the debug actions menu and click the pin icons.</i>";

	public override bool IsDebug => true;

	protected override float Margin => 4f;

	private List<DebugActionNode> Nodes
	{
		get
		{
			if (cachedNodes == null)
			{
				cachedNodes = new List<DebugActionNode>();
				for (int i = 0; i < Prefs.DebugActionsPalette.Count; i++)
				{
					DebugActionNode node = Dialog_Debug.GetNode(Prefs.DebugActionsPalette[i]);
					if (node != null)
					{
						cachedNodes.Add(node);
					}
				}
			}
			return cachedNodes;
		}
	}

	public Dialog_DevPalette()
	{
		draggable = true;
		focusWhenOpened = false;
		drawShadow = false;
		closeOnAccept = false;
		closeOnCancel = false;
		preventCameraMotion = false;
		drawInScreenshotMode = false;
		windowPosition = Prefs.DevPalettePosition;
		onlyDrawInDevMode = true;
		doCloseX = true;
		EnsureAllNodesValid();
	}

	private void EnsureAllNodesValid()
	{
		cachedNodes = null;
		for (int num = Prefs.DebugActionsPalette.Count - 1; num >= 0; num--)
		{
			string text = Prefs.DebugActionsPalette[num];
			if (Dialog_Debug.GetNode(text) == null)
			{
				Log.Warning("Could not find node from path '" + text + "'. Removing.");
				Prefs.DebugActionsPalette.RemoveAt(num);
				Prefs.Save();
			}
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		DevGUI.Label(new Rect(inRect.x, inRect.y, inRect.width, 24f), "Dev palette");
		inRect.yMin += 26f;
		if (Prefs.DebugActionsPalette.Count == 0)
		{
			GUI.color = ColoredText.SubtleGrayColor;
			DevGUI.Label(inRect, "<i>To add commands here, open the debug actions menu and click the pin icons.</i>");
			GUI.color = Color.white;
		}
		else
		{
			if (Event.current.type == EventType.Repaint)
			{
				reorderableGroupID = ReorderableWidget.NewGroup(delegate(int from, int to)
				{
					string item = Prefs.DebugActionsPalette[from];
					Prefs.DebugActionsPalette.Insert(to, item);
					Prefs.DebugActionsPalette.RemoveAt((from < to) ? from : (from + 1));
					cachedNodes = null;
					Prefs.Save();
				}, ReorderableDirection.Vertical, inRect, 2f, null, playSoundOnStartReorder: false);
			}
			GUI.BeginGroup(inRect);
			float num = 0f;
			Text.Font = GameFont.Tiny;
			for (int num2 = 0; num2 < Nodes.Count; num2++)
			{
				DebugActionNode debugActionNode = Nodes[num2];
				float num3 = 0f;
				num3 += 22f;
				Rect rect = new Rect(num3, num, inRect.width - 44f, 22f);
				if (debugActionNode.ActiveNow)
				{
					if (debugActionNode.settingsField != null)
					{
						Rect rect2 = rect;
						rect2.xMax -= rect2.height + 4f;
						DevGUI.Label(rect2, "  " + PrettifyNodeName(debugActionNode));
						GUI.DrawTexture(new Rect(rect2.xMax, rect2.y, rect2.height, rect2.height), debugActionNode.On ? DevGUI.CheckOn : DevGUI.CheckOff);
						DevGUI.DrawHighlightIfMouseover(rect);
						if (DevGUI.ButtonInvisible(rect))
						{
							debugActionNode.Enter(null);
						}
					}
					else if (DevGUI.ButtonText(rect, "  " + PrettifyNodeName(debugActionNode), TextAnchor.MiddleLeft))
					{
						debugActionNode.Enter(Find.WindowStack.WindowOfType<Dialog_Debug>());
					}
				}
				else
				{
					DevGUI.Label(rect, "  " + PrettifyNodeName(debugActionNode));
				}
				num3 += rect.width;
				Rect rect3 = new Rect(0f, num, 22f, 22f);
				Rect rect4 = new Rect(rect3.x, rect3.y, inRect.width - 22f, 22f);
				if ((Event.current.type != EventType.MouseDown || Mouse.IsOver(rect3)) && ReorderableWidget.Reorderable(reorderableGroupID, rect4))
				{
					DevGUI.DrawRect(rect4, DevGUI.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
				}
				DevGUI.ButtonImage(rect3.ContractedBy(1f), TexButton.DragHash);
				Rect butRect = new Rect(num3, num, 22f, 22f);
				if (Widgets.ButtonImage(butRect, TexButton.Delete))
				{
					Prefs.DebugActionsPalette.RemoveAt(num2);
					Prefs.Save();
					cachedNodes = null;
					SetInitialSizeAndPosition();
				}
				num3 += butRect.width;
				num += 24f;
			}
			Text.Font = GameFont.Small;
			GUI.EndGroup();
		}
		if (!Mathf.Approximately(windowRect.x, windowPosition.x) || !Mathf.Approximately(windowRect.y, windowPosition.y))
		{
			windowPosition = new Vector2(windowRect.x, windowRect.y);
			Prefs.DevPalettePosition = windowPosition;
		}
	}

	public static void ToggleAction(string actionLabel)
	{
		if (Prefs.DebugActionsPalette.Contains(actionLabel))
		{
			Prefs.DebugActionsPalette.Remove(actionLabel);
		}
		else
		{
			Prefs.DebugActionsPalette.Add(actionLabel);
		}
		Prefs.Save();
		cachedNodes = null;
		Find.WindowStack.WindowOfType<Dialog_DevPalette>()?.SetInitialSizeAndPosition();
	}

	protected override void SetInitialSizeAndPosition()
	{
		GameFont font = Text.Font;
		Text.Font = GameFont.Small;
		Vector2 vector = new Vector2(Text.CalcSize("Dev palette").x + 48f + 10f, 28f);
		if (!Nodes.Any())
		{
			vector.x = Mathf.Max(vector.x, 200f);
			vector.y += Text.CalcHeight("<i>To add commands here, open the debug actions menu and click the pin icons.</i>", vector.x) + Margin * 2f;
		}
		else
		{
			Text.Font = GameFont.Tiny;
			for (int i = 0; i < Nodes.Count; i++)
			{
				vector.x = Mathf.Max(vector.x, Text.CalcSize("  " + PrettifyNodeName(Nodes[i]) + "  ").x + 48f);
			}
			vector.y += (float)Nodes.Count * 22f + (float)((Nodes.Count + 1) * 2) + Margin;
		}
		windowPosition.x = Mathf.Clamp(windowPosition.x, 0f, (float)UI.screenWidth - vector.x);
		windowPosition.y = Mathf.Clamp(windowPosition.y, 0f, (float)UI.screenHeight - vector.y);
		windowRect = new Rect(windowPosition.x, windowPosition.y, vector.x, vector.y);
		windowRect = windowRect.Rounded();
		Text.Font = font;
	}

	private string PrettifyNodeName(DebugActionNode node)
	{
		string path = node.Path;
		if (nameCache.TryGetValue(path, out var value))
		{
			return value;
		}
		DebugActionNode debugActionNode = node;
		value = debugActionNode.LabelNow.Replace("...", "");
		while (debugActionNode.parent != null && !debugActionNode.parent.IsRoot && (debugActionNode.parent.parent == null || !debugActionNode.parent.parent.IsRoot))
		{
			value = debugActionNode.parent.LabelNow.Replace("...", "") + "\\" + value;
			debugActionNode = debugActionNode.parent;
		}
		nameCache[path] = value;
		return value;
	}

	public override void PostClose()
	{
		base.PostOpen();
		DebugSettings.devPalette = false;
	}
}
