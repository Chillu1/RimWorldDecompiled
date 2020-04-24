using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class RoomRequirement
	{
		[NoTranslate]
		public string labelKey;

		public abstract bool Met(Room r, Pawn p = null);

		public virtual string Label(Room r = null)
		{
			return labelKey.Translate();
		}

		public string LabelCap(Room r = null)
		{
			return Label(r).CapitalizeFirst();
		}

		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}

		public virtual bool SameOrSubsetOf(RoomRequirement other)
		{
			return GetType() == other.GetType();
		}

		public virtual bool PlayerHasResearched()
		{
			return true;
		}
	}
}
