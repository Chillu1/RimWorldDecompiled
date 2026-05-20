using Verse;

namespace RimWorld;

public class ThoughtMemory_Inhumanized : Thought_Memory
{
	public override int CurStageIndex
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return 0;
			}
			if (!pawn.Inhumanized())
			{
				return 0;
			}
			return 1;
		}
	}
}
