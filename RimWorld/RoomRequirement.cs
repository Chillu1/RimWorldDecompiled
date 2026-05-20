using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class RoomRequirement : IExposable
	{
		public List<PreceptDef> disablingPrecepts;

		[NoTranslate]
		public string labelKey;

		public bool MetOrDisabled(Room room, Pawn p = null)
		{
			if (!Disabled(room, p))
			{
				return Met(room, p);
			}
			return true;
		}

		public virtual bool Disabled(Room room, Pawn p = null)
		{
			if (disablingPrecepts != null && p != null && p.Ideo != null)
			{
				foreach (Precept item in p.Ideo.PreceptsListForReading)
				{
					if (disablingPrecepts.Contains(item.def))
					{
						return true;
					}
				}
			}
			return false;
		}

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
			return Enumerable.Empty<string>();
		}

		public virtual bool SameOrSubsetOf(RoomRequirement other)
		{
			return GetType() == other.GetType();
		}

		public virtual bool PlayerCanBuildNow()
		{
			return true;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref labelKey, "labelKey");
		}
	}
}
