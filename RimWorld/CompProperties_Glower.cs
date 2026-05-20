using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_Glower : CompProperties
{
	public float overlightRadius;

	public float glowRadius = 14f;

	public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 1.45f;

	public bool colorPickerEnabled;

	public bool darklightToggle;

	public bool overrideIsCavePlant;

	public CompProperties_Glower()
	{
		compClass = typeof(CompGlower);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (Mathf.CeilToInt(glowRadius) > 40)
		{
			yield return $"{parentDef.defName} has glow radius {glowRadius} is higher than max possible value {40}";
		}
	}
}
