using Verse;

namespace RimWorld;

public abstract class SpecialThingFilterWorker_AllowBookSkill : SpecialThingFilterWorker
{
	private readonly SkillDef skill;

	protected SpecialThingFilterWorker_AllowBookSkill(SkillDef skill)
	{
		this.skill = skill;
	}

	public override bool Matches(Thing t)
	{
		if (t is Book book && book.BookComp.TryGetDoer<BookOutcomeDoerGainSkillExp>(out var doer))
		{
			return doer.Values.ContainsKey(skill);
		}
		return false;
	}

	public override bool CanEverMatch(ThingDef def)
	{
		return def.HasComp<CompBook>();
	}
}
