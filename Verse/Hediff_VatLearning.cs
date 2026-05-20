using System.Linq;
using RimWorld;

namespace Verse;

public class Hediff_VatLearning : HediffWithComps
{
	private const float XPToAward = 8000f;

	public override bool ShouldRemove
	{
		get
		{
			if (!pawn.Spawned)
			{
				return !(pawn.ParentHolder is Building_GrowthVat);
			}
			return true;
		}
	}

	public override string LabelInBrackets => Severity.ToStringPercent();

	public override void PostTickInterval(int delta)
	{
		base.PostTickInterval(delta);
		if (Severity >= def.maxSeverity)
		{
			Learn();
		}
	}

	public void Learn()
	{
		if (pawn.skills != null && pawn.skills.skills.Where((SkillRecord x) => !x.TotallyDisabled).TryRandomElement(out var result))
		{
			result.Learn(8000f, direct: true);
		}
		Severity = def.initialSeverity;
	}
}
