using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Gene_Hemogen : Gene_Resource, IGeneResourceDrain
{
	public bool hemogenPacksAllowed = true;

	public Gene_Resource Resource => this;

	public Pawn Pawn => pawn;

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

	public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

	public float ResourceLossPerDay => def.resourceLossPerDay;

	public override float InitialResourceMax => 1f;

	public override float MinLevelForAlert => 0.15f;

	public override float MaxLevelOffset => 0.1f;

	protected override Color BarColor => new ColorInt(138, 3, 3).ToColor;

	protected override Color BarHighlightColor => new ColorInt(145, 42, 42).ToColor;

	public override void PostAdd()
	{
		if (ModLister.CheckBiotech("Hemogen"))
		{
			base.PostAdd();
			Reset();
		}
	}

	public override void Notify_IngestedThing(Thing thing, int numTaken)
	{
		if (thing.def.IsMeat)
		{
			IngestibleProperties ingestible = thing.def.ingestible;
			if (ingestible != null && ingestible.sourceDef?.race?.Humanlike == true)
			{
				GeneUtility.OffsetHemogen(pawn, 0.0375f * thing.GetStatValue(StatDefOf.Nutrition) * (float)numTaken);
			}
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		GeneResourceDrainUtility.TickResourceDrainInterval(this, delta);
	}

	public override void SetTargetValuePct(float val)
	{
		targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
	}

	public bool ShouldConsumeHemogenNow()
	{
		return Value < targetValue;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (!Active)
		{
			yield break;
		}
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo resourceDrainGizmo in GeneResourceDrainUtility.GetResourceDrainGizmos(this))
		{
			yield return resourceDrainGizmo;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref hemogenPacksAllowed, "hemogenPacksAllowed", defaultValue: true);
	}
}
