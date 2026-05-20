using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

[StaticConstructorOnStartup]
public class Building_GrowthVat : Building_Enterable, IStoreSettingsParent, IThingHolderWithDrawnPawn, IThingHolder
{
	public HumanEmbryo selectedEmbryo;

	private float embryoStarvation;

	private float containedNutrition;

	private StorageSettings allowedNutritionSettings;

	[Unsaved(false)]
	private CompPowerTrader cachedPowerComp;

	[Unsaved(false)]
	private Graphic cachedTopGraphic;

	[Unsaved(false)]
	private Graphic fetusEarlyStageGraphic;

	[Unsaved(false)]
	private Graphic fetusLateStageGraphic;

	[Unsaved(false)]
	private Sustainer sustainerWorking;

	[Unsaved(false)]
	private Hediff cachedVatLearning;

	[Unsaved(false)]
	private Effecter bubbleEffecter;

	private static readonly Texture2D CancelLoadingIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

	public static readonly CachedTexture InsertPawnIcon = new CachedTexture("UI/Gizmos/InsertPawn");

	public static readonly CachedTexture InsertEmbryoIcon = new CachedTexture("UI/Gizmos/InsertEmbryo");

	private const float BiostarvationGainPerDayNoFoodOrPower = 0.5f;

	private const float BiostarvationFallPerDayPoweredAndFed = 0.1f;

	private const float BasePawnConsumedNutritionPerDay = 3f;

	private const float BaseEmbryoConsumedNutritionPerDay = 6f;

	private const float AgeToEject = 18f;

	public const float NutritionBuffer = 10f;

	public const int AgeTicksPerTickInGrowthVat = 20;

	private const float EmbryoBirthQuality = 0.7f;

	public const int EmbryoGestationTicks = 540000;

	private const int EmbryoLateStageGraphicTicksRemaining = 540000;

	private const float FetusMinSize = 0.4f;

	private const float FetusMaxSize = 0.95f;

	private const int GlowIntervalTicks = 132;

	private static Dictionary<Rot4, ThingDef> GlowMotePerRotation;

	private static Dictionary<Rot4, EffecterDef> BubbleEffecterPerRotation;

	public bool StorageTabVisible => true;

	public float HeldPawnDrawPos_Y => DrawPos.y + 0.03658537f;

	public float HeldPawnBodyAngle => base.Rotation.AsAngle;

	public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

	public bool PowerOn => PowerTraderComp.PowerOn;

	public override Vector3 PawnDrawOffset => CompBiosculpterPod.FloatingOffset(Find.TickManager.TicksGame);

	private CompPowerTrader PowerTraderComp
	{
		get
		{
			if (cachedPowerComp == null)
			{
				cachedPowerComp = this.TryGetComp<CompPowerTrader>();
			}
			return cachedPowerComp;
		}
	}

	public float BiostarvationDailyOffset
	{
		get
		{
			if (!base.Working)
			{
				return 0f;
			}
			if (!PowerOn || containedNutrition <= 0f)
			{
				return 0.5f;
			}
			return -0.1f;
		}
	}

	private float BiostarvationSeverityPercent
	{
		get
		{
			if (selectedEmbryo != null)
			{
				return embryoStarvation;
			}
			if (selectedPawn != null)
			{
				Hediff firstHediffOfDef = selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
				if (firstHediffOfDef != null)
				{
					return firstHediffOfDef.Severity / HediffDefOf.BioStarvation.maxSeverity;
				}
			}
			return 0f;
		}
	}

	public float NutritionConsumedPerDay
	{
		get
		{
			float num = ((selectedEmbryo != null) ? 6f : 3f);
			if (BiostarvationSeverityPercent > 0f)
			{
				float num2 = 1.1f;
				num *= num2;
			}
			return num;
		}
	}

	public float NutritionStored
	{
		get
		{
			float num = containedNutrition;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				num += (float)thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
			}
			return num;
		}
	}

	public float NutritionNeeded
	{
		get
		{
			if (selectedPawn == null && selectedEmbryo == null)
			{
				return 0f;
			}
			return 10f - NutritionStored;
		}
	}

	public int EmbryoGestationTicksRemaining => startTick - Find.TickManager.TicksGame;

	public float EmbryoGestationPct => 1f - Mathf.Clamp01((float)EmbryoGestationTicksRemaining / 540000f);

	private Graphic TopGraphic
	{
		get
		{
			if (cachedTopGraphic == null)
			{
				cachedTopGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Misc/GrowthVat/GrowthVatTop", ShaderDatabase.Transparent, def.graphicData.drawSize, Color.white);
			}
			return cachedTopGraphic;
		}
	}

	private Graphic FetusEarlyStage
	{
		get
		{
			if (fetusEarlyStageGraphic == null)
			{
				fetusEarlyStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_EarlyStage", ShaderDatabase.Cutout, Vector2.one, Color.white);
			}
			return fetusEarlyStageGraphic;
		}
	}

	private Graphic FetusLateStage
	{
		get
		{
			if (fetusLateStageGraphic == null)
			{
				fetusLateStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_LateStage", ShaderDatabase.Cutout, Vector2.one, Color.white);
			}
			return fetusLateStageGraphic;
		}
	}

	private Hediff VatLearning
	{
		get
		{
			if (cachedVatLearning == null)
			{
				cachedVatLearning = selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VatLearning) ?? selectedPawn.health.AddHediff(HediffDefOf.VatLearning);
			}
			return cachedVatLearning;
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		allowedNutritionSettings = new StorageSettings(this);
		if (def.building.defaultStorageSettings != null)
		{
			allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (respawningAfterLoad && selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Color color = EmbryoColor();
				fetusEarlyStageGraphic = FetusEarlyStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
				fetusLateStageGraphic = FetusLateStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
			});
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			if (selectedPawn != null && innerContainer.Contains(selectedPawn))
			{
				Notify_PawnRemoved();
			}
			DestroyEmbryo();
		}
		sustainerWorking = null;
		cachedVatLearning = null;
		base.DeSpawn(mode);
	}

	protected override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (base.Working && selectedPawn != null && innerContainer.Contains(selectedPawn))
		{
			VatLearning?.TickInterval(delta);
			VatLearning?.PostTickInterval(delta);
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (this.IsHashIntervalTick(250))
		{
			PowerTraderComp.PowerOutput = (base.Working ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
		}
		Pawn pawn = selectedPawn;
		if (pawn == null || !pawn.Destroyed)
		{
			HumanEmbryo humanEmbryo = selectedEmbryo;
			if (humanEmbryo == null || !humanEmbryo.Destroyed)
			{
				goto IL_0078;
			}
		}
		OnStop();
		goto IL_0078;
		IL_0078:
		foreach (Thing item in (IEnumerable<Thing>)innerContainer)
		{
			if (item is HumanEmbryo humanEmbryo2 && humanEmbryo2 != selectedEmbryo)
			{
				innerContainer.TryDrop(item, InteractionCell, base.Map, ThingPlaceMode.Near, 1, out var _);
			}
		}
		if (base.Working)
		{
			if (selectedPawn != null)
			{
				if (selectedPawn.ageTracker.AgeBiologicalYearsFloat >= 18f)
				{
					Messages.Message("OccupantEjectedFromGrowthVat".Translate(selectedPawn.Named("PAWN")) + ": " + "PawnIsTooOld".Translate(selectedPawn.Named("PAWN")), selectedPawn, MessageTypeDefOf.NeutralEvent);
					Finish();
					return;
				}
				if (innerContainer.Contains(selectedPawn))
				{
					int ticks = Mathf.RoundToInt(20f * selectedPawn.GetStatValue(StatDefOf.GrowthVatOccupantSpeed));
					selectedPawn.ageTracker.Notify_TickedInGrowthVat(ticks);
					VatLearning?.Tick();
					VatLearning?.PostTick();
				}
				float num = BiostarvationDailyOffset / 60000f * HediffDefOf.BioStarvation.maxSeverity;
				Hediff firstHediffOfDef = selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += num;
					if (firstHediffOfDef.ShouldRemove)
					{
						selectedPawn.health.RemoveHediff(firstHediffOfDef);
					}
				}
				else if (num > 0f)
				{
					Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BioStarvation, selectedPawn);
					hediff.Severity = num;
					selectedPawn.health.AddHediff(hediff);
				}
			}
			else if (selectedEmbryo != null)
			{
				if (EmbryoGestationTicksRemaining <= 0)
				{
					Finish();
					return;
				}
				embryoStarvation = Mathf.Clamp01(embryoStarvation + BiostarvationDailyOffset / 60000f);
			}
			if (BiostarvationSeverityPercent >= 1f)
			{
				Fail();
				return;
			}
			if (sustainerWorking == null || sustainerWorking.Ended)
			{
				sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			}
			else
			{
				sustainerWorking.Maintain();
			}
			containedNutrition = Mathf.Clamp(containedNutrition - NutritionConsumedPerDay / 60000f, 0f, 2.1474836E+09f);
			if (containedNutrition <= 0f)
			{
				TryAbsorbNutritiousThing();
			}
			if (GlowMotePerRotation == null)
			{
				GlowMotePerRotation = new Dictionary<Rot4, ThingDef>
				{
					{
						Rot4.South,
						ThingDefOf.Mote_VatGlowVertical
					},
					{
						Rot4.East,
						ThingDefOf.Mote_VatGlowHorizontal
					},
					{
						Rot4.West,
						ThingDefOf.Mote_VatGlowHorizontal
					},
					{
						Rot4.North,
						ThingDefOf.Mote_VatGlowVertical
					}
				};
				BubbleEffecterPerRotation = new Dictionary<Rot4, EffecterDef>
				{
					{
						Rot4.South,
						EffecterDefOf.Vat_Bubbles_South
					},
					{
						Rot4.East,
						EffecterDefOf.Vat_Bubbles_East
					},
					{
						Rot4.West,
						EffecterDefOf.Vat_Bubbles_West
					},
					{
						Rot4.North,
						EffecterDefOf.Vat_Bubbles_North
					}
				};
			}
			if (this.IsHashIntervalTick(132))
			{
				MoteMaker.MakeStaticMote(DrawPos, base.MapHeld, GlowMotePerRotation[base.Rotation]);
			}
			if (bubbleEffecter == null)
			{
				bubbleEffecter = BubbleEffecterPerRotation[base.Rotation].SpawnAttached(this, base.MapHeld);
			}
			bubbleEffecter.EffectTick(this, this);
		}
		else
		{
			TryGrowEmbryo();
			bubbleEffecter?.Cleanup();
			bubbleEffecter = null;
		}
	}

	public override AcceptanceReport CanAcceptPawn(Pawn pawn)
	{
		if (base.Working)
		{
			return "Occupied".Translate();
		}
		if (!PowerOn)
		{
			return "NoPower".Translate().CapitalizeFirst();
		}
		if (selectedEmbryo != null)
		{
			return "EmbryoSelected".Translate();
		}
		if (pawn.ageTracker.AgeBiologicalYearsFloat >= 18f)
		{
			return "TooOld".Translate(pawn.Named("PAWN"), 18f.Named("AGEYEARS"));
		}
		if (selectedPawn != null && selectedPawn != pawn)
		{
			return "WaitingForPawn".Translate(selectedPawn.Named("PAWN"));
		}
		if (pawn.health.hediffSet.HasHediff(HediffDefOf.BioStarvation))
		{
			return "PawnBiostarving".Translate(pawn.Named("PAWN"));
		}
		return pawn.IsColonist && !pawn.IsQuestLodger();
	}

	public override void TryAcceptPawn(Pawn pawn)
	{
		if (selectedPawn == null || !CanAcceptPawn(pawn))
		{
			return;
		}
		selectedPawn = pawn;
		bool num = pawn.DeSpawnOrDeselect();
		if (innerContainer.TryAddOrTransfer(pawn))
		{
			SoundDefOf.GrowthVat_Close.PlayOneShot(SoundInfo.InMap(this));
			startTick = Find.TickManager.TicksGame;
			if (!pawn.health.hediffSet.HasHediff(HediffDefOf.VatLearning))
			{
				pawn.health.AddHediff(HediffDefOf.VatLearning);
			}
			if (!pawn.health.hediffSet.HasHediff(HediffDefOf.VatGrowing))
			{
				pawn.health.AddHediff(HediffDefOf.VatGrowing);
			}
		}
		if (num)
		{
			Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
		}
	}

	private void TryGrowEmbryo()
	{
		if (!base.Working && PowerOn && selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
		{
			SoundDefOf.GrowthVat_Close.PlayOneShot(SoundInfo.InMap(this));
			startTick = Find.TickManager.TicksGame + 540000;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				Color color = EmbryoColor();
				fetusEarlyStageGraphic = FetusEarlyStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
				fetusLateStageGraphic = FetusLateStage.GetColoredVersion(ShaderDatabase.Cutout, color, color);
			});
			if (selectedPawn != null)
			{
				Log.Error("Growing embryo while pawn was somehow marked as selected");
				selectedPawn = null;
			}
		}
	}

	private void TryAbsorbNutritiousThing()
	{
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (innerContainer[i] != selectedPawn && innerContainer[i].def != ThingDefOf.Xenogerm && innerContainer[i].def != ThingDefOf.HumanEmbryo)
			{
				float statValue = innerContainer[i].GetStatValue(StatDefOf.Nutrition);
				if (statValue > 0f)
				{
					containedNutrition += statValue;
					innerContainer[i].SplitOff(1).Destroy();
					break;
				}
			}
		}
	}

	private void Finish()
	{
		if (selectedPawn != null)
		{
			FinishPawn();
		}
		else if (selectedEmbryo != null)
		{
			FinishEmbryo();
		}
	}

	private void FinishEmbryo()
	{
		EmbryoBirth();
		DestroyEmbryo();
		OnStop();
	}

	private void FinishPawn()
	{
		if (selectedPawn != null && innerContainer.Contains(selectedPawn))
		{
			Notify_PawnRemoved();
			innerContainer.TryDrop(selectedPawn, InteractionCell, base.Map, ThingPlaceMode.Near, 1, out var _);
			OnStop();
		}
	}

	private void Fail()
	{
		if (innerContainer.Contains(selectedPawn))
		{
			Notify_PawnRemoved();
			innerContainer.TryDrop(selectedPawn, InteractionCell, base.Map, ThingPlaceMode.Near, 1, out var _);
			Hediff firstHediffOfDef = selectedPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BioStarvation);
			selectedPawn.Kill(null, firstHediffOfDef);
		}
		DestroyEmbryo(biostarvation: true);
		OnStop();
	}

	private void OnStop()
	{
		selectedPawn = null;
		selectedEmbryo = null;
		startTick = -1;
		embryoStarvation = 0f;
		sustainerWorking = null;
		cachedVatLearning = null;
	}

	private void DestroyEmbryo(bool biostarvation = false)
	{
		if (startTick < 0 || selectedEmbryo == null || !innerContainer.Contains(selectedEmbryo))
		{
			return;
		}
		if (startTick > Find.TickManager.TicksGame)
		{
			if (biostarvation)
			{
				Messages.Message("EmbryoEjectedFromGrowthVatBiostarvation".Translate(selectedEmbryo.Label), this, MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				Messages.Message("EmbryoEjectedFromGrowthVat".Translate(selectedEmbryo.Label), this, MessageTypeDefOf.NegativeEvent);
			}
		}
		innerContainer.Remove(selectedEmbryo);
		selectedEmbryo.Destroy();
		selectedEmbryo = null;
	}

	private void EmbryoBirth()
	{
		if (selectedEmbryo != null && innerContainer.Contains(selectedEmbryo) && startTick <= Find.TickManager.TicksGame)
		{
			Precept_Ritual ritual = Faction.OfPlayer.ideos.PrimaryIdeo.GetPrecept(PreceptDefOf.ChildBirth) as Precept_Ritual;
			Thing thing = PregnancyUtility.ApplyBirthOutcome(((RitualOutcomeEffectWorker_ChildBirth)RitualOutcomeEffectDefOf.ChildBirth.GetInstance()).GetOutcome(0.7f, null), 0.7f, ritual, selectedEmbryo?.GeneSet?.GenesListForReading, selectedEmbryo.Mother, this, selectedEmbryo.Father);
			if (thing != null && embryoStarvation > 0f)
			{
				Pawn pawn = ((thing is Corpse corpse) ? corpse.InnerPawn : ((Pawn)thing));
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BioStarvation, pawn);
				hediff.Severity = Mathf.Lerp(0f, HediffDefOf.BioStarvation.maxSeverity, embryoStarvation);
				pawn.health.AddHediff(hediff);
			}
		}
	}

	private void Notify_PawnRemoved()
	{
		SoundDefOf.GrowthVat_Open.PlayOneShot(SoundInfo.InMap(this));
	}

	public bool CanAcceptNutrition(Thing thing)
	{
		return allowedNutritionSettings.AllowedToAccept(thing);
	}

	public StorageSettings GetStoreSettings()
	{
		return allowedNutritionSettings;
	}

	public StorageSettings GetParentStoreSettings()
	{
		return def.building.fixedStorageSettings;
	}

	public void Notify_SettingsChanged()
	{
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(allowedNutritionSettings))
		{
			yield return item;
		}
		if (base.Working)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandCancelGrowth".Translate();
			command_Action.defaultDesc = "CommandCancelGrowthDesc".Translate();
			command_Action.icon = CancelLoadingIcon;
			command_Action.activateSound = SoundDefOf.Designate_Cancel;
			command_Action.action = delegate
			{
				Action action = delegate
				{
					Finish();
					innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
				};
				if (startTick > Find.TickManager.TicksGame && selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
				{
					Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation("ImplantEmbryoCancelVat".Translate(selectedEmbryo.Label), action, destructive: true);
					Find.WindowStack.Add(window);
				}
				else
				{
					action();
				}
			};
			yield return command_Action;
			if (selectedEmbryo != null)
			{
				yield return new Command_Action
				{
					defaultLabel = "InspectGenes".Translate() + "...",
					defaultDesc = "InspectGenesEmbryoDesc".Translate(),
					icon = GeneSetHolderBase.GeneticInfoTex.Texture,
					action = delegate
					{
						InspectPaneUtility.OpenTab(typeof(ITab_Genes));
					}
				};
			}
			if (DebugSettings.ShowDevGizmos)
			{
				if (selectedPawn != null && innerContainer.Contains(selectedPawn))
				{
					yield return new Command_Action
					{
						defaultLabel = "DEV: Advance 1 year",
						action = delegate
						{
							selectedPawn.ageTracker.Notify_TickedInGrowthVat(3600000);
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "DEV: Learn",
						action = ((Hediff_VatLearning)VatLearning).Learn
					};
				}
				if (selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
				{
					yield return new Command_Action
					{
						defaultLabel = "DEV: Advance gestation 1 day",
						action = delegate
						{
							startTick = Mathf.Max(0, startTick - 60000);
						}
					};
					yield return new Command_Action
					{
						defaultLabel = "DEV: Embryo birth now",
						action = delegate
						{
							startTick = Find.TickManager.TicksGame;
							Finish();
						}
					};
				}
			}
		}
		else
		{
			if (selectedPawn != null || selectedEmbryo != null)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "CommandCancelLoad".Translate();
				command_Action2.defaultDesc = "CommandCancelLoadDesc".Translate();
				command_Action2.icon = CancelLoadingIcon;
				command_Action2.activateSound = SoundDefOf.Designate_Cancel;
				command_Action2.action = delegate
				{
					DestroyEmbryo();
					innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);
					if (innerContainer.Contains(selectedPawn))
					{
						Notify_PawnRemoved();
					}
					if (selectedPawn?.CurJobDef == JobDefOf.EnterBuilding)
					{
						selectedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					OnStop();
				};
				yield return command_Action2;
			}
			if (selectedPawn == null)
			{
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "InsertPerson".Translate() + "...";
				command_Action3.defaultDesc = "InsertPersonGrowthVatDesc".Translate();
				command_Action3.icon = InsertPawnIcon.Texture;
				command_Action3.action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (Pawn item2 in base.Map.mapPawns.AllPawnsSpawned)
					{
						Pawn pawn = item2;
						if ((bool)CanAcceptPawn(item2))
						{
							list.Add(new FloatMenuOption(item2.LabelCap, delegate
							{
								SelectPawn(pawn);
							}, pawn, Color.white));
						}
					}
					if (!list.Any())
					{
						list.Add(new FloatMenuOption("NoViablePawns".Translate(), null));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				};
				if (selectedEmbryo != null)
				{
					command_Action3.Disable("EmbryoSelected".Translate().CapitalizeFirst());
				}
				else if (!PowerOn)
				{
					command_Action3.Disable("NoPower".Translate().CapitalizeFirst());
				}
				else if (!base.AnyAcceptablePawns)
				{
					command_Action3.Disable("NoPawnsCanEnterGrowthVat".Translate(18f).ToString());
				}
				yield return command_Action3;
			}
			if (selectedEmbryo == null && Find.Storyteller.difficulty.ChildrenAllowed)
			{
				List<Thing> embryos = base.Map.listerThings.ThingsOfDef(ThingDefOf.HumanEmbryo);
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "ImplantEmbryo".Translate() + "...";
				command_Action4.defaultDesc = "InsertEmbryoGrowthVatDesc".Translate(540000.ToStringTicksToPeriod()).Resolve();
				command_Action4.icon = InsertEmbryoIcon.Texture;
				command_Action4.action = delegate
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (Thing embryo in embryos)
					{
						list.Add(new FloatMenuOption(embryo.LabelCap, delegate
						{
							SelectEmbryo(embryo as HumanEmbryo);
						}, embryo, Color.white));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				};
				if (embryos.NullOrEmpty())
				{
					command_Action4.Disable("ImplantNoEmbryos".Translate().CapitalizeFirst());
				}
				else if (selectedPawn != null)
				{
					command_Action4.Disable("PersonSelected".Translate().CapitalizeFirst());
				}
				else if (!PowerOn)
				{
					command_Action4.Disable("NoPower".Translate().CapitalizeFirst());
				}
				yield return command_Action4;
			}
		}
		if (DebugSettings.ShowDevGizmos)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fill nutrition",
				action = delegate
				{
					containedNutrition = 10f;
				}
			};
			yield return new Command_Action
			{
				defaultLabel = "DEV: Empty nutrition",
				action = delegate
				{
					containedNutrition = 0f;
				}
			};
		}
	}

	public void SelectEmbryo(HumanEmbryo embryo)
	{
		selectedEmbryo = embryo;
		embryo.implantTarget = this;
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		if (base.Working && selectedPawn != null && innerContainer.Contains(selectedPawn))
		{
			selectedPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc + PawnDrawOffset, null, neverAimWeapon: true);
		}
		base.DynamicDrawPhaseAt(phase, drawLoc, flip);
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		base.DrawAt(drawLoc, flip);
		if (base.Working && selectedPawn == null && selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
		{
			Vector2 drawSize = Vector2.one * Mathf.Lerp(0.4f, 0.95f, EmbryoGestationPct);
			if (EmbryoGestationTicksRemaining > 540000)
			{
				FetusEarlyStage.drawSize = drawSize;
				FetusEarlyStage.DrawFromDef(DrawPos + PawnDrawOffset + Altitudes.AltIncVect * 0.25f, base.Rotation, null);
			}
			else
			{
				FetusLateStage.drawSize = drawSize;
				FetusLateStage.DrawFromDef(DrawPos + PawnDrawOffset + Altitudes.AltIncVect * 0.25f, base.Rotation, null);
			}
		}
		TopGraphic.Draw(DrawPos + Altitudes.AltIncVect * 2f, base.Rotation, this);
	}

	private Color EmbryoColor()
	{
		Color result = PawnSkinColors.GetSkinColor(0.5f);
		if (selectedEmbryo?.GeneSet != null)
		{
			foreach (GeneDef item in selectedEmbryo.GeneSet.GenesListForReading)
			{
				if (item.skinColorOverride.HasValue)
				{
					return item.skinColorOverride.Value;
				}
				if (item.skinColorBase.HasValue)
				{
					result = item.skinColorBase.Value;
				}
			}
		}
		return result;
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (selectedEmbryo != null && selectedEmbryo.Map == base.Map)
		{
			GenDraw.DrawLineBetween(this.TrueCenter(), selectedEmbryo.TrueCenter());
		}
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (base.Working)
		{
			if (selectedPawn != null && innerContainer.Contains(selectedPawn))
			{
				stringBuilder.AppendLineIfNotEmpty().Append(string.Format("{0}: {1}, {2}", "CasketContains".Translate().ToString(), selectedPawn.NameShortColored.Resolve(), selectedPawn.ageTracker.AgeBiologicalYears));
			}
			if (selectedEmbryo != null && innerContainer.Contains(selectedEmbryo))
			{
				stringBuilder.AppendLineIfNotEmpty().AppendLine("Gestating".Translate() + ": " + selectedEmbryo.Label.CapitalizeFirst());
				stringBuilder.AppendLineTagged("EmbryoTimeUntilBirth".Translate() + ": " + EmbryoGestationTicksRemaining.ToStringTicksToDays().Colorize(ColoredText.DateTimeColor));
				stringBuilder.Append("EmbryoBirthQuality".Translate() + ": " + 0.7f.ToStringPercent());
			}
			float biostarvationSeverityPercent = BiostarvationSeverityPercent;
			if (biostarvationSeverityPercent > 0f)
			{
				string text = ((BiostarvationDailyOffset >= 0f) ? "+" : string.Empty);
				stringBuilder.AppendLineIfNotEmpty().Append(string.Format("{0}: {1} ({2})", "Biostarvation".Translate(), biostarvationSeverityPercent.ToStringPercent(), "PerDay".Translate(text + BiostarvationDailyOffset.ToStringPercent())));
			}
		}
		else if (selectedPawn != null)
		{
			stringBuilder.AppendLineIfNotEmpty().Append("WaitingForPawn".Translate(selectedPawn.Named("PAWN")).Resolve());
		}
		stringBuilder.AppendLineIfNotEmpty().Append("Nutrition".Translate()).Append(": ")
			.Append(NutritionStored.ToStringByStyle(ToStringStyle.FloatMaxOne));
		if (base.Working)
		{
			stringBuilder.Append(" (-").Append("PerDay".Translate(NutritionConsumedPerDay.ToString("F1"))).Append(")");
		}
		return stringBuilder.ToString();
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption;
		}
		if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
		{
			yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
			yield break;
		}
		AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
		if (acceptanceReport.Accepted)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
			{
				SelectPawn(selPawn);
			}), selPawn, this);
		}
		else if (!acceptanceReport.Reason.NullOrEmpty())
		{
			yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref selectedEmbryo, "selectedEmbryo");
		Scribe_Values.Look(ref embryoStarvation, "embryoStarvation", 0f);
		Scribe_Values.Look(ref containedNutrition, "containedNutrition", 0f);
		Scribe_Deep.Look(ref allowedNutritionSettings, "allowedNutritionSettings", this);
		if (allowedNutritionSettings == null)
		{
			allowedNutritionSettings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				allowedNutritionSettings.CopyFrom(def.building.defaultStorageSettings);
			}
		}
	}
}
