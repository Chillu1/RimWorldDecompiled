using Verse;

namespace RimWorld
{
	public abstract class AbilityComp
	{
		public Ability parent;

		public AbilityCompProperties props;

		public virtual void Initialize(AbilityCompProperties props)
		{
			this.props = props;
		}

		public override string ToString()
		{
			return string.Concat(GetType().Name, "(parent=", parent, " at=", (parent != null) ? parent.pawn.Position : IntVec3.Invalid, ")");
		}

		public virtual bool GizmoDisabled(out string reason)
		{
			reason = null;
			return false;
		}
	}
}
