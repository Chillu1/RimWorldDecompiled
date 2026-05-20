using UnityEngine;

namespace Verse;

public class Hediff_Level : HediffWithComps
{
	public int level = 1;

	public override string Label
	{
		get
		{
			if (!def.levelIsQuantity)
			{
				return def.label + " (" + "LevelNum".Translate(level).ToString() + ")";
			}
			return def.label + " x" + level;
		}
	}

	public override bool ShouldRemove => level == 0;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		if (base.Part == null)
		{
			Log.Error(def.defName + " has null Part. It should be set before PostAdd.");
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		Severity = level;
	}

	public virtual void ChangeLevel(int levelOffset)
	{
		level = (int)Mathf.Clamp(level + levelOffset, def.minSeverity, def.maxSeverity);
	}

	public virtual void SetLevelTo(int targetLevel)
	{
		if (targetLevel != level)
		{
			ChangeLevel(targetLevel - level);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref level, "level", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
		{
			Log.Error(GetType().Name + " has null part after loading.");
			pawn.health.hediffSet.hediffs.Remove(this);
		}
	}
}
