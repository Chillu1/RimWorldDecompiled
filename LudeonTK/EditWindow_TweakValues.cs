using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class EditWindow_TweakValues : EditWindow
{
	private struct TweakInfo
	{
		public FieldInfo field;

		public TweakValue tweakValue;

		public float initial;
	}

	[TweakValue("TweakValue", 0f, 300f)]
	public static float CategoryWidth = 180f;

	[TweakValue("TweakValue", 0f, 300f)]
	public static float TitleWidth = 300f;

	[TweakValue("TweakValue", 0f, 300f)]
	public static float NumberWidth = 140f;

	private Vector2 scrollPosition;

	private static List<TweakInfo> tweakValueFields;

	public override Vector2 InitialSize => new Vector2(1000f, 600f);

	public override bool IsDebug => true;

	public EditWindow_TweakValues()
	{
		optionalTitle = "TweakValues";
		if (tweakValueFields == null)
		{
			tweakValueFields = (from field in FindAllTweakables()
				select new TweakInfo
				{
					field = field,
					tweakValue = field.TryGetAttribute<TweakValue>(),
					initial = GetAsFloat(field)
				} into ti
				orderby $"{ti.tweakValue.category}.{ti.field.DeclaringType.Name}"
				select ti).ToList();
		}
	}

	private IEnumerable<FieldInfo> FindAllTweakables()
	{
		foreach (Type allType in GenTypes.AllTypes)
		{
			FieldInfo[] fields = allType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.TryGetAttribute<TweakValue>() != null)
				{
					if (!fieldInfo.IsStatic)
					{
						Log.Error($"Field {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name} is marked with TweakValue, but isn't static; TweakValue won't work");
					}
					else if (fieldInfo.IsLiteral)
					{
						Log.Error($"Field {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name} is marked with TweakValue, but is const; TweakValue won't work");
					}
					else if (fieldInfo.IsInitOnly)
					{
						Log.Error($"Field {fieldInfo.DeclaringType.FullName}.{fieldInfo.Name} is marked with TweakValue, but is readonly; TweakValue won't work");
					}
					else
					{
						yield return fieldInfo;
					}
				}
			}
		}
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		Rect rect;
		Rect outRect = (rect = inRect.ContractedBy(4f));
		rect.xMax -= 33f;
		Rect rect2 = new Rect(0f, 0f, CategoryWidth, Text.CalcHeight("test", 1000f));
		Rect rect3 = new Rect(rect2.xMax, 0f, TitleWidth, rect2.height);
		Rect rect4 = new Rect(rect3.xMax, 0f, NumberWidth, rect2.height);
		Rect rect5 = new Rect(rect4.xMax, 0f, rect.width - rect4.xMax, rect2.height);
		DevGUI.BeginScrollView(outRect, ref scrollPosition, new Rect(0f, 0f, rect.width, rect2.height * (float)tweakValueFields.Count));
		foreach (TweakInfo tweakValueField in tweakValueFields)
		{
			DevGUI.Label(rect2, tweakValueField.tweakValue.category);
			DevGUI.Label(rect3, $"{tweakValueField.field.DeclaringType.Name}.{tweakValueField.field.Name}");
			float num;
			bool flag;
			if (tweakValueField.field.FieldType == typeof(float) || tweakValueField.field.FieldType == typeof(int) || tweakValueField.field.FieldType == typeof(ushort))
			{
				float asFloat = GetAsFloat(tweakValueField.field);
				num = DevGUI.HorizontalSlider(rect5, GetAsFloat(tweakValueField.field), tweakValueField.tweakValue.min, tweakValueField.tweakValue.max);
				SetFromFloat(tweakValueField.field, num);
				flag = asFloat != num;
			}
			else if (tweakValueField.field.FieldType == typeof(bool))
			{
				bool num2 = (bool)tweakValueField.field.GetValue(null);
				bool checkOn = num2;
				DevGUI.Checkbox(new Rect(rect5.xMin, rect5.yMin, rect2.height, rect2.height), ref checkOn);
				tweakValueField.field.SetValue(null, checkOn);
				num = (checkOn ? 1 : 0);
				flag = num2 != checkOn;
			}
			else
			{
				Log.ErrorOnce($"Attempted to tweakvalue unknown field type {tweakValueField.field.FieldType}", 83944645);
				flag = false;
				num = tweakValueField.initial;
			}
			if (num != tweakValueField.initial)
			{
				GUI.color = Color.red;
				Text.WordWrap = false;
				DevGUI.Label(rect4, $"{tweakValueField.initial} -> {num}");
				Text.WordWrap = true;
				GUI.color = Color.white;
				if (DevGUI.ButtonInvisible(rect4))
				{
					flag = true;
					if (tweakValueField.field.FieldType == typeof(float) || tweakValueField.field.FieldType == typeof(int) || tweakValueField.field.FieldType == typeof(ushort))
					{
						SetFromFloat(tweakValueField.field, tweakValueField.initial);
					}
					else if (tweakValueField.field.FieldType == typeof(bool))
					{
						tweakValueField.field.SetValue(null, tweakValueField.initial != 0f);
					}
					else
					{
						Log.ErrorOnce($"Attempted to tweakvalue unknown field type {tweakValueField.field.FieldType}", 83944646);
					}
				}
			}
			else
			{
				DevGUI.Label(rect4, $"{tweakValueField.initial}");
			}
			if (flag)
			{
				MethodInfo method = tweakValueField.field.DeclaringType.GetMethod(tweakValueField.field.Name + "_Changed", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(null, null);
				}
			}
			rect2.y += rect2.height;
			rect3.y += rect2.height;
			rect4.y += rect2.height;
			rect5.y += rect2.height;
		}
		DevGUI.EndScrollView();
	}

	private float GetAsFloat(FieldInfo field)
	{
		if (field.FieldType == typeof(float))
		{
			return (float)field.GetValue(null);
		}
		if (field.FieldType == typeof(bool))
		{
			return ((bool)field.GetValue(null)) ? 1 : 0;
		}
		if (field.FieldType == typeof(int))
		{
			return (int)field.GetValue(null);
		}
		if (field.FieldType == typeof(ushort))
		{
			return (int)(ushort)field.GetValue(null);
		}
		Log.ErrorOnce($"Attempted to return unknown field type {field.FieldType} as a float", 83944644);
		return 0f;
	}

	private void SetFromFloat(FieldInfo field, float input)
	{
		if (field.FieldType == typeof(float))
		{
			field.SetValue(null, input);
		}
		else if (field.FieldType == typeof(bool))
		{
			field.SetValue(null, input != 0f);
		}
		else if (field.FieldType == typeof(int))
		{
			field.SetValue(field, (int)input);
		}
		else if (field.FieldType == typeof(ushort))
		{
			field.SetValue(field, (ushort)input);
		}
		else
		{
			Log.ErrorOnce($"Attempted to set unknown field type {field.FieldType} from a float", 83944645);
		}
	}
}
