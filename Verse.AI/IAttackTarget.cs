namespace Verse.AI
{
	public interface IAttackTarget : ILoadReferenceable
	{
		Thing Thing
		{
			get;
		}

		LocalTargetInfo TargetCurrentlyAimingAt
		{
			get;
		}

		float TargetPriorityFactor
		{
			get;
		}

		bool ThreatDisabled(IAttackTargetSearcher disabledFor);
	}
}
