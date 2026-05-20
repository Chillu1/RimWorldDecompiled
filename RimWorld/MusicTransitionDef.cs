using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class MusicTransitionDef : Def
{
	public Type workerType = typeof(MusicTransition);

	public int priority = 100;

	public MusicSequenceDef sequence;

	public MusicDangerRequirement dangerRequirement;

	public bool overridesInterruptions;

	public override IEnumerable<string> ConfigErrors()
	{
		if (workerType != null && !typeof(MusicTransition).IsAssignableFrom(workerType))
		{
			yield return "[def: " + defName + "] Music condition type is not a subclass of MusicTransition, type was: " + workerType.FullName;
		}
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
	}
}
