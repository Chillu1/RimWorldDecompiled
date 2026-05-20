using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Gene : IExposable, ILoadReferenceable
{
	public GeneDef def;

	public Pawn pawn;

	public int loadID;

	public Gene overriddenByGene;

	public Passion? passionPreAdd;

	public virtual string Label => def.label;

	public virtual string LabelCap => Label.CapitalizeFirst();

	public bool Overridden => overriddenByGene != null;

	public virtual bool Active
	{
		get
		{
			if (Overridden)
			{
				return false;
			}
			if (pawn?.ageTracker != null && (float)pawn.ageTracker.AgeBiologicalYears < def.minAgeActive)
			{
				return false;
			}
			if (pawn?.mutant != null && pawn.mutant.Def.disablesGenes.Contains(def))
			{
				return false;
			}
			return true;
		}
	}

	public IEnumerable<WorkTypeDef> DisabledWorkTypes
	{
		get
		{
			if (!Active)
			{
				yield break;
			}
			List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				if ((def.disabledWorkTags & list[i].workTags) != WorkTags.None)
				{
					yield return list[i];
				}
			}
		}
	}

	public virtual void PostMake()
	{
	}

	public virtual void PostAdd()
	{
		if (def.HasDefinedGraphicProperties)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public virtual void PostRemove()
	{
		if (def.HasDefinedGraphicProperties)
		{
			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}

	public virtual void Tick()
	{
	}

	public virtual void TickInterval(int delta)
	{
		if (ModsConfig.BiotechActive && def.mentalBreakMtbDays > 0f && def.mentalBreakDef != null && pawn.Spawned && pawn.IsHashIntervalTick(60, delta) && !pawn.InMentalState && !pawn.Downed && Rand.MTBEventOccurs(def.mentalBreakMtbDays, 60000f, 60f) && def.mentalBreakDef.Worker.BreakCanOccur(pawn))
		{
			def.mentalBreakDef.Worker.TryStart(pawn, "MentalStateReason_Gene".Translate() + ": " + LabelCap, causedByMood: false);
		}
	}

	public void OverrideBy(Gene overriddenBy)
	{
		if (ModsConfig.BiotechActive)
		{
			overriddenByGene = overriddenBy;
		}
	}

	public Passion NewPassionForOnRemoval(SkillRecord skillRecord)
	{
		if (def.passionMod == null)
		{
			return skillRecord.passion;
		}
		switch (def.passionMod.modType)
		{
		case PassionMod.PassionModType.AddOneLevel:
			switch (skillRecord.passion)
			{
			case Passion.Major:
				return Passion.Minor;
			case Passion.Minor:
				return Passion.None;
			}
			break;
		case PassionMod.PassionModType.DropAll:
			if (passionPreAdd.HasValue)
			{
				return passionPreAdd.Value;
			}
			return skillRecord.passion;
		}
		return skillRecord.passion;
	}

	public virtual void Notify_IngestedThing(Thing thing, int numTaken)
	{
	}

	public virtual void Notify_NewColony()
	{
	}

	public virtual void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		if (def.deathHistoryEvent != null)
		{
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(def.deathHistoryEvent));
		}
	}

	public virtual void Reset()
	{
	}

	public virtual IEnumerable<Gizmo> GetGizmos()
	{
		return null;
	}

	public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		return null;
	}

	public string GetUniqueLoadID()
	{
		return "Gene_" + loadID;
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
		Scribe_References.Look(ref overriddenByGene, "overriddenByGene");
		Scribe_Values.Look(ref loadID, "loadID", 0);
		Scribe_Values.Look(ref passionPreAdd, "passionPreAdd");
	}
}
