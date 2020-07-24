using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Projectile : ThingWithComps
	{
		protected Vector3 origin;

		protected Vector3 destination;

		public LocalTargetInfo usedTarget;

		public LocalTargetInfo intendedTarget;

		protected ThingDef equipmentDef;

		protected Thing launcher;

		protected ThingDef targetCoverDef;

		private ProjectileHitFlags desiredHitFlags = ProjectileHitFlags.All;

		protected float weaponDamageMultiplier = 1f;

		protected bool landed;

		protected int ticksToImpact;

		private Sustainer ambientSustainer;

		private static List<IntVec3> checkedCells = new List<IntVec3>();

		private static readonly List<Thing> cellThingsFiltered = new List<Thing>();

		public ProjectileHitFlags HitFlags
		{
			get
			{
				if (def.projectile.alwaysFreeIntercept)
				{
					return ProjectileHitFlags.All;
				}
				if (def.projectile.flyOverhead)
				{
					return ProjectileHitFlags.None;
				}
				return desiredHitFlags;
			}
			set
			{
				desiredHitFlags = value;
			}
		}

		protected float StartingTicksToImpact
		{
			get
			{
				float num = (origin - destination).magnitude / def.projectile.SpeedTilesPerTick;
				if (num <= 0f)
				{
					num = 0.001f;
				}
				return num;
			}
		}

		protected IntVec3 DestinationCell => new IntVec3(destination);

		public virtual Vector3 ExactPosition
		{
			get
			{
				Vector3 b = (destination - origin) * Mathf.Clamp01(1f - (float)ticksToImpact / StartingTicksToImpact);
				return origin + b + Vector3.up * def.Altitude;
			}
		}

		public virtual Quaternion ExactRotation => Quaternion.LookRotation(destination - origin);

		public override Vector3 DrawPos => ExactPosition;

		public int DamageAmount => def.projectile.GetDamageAmount(weaponDamageMultiplier);

		public float ArmorPenetration => def.projectile.GetArmorPenetration(weaponDamageMultiplier);

		public ThingDef EquipmentDef => equipmentDef;

		public Thing Launcher => launcher;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref origin, "origin");
			Scribe_Values.Look(ref destination, "destination");
			Scribe_Values.Look(ref ticksToImpact, "ticksToImpact", 0);
			Scribe_TargetInfo.Look(ref usedTarget, "usedTarget");
			Scribe_TargetInfo.Look(ref intendedTarget, "intendedTarget");
			Scribe_References.Look(ref launcher, "launcher");
			Scribe_Defs.Look(ref equipmentDef, "equipmentDef");
			Scribe_Defs.Look(ref targetCoverDef, "targetCoverDef");
			Scribe_Values.Look(ref desiredHitFlags, "desiredHitFlags", ProjectileHitFlags.All);
			Scribe_Values.Look(ref weaponDamageMultiplier, "weaponDamageMultiplier", 1f);
			Scribe_Values.Look(ref landed, "landed", defaultValue: false);
		}

		public void Launch(Thing launcher, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null)
		{
			Launch(launcher, base.Position.ToVector3Shifted(), usedTarget, intendedTarget, hitFlags, equipment);
		}

		public void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment = null, ThingDef targetCoverDef = null)
		{
			this.launcher = launcher;
			this.origin = origin;
			this.usedTarget = usedTarget;
			this.intendedTarget = intendedTarget;
			this.targetCoverDef = targetCoverDef;
			HitFlags = hitFlags;
			if (equipment != null)
			{
				equipmentDef = equipment.def;
				weaponDamageMultiplier = equipment.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier);
			}
			else
			{
				equipmentDef = null;
				weaponDamageMultiplier = 1f;
			}
			destination = usedTarget.Cell.ToVector3Shifted() + Gen.RandomHorizontalVector(0.3f);
			ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
			if (ticksToImpact < 1)
			{
				ticksToImpact = 1;
			}
			if (!def.projectile.soundAmbient.NullOrUndefined())
			{
				SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
				ambientSustainer = def.projectile.soundAmbient.TrySpawnSustainer(info);
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (landed)
			{
				return;
			}
			Vector3 exactPosition = ExactPosition;
			ticksToImpact--;
			if (!ExactPosition.InBounds(base.Map))
			{
				ticksToImpact++;
				base.Position = ExactPosition.ToIntVec3();
				Destroy();
				return;
			}
			Vector3 exactPosition2 = ExactPosition;
			if (CheckForFreeInterceptBetween(exactPosition, exactPosition2))
			{
				return;
			}
			base.Position = ExactPosition.ToIntVec3();
			if (ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && def.projectile.soundImpactAnticipate != null)
			{
				def.projectile.soundImpactAnticipate.PlayOneShot(this);
			}
			if (ticksToImpact <= 0)
			{
				if (DestinationCell.InBounds(base.Map))
				{
					base.Position = DestinationCell;
				}
				ImpactSomething();
			}
			else if (ambientSustainer != null)
			{
				ambientSustainer.Maintain();
			}
		}

		private bool CheckForFreeInterceptBetween(Vector3 lastExactPos, Vector3 newExactPos)
		{
			if (lastExactPos == newExactPos)
			{
				return false;
			}
			List<Thing> list = base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, lastExactPos, newExactPos))
				{
					Destroy();
					return true;
				}
			}
			IntVec3 intVec = lastExactPos.ToIntVec3();
			IntVec3 intVec2 = newExactPos.ToIntVec3();
			if (intVec2 == intVec)
			{
				return false;
			}
			if (!intVec.InBounds(base.Map) || !intVec2.InBounds(base.Map))
			{
				return false;
			}
			if (intVec2.AdjacentToCardinal(intVec))
			{
				return CheckForFreeIntercept(intVec2);
			}
			if (VerbUtility.InterceptChanceFactorFromDistance(origin, intVec2) <= 0f)
			{
				return false;
			}
			Vector3 vect = lastExactPos;
			Vector3 v = newExactPos - lastExactPos;
			Vector3 vector = v.normalized * 0.2f;
			int num = (int)(v.MagnitudeHorizontal() / 0.2f);
			checkedCells.Clear();
			int num2 = 0;
			IntVec3 intVec3;
			do
			{
				vect += vector;
				intVec3 = vect.ToIntVec3();
				if (!checkedCells.Contains(intVec3))
				{
					if (CheckForFreeIntercept(intVec3))
					{
						return true;
					}
					checkedCells.Add(intVec3);
				}
				num2++;
				if (num2 > num)
				{
					return false;
				}
			}
			while (!(intVec3 == intVec2));
			return false;
		}

		private bool CheckForFreeIntercept(IntVec3 c)
		{
			if (destination.ToIntVec3() == c)
			{
				return false;
			}
			float num = VerbUtility.InterceptChanceFactorFromDistance(origin, c);
			if (num <= 0f)
			{
				return false;
			}
			bool flag = false;
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (!CanHit(thing))
				{
					continue;
				}
				bool flag2 = false;
				if (thing.def.Fillage == FillCategory.Full)
				{
					Building_Door building_Door = thing as Building_Door;
					if (building_Door == null || !building_Door.Open)
					{
						ThrowDebugText("int-wall", c);
						Impact(thing);
						return true;
					}
					flag2 = true;
				}
				float num2 = 0f;
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					num2 = 0.4f * Mathf.Clamp(pawn.BodySize, 0.1f, 2f);
					if (pawn.GetPosture() != 0)
					{
						num2 *= 0.1f;
					}
					if (launcher != null && pawn.Faction != null && launcher.Faction != null && !pawn.Faction.HostileTo(launcher.Faction))
					{
						num2 *= 0.4f;
					}
				}
				else if (thing.def.fillPercent > 0.2f)
				{
					num2 = (flag2 ? 0.05f : ((!DestinationCell.AdjacentTo8Way(c)) ? (thing.def.fillPercent * 0.15f) : (thing.def.fillPercent * 1f)));
				}
				num2 *= num;
				if (num2 > 1E-05f)
				{
					if (Rand.Chance(num2))
					{
						ThrowDebugText("int-" + num2.ToStringPercent(), c);
						Impact(thing);
						return true;
					}
					flag = true;
					ThrowDebugText(num2.ToStringPercent(), c);
				}
			}
			if (!flag)
			{
				ThrowDebugText("o", c);
			}
			return false;
		}

		private void ThrowDebugText(string text, IntVec3 c)
		{
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(c.ToVector3Shifted(), base.Map, text);
			}
		}

		public override void Draw()
		{
			Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize), DrawPos, ExactRotation, def.DrawMatSingle, 0);
			Comps_PostDraw();
		}

		protected bool CanHit(Thing thing)
		{
			if (!thing.Spawned)
			{
				return false;
			}
			if (thing == launcher)
			{
				return false;
			}
			bool flag = false;
			foreach (IntVec3 item in thing.OccupiedRect())
			{
				List<Thing> thingList = item.GetThingList(base.Map);
				bool flag2 = false;
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] != thing && thingList[i].def.Fillage == FillCategory.Full && thingList[i].def.Altitude >= thing.def.Altitude)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			ProjectileHitFlags hitFlags = HitFlags;
			if (thing == intendedTarget && (hitFlags & ProjectileHitFlags.IntendedTarget) != 0)
			{
				return true;
			}
			if (thing != intendedTarget)
			{
				if (thing is Pawn)
				{
					if ((hitFlags & ProjectileHitFlags.NonTargetPawns) != 0)
					{
						return true;
					}
				}
				else if ((hitFlags & ProjectileHitFlags.NonTargetWorld) != 0)
				{
					return true;
				}
			}
			if (thing == intendedTarget && thing.def.Fillage == FillCategory.Full)
			{
				return true;
			}
			return false;
		}

		private void ImpactSomething()
		{
			if (def.projectile.flyOverhead)
			{
				RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
				if (roofDef != null)
				{
					if (roofDef.isThickRoof)
					{
						ThrowDebugText("hit-thick-roof", base.Position);
						def.projectile.soundHitThickRoof.PlayOneShot(new TargetInfo(base.Position, base.Map));
						Destroy();
						return;
					}
					if (base.Position.GetEdifice(base.Map) == null || base.Position.GetEdifice(base.Map).def.Fillage != FillCategory.Full)
					{
						RoofCollapserImmediate.DropRoofInCells(base.Position, base.Map);
					}
				}
			}
			if (usedTarget.HasThing && CanHit(usedTarget.Thing))
			{
				Pawn pawn = usedTarget.Thing as Pawn;
				if (pawn != null && pawn.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f && !Rand.Chance(0.2f))
				{
					ThrowDebugText("miss-laying", base.Position);
					Impact(null);
				}
				else
				{
					Impact(usedTarget.Thing);
				}
				return;
			}
			cellThingsFiltered.Clear();
			List<Thing> thingList = base.Position.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if ((thing.def.category == ThingCategory.Building || thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Plant) && CanHit(thing))
				{
					cellThingsFiltered.Add(thing);
				}
			}
			cellThingsFiltered.Shuffle();
			for (int j = 0; j < cellThingsFiltered.Count; j++)
			{
				Thing thing2 = cellThingsFiltered[j];
				Pawn pawn2 = thing2 as Pawn;
				float num;
				if (pawn2 != null)
				{
					num = 0.5f * Mathf.Clamp(pawn2.BodySize, 0.1f, 2f);
					if (pawn2.GetPosture() != 0 && (origin - destination).MagnitudeHorizontalSquared() >= 20.25f)
					{
						num *= 0.2f;
					}
					if (launcher != null && pawn2.Faction != null && launcher.Faction != null && !pawn2.Faction.HostileTo(launcher.Faction))
					{
						num *= VerbUtility.InterceptChanceFactorFromDistance(origin, base.Position);
					}
				}
				else
				{
					num = 1.5f * thing2.def.fillPercent;
				}
				if (Rand.Chance(num))
				{
					ThrowDebugText("hit-" + num.ToStringPercent(), base.Position);
					Impact(cellThingsFiltered.RandomElement());
					return;
				}
				ThrowDebugText("miss-" + num.ToStringPercent(), base.Position);
			}
			Impact(null);
		}

		protected virtual void Impact(Thing hitThing)
		{
			GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
			Destroy();
		}
	}
}
