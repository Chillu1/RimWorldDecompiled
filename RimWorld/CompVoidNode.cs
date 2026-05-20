using System.Collections.Generic;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompVoidNode : CompInteractable
{
	private const int SlowFade = 300;

	private const int QuickFade = 24;

	private const int MusicIntroLengthTicks = 260;

	private bool activated;

	private Pawn interactedPawn;

	private bool embracing;

	private int closeTick = -99999;

	private Sustainer ambientSustainer;

	private float AgeSecs()
	{
		return (float)(Find.TickManager.TicksGame - parent.TickSpawned) / 60f;
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Void node"))
		{
			parent.Destroy();
			return;
		}
		base.PostSpawnSetup(respawningAfterLoad);
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			parent.Graphic.MatSingle.SetFloat(ShaderPropertyIDs.VoidNodeDestructionStartAge, float.MaxValue);
			ambientSustainer = SoundDefOf.VoidNode_Ambient.TrySpawnSustainer(SoundInfo.InMap(parent));
		});
	}

	public override void CompTick()
	{
		base.CompTick();
		if (closeTick <= 0)
		{
			return;
		}
		ambientSustainer.End();
		if (Find.TickManager.TicksGame + 260 == closeTick)
		{
			Find.MusicManagerPlay.ForcePlaySong(SongDefOf.AnomalyCreditsSong, ignorePrefsVolume: false);
		}
		if (Find.TickManager.TicksGame == closeTick)
		{
			Find.TickManager.Pause();
			Thing.allowDestroyNonDestroyable = true;
			parent.Destroy();
			Thing.allowDestroyNonDestroyable = false;
			CameraJumper.TryJump(Find.Anomaly.monolith, CameraJumper.MovementMode.Cut);
			if (embracing)
			{
				VoidAwakeningUtility.EmbraceTheVoid(interactedPawn);
			}
			else
			{
				VoidAwakeningUtility.DisruptTheLink(interactedPawn);
			}
		}
	}

	private void OpenDialog(Pawn pawn)
	{
		Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(new DiaNode("VoidNodeLetter".Translate(pawn.Named("PAWN")))
		{
			options = 
			{
				new DiaOption("VoidNodeDisrupt".Translate())
				{
					action = DisruptNode,
					resolveTree = true
				},
				new DiaOption("VoidNodeEmbrace".Translate())
				{
					action = delegate
					{
						CloseNode(embraceVoid: true, 300, 0);
					},
					resolveTree = true
				},
				new DiaOption("VoidNodePostpone".Translate())
				{
					resolveTree = true
				}
			}
		});
		dialog_NodeTree.forcePause = true;
		Find.WindowStack.Add(dialog_NodeTree);
	}

	private void DisruptNode()
	{
		SoundDefOf.VoidNode_Explode.PlayOneShotOnCamera();
		EffecterDefOf.VoidNodeDisrupted.SpawnMaintained(parent, parent.Map);
		EffecterDefOf.VoidNodeDisruptedFlashes.SpawnMaintained(parent, parent.Map);
		parent.Graphic.MatSingle.SetFloat(ShaderPropertyIDs.VoidNodeDestructionStartAge, AgeSecs());
		CloseNode(embraceVoid: false, 24, 600);
	}

	private void CloseNode(bool embraceVoid, int fadeTimeTicks, int fadeDelayTicks)
	{
		closeTick = Find.TickManager.TicksGame + fadeTimeTicks + fadeDelayTicks;
		embracing = embraceVoid;
		activated = true;
		QuestUtility.SendQuestTargetSignals(parent.questTags, "NodeClosed");
		Find.TickManager.slower.SignalForceNormalSpeed();
		Find.MusicManagerPlay.ForceSilenceFor(999f);
		CameraJumper.TryJump(parent, CameraJumper.MovementMode.Cut);
		ScreenFader.StartFade(Color.white, (float)(fadeTimeTicks - 5) / 60f, (float)fadeDelayTicks / 60f);
	}

	protected override void OnInteracted(Pawn caster)
	{
		interactedPawn = caster;
		OpenDialog(caster);
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (activated)
		{
			return false;
		}
		return base.CanInteract(activateBy, checkOptionalItems);
	}

	public void Reset()
	{
		activated = false;
	}

	public override string CompInspectStringExtra()
	{
		if (activated)
		{
			return "";
		}
		return base.CompInspectStringExtra();
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (activated)
		{
			yield break;
		}
		foreach (FloatMenuOption item in base.CompFloatMenuOptions(selPawn))
		{
			yield return item;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (activated)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref activated, "activated", defaultValue: false);
		Scribe_References.Look(ref interactedPawn, "interactedPawn");
		Scribe_Values.Look(ref embracing, "embracing", defaultValue: false);
		Scribe_Values.Look(ref closeTick, "closeTick", 0);
	}
}
