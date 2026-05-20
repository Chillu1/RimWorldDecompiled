using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse;

public class TreeNode_Editor : TreeNode
{
	public object obj;

	public FieldInfo owningField;

	public int owningIndex = -1;

	private MethodInfo editWidgetsMethod;

	public EditTreeNodeType nodeType;

	private int indexToDelete = -1;

	public object ParentObj => ((TreeNode_Editor)parentNode).obj;

	public Type ObjectType
	{
		get
		{
			if (owningField != null)
			{
				return owningField.FieldType;
			}
			if (IsListItem)
			{
				return ListRootObject.GetType().GetGenericArguments()[0];
			}
			if (obj != null)
			{
				return obj.GetType();
			}
			throw new InvalidOperationException();
		}
	}

	public bool HasValue
	{
		get
		{
			if (owningField != null)
			{
				return true;
			}
			if (IsListItem)
			{
				return true;
			}
			return false;
		}
	}

	public object Value
	{
		get
		{
			if (owningField != null)
			{
				return owningField.GetValue(ParentObj);
			}
			if (IsListItem)
			{
				return ListRootObject.GetType().GetProperty("Item").GetValue(ListRootObject, new object[1] { owningIndex });
			}
			throw new InvalidOperationException();
		}
		set
		{
			if (owningField != null)
			{
				owningField.SetValue(ParentObj, value);
			}
			if (IsListItem)
			{
				ListRootObject.GetType().GetProperty("Item").SetValue(ListRootObject, value, new object[1] { owningIndex });
			}
		}
	}

	public bool IsListItem => owningIndex >= 0;

	private object ListRootObject => ParentObj;

	public override bool Openable
	{
		get
		{
			if (obj == null)
			{
				return false;
			}
			if (nodeType == EditTreeNodeType.TerminalValue)
			{
				return false;
			}
			if (nodeType == EditTreeNodeType.ListRoot && (int)obj.GetType().GetProperty("Count").GetValue(obj, null) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool HasContentLines => nodeType != EditTreeNodeType.TerminalValue;

	public bool HasNewButton
	{
		get
		{
			if (nodeType == EditTreeNodeType.ComplexObject && obj == null)
			{
				return true;
			}
			if (owningField != null && owningField.FieldType.HasAttribute<EditorReplaceableAttribute>())
			{
				return true;
			}
			return false;
		}
	}

	public bool HasDeleteButton
	{
		get
		{
			if (IsListItem)
			{
				return true;
			}
			if (owningField != null && owningField.FieldType.HasAttribute<EditorNullableAttribute>())
			{
				return true;
			}
			return false;
		}
	}

	public string ExtraInfoText
	{
		get
		{
			if (obj == null)
			{
				return "null";
			}
			if (obj.GetType().HasAttribute<EditorShowClassNameAttribute>())
			{
				return obj.GetType().Name;
			}
			if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
			{
				int num = (int)obj.GetType().GetProperty("Count").GetValue(obj, null);
				return "(" + num + " " + ((num == 1) ? "element" : "elements") + ")";
			}
			return "";
		}
	}

	public string LabelText
	{
		get
		{
			if (owningField != null)
			{
				return owningField.Name;
			}
			if (IsListItem)
			{
				return owningIndex.ToString();
			}
			return ObjectType.Name;
		}
	}

	private TreeNode_Editor()
	{
	}

	public static TreeNode_Editor NewRootNode(object rootObj)
	{
		if (rootObj.GetType().IsValueEditable())
		{
			throw new ArgumentException();
		}
		TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
		treeNode_Editor.owningField = null;
		treeNode_Editor.obj = rootObj;
		treeNode_Editor.nestDepth = 0;
		treeNode_Editor.RebuildChildNodes();
		treeNode_Editor.InitiallyCacheData();
		return treeNode_Editor;
	}

	public static TreeNode_Editor NewChildNodeFromField(TreeNode_Editor parent, FieldInfo fieldInfo)
	{
		TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
		treeNode_Editor.parentNode = parent;
		treeNode_Editor.nestDepth = parent.nestDepth + 1;
		treeNode_Editor.owningField = fieldInfo;
		if (!fieldInfo.FieldType.IsValueEditable())
		{
			treeNode_Editor.obj = fieldInfo.GetValue(parent.obj);
			treeNode_Editor.RebuildChildNodes();
		}
		treeNode_Editor.InitiallyCacheData();
		return treeNode_Editor;
	}

	private static TreeNode_Editor NewChildNodeFromListItem(TreeNode_Editor parent, int listIndex)
	{
		TreeNode_Editor treeNode_Editor = new TreeNode_Editor();
		treeNode_Editor.parentNode = parent;
		treeNode_Editor.nestDepth = parent.nestDepth + 1;
		treeNode_Editor.owningIndex = listIndex;
		object obj = parent.obj;
		Type type = obj.GetType();
		if (!type.GetGenericArguments()[0].IsValueEditable())
		{
			object value = type.GetProperty("Item").GetValue(obj, new object[1] { listIndex });
			treeNode_Editor.obj = value;
			treeNode_Editor.RebuildChildNodes();
		}
		treeNode_Editor.InitiallyCacheData();
		return treeNode_Editor;
	}

	private void InitiallyCacheData()
	{
		if (obj != null && obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
		{
			nodeType = EditTreeNodeType.ListRoot;
		}
		else if (ObjectType.IsValueEditable())
		{
			nodeType = EditTreeNodeType.TerminalValue;
		}
		else
		{
			nodeType = EditTreeNodeType.ComplexObject;
		}
		if (obj != null)
		{
			editWidgetsMethod = obj.GetType().GetMethod("DoEditWidgets");
		}
	}

	public void RebuildChildNodes()
	{
		if (obj == null)
		{
			return;
		}
		children = new List<TreeNode>();
		Type objType = obj.GetType();
		if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(List<>))
		{
			int num = (int)objType.GetProperty("Count").GetValue(obj, null);
			for (int i = 0; i < num; i++)
			{
				TreeNode_Editor item = NewChildNodeFromListItem(this, i);
				children.Add(item);
			}
			return;
		}
		foreach (FieldInfo item3 in from f in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			orderby InheritanceDistanceBetween(objType, f.DeclaringType) descending
			select f)
		{
			if (item3.GetCustomAttributes(typeof(UnsavedAttribute), inherit: true).Length == 0 && item3.GetCustomAttributes(typeof(EditorHiddenAttribute), inherit: true).Length == 0 && !item3.IsNotSerialized)
			{
				TreeNode_Editor item2 = NewChildNodeFromField(this, item3);
				children.Add(item2);
			}
		}
	}

	private int InheritanceDistanceBetween(Type childType, Type parentType)
	{
		Type type = childType;
		int num = 0;
		do
		{
			if (type == parentType)
			{
				return num;
			}
			type = type.BaseType;
			num++;
		}
		while (!(type == null));
		Log.Error(childType?.ToString() + " is not a subclass of " + parentType);
		return -1;
	}

	public void CheckLatentDelete()
	{
		if (indexToDelete >= 0)
		{
			obj.GetType().GetMethod("RemoveAt").Invoke(obj, new object[1] { indexToDelete });
			RebuildChildNodes();
			indexToDelete = -1;
		}
	}

	public void Delete()
	{
		if (owningField != null)
		{
			owningField.SetValue(obj, null);
			return;
		}
		if (IsListItem)
		{
			((TreeNode_Editor)parentNode).indexToDelete = owningIndex;
			return;
		}
		throw new InvalidOperationException();
	}

	public void DoSpecialPreElements(Listing_TreeDefs listing)
	{
		if (obj == null)
		{
			return;
		}
		if (editWidgetsMethod != null)
		{
			WidgetRow widgetRow = listing.StartWidgetsRow(nestDepth);
			editWidgetsMethod.Invoke(obj, new object[1] { widgetRow });
		}
		if (!(obj is Editable editable))
		{
			return;
		}
		GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
		foreach (string item in editable.ConfigErrors())
		{
			listing.InfoText(item, nestDepth);
		}
		GUI.color = Color.white;
	}

	public override string ToString()
	{
		string text = "EditTreeNode(";
		if (ParentObj != null)
		{
			text = text + " owningObj=" + ParentObj;
		}
		if (owningField != null)
		{
			text = text + " owningField=" + owningField;
		}
		if (owningIndex >= 0)
		{
			text = text + " owningIndex=" + owningIndex;
		}
		return text + ")";
	}
}
