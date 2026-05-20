using Verse;

namespace RimWorld
{
	public class Thought_IdeoRoleLost : Thought_Memory
	{
		public Precept_Role Role => (Precept_Role)sourcePrecept;

		public override bool ShouldDiscard
		{
			get
			{
				if (Role != null && pawn.Ideo == Role.ideo && !Role.IsAssigned(pawn))
				{
					return base.ShouldDiscard;
				}
				return true;
			}
		}

		public override string LabelCap => base.CurStage.LabelCap.Formatted(Role.Named("ROLE"));

		public override string Description => base.CurStage.description.Formatted(Role.Named("ROLE"));
	}
}
