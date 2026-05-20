using System;
using Verse;

namespace RimWorld;

public class QuestPart_Filter_ArgNotEqual : QuestPart_Filter
{
	public string name;

	public object obj;

	private Type objType;

	private LookMode objLookMode;

	protected override bool Pass(SignalArgs args)
	{
		if (args.TryGetArg(name, out var arg))
		{
			return !object.Equals(obj, arg.arg);
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref name, "name");
		Scribe_Universal.Look(ref obj, "obj", ref objLookMode, ref objType);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		name = "test";
		obj = "value";
	}
}
