using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class RitualOutcomeComp
{
	[MustTranslate]
	protected string label;

	[MustTranslate]
	protected string labelAbstract;

	protected float qualityOffset;

	public virtual bool DataRequired => true;

	public virtual float QualityOffset(LordJob_Ritual ritual, RitualOutcomeComp_Data data)
	{
		return 0f;
	}

	public virtual RitualOutcomeComp_Data MakeData()
	{
		return null;
	}

	public abstract bool Applies(LordJob_Ritual ritual);

	public virtual void Tick(LordJob_Ritual ritual, RitualOutcomeComp_Data data, float progressAmount)
	{
	}

	public virtual string GetDesc(LordJob_Ritual ritual = null, RitualOutcomeComp_Data data = null)
	{
		return label;
	}

	public virtual string GetDescAbstract(bool positive, float quality = -1f)
	{
		return label;
	}

	public virtual string GetBonusDescShort()
	{
		return label;
	}

	public virtual QualityFactor GetQualityFactor(Precept_Ritual ritual, TargetInfo ritualTarget, RitualObligation obligation, RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
		return null;
	}

	protected virtual string ExpectedOffsetDesc(bool positive, float quality = 0f)
	{
		return "";
	}

	public virtual void Notify_AssignmentsChanged(RitualRoleAssignments assignments, RitualOutcomeComp_Data data)
	{
	}

	public virtual IEnumerable<string> BlockingIssues(Precept_Ritual ritual, TargetInfo target, RitualRoleAssignments assignments)
	{
		yield break;
	}
}
