using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_ArchonexusVictory_ThirdCycle : QuestNode_Root_ArchonexusVictory_Cycle
{
	private const int MinDistanceFromColony = 10;

	private const int MaxDistanceFromColony = 40;

	private static float ThreatPointsFactor = 0.6f;

	protected override int ArchonexusCycle => 3;

	protected override void RunInt()
	{
		base.RunInt();
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		float num = slate.Get("points", 0f);
		Faction faction = slate.Get<Faction>("roughTribe");
		TryFindSiteTile(out var tile);
		if (faction != null)
		{
			quest.RequirementsToAcceptFactionRelation(faction, FactionRelationKind.Ally, acceptIfDefeated: true);
		}
		TryAddStudyRequirement(quest, slate, ThingDefOf.GrandArchotechStructure);
		quest.DialogWithCloseBehavior("[questDescriptionBeforeAccepted]", null, quest.AddedSignal, null, null, QuestPart.SignalListenMode.NotYetAcceptedOnly, QuestPartDialogCloseAction.CloseActionKey.ArchonexusVictorySound3rd);
		quest.DescriptionPart("[questDescriptionBeforeAccepted]", quest.AddedSignal, quest.InitiateSignal, QuestPart.SignalListenMode.OngoingOrNotYetAccepted);
		quest.DescriptionPart("[questDescriptionAfterAccepted]", quest.InitiateSignal, null, QuestPart.SignalListenMode.OngoingOrNotYetAccepted);
		quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[questAcceptedLetterText]", null, "[questAcceptedLetterLabel]");
		float threatPoints = (Find.Storyteller.difficulty.allowViolentQuests ? (num * ThreatPointsFactor) : 0f);
		SitePartParams parms = new SitePartParams
		{
			threatPoints = threatPoints
		};
		Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.Archonexus, parms)), tile, Faction.OfAncients);
		if (num <= 0f && Find.Storyteller.difficulty.allowViolentQuests)
		{
			quest.SetSitePartThreatPointsToCurrent(site, SitePartDefOf.Archonexus, map.Parent, null, ThreatPointsFactor);
		}
		quest.SpawnWorldObject(site);
		slate.Set("factionless", faction == null);
		slate.Set("threatsEnabled", Find.Storyteller.difficulty.allowViolentQuests);
	}

	private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
	{
		return TileFinder.TryFindNewSiteTile(out tile, 10, 40, allowCaravans: false, null, 0f, canSelectComboLandmarks: true, TileFinderMode.Near, exitOnFirstTileFound, canBeSpace: false, null, (PlanetTile p) => !ModsConfig.OdysseyActive || Find.World.landmarks[p] == null);
	}

	protected override bool TestRunInt(Slate slate)
	{
		PlanetTile tile;
		if (base.TestRunInt(slate))
		{
			return TryFindSiteTile(out tile, exitOnFirstTileFound: true);
		}
		return false;
	}
}
