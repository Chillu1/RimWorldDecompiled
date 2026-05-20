using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_ThingAnyOfCount : RoomRequirement_ThingAnyOf
	{
		public int count;

		public override string Label(Room r = null)
		{
			bool flag = !labelKey.NullOrEmpty();
			string text = (flag ? ((string)labelKey.Translate()) : things[0].label);
			if (r != null)
			{
				return text + " " + ThingCount(r) + "/" + count;
			}
			if (!flag)
			{
				return GenLabel.ThingLabel(things[0], null, count);
			}
			return text + " x" + count;
		}

		public override bool Met(Room r, Pawn p = null)
		{
			return ThingCount(r) >= count;
		}

		public override bool SameOrSubsetOf(RoomRequirement other)
		{
			if (!base.SameOrSubsetOf(other))
			{
				return false;
			}
			return count <= (other as RoomRequirement_ThingAnyOfCount).count;
		}

		private int ThingCount(Room r)
		{
			int num = 0;
			foreach (Thing item in r.ContainedThingsList(things))
			{
				if (CountThing(item))
				{
					num++;
				}
			}
			return num;
		}

		protected virtual bool CountThing(Thing t)
		{
			return true;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (count <= 0)
			{
				yield return "count must be larger than 0";
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref count, "count", 0);
		}
	}
}
