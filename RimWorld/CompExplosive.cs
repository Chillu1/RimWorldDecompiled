using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompExplosive : ThingComp
	{
		public bool wickStarted;

		protected int wickTicksLeft;

		private Thing instigator;

		private int countdownTicksLeft = -1;

		public bool destroyedThroughDetonation;

		private List<Thing> thingsIgnoredByExplosion;

		protected Sustainer wickSoundSustainer;

		public CompProperties_Explosive Props => (CompProperties_Explosive)props;

		protected int StartWickThreshold => Mathf.RoundToInt(Props.startWickHitPointsPercent * (float)parent.MaxHitPoints);

		private bool CanEverExplodeFromDamage
		{
			get
			{
				if (Props.chanceNeverExplodeFromDamage < 1E-05f)
				{
					return true;
				}
				Rand.PushState();
				Rand.Seed = parent.thingIDNumber.GetHashCode();
				bool result = Rand.Value < Props.chanceNeverExplodeFromDamage;
				Rand.PopState();
				return result;
			}
		}

		public void AddThingsIgnoredByExplosion(List<Thing> things)
		{
			if (thingsIgnoredByExplosion == null)
			{
				thingsIgnoredByExplosion = new List<Thing>();
			}
			thingsIgnoredByExplosion.AddRange(things);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref instigator, "instigator");
			Scribe_Collections.Look(ref thingsIgnoredByExplosion, "thingsIgnoredByExplosion", LookMode.Reference);
			Scribe_Values.Look(ref wickStarted, "wickStarted", defaultValue: false);
			Scribe_Values.Look(ref wickTicksLeft, "wickTicksLeft", 0);
			Scribe_Values.Look(ref destroyedThroughDetonation, "destroyedThroughDetonation", defaultValue: false);
			Scribe_Values.Look(ref countdownTicksLeft, "countdownTicksLeft", 0);
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (Props.countdownTicks.HasValue)
			{
				countdownTicksLeft = Props.countdownTicks.Value.RandomInRange;
			}
		}

		public override void CompTick()
		{
			if (countdownTicksLeft > 0)
			{
				countdownTicksLeft--;
				if (countdownTicksLeft == 0)
				{
					StartWick();
					countdownTicksLeft = -1;
				}
			}
			if (wickStarted)
			{
				if (wickSoundSustainer == null)
				{
					StartWickSustainer();
				}
				else
				{
					wickSoundSustainer.Maintain();
				}
				wickTicksLeft--;
				if (wickTicksLeft <= 0)
				{
					Detonate(parent.MapHeld);
				}
			}
		}

		private void StartWickSustainer()
		{
			SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
			wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
		}

		private void EndWickSustainer()
		{
			if (wickSoundSustainer != null)
			{
				wickSoundSustainer.End();
				wickSoundSustainer = null;
			}
		}

		public override void PostDraw()
		{
			if (wickStarted)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.BurningWick);
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			if (mode == DestroyMode.KillFinalize && Props.explodeOnKilled)
			{
				Detonate(previousMap, ignoreUnspawned: true);
			}
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
			if (!CanEverExplodeFromDamage)
			{
				return;
			}
			if (dinfo.Def.ExternalViolenceFor(parent) && dinfo.Amount >= (float)parent.HitPoints && CanExplodeFromDamageType(dinfo.Def))
			{
				if (parent.MapHeld != null)
				{
					Detonate(parent.MapHeld);
					if (parent.Destroyed)
					{
						absorbed = true;
					}
				}
			}
			else if (!wickStarted && Props.startWickOnDamageTaken != null && Props.startWickOnDamageTaken.Contains(dinfo.Def))
			{
				StartWick(dinfo.Instigator);
			}
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (CanEverExplodeFromDamage && CanExplodeFromDamageType(dinfo.Def) && !parent.Destroyed)
			{
				if (wickStarted && dinfo.Def == DamageDefOf.Stun)
				{
					StopWick();
				}
				else if (!wickStarted && parent.HitPoints <= StartWickThreshold && dinfo.Def.ExternalViolenceFor(parent))
				{
					StartWick(dinfo.Instigator);
				}
			}
		}

		public void StartWick(Thing instigator = null)
		{
			if (!wickStarted && !(ExplosiveRadius() <= 0f))
			{
				this.instigator = instigator;
				wickStarted = true;
				wickTicksLeft = Props.wickTicks.RandomInRange;
				StartWickSustainer();
				GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(parent, Props.explosiveDamageType);
			}
		}

		public void StopWick()
		{
			wickStarted = false;
			instigator = null;
		}

		public float ExplosiveRadius()
		{
			CompProperties_Explosive props = Props;
			float num = props.explosiveRadius;
			if (parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0f)
			{
				num += Mathf.Sqrt((float)(parent.stackCount - 1) * props.explosiveExpandPerStackcount);
			}
			if (props.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
			{
				num += Mathf.Sqrt(parent.GetComp<CompRefuelable>().Fuel * props.explosiveExpandPerFuel);
			}
			return num;
		}

		protected void Detonate(Map map, bool ignoreUnspawned = false)
		{
			if (!ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			CompProperties_Explosive props = Props;
			float num = ExplosiveRadius();
			if (props.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
			{
				parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
			}
			if (props.destroyThingOnExplosionSize <= num && !parent.Destroyed)
			{
				destroyedThroughDetonation = true;
				parent.Kill();
			}
			EndWickSustainer();
			wickStarted = false;
			if (map == null)
			{
				Log.Warning("Tried to detonate CompExplosive in a null map.");
				return;
			}
			if (props.explosionEffect != null)
			{
				Effecter effecter = props.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(instigator: (instigator == null || instigator.HostileTo(parent.Faction)) ? parent : instigator, center: parent.PositionHeld, map: map, radius: num, damType: props.explosiveDamageType, damAmount: props.damageAmountBase, armorPenetration: props.armorPenetrationBase, explosionSound: props.explosionSound, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: props.postExplosionSpawnThingDef, postExplosionSpawnChance: props.postExplosionSpawnChance, postExplosionSpawnThingCount: props.postExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: props.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: props.preExplosionSpawnThingDef, preExplosionSpawnChance: props.preExplosionSpawnChance, preExplosionSpawnThingCount: props.preExplosionSpawnThingCount, chanceToStartFire: props.chanceToStartFire, damageFalloff: props.damageFalloff, direction: null, ignoredThings: thingsIgnoredByExplosion);
		}

		private bool CanExplodeFromDamageType(DamageDef damage)
		{
			if (Props.requiredDamageTypeToExplode != null)
			{
				return Props.requiredDamageTypeToExplode == damage;
			}
			return true;
		}

		public override string CompInspectStringExtra()
		{
			string text = "";
			if (countdownTicksLeft != -1)
			{
				text += "DetonationCountdown".Translate(countdownTicksLeft.TicksToDays().ToString("0.0"));
			}
			if (Props.extraInspectStringKey != null)
			{
				text += ((text != "") ? "\n" : "") + Props.extraInspectStringKey.Translate();
			}
			return text;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (countdownTicksLeft > 0)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Trigger countdown";
				command_Action.action = delegate
				{
					countdownTicksLeft = 1;
				};
				yield return command_Action;
			}
		}
	}
}
