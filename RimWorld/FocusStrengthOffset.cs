using Verse;

namespace RimWorld
{
	public abstract class FocusStrengthOffset
	{
		public float offset;

		public virtual string GetExplanation(Thing parent)
		{
			return "";
		}

		public virtual string GetExplanationAbstract(ThingDef def = null)
		{
			return "";
		}

		public virtual string InspectStringExtra(Thing parent, Pawn user = null)
		{
			return "";
		}

		public virtual float GetOffset(Thing parent, Pawn user = null)
		{
			return 0f;
		}

		public virtual bool CanApply(Thing parent, Pawn user = null)
		{
			return true;
		}

		public virtual void PostDrawExtraSelectionOverlays(Thing parent, Pawn user = null)
		{
		}

		public virtual float MaxOffset(bool forAbstract = false)
		{
			return offset;
		}
	}
}
