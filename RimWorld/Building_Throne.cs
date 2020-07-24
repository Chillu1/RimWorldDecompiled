using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_Throne : Building
	{
		private static List<RoyalTitleDef> tmpTitles = new List<RoyalTitleDef>();

		public static IEnumerable<RoyalTitleDef> AllTitlesForThroneStature => from title in DefDatabase<RoyalTitleDef>.AllDefsListForReading
			where title.MinThroneRoomImpressiveness > 0f
			orderby title.MinThroneRoomImpressiveness
			select title;

		public Pawn AssignedPawn
		{
			get
			{
				if (!ModLister.RoyaltyInstalled)
				{
					Log.ErrorOnce("Thrones are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 1222185);
					return null;
				}
				if (CompAssignableToPawn == null || !CompAssignableToPawn.AssignedPawnsForReading.Any())
				{
					return null;
				}
				return CompAssignableToPawn.AssignedPawnsForReading[0];
			}
		}

		public CompAssignableToPawn_Throne CompAssignableToPawn => GetComp<CompAssignableToPawn_Throne>();

		public RoyalTitleDef TitleStature
		{
			get
			{
				Room room = this.GetRoom();
				if (room == null || room.OutdoorsForWork)
				{
					return null;
				}
				float stat = room.GetStat(RoomStatDefOf.Impressiveness);
				RoyalTitleDef result = null;
				foreach (RoyalTitleDef item in AllTitlesForThroneStature)
				{
					if (stat > item.MinThroneRoomImpressiveness)
					{
						result = item;
						continue;
					}
					return result;
				}
				return result;
			}
		}

		public override string GetInspectString()
		{
			string inspectString = base.GetInspectString();
			Room room = this.GetRoom();
			Pawn p = (CompAssignableToPawn.AssignedPawnsForReading.Count == 1) ? CompAssignableToPawn.AssignedPawnsForReading[0] : null;
			RoyalTitleDef titleStature = TitleStature;
			inspectString += "\n" + "ThroneTitleStature".Translate((titleStature == null) ? "None".Translate() : (titleStature.GetLabelCapFor(p) + " " + "ThroneRoomImpressivenessInfo".Translate(titleStature.MinThroneRoomImpressiveness.ToString())));
			string text = RoomRoleWorker_ThroneRoom.Validate(room);
			if (text != null)
			{
				return inspectString + "\n" + text;
			}
			tmpTitles.Clear();
			tmpTitles.AddRange(AllTitlesForThroneStature);
			int num = tmpTitles.IndexOf(titleStature);
			int num2 = num - 1;
			int num3 = num + 1;
			if (num2 >= 0)
			{
				inspectString += "\n" + "ThronePrevTitleStature".Translate(tmpTitles[num2].GetLabelCapFor(p)) + " " + "ThroneRoomImpressivenessInfo".Translate(tmpTitles[num2].MinThroneRoomImpressiveness.ToString());
			}
			if (num3 < tmpTitles.Count)
			{
				inspectString += "\n" + "ThroneNextTitleStature".Translate(tmpTitles[num3].GetLabelCapFor(p)) + " " + "ThroneRoomImpressivenessInfo".Translate(tmpTitles[num3].MinThroneRoomImpressiveness.ToString());
			}
			return inspectString;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Thrones are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.  See rules on the Ludeon forum for more info.", 1222185);
				yield break;
			}
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
		}
	}
}
