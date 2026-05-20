using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class JoyGiverDef : Def
{
	public Type giverClass;

	public float baseChance;

	public bool requireChair = true;

	public List<ThingDef> thingDefs;

	public JobDef jobDef;

	public bool desireSit = true;

	public float pctPawnsEverDo = 1f;

	public bool unroofedOnly;

	public JoyKindDef joyKind;

	public List<PawnCapacityDef> requiredCapacities = new List<PawnCapacityDef>();

	public bool canDoWhileInBed;

	public bool requiresEnjoyOutdoors;

	public bool countsForRecRoom = true;

	private JoyGiver workerInt;

	public JoyGiver Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (JoyGiver)Activator.CreateInstance(giverClass);
				workerInt.def = this;
			}
			return workerInt;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (jobDef != null && jobDef.joyKind != joyKind)
		{
			yield return "jobDef " + jobDef?.ToString() + " has joyKind " + jobDef.joyKind?.ToString() + " which does not match our joyKind " + joyKind;
		}
	}
}
