using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.QuestGen;

public class QuestPart_MetalHell : QuestPartActivable
{
	private const int MetalHellSize = 75;

	private const int MinSpawnDistFromMetalHellCenter = 10;

	private const int SkipDelayTicks = 540;

	private const int EffectsDurationTicks = 300;

	private const int FadeOutDurationTicks = 0;

	private const int FadeInDurationTicks = 420;

	private string voidNodeTag;

	private Map metalHell;

	private Pawn pawnToSend;

	[Unsaved(false)]
	private Effecter monolithEffecter;

	[Unsaved(false)]
	private Effecter pawnEffecter;

	private int VFXStartTick => enableTick + 540 - 300;

	private int SkipTick => enableTick + 540;

	public QuestPart_MetalHell()
	{
	}

	public QuestPart_MetalHell(string activateMonolithSignal, string voidNodeTag)
	{
		inSignalEnable = activateMonolithSignal;
		this.voidNodeTag = voidNodeTag;
		reactivatable = true;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignalEnable)
		{
			signal.args.TryGetArg("ACTIVATOR", out pawnToSend);
		}
		base.Notify_QuestSignalReceived(signal);
	}

	public override void QuestPartTick()
	{
		if (pawnToSend.Downed || pawnToSend.Dead)
		{
			Messages.Message("GleamingMonolithInterrupted".Translate(pawnToSend.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
			Disable();
			return;
		}
		if (Find.TickManager.TicksGame > VFXStartTick && Find.TickManager.TicksGame < SkipTick)
		{
			if (monolithEffecter == null)
			{
				monolithEffecter = EffecterDefOf.AbductWarmup_Monolith.Spawn(Find.Anomaly.monolith, Find.Anomaly.monolith.Map);
			}
			if (pawnEffecter == null)
			{
				pawnEffecter = EffecterDefOf.AbductWarmup_Pawn.Spawn(pawnToSend, pawnToSend.Map);
			}
			pawnEffecter.EffectTick(pawnToSend, pawnToSend);
			monolithEffecter.EffectTick(Find.Anomaly.monolith, Find.Anomaly.monolith);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
		}
		if (Find.TickManager.TicksGame == SkipTick)
		{
			ScreenFader.StartFade(Color.black, 0f);
		}
		if (Find.TickManager.TicksGame == SkipTick)
		{
			GenerateMetalHellAndSkipPawn();
			ScreenFader.StartFade(Color.clear, 7f);
			Disable();
		}
	}

	private void GenerateMetalHellAndSkipPawn()
	{
		if (metalHell == null)
		{
			metalHell = PocketMapUtility.GeneratePocketMap(new IntVec3(75, 1, 75), MapGeneratorDefOf.MetalHell, null, Find.Anomaly.monolith.Map);
		}
		Thing voidNode = metalHell.listerThings.ThingsOfDef(ThingDefOf.VoidNode).FirstOrDefault();
		QuestUtility.AddQuestTag(ref voidNode.questTags, voidNodeTag);
		IntVec3 cell = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(metalHell) && !c.InHorDistOf(metalHell.Center, 10f) && metalHell.reachability.CanReach(c, voidNode, PathEndMode.Touch, TraverseMode.PassDoors), metalHell);
		SkipUtility.SkipTo(pawnToSend, cell, metalHell);
		pawnToSend.jobs.EndCurrentJob(JobCondition.InterruptForced);
		pawnToSend.drafter.Drafted = true;
		CameraJumper.TryJump(pawnToSend, CameraJumper.MovementMode.Cut);
		Find.TickManager.slower.SignalForceNormalSpeed();
		Find.LetterStack.ReceiveLetter("VoidAwakeningFinalStructuresActivatedLabel".Translate(pawnToSend.Named("STUDIER")), "VoidAwakeningFinalStructuresActivatedText".Translate(pawnToSend.Named("STUDIER")), LetterDefOf.NeutralEvent, pawnToSend, null, null, null, null, 0, playSound: false);
		Find.MusicManagerPlay.CheckTransitions();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.Saving && metalHell != null && !Find.World.pocketMaps.Contains(metalHell.Parent))
		{
			metalHell = null;
		}
		Scribe_Values.Look(ref voidNodeTag, "voidNodeTag");
		Scribe_References.Look(ref metalHell, "metalHell");
		Scribe_References.Look(ref pawnToSend, "pawnToSend");
	}

	public override void Cleanup()
	{
		base.Cleanup();
		metalHell = null;
		pawnToSend = null;
	}
}
