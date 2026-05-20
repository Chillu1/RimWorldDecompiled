using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompBandNode : ThingComp
{
	private static readonly Material UntunedMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.red);

	private static readonly Material TuningMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.yellow);

	private static readonly Material TunedMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.green);

	private static readonly Vector2 TuningBarSize = new Vector3(0.255f, 0.035f);

	private const float TuningBarYOffset = -0.4f;

	public Pawn tunedTo;

	public int tuningTimeLeft;

	public Pawn tuningTo;

	private Effecter effecter;

	private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

	public CompProperties_BandNode Props => (CompProperties_BandNode)props;

	private int RetuneTimeTicks => (int)(60000f * Props.retuneDays);

	private int TuningTimeTicks => (int)(Props.tuneSeconds * 60f);

	private BandNodeState State
	{
		get
		{
			if (tunedTo != null && tuningTo != null)
			{
				return BandNodeState.Retuning;
			}
			if (tuningTo != null)
			{
				return BandNodeState.Tuning;
			}
			if (tunedTo != null)
			{
				return BandNodeState.Tuned;
			}
			return BandNodeState.Untuned;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckBiotech("Band node"))
		{
			parent.Destroy();
		}
		else
		{
			base.PostSpawnSetup(respawningAfterLoad);
		}
	}

	public override void PostExposeData()
	{
		Scribe_References.Look(ref tunedTo, "tunedTo");
		Scribe_References.Look(ref tuningTo, "tuningTo");
		Scribe_Values.Look(ref tuningTimeLeft, "tuningTimeLeft", 0);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = ((tunedTo == null) ? ("BandNodeTuneTo".Translate() + "...") : ("BandNodeRetuneTo".Translate() + "..."));
		command_Action.defaultDesc = ((tunedTo == null) ? "BandNodeTuningDesc".Translate("PeriodSeconds".Translate(Props.tuneSeconds)) : "BandNodeRetuningDesc".Translate(Props.retuneDays + " " + "Days".Translate()));
		command_Action.onHover = (Action)Delegate.Combine(command_Action.onHover, (Action)delegate
		{
			Pawn pawn = ((tuningTo != null) ? tuningTo : tunedTo);
			if (pawn != null)
			{
				GenDraw.DrawLineBetween(parent.DrawPos, pawn.DrawPos);
			}
		});
		bool flag = false;
		foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
		{
			if (MechanitorUtility.IsMechanitor(item) && tunedTo != item && tuningTo != item)
			{
				flag = true;
				break;
			}
		}
		command_Action.Disabled = !flag;
		command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning");
		command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Pawn pawn in parent.Map.mapPawns.AllPawnsSpawned)
			{
				if (MechanitorUtility.IsMechanitor(pawn))
				{
					IEnumerable<CompBandNode> bandNodes = from t in Find.Selector.SelectedObjects
						where t is Thing thing && thing.TryGetComp<CompBandNode>() != null
						select ((Thing)t).TryGetComp<CompBandNode>() into n
						where n.tunedTo != pawn && n.tuningTo != pawn
						select n;
					if (bandNodes.Any())
					{
						Pawn localPawn = pawn;
						string text = pawn.Name.ToStringFull;
						if (bandNodes.All((CompBandNode b) => b.tunedTo == null) || bandNodes.All((CompBandNode b) => b.tunedTo != null))
						{
							text = ((tunedTo != null) ? (text + " (" + RetuneTimeTicks.ToStringTicksToPeriod() + ")") : ((string)(text + (" (" + Props.tuneSeconds + " " + "SecondsLower".Translate() + ")"))));
						}
						list.Add(new FloatMenuOption(text, delegate
						{
							foreach (CompBandNode item2 in bandNodes)
							{
								item2.TuneTo(localPawn);
							}
						}));
					}
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		});
		yield return command_Action;
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: complete tuning";
			command_Action2.action = delegate
			{
				tuningTimeLeft = 0;
			};
			yield return command_Action2;
		}
	}

	public void TuneTo(Pawn pawn)
	{
		tuningTimeLeft = ((tunedTo == null) ? TuningTimeTicks : RetuneTimeTicks);
		tuningTo = pawn;
	}

	public override void PostDraw()
	{
		base.PostDraw();
		Material material = State switch
		{
			BandNodeState.Tuned => TunedMaterial, 
			BandNodeState.Untuned => UntunedMaterial, 
			_ => TuningMaterial, 
		};
		Vector3 s = new Vector3(TuningBarSize.x, 1f, TuningBarSize.y);
		Vector3 pos = parent.DrawPos + new Vector3(0f, 0f, -0.4f);
		pos.y = parent.def.altitudeLayer.AltitudeFor() + 0.03658537f;
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
	}

	public override void CompTick()
	{
		PowerTrader.PowerOutput = ((tunedTo == null && tuningTo == null) ? ((float)(-Props.powerConsumptionIdle)) : (0f - PowerTrader.Props.PowerConsumption));
		if (tunedTo != null && tunedTo.Dead)
		{
			tunedTo = null;
		}
		if (tuningTo != null && tuningTo.Dead)
		{
			tuningTo = null;
		}
		if (PowerTrader != null && !PowerTrader.PowerOn)
		{
			effecter?.Cleanup();
			effecter = null;
			return;
		}
		if (tuningTo != null)
		{
			tuningTimeLeft--;
			if (tuningTimeLeft <= 0)
			{
				tunedTo = tuningTo;
				tuningTo = null;
				if (Props.tuningCompleteSound != null)
				{
					Props.tuningCompleteSound.PlayOneShot(parent);
				}
			}
		}
		if (tuningTo == null && tunedTo != null && !tunedTo.health.hediffSet.HasHediff(Props.hediff))
		{
			tunedTo.health.AddHediff(Props.hediff, tunedTo.health.hediffSet.GetBrain());
		}
		if (State == BandNodeState.Untuned)
		{
			if (effecter == null || effecter.def != Props.untunedEffect)
			{
				effecter?.Cleanup();
				effecter = Props.untunedEffect.Spawn();
			}
		}
		else if (State == BandNodeState.Tuned)
		{
			if (effecter == null || effecter.def != Props.tunedEffect)
			{
				effecter?.Cleanup();
				effecter = Props.tunedEffect.Spawn();
			}
		}
		else if (State == BandNodeState.Tuning)
		{
			if (effecter == null || effecter.def != Props.tuningEffect)
			{
				effecter?.Cleanup();
				effecter = Props.tuningEffect.Spawn();
			}
		}
		else if (State == BandNodeState.Retuning)
		{
			if (effecter == null || effecter.def != Props.retuningEffect)
			{
				effecter?.Cleanup();
				effecter = Props.retuningEffect.Spawn();
			}
		}
		else
		{
			effecter?.Cleanup();
			effecter = null;
		}
		if (effecter != null)
		{
			effecter.EffectTick(parent, parent);
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = null;
		if (!PowerTrader.PowerOn)
		{
			text = "\n" + "Unpowered".Translate().CapitalizeFirst().Resolve();
		}
		if (tuningTo != null)
		{
			return "BandNodeTuningTo".Translate() + ": " + tuningTo.Name.ToStringFull + " - " + tuningTimeLeft.ToStringTicksToPeriod() + text;
		}
		return "BandNodeTunedTo".Translate() + ": " + ((tunedTo == null) ? "Nobody".Translate().Resolve() : tunedTo.Name.ToStringFull) + text;
	}
}
