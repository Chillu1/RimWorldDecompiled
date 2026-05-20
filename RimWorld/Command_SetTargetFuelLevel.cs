using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Command_SetTargetFuelLevel : Command
{
	public CompRefuelable refuelable;

	private List<CompRefuelable> refuelables;

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		if (refuelables == null)
		{
			refuelables = new List<CompRefuelable>();
		}
		if (!refuelables.Contains(refuelable))
		{
			refuelables.Add(refuelable);
		}
		int num = int.MaxValue;
		for (int i = 0; i < refuelables.Count; i++)
		{
			if ((int)refuelables[i].Props.fuelCapacity < num)
			{
				num = (int)refuelables[i].Props.fuelCapacity;
			}
		}
		int startingValue = num / 2;
		for (int j = 0; j < refuelables.Count; j++)
		{
			if ((int)refuelables[j].TargetFuelLevel <= num)
			{
				startingValue = (int)refuelables[j].TargetFuelLevel;
				break;
			}
		}
		Func<int, string> textGetter = ((!refuelable.parent.def.building.hasFuelingPort) ? ((Func<int, string>)((int x) => "SetTargetFuelLevel".Translate(x))) : ((Func<int, string>)delegate(int x)
		{
			CompLaunchable compLaunchable = FuelingPortUtility.LaunchableAt(FuelingPortUtility.GetFuelingPortCell(refuelable.parent.Position, refuelable.parent.Rotation), refuelable.parent.Map);
			if (compLaunchable == null)
			{
				return "SetTargetFuelLevel".Translate(x);
			}
			int num2 = compLaunchable.MaxLaunchDistanceAtFuelLevel(x);
			return "SetPodLauncherTargetFuelLevel".Translate(x, num2);
		}));
		Dialog_Slider dialog_Slider = new Dialog_Slider(textGetter, 0, num, delegate(int value)
		{
			for (int k = 0; k < refuelables.Count; k++)
			{
				refuelables[k].TargetFuelLevel = value;
			}
		}, startingValue);
		if (refuelable.parent.def.building.hasFuelingPort)
		{
			dialog_Slider.extraBottomSpace = Text.LineHeight + 4f;
		}
		Find.WindowStack.Add(dialog_Slider);
	}

	public override bool InheritInteractionsFrom(Gizmo other)
	{
		if (refuelables == null)
		{
			refuelables = new List<CompRefuelable>();
		}
		refuelables.Add(((Command_SetTargetFuelLevel)other).refuelable);
		return false;
	}
}
