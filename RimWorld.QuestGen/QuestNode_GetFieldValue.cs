using System;
using System.Reflection;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetFieldValue : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	[NoTranslate]
	public SlateRef<object> obj;

	[NoTranslate]
	public SlateRef<string> field;

	public SlateRef<Type> type;

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		return true;
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		object obj = ((type.GetValue(slate) != null) ? ConvertHelper.Convert(this.obj.GetValue(slate), type.GetValue(slate)) : this.obj.GetValue(slate));
		FieldInfo fieldInfo = obj.GetType().GetField(field.GetValue(slate), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fieldInfo == null)
		{
			Log.Error("QuestNode error: " + obj.GetType().Name + " doesn't have a field named " + field.GetValue(slate));
		}
		else
		{
			slate.Set(storeAs.GetValue(slate), fieldInfo.GetValue(obj));
		}
	}
}
