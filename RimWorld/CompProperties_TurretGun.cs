using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_TurretGun : CompProperties
{
	public ThingDef turretDef;

	public float angleOffset;

	public bool autoAttack = true;

	public List<PawnRenderNodeProperties> renderNodeProperties;

	public CompProperties_TurretGun()
	{
		compClass = typeof(CompTurretGun);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (renderNodeProperties.NullOrEmpty())
		{
			yield break;
		}
		foreach (PawnRenderNodeProperties renderNodeProperty in renderNodeProperties)
		{
			if (!typeof(PawnRenderNode_TurretGun).IsAssignableFrom(renderNodeProperty.nodeClass))
			{
				yield return "contains nodeClass which is not PawnRenderNode_TurretGun or subclass thereof.";
			}
		}
	}
}
