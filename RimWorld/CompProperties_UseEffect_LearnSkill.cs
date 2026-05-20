namespace RimWorld;

public class CompProperties_UseEffect_LearnSkill : CompProperties_UseEffect
{
	public SkillDef skill;

	public float xpGainAmount = 50000f;

	public CompProperties_UseEffect_LearnSkill()
	{
		compClass = typeof(CompUseEffect_LearnSkill);
	}
}
