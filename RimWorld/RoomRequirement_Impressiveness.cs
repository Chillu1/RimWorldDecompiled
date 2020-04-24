using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomRequirement_Impressiveness : RoomRequirement
	{
		public int impressiveness;

		public override string Label(Room r = null)
		{
			return (string)(((!labelKey.NullOrEmpty()) ? labelKey : "RoomRequirementImpressiveness").Translate() + " " + ((r != null) ? (Mathf.Round(r.GetStat(RoomStatDefOf.Impressiveness)) + "/") : "")) + impressiveness;
		}

		public override bool Met(Room r, Pawn p = null)
		{
			return Mathf.Round(r.GetStat(RoomStatDefOf.Impressiveness)) >= (float)impressiveness;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (impressiveness <= 0)
			{
				yield return "impressiveness must be larger than 0";
			}
		}
	}
}
