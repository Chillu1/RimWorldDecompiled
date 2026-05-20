using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ScenPartDef : Def
{
	public ScenPartCategory category;

	public Type scenPartClass;

	public float summaryPriority = -1f;

	public float selectionWeight = 1f;

	public int maxUses = 999999;

	public bool canBeRandomlyAdded = true;

	public bool canBePlayerAddedRemoved = true;

	public Type pageClass;

	public GameConditionDef gameCondition;

	public bool gameConditionTargetsWorld;

	public FloatRange durationRandomRange = new FloatRange(30f, 100f);

	public Type designatorType;

	public GenStepDef genStep;

	public FactionDef preventRemovalOfFaction;

	public bool PlayerAddRemovable
	{
		get
		{
			if (category != ScenPartCategory.Fixed)
			{
				return canBePlayerAddedRemoved;
			}
			return false;
		}
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (scenPartClass == null)
		{
			yield return "scenPartClass is null";
		}
	}
}
