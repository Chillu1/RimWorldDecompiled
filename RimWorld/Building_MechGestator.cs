using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Building_MechGestator : Building_WorkTableAutonomous
	{
		private Mote workingMote;

		private Sustainer workingSound;

		private Graphic cylinderGraphic;

		private Graphic topGraphic;

		private CompPowerTrader power;

		private CompWasteProducer wasteProducer;

		private static Material FormingCycleBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.98f, 0.46f, 0f));

		private static Material FormingCycleUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 0f, 0f));

		public Bill_Mech ActiveMechBill => base.ActiveBill as Bill_Mech;

		public CompWasteProducer WasteProducer
		{
			get
			{
				if (wasteProducer == null)
				{
					wasteProducer = GetComp<CompWasteProducer>();
				}
				return wasteProducer;
			}
		}

		public CompPowerTrader Power
		{
			get
			{
				if (power == null)
				{
					power = this.TryGetComp<CompPowerTrader>();
				}
				return power;
			}
		}

		public bool PoweredOn => Power.PowerOn;

		public bool BoundPawnStateAllowsForming
		{
			get
			{
				if (ActiveMechBill.BoundPawn != null && !ActiveMechBill.BoundPawn.Dead)
				{
					return !ActiveMechBill.BoundPawn.Suspended;
				}
				return false;
			}
		}

		public Pawn GestatingMech
		{
			get
			{
				Pawn pawn = (Pawn)innerContainer.FirstOrDefault((Thing t) => t is Pawn);
				if (pawn != null)
				{
					return pawn;
				}
				return ResurrectingMechCorpse?.InnerPawn;
			}
		}

		public Corpse ResurrectingMechCorpse => (Corpse)innerContainer.FirstOrDefault((Thing t) => t is Corpse);

		public override void PostPostMake()
		{
			if (!ModLister.CheckBiotech("Mech gestator"))
			{
				Destroy();
			}
			else
			{
				base.PostPostMake();
			}
		}

		public bool CanBeUsedNowBy(Pawn user)
		{
			if (ActiveMechBill != null)
			{
				return ActiveMechBill.BoundPawn == user;
			}
			return true;
		}

		public override void Notify_StartForming(Pawn billDoer)
		{
			SoundDefOf.MechGestatorCycle_Started.PlayOneShot(this);
		}

		public override void Notify_FormingCompleted()
		{
			Pawn pawn = activeBill.CreateProducts() as Pawn;
			Messages.Message("GestationComplete".Translate() + ": " + pawn.kindDef.LabelCap, this, MessageTypeDefOf.PositiveEvent);
			innerContainer.ClearAndDestroyContents();
			innerContainer.TryAdd(pawn);
			WasteProducer.ProduceWaste((int)pawn.GetStatValue(StatDefOf.WastepacksPerRecharge));
			SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
		}

		public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
		{
			SoundDefOf.MechGestator_MaterialInserted.PlayOneShot(this);
		}

		public override void EjectContents()
		{
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				if (innerContainer[num] is Pawn pawn)
				{
					innerContainer.RemoveAt(num);
					pawn.Destroy();
				}
			}
			base.EjectContents();
		}

		protected override void Tick()
		{
			base.Tick();
			if (activeBill != null && PoweredOn && BoundPawnStateAllowsForming)
			{
				activeBill.BillTick();
				ThingDef thingDef = null;
				if (ActiveMechBill.State == FormingState.Forming)
				{
					thingDef = def.building.gestatorFormingMote.GetForRotation(base.Rotation);
				}
				else if (ActiveMechBill.State == FormingState.Preparing && ActiveMechBill.GestationCyclesCompleted > 0)
				{
					thingDef = def.building.gestatorCycleCompleteMote.GetForRotation(base.Rotation);
				}
				else if (ActiveMechBill.State == FormingState.Formed)
				{
					thingDef = def.building.gestatorFormedMote.GetForRotation(base.Rotation);
				}
				if (thingDef != null)
				{
					if (workingMote == null || workingMote.Destroyed || workingMote.def != thingDef)
					{
						workingMote = MoteMaker.MakeAttachedOverlay(this, thingDef, Vector3.zero);
					}
					workingMote?.Maintain();
				}
			}
			if (this.IsHashIntervalTick(250))
			{
				if (activeBill != null && activeBill.State == FormingState.Forming)
				{
					Power.PowerOutput = 0f - Power.Props.PowerConsumption;
				}
				else
				{
					Power.PowerOutput = 0f - Power.Props.idlePowerDraw;
				}
			}
			if (activeBill != null && PoweredOn && activeBill.State != FormingState.Gathering)
			{
				if (workingSound == null || workingSound.Ended)
				{
					workingSound = SoundDefOf.MechGestator_Ambience.TrySpawnSustainer(this);
				}
				workingSound.Maintain();
			}
			else if (workingSound != null)
			{
				workingSound.End();
				workingSound = null;
			}
		}

		protected override string GetInspectStringExtra()
		{
			if (!(activeBill is Bill_Mech { State: FormingState.Forming } bill_Mech))
			{
				return null;
			}
			return string.Format("{0}: {1}", "GestatingInspect".Translate(), Mathf.CeilToInt(bill_Mech.formingTicks * 1f / bill_Mech.WorkSpeedMultiplier).ToStringTicksToPeriod());
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if (activeBill != null && activeBill.State != FormingState.Gathering && def.building.formingGraphicData != null)
			{
				Vector3 loc = drawLoc + def.building.formingMechPerRotationOffset[base.Rotation.AsInt];
				loc.y += 0.018292684f;
				loc.z += Mathf.PingPong((float)Find.TickManager.TicksGame * def.building.formingMechBobSpeed, def.building.formingMechYBobDistance);
				if (TryGetMechFormingGraphic(out var graphic))
				{
					graphic.Draw(loc, Rot4.South, this);
				}
				else
				{
					def.building.formingGraphicData.Graphic.Draw(loc, Rot4.North, this);
				}
			}
			GenDraw.FillableBarRequest barDrawData = base.BarDrawData;
			barDrawData.center = drawLoc;
			barDrawData.fillPercent = base.CurrentBillFormingPercent;
			barDrawData.filledMat = FormingCycleBarFilledMat;
			barDrawData.unfilledMat = FormingCycleUnfilledMat;
			barDrawData.rotation = base.Rotation;
			GenDraw.DrawFillableBar(barDrawData);
			if (topGraphic == null)
			{
				topGraphic = def.building.mechGestatorTopGraphic.GraphicColoredFor(this);
			}
			if (cylinderGraphic == null)
			{
				cylinderGraphic = def.building.mechGestatorCylinderGraphic.GraphicColoredFor(this);
			}
			Vector3 loc2 = new Vector3(drawLoc.x, AltitudeLayer.BuildingBelowTop.AltitudeFor(), drawLoc.z);
			cylinderGraphic.Draw(loc2, base.Rotation, this);
			Vector3 loc3 = new Vector3(drawLoc.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), drawLoc.z);
			topGraphic.Draw(loc3, base.Rotation, this);
		}

		private bool TryGetMechFormingGraphic(out Graphic graphic)
		{
			graphic = null;
			if (ResurrectingMechCorpse != null)
			{
				graphic = ResurrectingMechCorpse.InnerPawn.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
			}
			else if (GestatingMech != null)
			{
				graphic = GestatingMech.ageTracker.CurKindLifeStage.bodyGraphicData.Graphic;
			}
			if (graphic != null && graphic.drawSize.x <= def.building.maxFormedMechDrawSize.x && graphic.drawSize.y <= def.building.maxFormedMechDrawSize.y)
			{
				return true;
			}
			graphic = null;
			return false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = delegate
				{
					WasteProducer.ProduceWaste(5);
				};
				command_Action.defaultLabel = "DEV: Generate 5 waste";
				yield return command_Action;
				if (ActiveMechBill != null && ActiveMechBill.State != FormingState.Gathering && ActiveMechBill.State != FormingState.Formed)
				{
					Command_Action command_Action2 = new Command_Action();
					command_Action2.action = ActiveMechBill.ForceCompleteAllCycles;
					command_Action2.defaultLabel = "DEV: Complete all cycles";
					yield return command_Action2;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && ActiveMechBill is Bill_ResurrectMech { State: not FormingState.Gathering } && GestatingMech == null)
			{
				Log.Error($"Bill {base.ActiveBill} with recipe {base.ActiveBill.recipe} loaded with no mech to resurrect. Resetting bill.");
				base.ActiveBill.Reset();
			}
		}
	}
}
