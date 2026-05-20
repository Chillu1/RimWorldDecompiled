using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Plant : ThingWithComps
	{
		public enum LeaflessCause
		{
			Cold,
			Poison,
			Pollution,
			NoPollution
		}

		protected float growthInt = 0.15f;

		protected int ageInt;

		protected int unlitTicks;

		protected int madeLeaflessTick = -99999;

		public bool sown;

		private string cachedLabelMouseover;

		private static Color32[] workingColors = new Color32[4];

		public const float BaseGrowthPercent = 0.15f;

		public const float BaseSownGrowthPercent = 0.0001f;

		public const float MinGrowthForAnimalIngestion = 0.1f;

		private const float BaseDyingDamagePerTick = 0.005f;

		private const float GridPosRandomnessFactor = 0.3f;

		private const int TicksWithoutLightBeforeStartDying = 450000;

		private const int LeaflessMinRecoveryTicks = 60000;

		public const float DefaultMinGrowthTemperature = 0f;

		public const float DefaultMinOptimalGrowthTemperature = 6f;

		public const float DefaultMaxOptimalGrowthTemperature = 42f;

		public const float DefaultMaxGrowthTemperature = 58f;

		private const float MinLeaflessTemperatureOffset = -18f;

		public const float MaxLeaflessTemperatureOffset = -10f;

		public const float TopVerticesAltitudeBias = 0.1f;

		private static Graphic GraphicSowing = GraphicDatabase.Get<Graphic_Single>("Things/Plant/Plant_Sowing", ShaderDatabase.Cutout, Vector2.one, Color.white);

		private static readonly FloatRange DyingDamagePerTickBecauseExposedToLight = new FloatRange(0.0001f, 0.001f);

		private static readonly FloatRange PollutionDamagePerTickRange = new FloatRange(1.6666667E-05f, 0.00016666666f);

		private const float VacuumDamagePerTick = 1f;

		private static readonly Texture2D CutAllBlightTex = ContentFinder<Texture2D>.Get("UI/Commands/CutAllBlightedPlants");

		[TweakValue("Graphics", -1f, 1f)]
		private static float LeafSpawnRadius = 0.4f;

		[TweakValue("Graphics", 0f, 2f)]
		private static float LeafSpawnYMin = 0.3f;

		[TweakValue("Graphics", 0f, 2f)]
		private static float LeafSpawnYMax = 1f;

		public virtual float Growth
		{
			get
			{
				return growthInt;
			}
			set
			{
				growthInt = Mathf.Clamp01(value);
				cachedLabelMouseover = null;
			}
		}

		public virtual int Age
		{
			get
			{
				return ageInt;
			}
			set
			{
				ageInt = value;
				cachedLabelMouseover = null;
			}
		}

		public virtual bool HarvestableNow
		{
			get
			{
				if (def.plant.Harvestable)
				{
					return growthInt > def.plant.harvestMinGrowth;
				}
				return false;
			}
		}

		public bool HarvestableSoon
		{
			get
			{
				if (HarvestableNow)
				{
					return true;
				}
				if (!def.plant.Harvestable)
				{
					return false;
				}
				float num = Mathf.Max(1f - Growth, 0f) * def.plant.growDays;
				float num2 = Mathf.Max(1f - def.plant.harvestMinGrowth, 0f) * def.plant.growDays;
				if ((num <= 10f || num2 <= 1f) && GrowthRateFactor_Fertility > 0f)
				{
					return GrowthRateFactor_Temperature > 0f;
				}
				return false;
			}
		}

		public virtual bool BlightableNow
		{
			get
			{
				if (!Blighted && def.plant.Blightable && sown && LifeStage != PlantLifeStage.Sowing)
				{
					return !base.Map.wildPlantSpawner.AllWildPlants.Contains(def);
				}
				return false;
			}
		}

		public Blight Blight
		{
			get
			{
				if (!base.Spawned || !def.plant.Blightable)
				{
					return null;
				}
				return base.Position.GetFirstBlight(base.Map);
			}
		}

		public bool Blighted => Blight != null;

		public override bool IngestibleNow
		{
			get
			{
				if (!base.IngestibleNow)
				{
					return false;
				}
				if (def.plant.IsTree)
				{
					return true;
				}
				if (growthInt < def.plant.harvestMinGrowth)
				{
					return false;
				}
				if (growthInt < 0.1f)
				{
					return false;
				}
				if (LeaflessNow)
				{
					return false;
				}
				if (base.Spawned && Math.Max(base.Position.GetSnowDepth(base.Map), base.Position.GetSandDepth(base.Map)) > def.hideAtSnowOrSandDepth)
				{
					return false;
				}
				return true;
			}
		}

		public virtual float CurrentDyingDamagePerTick
		{
			get
			{
				if (!base.Spawned)
				{
					return 0f;
				}
				float num = 0f;
				if (def.plant.LimitedLifespan && ageInt > def.plant.LifespanTicks)
				{
					num = Mathf.Max(num, 0.005f);
				}
				if (!def.plant.diesToLight && def.plant.dieIfNoSunlight && unlitTicks > 450000)
				{
					num = Mathf.Max(num, 0.005f);
				}
				if (DyingBecauseExposedToLight)
				{
					float lerpPct = base.Map.glowGrid.GroundGlowAt(base.Position, ignoreCavePlants: true);
					num = Mathf.Max(num, DyingDamagePerTickBecauseExposedToLight.LerpThroughRange(lerpPct));
				}
				if (DyingBecauseExposedToVacuum)
				{
					num = Mathf.Max(num, 1f * base.Position.GetVacuum(base.Map));
				}
				if (DyingFromPollution || DyingFromNoPollution)
				{
					num = Mathf.Max(num, PollutionDamagePerTickRange.RandomInRangeSeeded(base.Position.GetHashCode()));
				}
				if (DyingBecauseOfTerrainTags)
				{
					num = Mathf.Max(num, 0.005f);
				}
				return num;
			}
		}

		public virtual bool DyingBecauseExposedToLight
		{
			get
			{
				if (def.plant.diesToLight && base.Spawned)
				{
					return base.Map.glowGrid.GroundGlowAt(base.Position, ignoreCavePlants: true) > 0f;
				}
				return false;
			}
		}

		public virtual bool DyingBecauseExposedToVacuum
		{
			get
			{
				if (!def.plant.vacuumResistant && base.Spawned)
				{
					return base.Position.GetVacuum(base.Map) >= 0.5f;
				}
				return false;
			}
		}

		public virtual bool DyingBecauseOfTerrainTags
		{
			get
			{
				if (def.plant.WildTerrainTags.Count > 0)
				{
					return !def.plant.WildTerrainTags.Overlaps(base.Position.GetTerrain(base.Map).tags.OrElseEmptyEnumerable());
				}
				return false;
			}
		}

		public bool Dying => CurrentDyingDamagePerTick > 0f;

		protected virtual bool Resting
		{
			get
			{
				if (!(GenLocalDate.DayPercent(this) < 0.25f))
				{
					return GenLocalDate.DayPercent(this) > 0.8f;
				}
				return true;
			}
		}

		public virtual float GrowthRate
		{
			get
			{
				if (Blighted)
				{
					return 0f;
				}
				if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map, def))
				{
					return 0f;
				}
				return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought;
			}
		}

		public virtual string GrowthRateCalcDesc
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (GrowthRateFactor_Fertility != 1f)
				{
					stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate("FertilityLower".Translate()) + ": " + GrowthRateFactor_Fertility.ToStringPercent());
				}
				if (GrowthRateFactor_Temperature != 1f)
				{
					stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate("TemperatureLower".Translate()) + ": " + GrowthRateFactor_Temperature.ToStringPercent());
				}
				if (GrowthRateFactor_Light != 1f)
				{
					stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate("LightLower".Translate()) + ": " + GrowthRateFactor_Light.ToStringPercent());
				}
				if (ModsConfig.BiotechActive && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze) && GrowthRateFactor_NoxiousHaze != 1f)
				{
					stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate(GameConditionDefOf.NoxiousHaze.label) + ": " + GrowthRateFactor_NoxiousHaze.ToStringPercent());
				}
				if (ModsConfig.OdysseyActive && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Drought) && GrowthRateFactor_Drought != 1f)
				{
					stringBuilder.AppendInNewLine("StatsReport_MultiplierFor".Translate(GameConditionDefOf.Drought.label) + ": " + GrowthRateFactor_Drought.ToStringPercent());
				}
				return stringBuilder.ToString();
			}
		}

		protected float GrowthPerTick
		{
			get
			{
				if (LifeStage != PlantLifeStage.Growing || Resting)
				{
					return 0f;
				}
				if (!def.plant.vacuumResistant && base.Position.GetVacuum(base.Map) >= 0.5f)
				{
					return 0f;
				}
				return 1f / (60000f * def.plant.growDays) * GrowthRate;
			}
		}

		public float GrowthRateFactor_Fertility => PlantUtility.GrowthRateFactorFor_Fertility(def, base.Map.fertilityGrid.FertilityAt(base.Position));

		public float GrowthRateFactor_Light
		{
			get
			{
				float glow = base.Map.glowGrid.GroundGlowAt(base.Position);
				return PlantUtility.GrowthRateFactorFor_Light(def, glow);
			}
		}

		public float GrowthRateFactor_Temperature
		{
			get
			{
				if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out var tempResult))
				{
					return 1f;
				}
				return PlantUtility.GrowthRateFactorFor_Temperature(def, tempResult);
			}
		}

		public float GrowthRateFactor_NoxiousHaze
		{
			get
			{
				if (NoxiousHazeUtility.IsExposedToNoxiousHaze(this))
				{
					return 0.5f;
				}
				return 1f;
			}
		}

		public float GrowthRateFactor_Drought
		{
			get
			{
				if (!ModsConfig.OdysseyActive)
				{
					return 1f;
				}
				if (CurrentlyCultivated())
				{
					Building edifice = base.Position.GetEdifice(base.Map);
					if (edifice != null && edifice.def.building.SupportsPlants)
					{
						return 1f;
					}
				}
				if (def.plant.cavePlant)
				{
					return 1f;
				}
				if (base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Drought))
				{
					return 0.3f;
				}
				return 1f;
			}
		}

		protected int TicksUntilFullyGrown
		{
			get
			{
				if (growthInt > 0.9999f)
				{
					return 0;
				}
				float growthPerTick = GrowthPerTick;
				if (growthPerTick == 0f)
				{
					return int.MaxValue;
				}
				return (int)((1f - growthInt) / growthPerTick);
			}
		}

		protected string GrowthPercentString => (Mathf.Floor((growthInt + 0.0001f) * 100f) / 100f).ToStringPercent();

		public override string LabelMouseover
		{
			get
			{
				if (cachedLabelMouseover == null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(def.LabelCap);
					stringBuilder.Append(" (" + "PercentGrowth".Translate(GrowthPercentString));
					if (Dying)
					{
						stringBuilder.Append(", " + "DyingLower".Translate());
					}
					stringBuilder.Append(")");
					cachedLabelMouseover = stringBuilder.ToString();
				}
				return cachedLabelMouseover;
			}
		}

		protected virtual bool HasEnoughLightToGrow => GrowthRateFactor_Light > 0.001f;

		public virtual PlantLifeStage LifeStage
		{
			get
			{
				if (growthInt < 0.0001f)
				{
					return PlantLifeStage.Sowing;
				}
				if (growthInt > 0.999f)
				{
					return PlantLifeStage.Mature;
				}
				return PlantLifeStage.Growing;
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (LifeStage == PlantLifeStage.Sowing)
				{
					return GraphicSowing;
				}
				if (def.plant.pollutedGraphic != null && base.PositionHeld.IsPolluted(base.MapHeld))
				{
					return def.plant.pollutedGraphic;
				}
				if (def.plant.leaflessImmatureGraphic != null && LeaflessNow && !HarvestableNow)
				{
					return def.plant.leaflessImmatureGraphic;
				}
				if (def.plant.leaflessGraphic != null && LeaflessNow && (!sown || !HarvestableNow))
				{
					return def.plant.leaflessGraphic;
				}
				if (def.plant.immatureGraphic != null && !HarvestableNow)
				{
					return def.plant.immatureGraphic;
				}
				return base.Graphic;
			}
		}

		public Graphic SnowOverlayGraphic
		{
			get
			{
				if (def.plant.immatureSnowOverlayGraphic != null && !HarvestableNow)
				{
					return def.plant.immatureSnowOverlayGraphic;
				}
				if (def.plant.leaflessSnowOverlayGraphic != null && LeaflessNow && (!sown || !HarvestableNow))
				{
					return def.plant.leaflessSnowOverlayGraphic;
				}
				return def.plant.snowOverlayGraphic;
			}
		}

		public bool LeaflessNow
		{
			get
			{
				if (Find.TickManager.TicksGame - madeLeaflessTick < 60000)
				{
					return true;
				}
				return false;
			}
		}

		protected virtual float LeaflessTemperatureThresh => def.plant.minGrowthTemperature + Rand.RangeSeeded(-18f, -10f, thingIDNumber ^ 0x31F3A5C1);

		public bool IsCrop
		{
			get
			{
				if (!def.plant.Sowable)
				{
					return false;
				}
				if (!base.Spawned)
				{
					Log.Warning("Can't determine if crop when unspawned.");
					return false;
				}
				return def == WorkGiver_Grower.CalculateWantedPlantDef(base.Position, base.Map);
			}
		}

		public bool DyingFromPollution
		{
			get
			{
				if (def.plant.RequiresNoPollution)
				{
					return base.Position.IsPolluted(base.Map);
				}
				return false;
			}
		}

		public bool DyingFromNoPollution
		{
			get
			{
				if (def.plant.RequiresPollution)
				{
					return !base.Position.IsPolluted(base.Map);
				}
				return false;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (Current.ProgramState == ProgramState.Playing && !respawningAfterLoad && !base.BeingTransportedOnGravship)
			{
				CheckMakeLeafless();
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Blight firstBlight = base.Position.GetFirstBlight(base.Map);
			base.DeSpawn(mode);
			if (mode != DestroyMode.WillReplace)
			{
				firstBlight?.Notify_PlantDeSpawned();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref growthInt, "growth", 0f);
			Scribe_Values.Look(ref ageInt, "age", 0);
			Scribe_Values.Look(ref unlitTicks, "unlitTicks", 0);
			Scribe_Values.Look(ref madeLeaflessTick, "madeLeaflessTick", -99999);
			Scribe_Values.Look(ref sown, "sown", defaultValue: false);
		}

		public override void PostMapInit()
		{
			CheckMakeLeafless();
		}

		protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			float statValue = this.GetStatValue(StatDefOf.Nutrition);
			float num = growthInt * statValue;
			nutritionIngested = Mathf.Min(nutritionWanted, num);
			if (nutritionIngested >= num)
			{
				numTaken = 1;
				return;
			}
			numTaken = 0;
			growthInt -= nutritionIngested / statValue;
			if (base.Spawned)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
			}
		}

		public virtual void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
		{
			if (def.plant.HarvestDestroys)
			{
				if (def.plant.IsTree && def.plant.treeLoversCareIfChopped)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.CutTree, by.Named(HistoryEventArgsNames.Doer)));
					base.Map.treeDestructionTracker.Notify_TreeCut(by);
				}
				if (Blighted && plantDestructionMode == PlantDestructionMode.Cut)
				{
					base.Map.floodFiller.FloodFill(base.Position, delegate(IntVec3 cell)
					{
						if (base.Position == cell)
						{
							return true;
						}
						Plant plant = cell.GetPlant(base.Map);
						return plant != null && !base.Map.designationManager.HasMapDesignationOn(plant) && plant.Blighted;
					}, delegate(IntVec3 cell)
					{
						if (!(base.Position == cell))
						{
							Plant plant = cell.GetPlant(base.Map);
							base.Map.designationManager.AddDesignation(new Designation(plant, DesignationDefOf.CutPlant));
							by.jobs?.curJob?.targetQueueA?.Add(plant);
						}
					}, 100);
				}
				Thing thing = TrySpawnStump(plantDestructionMode);
				Map map = base.Map;
				Destroy(DestroyMode.KillFinalizeLeavingsOnly);
				if (thing != null && plantDestructionMode == PlantDestructionMode.Cut && by.Faction == Faction.OfPlayer)
				{
					map.designationManager.AddDesignation(new Designation(thing, DesignationDefOf.CutPlant));
					by.jobs?.curJob?.targetQueueA?.Add(thing);
				}
			}
			else
			{
				growthInt = def.plant.harvestAfterGrowth;
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
			}
		}

		public Thing TrySpawnStump(PlantDestructionMode treeDestructionMode)
		{
			if (!base.Spawned || LifeStage == PlantLifeStage.Sowing)
			{
				return null;
			}
			if (!HarvestableNow)
			{
				return null;
			}
			ThingDef thingDef = null;
			switch (treeDestructionMode)
			{
			case PlantDestructionMode.Smash:
				thingDef = def.plant.smashedThingDef;
				break;
			case PlantDestructionMode.Flame:
				thingDef = def.plant.burnedThingDef;
				break;
			case PlantDestructionMode.Chop:
			case PlantDestructionMode.Cut:
				thingDef = def.plant.choppedThingDef;
				break;
			}
			if (thingDef != null)
			{
				Thing thing = GenSpawn.Spawn(thingDef, base.Position, base.Map);
				if (thing is DeadPlant deadPlant)
				{
					deadPlant.Growth = Growth;
				}
				if (Find.Selector.IsSelected(this))
				{
					Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
				}
				return thing;
			}
			return null;
		}

		public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
		{
			if (base.Spawned && dinfo.HasValue)
			{
				if (dinfo.Value.Def == DamageDefOf.Flame)
				{
					TrySpawnStump(PlantDestructionMode.Flame);
				}
				else
				{
					TrySpawnStump(PlantDestructionMode.Smash);
				}
				if (def.plant.IsTree && def.plant.treeLoversCareIfChopped)
				{
					base.Map.treeDestructionTracker.Notify_TreeDestroyed(dinfo.Value);
				}
			}
			base.Kill(dinfo, exactCulprit);
		}

		protected virtual void CheckMakeLeafless()
		{
			if (DyingFromPollution)
			{
				MakeLeafless(LeaflessCause.Pollution);
				return;
			}
			if (DyingFromNoPollution)
			{
				MakeLeafless(LeaflessCause.NoPollution);
				return;
			}
			Room room = this.GetRoom();
			if (room != null && room.UsesOutdoorTemperature && base.AmbientTemperature < LeaflessTemperatureThresh)
			{
				MakeLeafless(LeaflessCause.Cold);
			}
		}

		public virtual void MakeLeafless(LeaflessCause cause, bool sendMessage = true)
		{
			bool num = !LeaflessNow;
			Map map = base.Map;
			if (cause == LeaflessCause.Poison && def.plant.leaflessGraphic == null)
			{
				if (IsCrop && sendMessage && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + def.defName, 240f))
				{
					Messages.Message("MessagePlantDiedOfPoison".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
				}
				TakeDamage(new DamageInfo(DamageDefOf.Rotting, 99999f));
			}
			else if (def.plant.dieIfLeafless)
			{
				if (IsCrop)
				{
					switch (cause)
					{
					case LeaflessCause.Cold:
						if (sendMessage && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfCold-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfCold".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
						}
						break;
					case LeaflessCause.Poison:
						if (sendMessage && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfPoison".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
						}
						break;
					case LeaflessCause.Pollution:
						if (sendMessage && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPollution-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfPollution".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
						}
						break;
					case LeaflessCause.NoPollution:
						if (sendMessage && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfNoPollution-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfNoPollution".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
						}
						break;
					}
				}
				TakeDamage(new DamageInfo(DamageDefOf.Rotting, 99999f));
			}
			else
			{
				madeLeaflessTick = Find.TickManager.TicksGame;
			}
			if (num)
			{
				map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
			}
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			if (this.IsHashIntervalTick(2000, delta))
			{
				TickLong();
			}
		}

		public override void TickLong()
		{
			CheckMakeLeafless();
			if (base.Destroyed)
			{
				return;
			}
			base.TickLong();
			if (PlantUtility.GrowthSeasonNow(base.Position, base.Map, def))
			{
				float num = growthInt;
				bool num2 = LifeStage == PlantLifeStage.Mature;
				growthInt += GrowthPerTick * 2000f;
				if (growthInt > 1f)
				{
					growthInt = 1f;
				}
				if (((!num2 && LifeStage == PlantLifeStage.Mature) || (int)(num * 10f) != (int)(growthInt * 10f)) && CurrentlyCultivated())
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
				}
			}
			if (!HasEnoughLightToGrow)
			{
				unlitTicks += 2000;
			}
			else
			{
				unlitTicks = 0;
			}
			ageInt += 2000;
			if (Dying)
			{
				Map map = base.Map;
				bool isCrop = IsCrop;
				bool harvestableNow = HarvestableNow;
				bool dyingBecauseExposedToLight = DyingBecauseExposedToLight;
				bool dyingBecauseExposedToVacuum = DyingBecauseExposedToVacuum;
				int num3 = Mathf.CeilToInt(CurrentDyingDamagePerTick * 2000f);
				TakeDamage(new DamageInfo(DamageDefOf.Rotting, num3));
				if (base.Destroyed && !def.plant.skipDeteriorationMessage)
				{
					if (isCrop && def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + def.defName, 240f))
					{
						string key = (harvestableNow ? ((!ModsConfig.BiotechActive || !base.Position.IsPolluted(map)) ? "MessagePlantDiedOfRot_LeftUnharvested" : "MessagePlantDiedOfRot_PollutedTerrain") : (dyingBecauseExposedToLight ? "MessagePlantDiedOfRot_ExposedToLight" : ((!dyingBecauseExposedToVacuum) ? "MessagePlantDiedOfRot" : "MessagePlantDiedOfRot_ExposedToVacuum")));
						Messages.Message(key.Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
					}
					return;
				}
			}
			cachedLabelMouseover = null;
			if (def.plant.dropLeaves && MoteMaker.MakeStaticMote(Vector3.zero, base.Map, ThingDefOf.Mote_Leaf) is MoteLeaf moteLeaf)
			{
				float num4 = def.plant.visualSizeRange.LerpThroughRange(growthInt);
				float treeHeight = def.graphicData.drawSize.x * num4;
				Vector3 vector = Rand.InsideUnitCircleVec3 * LeafSpawnRadius;
				moteLeaf.Initialize(base.Position.ToVector3Shifted() + Vector3.up * Rand.Range(LeafSpawnYMin, LeafSpawnYMax) + vector + Vector3.forward * def.graphicData.shadowData.offset.z, Rand.Value * 2000.TicksToSeconds(), vector.z > 0f, treeHeight);
			}
		}

		protected virtual bool CurrentlyCultivated()
		{
			if (!def.plant.Sowable)
			{
				return false;
			}
			if (!base.Spawned)
			{
				return false;
			}
			Zone zone = base.Map.zoneManager.ZoneAt(base.Position);
			if (zone != null && zone is Zone_Growing)
			{
				return true;
			}
			Building edifice = base.Position.GetEdifice(base.Map);
			if (edifice != null && edifice.def.building.SupportsPlants)
			{
				return true;
			}
			return false;
		}

		public bool DeliberatelyCultivated()
		{
			if (!def.plant.Sowable)
			{
				return false;
			}
			if (!base.Spawned)
			{
				return false;
			}
			if (base.Map.zoneManager.ZoneAt(base.Position) is Zone_Growing zone_Growing && zone_Growing.GetPlantDefToGrow() == def)
			{
				return true;
			}
			Building edifice = base.Position.GetEdifice(base.Map);
			if (edifice != null && edifice.def.building.SupportsPlants)
			{
				return true;
			}
			return false;
		}

		public virtual bool CanYieldNow()
		{
			if (!HarvestableNow)
			{
				return false;
			}
			if (def.plant.harvestYield <= 0f)
			{
				return false;
			}
			if (Blighted)
			{
				return false;
			}
			return true;
		}

		public virtual int YieldNow()
		{
			if (!CanYieldNow())
			{
				return 0;
			}
			float harvestYield = def.plant.harvestYield;
			float num = Mathf.InverseLerp(def.plant.harvestMinGrowth, 1f, growthInt);
			num = 0.5f + num * 0.5f;
			harvestYield *= num;
			harvestYield *= Mathf.Lerp(0.5f, 1f, (float)HitPoints / (float)base.MaxHitPoints);
			if (def.plant.harvestYieldAffectedByDifficulty)
			{
				harvestYield *= Find.Storyteller.difficulty.cropYieldFactor;
			}
			return GenMath.RoundRandom(harvestYield);
		}

		public override void Print(SectionLayer layer)
		{
			Vector3 vector = this.TrueCenter();
			bool flag = base.Position.GetSnowDepth(base.Map) > 0.8f;
			Rand.PushState();
			Rand.Seed = base.Position.GetHashCode();
			int num = Mathf.CeilToInt(growthInt * (float)def.plant.maxMeshCount);
			if (num < 1)
			{
				num = 1;
			}
			float num2 = def.plant.visualSizeRange.LerpThroughRange(growthInt);
			float num3 = def.graphicData.drawSize.x * num2;
			int num4 = 0;
			int[] positionIndices = PlantPosIndices.GetPositionIndices(this);
			bool flag2 = false;
			foreach (int num6 in positionIndices)
			{
				Vector3 vector2;
				if (def.plant.maxMeshCount == 1)
				{
					vector2 = vector + Gen.RandomHorizontalVector(0.05f);
					float num7 = base.Position.z;
					if (vector2.z - num2 / 2f < num7)
					{
						vector2.z = num7 + num2 / 2f;
						flag2 = true;
					}
				}
				else
				{
					int num8 = 1;
					switch (def.plant.maxMeshCount)
					{
					case 1:
						num8 = 1;
						break;
					case 4:
						num8 = 2;
						break;
					case 9:
						num8 = 3;
						break;
					case 16:
						num8 = 4;
						break;
					case 25:
						num8 = 5;
						break;
					default:
						Log.Error(def?.ToString() + " must have plant.MaxMeshCount that is a perfect square.");
						break;
					}
					float num9 = 1f / (float)num8;
					vector2 = base.Position.ToVector3();
					vector2.y = def.Altitude;
					vector2.x += 0.5f * num9;
					vector2.z += 0.5f * num9;
					int num10 = num6 / num8;
					int num11 = num6 % num8;
					vector2.x += (float)num10 * num9;
					vector2.z += (float)num11 * num9;
					float max = num9 * 0.3f;
					vector2 += Gen.RandomHorizontalVector(max);
				}
				bool flipUv = Rand.Bool;
				object obj;
				if (flag)
				{
					Graphic snowOverlayGraphic = SnowOverlayGraphic;
					if (snowOverlayGraphic != null)
					{
						obj = snowOverlayGraphic.MatSingleFor(this);
						goto IL_024a;
					}
				}
				obj = null;
				goto IL_024a;
				IL_024a:
				Material material = (Material)obj;
				Material material2 = Graphic.MatSingleFor(this);
				if (Graphic is Graphic_Random { SubGraphicsCount: var subGraphicsCount } graphic_Random)
				{
					int index = Rand.Range(0, subGraphicsCount);
					material2 = graphic_Random.SubGraphicAtIndex(index).MatSingle;
					if (flag && SnowOverlayGraphic is Graphic_Random graphic_Random2)
					{
						material = graphic_Random2.SubGraphicAtIndex(index).MatSingle;
					}
				}
				Graphic.TryGetTextureAtlasReplacementInfo(material2, def.category.ToAtlasGroup(), flipUv, vertexColors: false, out material2, out var uvs, out var vertexColor);
				PlantUtility.SetWindExposureColors(workingColors, this);
				Vector2 size = new Vector2(num3, num3);
				Printer_Plane.PrintPlane(layer, vector2, size, material2, 0f, flipUv, uvs, workingColors, 0.1f, this.HashOffset() % 1024);
				if (flag && material != null)
				{
					Graphic.TryGetTextureAtlasReplacementInfo(material, def.category.ToAtlasGroup(), flipUv, vertexColors: false, out material, out uvs, out vertexColor);
					Printer_Plane.PrintPlane(layer, vector2.WithYOffset(0.0003658537f), size, material, 0f, flipUv, uvs, workingColors, 0.1f, this.HashOffset() % 1024);
				}
				num4++;
				if (num4 >= num)
				{
					break;
				}
			}
			if (def.graphicData.shadowData != null)
			{
				Vector3 center = vector + def.graphicData.shadowData.offset * num2;
				if (flag2)
				{
					center.z = base.Position.ToVector3Shifted().z + def.graphicData.shadowData.offset.z;
				}
				center.y -= 0.03658537f;
				Vector3 volume = def.graphicData.shadowData.volume * num2;
				Printer_Shadow.PrintShadow(layer, center, volume, Rot4.North);
			}
			Rand.PopState();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (def.plant.showGrowthInInspectPane)
			{
				if (LifeStage == PlantLifeStage.Growing)
				{
					stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
					stringBuilder.Append("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
					if (!Blighted)
					{
						string[] array = ArrayPool<string>.Shared.Rent(4);
						int count = 0;
						if (Resting)
						{
							AddCondition(array, ref count, "PlantResting".Translate());
						}
						if (!HasEnoughLightToGrow)
						{
							AddCondition(array, ref count, "PlantNeedsLightLevel".Translate() + " " + def.plant.growMinGlow.ToStringPercent());
						}
						float growthRateFactor_Temperature = GrowthRateFactor_Temperature;
						if (growthRateFactor_Temperature < 0.99f)
						{
							if (Mathf.Approximately(growthRateFactor_Temperature, 0f) || !PlantUtility.GrowthSeasonNow(base.Position, base.Map, def))
							{
								AddCondition(array, ref count, "OutOfIdealTemperatureRangeNotGrowing".Translate());
							}
							else
							{
								AddCondition(array, ref count, "OutOfIdealTemperatureRange".Translate(Mathf.Max(1, Mathf.RoundToInt(growthRateFactor_Temperature * 100f)).ToString()));
							}
						}
						if (GrowthRateFactor_Drought < 0.99f)
						{
							AddCondition(array, ref count, GameConditionDefOf.Drought.label);
						}
						string text = string.Join(", ", array, 0, count);
						ArrayPool<string>.Shared.Return(array);
						if (!text.NullOrEmpty())
						{
							stringBuilder.Append(" (").Append(text).Append(')');
						}
					}
					stringBuilder.AppendLine();
				}
				else if (LifeStage == PlantLifeStage.Mature)
				{
					stringBuilder.AppendLine(HarvestableNow ? "ReadyToHarvest".Translate() : "Mature".Translate());
				}
				if (DyingBecauseExposedToLight)
				{
					stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
				}
				if (DyingBecauseExposedToVacuum)
				{
					stringBuilder.AppendLine("DyingBecauseExposedToVacuum".Translate());
				}
				if (DyingBecauseOfTerrainTags)
				{
					stringBuilder.AppendLine("DyingBecauseOfTerrain".Translate());
				}
				if (Blighted)
				{
					stringBuilder.AppendLine(string.Format("{0} ({1})", "Blighted".Translate(), Blight.Severity.ToStringPercent()));
				}
			}
			string text2 = InspectStringPartsFromComps();
			if (!text2.NullOrEmpty())
			{
				stringBuilder.Append(text2);
			}
			return stringBuilder.ToString().TrimEndNewlines();
			static void AddCondition(string[] conditions, ref int reference, string condition)
			{
				if (reference < conditions.Length)
				{
					conditions[reference++] = condition;
				}
				else
				{
					Log.Error("Too many conditions for plant growth inspect string");
				}
			}
		}

		public virtual void CropBlighted()
		{
			if (!Blighted)
			{
				GenSpawn.Spawn(ThingDefOf.Blight, base.Position, base.Map);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (Blighted)
			{
				Designation designation = base.Map.designationManager.DesignationOn(this);
				if (designation == null || designation.def != DesignationDefOf.CutPlant)
				{
					yield return new Command_Action
					{
						defaultLabel = "CutAllBlight".Translate(),
						defaultDesc = "CutAllBlightDesc".Translate(),
						icon = CutAllBlightTex,
						action = delegate
						{
							foreach (Plant item in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Plant))
							{
								if (item != null && item.Blighted && !base.Map.designationManager.HasMapDesignationOn(item))
								{
									base.Map.designationManager.AddDesignation(new Designation(item, DesignationDefOf.CutPlant));
								}
							}
						}
					};
				}
			}
			if (!DebugSettings.ShowDevGizmos)
			{
				yield break;
			}
			if (Blighted)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Spread blight",
					action = delegate
					{
						Blight.TryReproduceNow();
					}
				};
			}
			else
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Make blighted",
					action = CropBlighted
				};
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			foreach (StatDrawEntry item in base.SpecialDisplayStats())
			{
				yield return item;
			}
			if (def.plant.LimitedLifespan)
			{
				string valueString = Age.ToStringTicksToPeriod();
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_Plant_Age".Translate(), valueString, "Stat_Thing_Plant_AgeDesc".Translate(), 4170);
			}
			if (LifeStage == PlantLifeStage.Growing && base.Spawned)
			{
				TaggedString taggedString = "Stat_Thing_Plant_GrowthRate_Desc".Translate();
				string growthRateCalcDesc = GrowthRateCalcDesc;
				if (!growthRateCalcDesc.NullOrEmpty())
				{
					taggedString += "\n\n" + growthRateCalcDesc;
				}
				taggedString += "\n" + "StatsReport_FinalValue".Translate() + ": " + GrowthRate.ToStringPercent();
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Stat_Thing_Plant_GrowthRate".Translate(), GrowthRate.ToStringPercent(), taggedString, 4158);
			}
		}
	}
}
