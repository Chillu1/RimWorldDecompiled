using Verse;

namespace RimWorld
{
	public class RoleRequirement_SameIdeo : RoleRequirement
	{
		public override string GetLabel(Precept_Role role)
		{
			return labelKey.Translate(Find.ActiveLanguageWorker.WithIndefiniteArticle(role.ideo.memberName, Gender.None));
		}

		public override bool Met(Pawn p, Precept_Role role)
		{
			return p.Ideo == role.ideo;
		}
	}
}
