using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptColonistWithTitle : QuestPart_RequirementsToAccept
	{
		public RoyalTitleDef minimumTitle;

		public Faction faction;

		public override bool RequiresAccepter => true;

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				yield return new Dialog_InfoCard.Hyperlink(minimumTitle, faction);
			}
		}

		public override AcceptanceReport CanAccept()
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
			{
				if (CanPawnAccept(allMapsCaravansAndTravelingTransportPods_Alive_Colonist))
				{
					return true;
				}
			}
			return new AcceptanceReport("QuestNoColonistWithTitle".Translate(minimumTitle.GetLabelCapForBothGenders()));
		}

		public override bool CanPawnAccept(Pawn p)
		{
			if (p.royalty == null)
			{
				return false;
			}
			RoyalTitleDef currentTitle = p.royalty.GetCurrentTitle(faction);
			if (currentTitle == null)
			{
				return false;
			}
			return faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(currentTitle) >= faction.def.RoyalTitlesAwardableInSeniorityOrderForReading.IndexOf(minimumTitle);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref minimumTitle, "minimumTitle");
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
