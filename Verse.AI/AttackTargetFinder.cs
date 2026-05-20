using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI.Group;

namespace Verse.AI
{
	public static class AttackTargetFinder
	{
		private const float FriendlyFireScoreOffsetPerHumanlikeOrMechanoid = 18f;

		private const float FriendlyFireScoreOffsetPerAnimal = 7f;

		private const float FriendlyFireScoreOffsetPerNonPawn = 10f;

		private const float FriendlyFireScoreOffsetSelf = 40f;

		private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>(128);

		private static List<Pair<IAttackTarget, float>> availableShootingTargets = new List<Pair<IAttackTarget, float>>();

		private static List<float> tmpTargetScores = new List<float>();

		private static List<bool> tmpCanShootAtTarget = new List<bool>();

		private static readonly List<IntVec3> tempDestList = new List<IntVec3>();

		private static readonly List<IntVec3> tempSourceList = new List<IntVec3>();

		public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = float.MaxValue, bool canBashDoors = false, bool canTakeTargetsCloserThanEffectiveMinRange = true, bool canBashFences = false, bool onlyRanged = false)
		{
			Thing searcherThing = searcher.Thing;
			Pawn searcherPawn = searcher as Pawn;
			Verb verb = searcher.CurrentEffectiveVerb;
			if (verb == null)
			{
				Log.Error("BestAttackTarget with " + searcher.ToStringSafe() + " who has no attack verb.");
				return null;
			}
			bool onlyTargetMachines = verb.IsEMP();
			float minDistSquared = minDist * minDist;
			float num = maxTravelRadiusFromLocus + verb.EffectiveRange;
			float maxLocusDistSquared = num * num;
			Func<IntVec3, bool> losValidator = null;
			if ((verb.EquipmentSource == null || !verb.EquipmentSource.TryGetComp<CompUniqueWeapon>(out var comp) || !comp.IgnoreAccuracyMaluses) && (flags & TargetScanFlags.LOSBlockableByGas) != TargetScanFlags.None)
			{
				losValidator = (IntVec3 vec3) => !vec3.AnyGas(searcherThing.Map, GasType.BlindSmoke);
			}
			Predicate<IAttackTarget> innerValidator = delegate(IAttackTarget t)
			{
				Thing thing = t.Thing;
				if (t == searcher)
				{
					return false;
				}
				if (minDistSquared > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < minDistSquared)
				{
					return false;
				}
				if (!canTakeTargetsCloserThanEffectiveMinRange)
				{
					float num4 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
					if (num4 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num4 * num4)
					{
						return false;
					}
				}
				if (maxTravelRadiusFromLocus < 9999f && (float)(thing.Position - locus).LengthHorizontalSquared > maxLocusDistSquared)
				{
					return false;
				}
				if (!searcherThing.HostileTo(thing))
				{
					return false;
				}
				if (validator != null && !validator(thing))
				{
					return false;
				}
				if (searcherPawn != null)
				{
					Lord lord = searcherPawn.GetLord();
					if (lord != null && !lord.LordJob.ValidateAttackTarget(searcherPawn, thing))
					{
						return false;
					}
				}
				if ((flags & TargetScanFlags.NeedNotUnderThickRoof) != TargetScanFlags.None)
				{
					RoofDef roof = thing.Position.GetRoof(thing.Map);
					if (roof != null && roof.isThickRoof)
					{
						return false;
					}
				}
				if ((flags & TargetScanFlags.NeedLOSToAll) != TargetScanFlags.None)
				{
					if (losValidator != null && (!losValidator(searcherThing.Position) || !losValidator(thing.Position)))
					{
						return false;
					}
					if (!searcherThing.CanSee(thing, losValidator))
					{
						if (t is Pawn)
						{
							if ((flags & TargetScanFlags.NeedLOSToPawns) != TargetScanFlags.None)
							{
								return false;
							}
						}
						else if ((flags & TargetScanFlags.NeedLOSToNonPawns) != TargetScanFlags.None)
						{
							return false;
						}
					}
				}
				if (((flags & TargetScanFlags.NeedThreat) != TargetScanFlags.None || (flags & TargetScanFlags.NeedAutoTargetable) != TargetScanFlags.None) && t.ThreatDisabled(searcher))
				{
					return false;
				}
				if ((flags & TargetScanFlags.NeedAutoTargetable) != TargetScanFlags.None && !IsAutoTargetable(t))
				{
					return false;
				}
				if ((flags & TargetScanFlags.NeedActiveThreat) != TargetScanFlags.None && !GenHostility.IsActiveThreatTo(t, searcher.Thing.Faction))
				{
					return false;
				}
				Pawn pawn = t as Pawn;
				if (onlyTargetMachines && pawn != null && pawn.RaceProps.IsFlesh)
				{
					return false;
				}
				if ((flags & TargetScanFlags.NeedNonBurning) != TargetScanFlags.None && thing.IsBurning())
				{
					return false;
				}
				if (searcherThing.def.race != null && (int)searcherThing.def.race.intelligence >= 2)
				{
					CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
					if (compExplosive != null && compExplosive.wickStarted)
					{
						return false;
					}
				}
				Pawn pawn2 = searcherPawn;
				if ((pawn2 != null && pawn2.IsColonist) || searcherThing?.Faction == Faction.OfPlayer)
				{
					if (thing.def.size.x == 1 && thing.def.size.z == 1)
					{
						if (thing.Position.Fogged(thing.Map))
						{
							return false;
						}
					}
					else
					{
						bool flag3 = false;
						foreach (IntVec3 item in thing.OccupiedRect())
						{
							if (!item.Fogged(thing.Map))
							{
								flag3 = true;
								break;
							}
						}
						if (!flag3)
						{
							return false;
						}
					}
				}
				return true;
			};
			if ((HasRangedAttack(searcher) || onlyRanged) && (searcherPawn == null || !searcherPawn.InAggroMentalState))
			{
				tmpTargets.Clear();
				tmpTargets.AddRange(searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher));
				tmpTargets.RemoveAll((IAttackTarget t) => ShouldIgnoreNoncombatant(searcherThing, t, flags));
				if ((flags & TargetScanFlags.NeedReachable) != TargetScanFlags.None)
				{
					Predicate<IAttackTarget> oldValidator = innerValidator;
					innerValidator = (IAttackTarget t) => oldValidator(t) && CanReach(searcherThing, t.Thing, canBashDoors, canBashFences);
				}
				bool flag = false;
				for (int num2 = 0; num2 < tmpTargets.Count; num2++)
				{
					IAttackTarget attackTarget = tmpTargets[num2];
					if (attackTarget.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) && innerValidator(attackTarget) && CanShootAtFromCurrentPosition(attackTarget, searcher, verb))
					{
						flag = true;
						break;
					}
				}
				IAttackTarget result;
				if (flag)
				{
					tmpTargets.RemoveAll((IAttackTarget x) => !x.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) || !innerValidator(x));
					result = GetRandomShootingTargetByScore(tmpTargets, searcher, verb);
				}
				else
				{
					bool num3 = (flags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) != 0;
					bool flag2 = (flags & TargetScanFlags.NeedReachable) != 0;
					result = (IAttackTarget)GenClosest.ClosestThing_Global(validator: (!num3 || flag2) ? ((Predicate<Thing>)((Thing t) => innerValidator((IAttackTarget)t))) : ((Predicate<Thing>)((Thing t) => innerValidator((IAttackTarget)t) && (CanReach(searcherThing, t, canBashDoors, canBashFences) || CanShootAtFromCurrentPosition((IAttackTarget)t, searcher, verb)))), center: searcherThing.Position, searchSet: tmpTargets, maxDistance: maxDist);
				}
				tmpTargets.Clear();
				return result;
			}
			if (searcherPawn != null && searcherPawn.mindState.duty != null && searcherPawn.mindState.duty.radius > 0f && !searcherPawn.InMentalState)
			{
				Predicate<IAttackTarget> oldValidator2 = innerValidator;
				innerValidator = delegate(IAttackTarget t)
				{
					if (!oldValidator2(t))
					{
						return false;
					}
					return t.Thing.Position.InHorDistOf(searcherPawn.mindState.duty.focus.Cell, searcherPawn.mindState.duty.radius) ? true : false;
				};
			}
			Predicate<IAttackTarget> oldValidator3 = innerValidator;
			innerValidator = delegate(IAttackTarget t)
			{
				if (!oldValidator3(t))
				{
					return false;
				}
				return !ShouldIgnoreNoncombatant(searcherThing, t, flags);
			};
			IAttackTarget attackTarget2 = (IAttackTarget)GenClosest.ClosestThingReachable(searcherThing.Position, searcherThing.Map, ThingRequest.ForGroup(ThingRequestGroup.AttackTarget), PathEndMode.Touch, TraverseParms.For(searcherPawn, Danger.Deadly, TraverseMode.ByPawn, canBashDoors, alwaysUseAvoidGrid: false, canBashFences), maxDist, (Thing x) => innerValidator((IAttackTarget)x), null, 0, (maxDist > 800f) ? (-1) : 40);
			if (attackTarget2 != null && PawnUtility.ShouldCollideWithPawns(searcherPawn))
			{
				IAttackTarget attackTarget3 = FindBestReachableMeleeTarget(innerValidator, searcherPawn, maxDist, canBashDoors, canBashFences);
				if (attackTarget3 != null)
				{
					float lengthHorizontal = (searcherPawn.Position - attackTarget2.Thing.Position).LengthHorizontal;
					float lengthHorizontal2 = (searcherPawn.Position - attackTarget3.Thing.Position).LengthHorizontal;
					if (Mathf.Abs(lengthHorizontal - lengthHorizontal2) < 50f)
					{
						attackTarget2 = attackTarget3;
					}
				}
			}
			return attackTarget2;
		}

		private static bool ShouldIgnoreNoncombatant(Thing searcherThing, IAttackTarget t, TargetScanFlags flags)
		{
			if (!(t is Pawn pawn))
			{
				return false;
			}
			if (pawn.IsCombatant())
			{
				return false;
			}
			if ((flags & TargetScanFlags.IgnoreNonCombatants) != TargetScanFlags.None)
			{
				return true;
			}
			if (GenSight.LineOfSightToThing(searcherThing.Position, pawn, searcherThing.Map))
			{
				return false;
			}
			return true;
		}

		private static bool CanReach(Thing searcher, Thing target, bool canBashDoors, bool canBashFences)
		{
			if (searcher is Pawn pawn)
			{
				if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Some, canBashDoors, canBashFences))
				{
					return false;
				}
			}
			else
			{
				TraverseMode mode = (canBashDoors ? TraverseMode.PassDoors : TraverseMode.NoPassClosedDoors);
				if (!searcher.Map.reachability.CanReach(searcher.Position, target, PathEndMode.Touch, TraverseParms.For(mode)))
				{
					return false;
				}
			}
			return true;
		}

		private static IAttackTarget FindBestReachableMeleeTarget(Predicate<IAttackTarget> validator, Pawn searcherPawn, float maxTargDist, bool canBashDoors, bool canBashFences)
		{
			maxTargDist = Mathf.Min(maxTargDist, 30f);
			IAttackTarget reachableTarget = null;
			searcherPawn.Map.floodFiller.FloodFill(searcherPawn.Position, (Predicate<IntVec3>)PassCheck, (Func<IntVec3, bool>)Processor, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
			return reachableTarget;
			IAttackTarget BestTargetOnCell(IntVec3 x)
			{
				List<Thing> thingList = x.GetThingList(searcherPawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is IAttackTarget attackTarget && validator(attackTarget) && ReachabilityImmediate.CanReachImmediate(x, thing, searcherPawn.Map, PathEndMode.Touch, searcherPawn) && (searcherPawn.CanReachImmediate(thing, PathEndMode.Touch) || searcherPawn.Map.attackTargetReservationManager.CanReserve(searcherPawn, attackTarget)))
					{
						return attackTarget;
					}
				}
				return null;
			}
			bool PassCheck(IntVec3 x)
			{
				if (!x.WalkableBy(searcherPawn.Map, searcherPawn))
				{
					return false;
				}
				if ((float)x.DistanceToSquared(searcherPawn.Position) > maxTargDist * maxTargDist)
				{
					return false;
				}
				Building edifice = x.GetEdifice(searcherPawn.Map);
				if (edifice != null)
				{
					if (!canBashDoors && edifice is Building_Door building_Door && !building_Door.CanPhysicallyPass(searcherPawn))
					{
						return false;
					}
					if (!canBashFences && edifice.def.IsFence && searcherPawn.FenceBlocked)
					{
						return false;
					}
				}
				if (PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, actAsIfHadCollideWithPawnsJob: true))
				{
					return false;
				}
				return true;
			}
			bool Processor(IntVec3 x)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = x + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(searcherPawn.Map) && ReachabilityImmediate.CanReachImmediate(x, intVec, searcherPawn.Map, PathEndMode.Touch, searcherPawn))
					{
						IAttackTarget attackTarget = BestTargetOnCell(intVec);
						if (attackTarget != null)
						{
							reachableTarget = attackTarget;
							break;
						}
					}
				}
				return reachableTarget != null;
			}
		}

		private static bool HasRangedAttack(IAttackTargetSearcher t)
		{
			Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null)
			{
				return !currentEffectiveVerb.verbProps.IsMeleeAttack;
			}
			return false;
		}

		private static bool CanShootAtFromCurrentPosition(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			return verb?.CanHitTargetFrom(searcher.Thing.Position, target.Thing) ?? false;
		}

		private static IAttackTarget GetRandomShootingTargetByScore(List<IAttackTarget> targets, IAttackTargetSearcher searcher, Verb verb)
		{
			if (GetAvailableShootingTargetsByScore(targets, searcher, verb).TryRandomElementByWeight((Pair<IAttackTarget, float> x) => x.Second, out var result))
			{
				return result.First;
			}
			return null;
		}

		private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(List<IAttackTarget> rawTargets, IAttackTargetSearcher searcher, Verb verb)
		{
			availableShootingTargets.Clear();
			if (rawTargets.Count == 0)
			{
				return availableShootingTargets;
			}
			tmpTargetScores.Clear();
			tmpCanShootAtTarget.Clear();
			float num = 0f;
			IAttackTarget attackTarget = null;
			for (int i = 0; i < rawTargets.Count; i++)
			{
				tmpTargetScores.Add(float.MinValue);
				tmpCanShootAtTarget.Add(item: false);
				if (rawTargets[i] == searcher)
				{
					continue;
				}
				bool flag = CanShootAtFromCurrentPosition(rawTargets[i], searcher, verb);
				tmpCanShootAtTarget[i] = flag;
				if (flag)
				{
					float shootingTargetScore = GetShootingTargetScore(rawTargets[i], searcher, verb);
					tmpTargetScores[i] = shootingTargetScore;
					if (attackTarget == null || shootingTargetScore > num)
					{
						attackTarget = rawTargets[i];
						num = shootingTargetScore;
					}
				}
			}
			if (num < 1f)
			{
				if (attackTarget != null)
				{
					availableShootingTargets.Add(new Pair<IAttackTarget, float>(attackTarget, 1f));
				}
			}
			else
			{
				float num2 = num - 30f;
				for (int j = 0; j < rawTargets.Count; j++)
				{
					if (rawTargets[j] != searcher && tmpCanShootAtTarget[j])
					{
						float num3 = tmpTargetScores[j];
						if (num3 >= num2)
						{
							float second = Mathf.InverseLerp(num - 30f, num, num3);
							availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[j], second));
						}
					}
				}
			}
			return availableShootingTargets;
		}

		private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			float num = 60f;
			num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
			if (target.TargetCurrentlyAimingAt == searcher.Thing)
			{
				num += 10f;
			}
			if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
			{
				num += 40f;
			}
			num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
			if (target is Pawn pawn)
			{
				num -= NonCombatantScore(pawn);
				if (verb.verbProps.ai_TargetHasRangedAttackScoreOffset != 0f && pawn.CurrentEffectiveVerb != null && pawn.CurrentEffectiveVerb.verbProps.Ranged)
				{
					num += verb.verbProps.ai_TargetHasRangedAttackScoreOffset;
				}
				if (pawn.Downed)
				{
					num -= 50f;
				}
			}
			num += FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb);
			num += FriendlyFireConeTargetScoreOffset(target, searcher, verb);
			return num * target.TargetPriorityFactor;
		}

		private static float NonCombatantScore(Thing target)
		{
			if (!(target is Pawn pawn))
			{
				return 0f;
			}
			if (!pawn.IsCombatant())
			{
				return 50f;
			}
			if (pawn.DevelopmentalStage.Juvenile())
			{
				return 25f;
			}
			return 0f;
		}

		private static float FriendlyFireBlastRadiusTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
			{
				return 0f;
			}
			Map map = target.Thing.Map;
			IntVec3 position = target.Thing.Position;
			int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map))
				{
					continue;
				}
				bool flag = true;
				List<Thing> thingList = intVec.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (!(thingList[j] is IAttackTarget) || thingList[j] == target)
					{
						continue;
					}
					if (flag)
					{
						if (!GenSight.LineOfSight(position, intVec, map, skipFirstCell: true))
						{
							break;
						}
						flag = false;
					}
					float num3 = ((thingList[j] == searcher) ? 40f : ((!(thingList[j] is Pawn)) ? 10f : (thingList[j].def.race.Animal ? 7f : 18f)));
					num2 = ((!searcher.Thing.HostileTo(thingList[j])) ? (num2 - num3) : (num2 + num3 * 0.6f));
				}
			}
			return num2;
		}

		private static float FriendlyFireConeTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			if (!(searcher.Thing is Pawn pawn))
			{
				return 0f;
			}
			if ((int)pawn.RaceProps.intelligence < 1)
			{
				return 0f;
			}
			if (pawn.RaceProps.IsMechanoid)
			{
				return 0f;
			}
			if (!(verb is Verb_Shoot verb_Shoot))
			{
				return 0f;
			}
			ThingDef defaultProjectile = verb_Shoot.verbProps.defaultProjectile;
			if (defaultProjectile == null)
			{
				return 0f;
			}
			if (defaultProjectile.projectile.flyOverhead)
			{
				return 0f;
			}
			Map map = pawn.Map;
			ShotReport report = ShotReport.HitReportFor(pawn, verb, (Thing)target);
			float radius = Mathf.Max(VerbUtility.CalculateAdjustedForcedMiss(verb.verbProps.ForcedMissRadius, report.ShootLine.Dest - report.ShootLine.Source), 1.5f);
			IEnumerable<IntVec3> enumerable = (from dest in GenRadial.RadialCellsAround(report.ShootLine.Dest, radius, useCenter: true)
				where dest.InBounds(map)
				select new ShootLine(report.ShootLine.Source, dest)).SelectMany((ShootLine line) => line.Points().Concat(line.Dest).TakeWhile((IntVec3 pos) => pos.CanBeSeenOverFast(map))).Distinct();
			float num = 0f;
			foreach (IntVec3 item in enumerable)
			{
				float num2 = VerbUtility.InterceptChanceFactorFromDistance(report.ShootLine.Source.ToVector3Shifted(), item);
				if (num2 <= 0f)
				{
					continue;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int num3 = 0; num3 < thingList.Count; num3++)
				{
					Thing thing = thingList[num3];
					if (thing is IAttackTarget && thing != target)
					{
						float num4 = ((thing == searcher) ? 40f : ((!(thing is Pawn)) ? 10f : (thing.def.race.Animal ? 7f : 18f)));
						num4 *= num2;
						num4 = ((!searcher.Thing.HostileTo(thing)) ? (num4 * -1f) : (num4 * 0.6f));
						num += num4;
					}
				}
			}
			return num;
		}

		public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
		{
			Verb currentEffectiveVerb = searcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe() + " who has no attack verb.");
				return null;
			}
			return BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.EffectiveRange), default(IntVec3), float.MaxValue, canBashDoors: false, canTakeTargetsCloserThanEffectiveMinRange: false);
		}

		public static bool CanSee(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
		{
			ShootLeanUtility.CalcShootableCellsOf(tempDestList, target, seer.Position);
			for (int i = 0; i < tempDestList.Count; i++)
			{
				if (GenSight.LineOfSight(seer.Position, tempDestList[i], seer.Map, skipFirstCell: true, validator))
				{
					return true;
				}
			}
			ShootLeanUtility.LeanShootingSourcesFromTo(seer.Position, target.Position, seer.Map, tempSourceList);
			for (int j = 0; j < tempSourceList.Count; j++)
			{
				if (!tempSourceList[j].CanBeSeenOver(seer.Map))
				{
					continue;
				}
				for (int k = 0; k < tempDestList.Count; k++)
				{
					if (GenSight.LineOfSight(tempSourceList[j], tempDestList[k], seer.Map, skipFirstCell: true, validator))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void DebugDrawAttackTargetScores_Update()
		{
			if (!(Find.Selector.SingleSelectedThing is IAttackTargetSearcher attackTargetSearcher) || attackTargetSearcher.Thing.Map != Find.CurrentMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null)
			{
				tmpTargets.Clear();
				List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					tmpTargets.Add((IAttackTarget)list[i]);
				}
				List<Pair<IAttackTarget, float>> availableShootingTargetsByScore = GetAvailableShootingTargetsByScore(tmpTargets, attackTargetSearcher, currentEffectiveVerb);
				for (int j = 0; j < availableShootingTargetsByScore.Count; j++)
				{
					GenDraw.DrawLineBetween(attackTargetSearcher.Thing.DrawPos, availableShootingTargetsByScore[j].First.Thing.DrawPos);
				}
			}
		}

		public static void DebugDrawAttackTargetScores_OnGUI()
		{
			if (!(Find.Selector.SingleSelectedThing is IAttackTargetSearcher attackTargetSearcher) || attackTargetSearcher.Thing.Map != Find.CurrentMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				return;
			}
			List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing != attackTargetSearcher)
				{
					string text;
					Color textColor;
					if (!CanShootAtFromCurrentPosition((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb))
					{
						text = "out of range";
						textColor = Color.red;
					}
					else
					{
						text = GetShootingTargetScore((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb).ToString("F0");
						textColor = new Color(0.25f, 1f, 0.25f);
					}
					GenMapUI.DrawThingLabel(thing.DrawPos.MapToUIPosition(), text, textColor);
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		public static void DebugDrawNonCombatantTimer_OnGUI()
		{
			List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, false))
			{
				foreach (Thing item in list)
				{
					if (!(item is Pawn { mindState: not null } pawn))
					{
						continue;
					}
					int lastCombatantTick = pawn.mindState.lastCombatantTick;
					Vector2 screenPos = pawn.DrawPos.MapToUIPosition();
					if (pawn.IsCombatant())
					{
						int num = lastCombatantTick + 3600 - Find.TickManager.TicksGame;
						if (pawn.IsPermanentCombatant() || num == 3600)
						{
							GenMapUI.DrawThingLabel(screenPos, "combatant", Color.red);
						}
						else
						{
							GenMapUI.DrawThingLabel(screenPos, $"combatant {num}", Color.red);
						}
					}
					else
					{
						GenMapUI.DrawThingLabel(screenPos, "non-combatant", Color.green);
					}
				}
			}
		}

		public static bool IsAutoTargetable(IAttackTarget target)
		{
			CompCanBeDormant compCanBeDormant = target.Thing.TryGetComp<CompCanBeDormant>();
			if (compCanBeDormant != null && !compCanBeDormant.Awake)
			{
				return false;
			}
			CompInitiatable compInitiatable = target.Thing.TryGetComp<CompInitiatable>();
			if (compInitiatable != null && !compInitiatable.Initiated)
			{
				return false;
			}
			if (target.Thing is Pawn pawn && pawn.RaceProps.IsMechanoid && !pawn.Awake())
			{
				return false;
			}
			return true;
		}
	}
}
