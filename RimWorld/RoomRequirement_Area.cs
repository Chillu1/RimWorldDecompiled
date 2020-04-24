using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_Area : RoomRequirement
	{
		public int area;

		public override string Label(Room r = null)
		{
			return ((!labelKey.NullOrEmpty()) ? labelKey : "RoomRequirementArea").Translate(((r != null) ? (r.CellCount + "/") : "") + area);
		}

		public override bool Met(Room r, Pawn p = null)
		{
			return r.CellCount >= area;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (area <= 0)
			{
				yield return "area must be larger than 0";
			}
		}
	}
}
