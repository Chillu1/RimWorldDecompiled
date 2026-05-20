using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse;

public class Listing_TreeDefs : Listing_Tree
{
	private float labelWidthInt;

	protected override float LabelWidth => labelWidthInt;

	public Listing_TreeDefs(float labelColumnWidth)
	{
		labelWidthInt = labelColumnWidth;
	}

	public void ContentLines(TreeNode_Editor node, int indentLevel)
	{
		node.DoSpecialPreElements(this);
		if (node.children == null)
		{
			Log.Error(node?.ToString() + " children is null.");
			return;
		}
		for (int i = 0; i < node.children.Count; i++)
		{
			Node((TreeNode_Editor)node.children[i], indentLevel, 64);
		}
	}

	private void Node(TreeNode_Editor node, int indentLevel, int openMask)
	{
		if (node.nodeType == EditTreeNodeType.TerminalValue)
		{
			node.DoSpecialPreElements(this);
			OpenCloseWidget(node, indentLevel, openMask);
			NodeLabelLeft(node, indentLevel);
			WidgetRow widgetRow = new WidgetRow(LabelWidth, curY);
			ControlButtonsRight(node, widgetRow);
			ValueEditWidgetRight(node, widgetRow.FinalX);
			EndLine();
			return;
		}
		OpenCloseWidget(node, indentLevel, openMask);
		NodeLabelLeft(node, indentLevel);
		WidgetRow widgetRow2 = new WidgetRow(LabelWidth, curY);
		ControlButtonsRight(node, widgetRow2);
		ExtraInfoText(node, widgetRow2);
		EndLine();
		if (IsOpen(node, openMask))
		{
			ContentLines(node, indentLevel + 1);
		}
		if (node.nodeType == EditTreeNodeType.ListRoot)
		{
			node.CheckLatentDelete();
		}
	}

	private void ControlButtonsRight(TreeNode_Editor node, WidgetRow widgetRow)
	{
		if (node.HasNewButton && widgetRow.ButtonIcon(TexButton.NewItem))
		{
			Action<object> addAction = delegate(object o)
			{
				node.owningField.SetValue(node.ParentObj, o);
				((TreeNode_Editor)node.parentNode).RebuildChildNodes();
			};
			MakeCreateNewObjectMenu(node, node.owningField, node.owningField.FieldType, addAction);
		}
		if (node.nodeType == EditTreeNodeType.ListRoot && widgetRow.ButtonIcon(TexButton.Add))
		{
			Type baseType = node.obj.GetType().GetGenericArguments()[0];
			Action<object> addAction2 = delegate(object o)
			{
				node.obj.GetType().GetMethod("Add").Invoke(node.obj, new object[1] { o });
			};
			MakeCreateNewObjectMenu(node, node.owningField, baseType, addAction2);
		}
		if (node.HasDeleteButton && widgetRow.ButtonIcon(TexButton.Delete, null, GenUI.SubtleMouseoverColor))
		{
			node.Delete();
		}
	}

	private void ExtraInfoText(TreeNode_Editor node, WidgetRow widgetRow)
	{
		string extraInfoText = node.ExtraInfoText;
		if (extraInfoText != "")
		{
			if (extraInfoText == "null")
			{
				GUI.color = new Color(1f, 0.6f, 0.6f, 0.5f);
			}
			else
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
			}
			widgetRow.Label(extraInfoText);
			GUI.color = Color.white;
		}
	}

	protected void NodeLabelLeft(TreeNode_Editor node, int indentLevel)
	{
		string tipText = "";
		if (node.owningField != null)
		{
			DescriptionAttribute[] array = (DescriptionAttribute[])node.owningField.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true);
			if (array.Length != 0)
			{
				tipText = array[0].description;
			}
		}
		LabelLeft(node.LabelText, tipText, indentLevel);
	}

	protected void MakeCreateNewObjectMenu(TreeNode_Editor owningNode, FieldInfo owningField, Type baseType, Action<object> addAction)
	{
		List<Type> list = baseType.InstantiableDescendantsAndSelf().ToList();
		List<FloatMenuOption> list2 = new List<FloatMenuOption>();
		foreach (Type item in list)
		{
			Type creatingType = item;
			Action action = delegate
			{
				owningNode.SetOpen(-1, val: true);
				object obj = ((!(creatingType == typeof(string))) ? Activator.CreateInstance(creatingType) : "");
				addAction(obj);
				if (owningNode != null)
				{
					owningNode.RebuildChildNodes();
				}
			};
			list2.Add(new FloatMenuOption(item.ToString(), action));
		}
		Find.WindowStack.Add(new FloatMenu(list2));
	}

	protected void ValueEditWidgetRight(TreeNode_Editor node, float leftX)
	{
		if (node.nodeType != EditTreeNodeType.TerminalValue)
		{
			throw new ArgumentException();
		}
		Rect rect = new Rect(leftX, curY, base.ColumnWidth - leftX, lineHeight);
		object obj = node.Value;
		Type objectType = node.ObjectType;
		if (objectType == typeof(string))
		{
			string text = (string)obj;
			string text2 = text;
			if (text2 == null)
			{
				text2 = "";
			}
			string text3 = text2;
			text2 = Widgets.TextField(rect, text2);
			if (text2 != text3)
			{
				text = text2;
			}
			obj = text;
		}
		else if (objectType == typeof(bool))
		{
			bool checkOn = (bool)obj;
			Widgets.Checkbox(new Vector2(rect.x, rect.y), ref checkOn, lineHeight);
			obj = checkOn;
		}
		else if (objectType == typeof(int))
		{
			rect.width = 100f;
			if (int.TryParse(Widgets.TextField(rect, obj.ToString()), out var result))
			{
				obj = result;
			}
		}
		else if (objectType == typeof(float))
		{
			EditSliderRangeAttribute[] array = (EditSliderRangeAttribute[])node.owningField.GetCustomAttributes(typeof(EditSliderRangeAttribute), inherit: true);
			if (array.Length != 0)
			{
				float value = (float)obj;
				value = Widgets.HorizontalSlider(new Rect(LabelWidth + 60f + 4f, curY, base.EditAreaWidth - 60f - 8f, lineHeight), value, array[0].min, array[0].max);
				obj = value;
			}
			rect.width = 60f;
			string text4 = obj.ToString();
			text4 = Widgets.TextField(rect, text4);
			if (float.TryParse(text4, out var result2))
			{
				obj = result2;
			}
		}
		else if (objectType.IsEnum)
		{
			if (Widgets.ButtonText(rect, obj.ToString()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (object value2 in Enum.GetValues(objectType))
				{
					object localVal = value2;
					list.Add(new FloatMenuOption(value2.ToString(), delegate
					{
						node.Value = localVal;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}
		else if (objectType == typeof(FloatRange))
		{
			float sliderMin = 0f;
			float sliderMax = 100f;
			EditSliderRangeAttribute[] array2 = (EditSliderRangeAttribute[])node.owningField.GetCustomAttributes(typeof(EditSliderRangeAttribute), inherit: true);
			if (array2.Length != 0)
			{
				sliderMin = array2[0].min;
				sliderMax = array2[0].max;
			}
			FloatRange fRange = (FloatRange)obj;
			Widgets.FloatRangeWithTypeIn(rect, rect.GetHashCode(), ref fRange, sliderMin, sliderMax);
			obj = fRange;
		}
		else if (objectType == typeof(IntRange))
		{
			int min = 0;
			int max = 100;
			EditSliderRangeAttribute[] array3 = (EditSliderRangeAttribute[])node.owningField.GetCustomAttributes(typeof(EditSliderRangeAttribute), inherit: true);
			if (array3.Length != 0)
			{
				min = Mathf.FloorToInt(array3[0].min);
				max = Mathf.FloorToInt(array3[0].max);
			}
			IntRange range = (IntRange)obj;
			Widgets.IntRange(rect, rect.GetHashCode(), ref range, min, max);
			obj = range;
		}
		else
		{
			GUI.color = new Color(1f, 1f, 1f, 0.4f);
			Widgets.Label(rect, "uneditable value type");
			GUI.color = Color.white;
		}
		node.Value = obj;
	}
}
