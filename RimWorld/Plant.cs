using System.Collections.Generic;
using System.Text;
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
			Poison
		}

		protected float growthInt = 0.05f;

		protected int ageInt;

		protected int unlitTicks;

		protected int madeLeaflessTick = -99999;

		public bool sown;

		private string cachedLabelMouseover;

		private static Color32[] workingColors = new Color32[4];

		public const float BaseGrowthPercent = 0.05f;

		private const float BaseDyingDamagePerTick = 0.005f;

		private static readonly FloatRange DyingDamagePerTickBecauseExposedToLight = new FloatRange(0.0001f, 0.001f);

		private const float GridPosRandomnessFactor = 0.3f;

		private const int TicksWithoutLightBeforeStartDying = 450000;

		private const int LeaflessMinRecoveryTicks = 60000;

		public const float MinGrowthTemperature = 0f;

		public const float MinOptimalGrowthTemperature = 10f;

		public const float MaxOptimalGrowthTemperature = 42f;

		public const float MaxGrowthTemperature = 58f;

		public const float MaxLeaflessTemperature = -2f;

		private const float MinLeaflessTemperature = -10f;

		private const float MinAnimalEatPlantsTemperature = 0f;

		public const float TopVerticesAltitudeBias = 0.1f;

		private static Graphic GraphicSowing = GraphicDatabase.Get<Graphic_Single>("Things/Plant/Plant_Sowing", ShaderDatabase.Cutout, Vector2.one, Color.white);

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
				if (!Blighted && def.plant.Blightable && sown && LifeStage != 0)
				{
					return !base.Map.Biome.AllWildPlants.Contains(def);
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
				if (LeaflessNow)
				{
					return false;
				}
				if (base.Spawned && base.Position.GetSnowDepth(base.Map) > def.hideAtSnowDepth)
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
				if (!def.plant.cavePlant && def.plant.dieIfNoSunlight && unlitTicks > 450000)
				{
					num = Mathf.Max(num, 0.005f);
				}
				if (DyingBecauseExposedToLight)
				{
					float lerpPct = base.Map.glowGrid.GameGlowAt(base.Position, ignoreCavePlants: true);
					num = Mathf.Max(num, DyingDamagePerTickBecauseExposedToLight.LerpThroughRange(lerpPct));
				}
				return num;
			}
		}

		public virtual bool DyingBecauseExposedToLight
		{
			get
			{
				if (def.plant.cavePlant && base.Spawned)
				{
					return base.Map.glowGrid.GameGlowAt(base.Position, ignoreCavePlants: true) > 0f;
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
				if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map))
				{
					return 0f;
				}
				return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light;
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
				return 1f / (60000f * def.plant.growDays) * GrowthRate;
			}
		}

		public float GrowthRateFactor_Fertility => base.Map.fertilityGrid.FertilityAt(base.Position) * def.plant.fertilitySensitivity + (1f - def.plant.fertilitySensitivity);

		public float GrowthRateFactor_Light
		{
			get
			{
				float num = base.Map.glowGrid.GameGlowAt(base.Position);
				if (def.plant.growMinGlow == def.plant.growOptimalGlow && num == def.plant.growOptimalGlow)
				{
					return 1f;
				}
				return GenMath.InverseLerp(def.plant.growMinGlow, def.plant.growOptimalGlow, num);
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
				if (tempResult < 10f)
				{
					return Mathf.InverseLerp(0f, 10f, tempResult);
				}
				if (tempResult > 42f)
				{
					return Mathf.InverseLerp(58f, 42f, tempResult);
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

		protected string GrowthPercentString => (growthInt + 0.0001f).ToStringPercent();

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
				if (growthInt < 0.001f)
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

		protected virtual float LeaflessTemperatureThresh
		{
			get
			{
				float num = 8f;
				return (float)this.HashOffset() * 0.01f % num - num + -2f;
			}
		}

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

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (Current.ProgramState == ProgramState.Playing && !respawningAfterLoad)
			{
				CheckTemperatureMakeLeafless();
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Blight firstBlight = base.Position.GetFirstBlight(base.Map);
			base.DeSpawn(mode);
			firstBlight?.Notify_PlantDeSpawned();
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
			CheckTemperatureMakeLeafless();
		}

		protected override void IngestedCalculateAmounts(Pawn ingester, float nutritionWanted, out int numTaken, out float nutritionIngested)
		{
			float statValue = this.GetStatValue(StatDefOf.Nutrition);
			if (def.plant.HarvestDestroys)
			{
				numTaken = 1;
			}
			else
			{
				growthInt -= 0.3f;
				if (growthInt < 0.08f)
				{
					growthInt = 0.08f;
				}
				if (base.Spawned)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				numTaken = 0;
			}
			nutritionIngested = statValue;
		}

		public virtual void PlantCollected()
		{
			if (def.plant.HarvestDestroys)
			{
				Destroy();
				return;
			}
			growthInt = def.plant.harvestAfterGrowth;
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
		}

		protected virtual void CheckTemperatureMakeLeafless()
		{
			if (base.AmbientTemperature < LeaflessTemperatureThresh)
			{
				MakeLeafless(LeaflessCause.Cold);
			}
		}

		public virtual void MakeLeafless(LeaflessCause cause)
		{
			bool num = !LeaflessNow;
			Map map = base.Map;
			if (cause == LeaflessCause.Poison && def.plant.leaflessGraphic == null)
			{
				if (IsCrop && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + def.defName, 240f))
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
						if (MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfCold-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfCold".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
						}
						break;
					case LeaflessCause.Poison:
						if (MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfPoison-" + def.defName, 240f))
						{
							Messages.Message("MessagePlantDiedOfPoison".Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
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
				map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		public override void TickLong()
		{
			CheckTemperatureMakeLeafless();
			if (base.Destroyed)
			{
				return;
			}
			base.TickLong();
			if (PlantUtility.GrowthSeasonNow(base.Position, base.Map))
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
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
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
				int num3 = Mathf.CeilToInt(CurrentDyingDamagePerTick * 2000f);
				TakeDamage(new DamageInfo(DamageDefOf.Rotting, num3));
				if (base.Destroyed)
				{
					if (isCrop && def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + def.defName, 240f))
					{
						string key = (harvestableNow ? "MessagePlantDiedOfRot_LeftUnharvested" : ((!dyingBecauseExposedToLight) ? "MessagePlantDiedOfRot" : "MessagePlantDiedOfRot_ExposedToLight"));
						Messages.Message(key.Translate(GetCustomLabelNoCount(includeHp: false)), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
					}
					return;
				}
			}
			cachedLabelMouseover = null;
			if (def.plant.dropLeaves)
			{
				MoteLeaf moteLeaf = MoteMaker.MakeStaticMote(Vector3.zero, base.Map, ThingDefOf.Mote_Leaf) as MoteLeaf;
				if (moteLeaf != null)
				{
					float num4 = def.plant.visualSizeRange.LerpThroughRange(growthInt);
					float treeHeight = def.graphicData.drawSize.x * num4;
					Vector3 b = Rand.InsideUnitCircleVec3 * LeafSpawnRadius;
					moteLeaf.Initialize(base.Position.ToVector3Shifted() + Vector3.up * Rand.Range(LeafSpawnYMin, LeafSpawnYMax) + b + Vector3.forward * def.graphicData.shadowData.offset.z, Rand.Value * 2000.TicksToSeconds(), b.z > 0f, treeHeight);
				}
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
			return GenMath.RoundRandom(harvestYield * num * Mathf.Lerp(0.5f, 1f, (float)HitPoints / (float)base.MaxHitPoints) * Find.Storyteller.difficultyValues.cropYieldFactor);
		}

		public override void Print(SectionLayer layer)
		{
			Vector3 a = this.TrueCenter();
			Rand.PushState();
			Rand.Seed = base.Position.GetHashCode();
			int num = Mathf.CeilToInt(growthInt * (float)def.plant.maxMeshCount);
			if (num < 1)
			{
				num = 1;
			}
			float num2 = def.plant.visualSizeRange.LerpThroughRange(growthInt);
			float num3 = def.graphicData.drawSize.x * num2;
			Vector3 center = Vector3.zero;
			int num4 = 0;
			int[] positionIndices = PlantPosIndices.GetPositionIndices(this);
			bool flag = false;
			foreach (int num5 in positionIndices)
			{
				if (def.plant.maxMeshCount == 1)
				{
					center = a + Gen.RandomHorizontalVector(0.05f);
					float num6 = base.Position.z;
					if (center.z - num2 / 2f < num6)
					{
						center.z = num6 + num2 / 2f;
						flag = true;
					}
				}
				else
				{
					int num7 = 1;
					switch (def.plant.maxMeshCount)
					{
					case 1:
						num7 = 1;
						break;
					case 4:
						num7 = 2;
						break;
					case 9:
						num7 = 3;
						break;
					case 16:
						num7 = 4;
						break;
					case 25:
						num7 = 5;
						break;
					default:
						Log.Error(string.Concat(def, " must have plant.MaxMeshCount that is a perfect square."));
						break;
					}
					float num8 = 1f / (float)num7;
					center = base.Position.ToVector3();
					center.y = def.Altitude;
					center.x += 0.5f * num8;
					center.z += 0.5f * num8;
					int num9 = num5 / num7;
					int num10 = num5 % num7;
					center.x += (float)num9 * num8;
					center.z += (float)num10 * num8;
					float max = num8 * 0.3f;
					center += Gen.RandomHorizontalVector(max);
				}
				bool @bool = Rand.Bool;
				Material matSingle = Graphic.MatSingle;
				PlantUtility.SetWindExposureColors(workingColors, this);
				Printer_Plane.PrintPlane(size: new Vector2(num3, num3), layer: layer, center: center, mat: matSingle, rot: 0f, flipUv: @bool, uvs: null, colors: workingColors, topVerticesAltitudeBias: 0.1f, uvzPayload: this.HashOffset() % 1024);
				num4++;
				if (num4 >= num)
				{
					break;
				}
			}
			if (def.graphicData.shadowData != null)
			{
				Vector3 center2 = a + def.graphicData.shadowData.offset * num2;
				if (flag)
				{
					center2.z = base.Position.ToVector3Shifted().z + def.graphicData.shadowData.offset.z;
				}
				center2.y -= 3f / 70f;
				Vector3 volume = def.graphicData.shadowData.volume * num2;
				Printer_Shadow.PrintShadow(layer, center2, volume, Rot4.North);
			}
			Rand.PopState();
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (LifeStage == PlantLifeStage.Growing)
			{
				stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
				stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
				if (!Blighted)
				{
					if (Resting)
					{
						stringBuilder.AppendLine("PlantResting".Translate());
					}
					if (!HasEnoughLightToGrow)
					{
						stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + def.plant.growMinGlow.ToStringPercent());
					}
					float growthRateFactor_Temperature = GrowthRateFactor_Temperature;
					if (growthRateFactor_Temperature < 0.99f)
					{
						if (growthRateFactor_Temperature < 0.01f)
						{
							stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
						}
						else
						{
							stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()));
						}
					}
				}
			}
			else if (LifeStage == PlantLifeStage.Mature)
			{
				if (HarvestableNow)
				{
					stringBuilder.AppendLine("ReadyToHarvest".Translate());
				}
				else
				{
					stringBuilder.AppendLine("Mature".Translate());
				}
			}
			if (DyingBecauseExposedToLight)
			{
				stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
			}
			if (Blighted)
			{
				stringBuilder.AppendLine("Blighted".Translate() + " (" + Blight.Severity.ToStringPercent() + ")");
			}
			string text = InspectStringPartsFromComps();
			if (!text.NullOrEmpty())
			{
				stringBuilder.Append(text);
			}
			return stringBuilder.ToString().TrimEndNewlines();
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
			if (Prefs.DevMode && Blighted)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Dev: Spread blight";
				command_Action.action = delegate
				{
					Blight.TryReproduceNow();
				};
				yield return command_Action;
			}
		}
	}
}
