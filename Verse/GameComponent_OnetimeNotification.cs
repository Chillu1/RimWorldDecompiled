using RimWorld;
using RimWorld.Planet;

namespace Verse
{
	public class GameComponent_OnetimeNotification : GameComponent
	{
		public bool sendAICoreRequestReminder = true;

		public GameComponent_OnetimeNotification(Game game)
		{
		}

		public override void GameComponentTick()
		{
			if (Find.TickManager.TicksGame % 2000 == 0 && Rand.Chance(0.05f) && sendAICoreRequestReminder && ResearchProjectTagDefOf.ShipRelated.CompletedProjects() >= 2 && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(ThingDefOf.AIPersonaCore) && !PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas(ThingDefOf.Ship_ComputerCore))
			{
				Faction faction = Find.FactionManager.RandomNonHostileFaction();
				if (faction != null && faction.leader != null)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelAICoreOffer".Translate(), "LetterAICoreOffer".Translate(faction.leader.LabelDefinite(), faction.Name, faction.leader.Named("PAWN")).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, faction);
					sendAICoreRequestReminder = false;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref sendAICoreRequestReminder, "sendAICoreRequestReminder", defaultValue: false);
		}
	}
}
