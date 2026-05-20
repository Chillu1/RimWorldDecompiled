using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class QuestPart_NewColony : QuestPart
{
	public string inSignal;

	public Faction otherFaction;

	public string outSignalCompleted;

	public string outSignalCancelled;

	public WorldObjectDef worldObjectDef;

	public int maxRelics = 1;

	private List<Pawn> tmpPawns = new List<Pawn>();

	public static bool IsGeneratingNewColony { get; private set; }

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		Find.MainTabsRoot.EscapeCurrentTab(playSound: false);
		Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
		Find.WindowStack.Add(new Dialog_ChooseThingsForNewColony(PostThingsSelected, 5, 5, maxRelics, 7, delegate
		{
			if (!outSignalCancelled.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(outSignalCancelled));
			}
		}));
	}

	private void PostThingsSelected(List<Thing> allThings)
	{
		Find.WindowStack.Add(new Screen_ArchonexusSettlementCinematics(delegate
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(allThings.First((Thing t) => t is Pawn)));
		}, delegate
		{
			MoveColonyUtility.PickNewColonyTile(delegate(PlanetTile choseTile)
			{
				TileChosen(choseTile, allThings);
			}, delegate
			{
				if (!outSignalCancelled.NullOrEmpty())
				{
					Find.SignalManager.SendSignal(new Signal(outSignalCancelled));
				}
			});
			ScreenFader.StartFade(Color.clear, 2f);
		}));
	}

	private void TileChosen(PlanetTile chosenTile, List<Thing> allThings)
	{
		if (ModsConfig.IdeologyActive && !Find.IdeoManager.classicMode)
		{
			tmpPawns.Clear();
			for (int i = 0; i < allThings.Count; i++)
			{
				if (allThings[i] is Pawn { IsColonist: not false } pawn)
				{
					tmpPawns.Add(pawn);
				}
			}
			Find.WindowStack.Add(new Dialog_ConfigureIdeo(tmpPawns, delegate
			{
				InitMoveColony(allThings, chosenTile);
			}, forArchonexusRestart: true));
			tmpPawns.Clear();
		}
		else
		{
			InitMoveColony(allThings, chosenTile);
		}
	}

	private void InitMoveColony(List<Thing> things, PlanetTile choseTile)
	{
		LongEventHandler.QueueLongEvent(delegate
		{
			Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(120f);
			List<Thing> list = new List<Thing>();
			list.AddRange(MoveColonyUtility.GetStartingThingsForNewColony());
			list.AddRange(things);
			IsGeneratingNewColony = true;
			Settlement settlement = MoveColonyUtility.MoveColonyAndReset(choseTile, list, otherFaction, worldObjectDef);
			IsGeneratingNewColony = false;
			CameraJumper.TryJump(MapGenerator.PlayerStartSpot, settlement.Map);
			Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
			if (!outSignalCompleted.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(outSignalCompleted));
			}
		}, "GeneratingMap", doAsynchronously: false, null);
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		base.Notify_FactionRemoved(f);
		if (otherFaction == f)
		{
			otherFaction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref otherFaction, "otherFaction");
		Scribe_Values.Look(ref outSignalCompleted, "outSignalCompleted");
		Scribe_Values.Look(ref outSignalCancelled, "outSignalCancelled");
		Scribe_Defs.Look(ref worldObjectDef, "worldObjecctDef");
		Scribe_Values.Look(ref maxRelics, "maxRelics", 0);
	}
}
