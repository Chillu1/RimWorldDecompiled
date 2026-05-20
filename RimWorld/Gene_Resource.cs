using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Gene_Resource : Gene
{
	public float targetValue = 0.5f;

	protected float cur;

	protected float max;

	[Unsaved(false)]
	protected GeneGizmo_Resource gizmo;

	[Unsaved(false)]
	private List<IGeneResourceDrain> tmpDrainGenes = new List<IGeneResourceDrain>();

	public virtual string ResourceLabel => def.resourceLabel;

	public abstract float InitialResourceMax { get; }

	protected abstract Color BarColor { get; }

	protected abstract Color BarHighlightColor { get; }

	public abstract float MinLevelForAlert { get; }

	public virtual int ValueForDisplay => PostProcessValue(cur);

	public virtual int MaxForDisplay => PostProcessValue(max);

	public virtual float MaxLevelOffset => 0f;

	public virtual float Max => max;

	public virtual float Value
	{
		get
		{
			return cur;
		}
		set
		{
			cur = Mathf.Clamp(value, 0f, max);
		}
	}

	public virtual float ValuePercent
	{
		get
		{
			return cur / max;
		}
		set
		{
			cur = max * value;
		}
	}

	private List<IGeneResourceDrain> DrainGenes
	{
		get
		{
			tmpDrainGenes.Clear();
			List<Gene> genesListForReading = pawn.genes.GenesListForReading;
			for (int i = 0; i < genesListForReading.Count; i++)
			{
				if (genesListForReading[i] is IGeneResourceDrain geneResourceDrain && geneResourceDrain.Resource == this)
				{
					tmpDrainGenes.Add(geneResourceDrain);
				}
			}
			return tmpDrainGenes;
		}
	}

	public override void PostAdd()
	{
		base.PostAdd();
		Reset();
	}

	public void SetMax(float newMax)
	{
		max = newMax;
		cur = Mathf.Clamp(cur, 0f, max);
		SetTargetValuePct(targetValue);
	}

	public void ResetMax()
	{
		max = InitialResourceMax;
		cur = Mathf.Clamp(cur, 0f, max);
		SetTargetValuePct(targetValue);
	}

	public override void Reset()
	{
		cur = (max = InitialResourceMax);
		targetValue = 0.5f;
	}

	public virtual void SetTargetValuePct(float val)
	{
		targetValue = val * Max;
	}

	public virtual int PostProcessValue(float value)
	{
		return Mathf.RoundToInt(value * 100f);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (Active)
		{
			if (gizmo == null)
			{
				gizmo = (GeneGizmo_Resource)Activator.CreateInstance(def.resourceGizmoType, this, DrainGenes, BarColor, BarHighlightColor);
			}
			if ((Find.Selector.SelectedPawns.Count == 1 || def.showGizmoOnMultiSelect) && (!pawn.Drafted || def.showGizmoWhenDrafted))
			{
				yield return gizmo;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref cur, "cur", 0f);
		Scribe_Values.Look(ref max, "max", 0f);
		Scribe_Values.Look(ref targetValue, "targetValue", 0.5f);
	}
}
