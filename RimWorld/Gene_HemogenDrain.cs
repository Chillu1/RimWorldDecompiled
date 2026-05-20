using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Gene_HemogenDrain : Gene, IGeneResourceDrain
{
	[Unsaved(false)]
	private Gene_Hemogen cachedHemogenGene;

	private const float MinAgeForDrain = 3f;

	public Gene_Resource Resource
	{
		get
		{
			if (cachedHemogenGene == null || !cachedHemogenGene.Active)
			{
				cachedHemogenGene = pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();
			}
			return cachedHemogenGene;
		}
	}

	public bool CanOffset
	{
		get
		{
			if (Active)
			{
				return !pawn.Deathresting;
			}
			return false;
		}
	}

	public float ResourceLossPerDay => def.resourceLossPerDay;

	public Pawn Pawn => pawn;

	public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		GeneResourceDrainUtility.TickResourceDrainInterval(this, delta);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!Active)
		{
			yield break;
		}
		foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
		{
			yield return resourceDrainGizmo;
		}
	}
}
