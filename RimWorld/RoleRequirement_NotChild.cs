using Verse;

namespace RimWorld
{
	public class RoleRequirement_NotChild : RoleRequirement
	{
		public static readonly RoleRequirement_NotChild Requirement = new RoleRequirement_NotChild();

		public override string GetLabel(Precept_Role role)
		{
			return "RoleRequirementCannotBe".Translate();
		}

		public override bool Met(Pawn p, Precept_Role role)
		{
			return !p.DevelopmentalStage.Juvenile();
		}
	}
}
