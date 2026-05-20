using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class CompCerebrexCore : CompInteractable
	{
		private const int FadeDurationTicks = 300;

		public const int StabilizerCount = 3;

		private static readonly Vector3 BrainDrawSize = new Vector3(7f, 7f, 7f);

		private float ZOffset = 2f;

		private float BobHeight = 0.35f;

		private const int RaidMTBHours = 48;

		private const int RaidMinTimeHours = 24;

		private bool defencesLowered;

		private bool deactivated;

		private Pawn interactedPawn;

		private bool scavenging;

		private int closeTick = -99999;

		public int stabilizersRemaining = 3;

		private Lord assemblerLord;

		private int lastRaidTick = -99999;

		[Unsaved(false)]
		private Graphic cachedBrainGraphic;

		private Graphic BrainGraphic => cachedBrainGraphic ?? (cachedBrainGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/CerebrexCore/CerebrexCore_Brain", ShaderDatabase.Cutout, BrainDrawSize, Color.white));

		public override bool Active => false;

		public Lord AssemblerLord => assemblerLord;

		public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
		{
			if (!defencesLowered)
			{
				return "CoreNotInteractableStabilizersRemaining".Translate(stabilizersRemaining);
			}
			if (deactivated)
			{
				return "CoreNotInteractableDeactivated".Translate();
			}
			return true;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref defencesLowered, "defencesLowered", defaultValue: false);
			Scribe_Values.Look(ref deactivated, "deactivated", defaultValue: false);
			Scribe_References.Look(ref interactedPawn, "interactedPawn");
			Scribe_Values.Look(ref scavenging, "scavenging", defaultValue: false);
			Scribe_Values.Look(ref closeTick, "closeTick", 0);
			Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", 0);
			Scribe_References.Look(ref assemblerLord, "assemblerLord");
			Scribe_Values.Look(ref stabilizersRemaining, "stabilizersRemaining", 0);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				assemblerLord = LordMaker.MakeNewLord(parent.Faction, new LordJob_DefendCerebrexCore(parent.Position), parent.Map);
				lastRaidTick = Find.TickManager.TicksGame;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Find.TickManager.TicksGame > lastRaidTick + 60000 && Rand.MTBEventOccurs(48f, 2500f, 1f))
			{
				List<Pawn> list = AssemblerLord.ownedPawns.ToList();
				if (list.Any())
				{
					Lord lord = LordMaker.MakeNewLord(parent.Faction, new LordJob_AssaultColony(parent.Faction), parent.Map);
					AssemblerLord.RemoveAllPawns();
					lord.AddPawns(list);
					lastRaidTick = Find.TickManager.TicksGame;
					Find.LetterStack.ReceiveLetter("MechhiveRaidLetterLabel".Translate(), "MechhiveRaidLetterText".Translate(), LetterDefOf.ThreatBig, list);
				}
			}
			if (closeTick > 0 && Find.TickManager.TicksGame == closeTick)
			{
				DeactivateCore(scavenging);
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			float z = ZOffset + 0.5f * (1f + Mathf.Sin(MathF.PI * 2f * (float)GenTicks.TicksGame / 300f)) * BobHeight;
			Matrix4x4 matrix = Matrix4x4.TRS(drawLoc + new Vector3(0f, 0.35f, z), Quaternion.AngleAxis(0f, Vector3.up), Vector3.one);
			GenDraw.DrawMeshNowOrLater(BrainGraphic.MeshAt(Rot4.South), matrix, BrainGraphic.MatSouth, drawNow: false);
		}

		private void DeactivateCore(bool scavenging)
		{
			Find.TickManager.Pause();
			QuestUtility.SendQuestTargetSignals(parent.questTags, "CoreDefeated", parent.Named("SUBJECT"));
			(Find.Scenario.AllParts.FirstOrDefault((ScenPart x) => x is ScenPart_PursuingMechanoids) as ScenPart_PursuingMechanoids)?.Notify_QuestCompleted();
			if (scavenging)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Apparel_CerebrexNode);
				GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
				Find.LetterStack.ReceiveLetter("CerebrexCoreScavengedLetterLabel".Translate(), "CerebrexCoreScavengedLetterText".Translate(interactedPawn.Named("PAWN")), LetterDefOf.PositiveEvent, thing);
				GameVictoryUtility.ShowCredits("OdysseyScavengedCredits".Translate(interactedPawn.Named("PAWN")), SongDefOf.OdysseyCreditsSong, exitToMainMenu: false, 0f);
				return;
			}
			Map map = parent.Map;
			Thing.allowDestroyNonDestroyable = true;
			parent.Destroy();
			Thing.allowDestroyNonDestroyable = false;
			GenSpawn.Spawn(ThingDefOf.CerebrexCore_Destroyed, parent.Position, map);
			Find.LetterStack.ReceiveLetter("CerebrexCoreDestroyedLetterLabel".Translate(), "CerebrexCoreDestroyedLetterText".Translate(interactedPawn.Named("PAWN")), LetterDefOf.PositiveEvent, parent);
			if (Faction.OfMechanoids != null)
			{
				Faction.OfMechanoids.deactivated = true;
			}
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
			{
				if (item == interactedPawn)
				{
					item.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.DestroyedMechhiveMainPawn);
				}
				else
				{
					item.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.DestroyedMechhive);
				}
			}
			GameVictoryUtility.ShowCredits("OdysseyDestroyedCredits".Translate(interactedPawn.Named("PAWN")), SongDefOf.OdysseyCreditsSong, exitToMainMenu: false, 0f);
		}

		public void Notify_StabilizerDisabled()
		{
			stabilizersRemaining--;
			if (stabilizersRemaining == 0 && !defencesLowered)
			{
				LowerDefences();
			}
		}

		private void LowerDefences()
		{
			defencesLowered = true;
			Find.LetterStack.ReceiveLetter("CerebrexCoreLoweredLetterLabel".Translate(), "CerebrexCoreLoweredLetterText".Translate(), LetterDefOf.PositiveEvent, parent);
		}

		protected override void OnInteracted(Pawn caster)
		{
			interactedPawn = caster;
			OpenDialog();
		}

		private void OpenDialog()
		{
			Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(new DiaNode("CerebrexCoreDialog".Translate(interactedPawn.Named("PAWN")))
			{
				options = 
				{
					new DiaOption("CerebrexCoreOption_Destroy".Translate())
					{
						action = delegate
						{
							StartCoreDeactivation(scavenge: false);
						},
						resolveTree = true
					},
					new DiaOption("CerebrexCoreOption_Scavenge".Translate())
					{
						action = delegate
						{
							StartCoreDeactivation(scavenge: true);
						},
						resolveTree = true
					},
					new DiaOption("CerebrexCoreOption_Postpone".Translate())
					{
						resolveTree = true
					}
				}
			});
			dialog_NodeTree.forcePause = true;
			Find.WindowStack.Add(dialog_NodeTree);
		}

		private void StartCoreDeactivation(bool scavenge)
		{
			deactivated = true;
			scavenging = scavenge;
			closeTick = Find.TickManager.TicksGame + 300;
			Find.TickManager.slower.SignalForceNormalSpeed();
			Find.MusicManagerPlay.ForceFadeoutAndSilenceFor(999f, 3f);
			SoundDefOf.CerebrexCore_Pain.PlayOneShotOnCamera();
			CameraJumper.TryJump(parent, CameraJumper.MovementMode.Cut);
			ScreenFader.StartFade(Color.white, 4.9166665f);
		}

		public override string CompInspectStringExtra()
		{
			return string.Format("{0}: {1}/{2}", "StabilizersRemaining".Translate(), stabilizersRemaining, 3);
		}
	}
}
