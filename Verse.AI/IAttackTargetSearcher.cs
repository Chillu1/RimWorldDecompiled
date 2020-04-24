namespace Verse.AI
{
	public interface IAttackTargetSearcher
	{
		Thing Thing
		{
			get;
		}

		Verb CurrentEffectiveVerb
		{
			get;
		}

		LocalTargetInfo LastAttackedTarget
		{
			get;
		}

		int LastAttackTargetTick
		{
			get;
		}
	}
}
