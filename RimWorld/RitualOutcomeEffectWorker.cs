using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualOutcomeEffectWorker : IExposable
{
	public RitualOutcomeEffectDef def;

	public List<RitualOutcomeComp_Data> compDatas;

	public virtual bool ApplyOnFailure => false;

	public virtual bool SupportsAttachableOutcomeEffect => false;

	public virtual bool ShowQuality => true;

	public virtual string Description => def.Description;

	public RitualOutcomeEffectWorker()
	{
	}

	public RitualOutcomeEffectWorker(RitualOutcomeEffectDef def)
	{
		this.def = def;
		FillCompData();
	}

	public void ResetCompDatas()
	{
		foreach (RitualOutcomeComp_Data compData in compDatas)
		{
			compData?.Reset();
		}
	}

	public void FillCompData()
	{
		if ((compDatas != null && compDatas.Count == def.comps.Count) || def.comps.NullOrEmpty())
		{
			return;
		}
		compDatas = new List<RitualOutcomeComp_Data>();
		foreach (RitualOutcomeComp comp in def.comps)
		{
			compDatas.Add(comp.MakeData());
		}
	}

	public RitualOutcomeComp_Data DataForComp(RitualOutcomeComp comp)
	{
		int num = def.comps.IndexOf(comp);
		if (num != -1)
		{
			return compDatas[num];
		}
		Log.Error("Can't find index for " + comp.GetType().Name);
		return null;
	}

	public abstract void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual);

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Collections.Look(ref compDatas, "compDatas", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && (compDatas.NullOrEmpty() || compDatas.Count != def.comps.Count))
		{
			FillCompData();
		}
	}

	public Thought_Memory MakeMemory(Pawn p, LordJob_Ritual ritual, ThoughtDef overrideDef = null)
	{
		Thought_Memory obj = (Thought_Memory)ThoughtMaker.MakeThought(overrideDef ?? def.memoryDef);
		obj.sourcePrecept = ritual.Ritual;
		return obj;
	}

	public virtual void Tick(LordJob_Ritual ritual, float progressAmount = 1f)
	{
		if (def.comps.NullOrEmpty())
		{
			return;
		}
		foreach (RitualOutcomeComp comp in def.comps)
		{
			comp.Tick(ritual, DataForComp(comp), progressAmount);
		}
	}

	public virtual string ExtraAlertParagraph(Precept_Ritual ritual)
	{
		return null;
	}

	public virtual string ExpectedQualityLabel()
	{
		return null;
	}

	public virtual RitualOutcomePossibility GetForcedOutcome(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		return null;
	}

	public virtual IEnumerable<string> BlockingIssues(Precept_Ritual ritual, TargetInfo target, RitualRoleAssignments assignments)
	{
		if (def.comps.NullOrEmpty())
		{
			yield break;
		}
		foreach (RitualOutcomeComp comp in def.comps)
		{
			foreach (string item in comp.BlockingIssues(ritual, target, assignments))
			{
				yield return item;
			}
		}
	}
}
