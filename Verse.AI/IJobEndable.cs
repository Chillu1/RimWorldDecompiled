using System;

namespace Verse.AI
{
	public interface IJobEndable
	{
		Pawn GetActor();

		void AddEndCondition(Func<JobCondition> newEndCondition);
	}
}
