using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_ApparelTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		private ThingOwner<Apparel> wornApparel;

		private List<Apparel> lockedApparel;

		private int lastApparelWearoutTick = -1;

		private const int RecordWalkedNakedTaleIntervalTicks = 60000;

		private const float AutoUnlockHealthPctThreshold = 0.5f;

		private static readonly List<Apparel> EmptyApparel = new List<Apparel>();

		private static List<Apparel> tmpApparelList = new List<Apparel>();

		private static List<Apparel> tmpApparel = new List<Apparel>();

		public IThingHolder ParentHolder => pawn;

		public List<Apparel> WornApparel => wornApparel.InnerListForReading;

		public int WornApparelCount => wornApparel.Count;

		public bool AnyApparel => wornApparel.Count != 0;

		public bool AnyApparelLocked => !lockedApparel.NullOrEmpty();

		public bool AnyApparelUnlocked
		{
			get
			{
				if (!AnyApparelLocked)
				{
					return AnyApparel;
				}
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (!IsLocked(wornApparel[i]))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AllApparelLocked
		{
			get
			{
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (!IsLocked(wornApparel[i]))
					{
						return false;
					}
				}
				return true;
			}
		}

		public List<Apparel> LockedApparel
		{
			get
			{
				if (lockedApparel == null)
				{
					return EmptyApparel;
				}
				return lockedApparel;
			}
		}

		public IEnumerable<Apparel> UnlockedApparel
		{
			get
			{
				if (!AnyApparelLocked)
				{
					return WornApparel;
				}
				return WornApparel.Where((Apparel x) => !IsLocked(x));
			}
		}

		public bool PsychologicallyNude
		{
			get
			{
				if (pawn.gender == Gender.None)
				{
					return false;
				}
				if (pawn.IsWildMan())
				{
					return false;
				}
				HasBasicApparel(out bool hasPants, out bool hasShirt);
				if (!hasPants)
				{
					bool flag = false;
					foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
					{
						if (notMissingPart.IsInGroup(BodyPartGroupDefOf.Legs))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						hasPants = true;
					}
				}
				if (pawn.gender == Gender.Male)
				{
					return !hasPants;
				}
				if (pawn.gender == Gender.Female)
				{
					if (hasPants)
					{
						return !hasShirt;
					}
					return true;
				}
				return false;
			}
		}

		public Pawn_ApparelTracker(Pawn pawn)
		{
			this.pawn = pawn;
			wornApparel = new ThingOwner<Apparel>(this);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref wornApparel, "wornApparel", this);
			Scribe_Collections.Look(ref lockedApparel, "lockedApparel", LookMode.Reference);
			Scribe_Values.Look(ref lastApparelWearoutTick, "lastApparelWearoutTick", 0);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				SortWornApparelIntoDrawOrder();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && lockedApparel != null)
			{
				lockedApparel.RemoveAll((Apparel x) => x == null);
			}
		}

		public void ApparelTrackerTickRare()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (lastApparelWearoutTick < 0)
			{
				lastApparelWearoutTick = ticksGame;
			}
			if (ticksGame - lastApparelWearoutTick < 60000)
			{
				return;
			}
			if (!pawn.IsWorldPawn())
			{
				for (int num = wornApparel.Count - 1; num >= 0; num--)
				{
					TakeWearoutDamageForDay(wornApparel[num]);
				}
			}
			lastApparelWearoutTick = ticksGame;
		}

		public void ApparelTrackerTick()
		{
			wornApparel.ThingOwnerTick();
			if (pawn.IsColonist && pawn.Spawned && !pawn.Dead && pawn.IsHashIntervalTick(60000) && PsychologicallyNude)
			{
				TaleRecorder.RecordTale(TaleDefOf.WalkedNaked, pawn);
			}
			if (lockedApparel == null)
			{
				return;
			}
			for (int num = lockedApparel.Count - 1; num >= 0; num--)
			{
				if (lockedApparel[num].def.useHitPoints && (float)lockedApparel[num].HitPoints / (float)lockedApparel[num].MaxHitPoints < 0.5f)
				{
					Unlock(lockedApparel[num]);
				}
			}
		}

		public bool IsLocked(Apparel apparel)
		{
			if (lockedApparel != null)
			{
				return lockedApparel.Contains(apparel);
			}
			return false;
		}

		public void Lock(Apparel apparel)
		{
			if (!IsLocked(apparel))
			{
				if (lockedApparel == null)
				{
					lockedApparel = new List<Apparel>();
				}
				lockedApparel.Add(apparel);
			}
		}

		public void Unlock(Apparel apparel)
		{
			if (IsLocked(apparel))
			{
				lockedApparel.Remove(apparel);
			}
		}

		public void LockAll()
		{
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Lock(wornApparel[i]);
			}
		}

		private void TakeWearoutDamageForDay(Thing ap)
		{
			int num = GenMath.RoundRandom(ap.def.apparel.wearPerDay);
			if (num > 0)
			{
				ap.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, num));
			}
			if (ap.Destroyed && PawnUtility.ShouldSendNotificationAbout(pawn) && !pawn.Dead)
			{
				Messages.Message(GenText.CapitalizeFirst("MessageWornApparelDeterioratedAway".Translate(GenLabel.ThingLabel(ap.def, ap.Stuff), pawn)), pawn, MessageTypeDefOf.NegativeEvent);
			}
		}

		public bool CanWearWithoutDroppingAnything(ThingDef apDef)
		{
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(apDef, wornApparel[i].def, pawn.RaceProps.body))
				{
					return false;
				}
			}
			return true;
		}

		public void Wear(Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
		{
			if (newApparel.Spawned)
			{
				newApparel.DeSpawn();
			}
			if (!ApparelUtility.HasPartsToWear(pawn, newApparel.def))
			{
				Log.Warning(string.Concat(pawn, " tried to wear ", newApparel, " but he has no body parts required to wear it."));
				return;
			}
			if (EquipmentUtility.IsBiocoded(newApparel) && !EquipmentUtility.IsBiocodedFor(newApparel, pawn))
			{
				CompBiocodable compBiocodable = newApparel.TryGetComp<CompBiocodable>();
				Log.Warning(string.Concat(pawn, " tried to wear ", newApparel, " but it is biocoded for ", compBiocodable.CodedPawnLabel, " ."));
				return;
			}
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				Apparel apparel = wornApparel[num];
				if (!ApparelUtility.CanWearTogether(newApparel.def, apparel.def, pawn.RaceProps.body))
				{
					if (dropReplacedApparel)
					{
						bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
						if (!TryDrop(apparel, out Apparel _, pawn.PositionHeld, forbid))
						{
							Log.Error(string.Concat(pawn, " could not drop ", apparel));
							return;
						}
					}
					else
					{
						Remove(apparel);
					}
				}
			}
			if (newApparel.Wearer != null)
			{
				Log.Warning(string.Concat(pawn, " is trying to wear ", newApparel, " but this apparel already has a wearer (", newApparel.Wearer, "). This may or may not cause bugs."));
			}
			wornApparel.TryAdd(newApparel, canMergeWithExistingStacks: false);
			if (locked)
			{
				Lock(newApparel);
			}
		}

		public void Remove(Apparel ap)
		{
			wornApparel.Remove(ap);
		}

		public bool TryDrop(Apparel ap)
		{
			Apparel resultingAp;
			return TryDrop(ap, out resultingAp);
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp)
		{
			return TryDrop(ap, out resultingAp, pawn.PositionHeld);
		}

		public bool TryDrop(Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true)
		{
			if (wornApparel.TryDrop(ap, pos, pawn.MapHeld, ThingPlaceMode.Near, out resultingAp))
			{
				if (resultingAp != null)
				{
					resultingAp.SetForbidden(forbid, warnOnFail: false);
				}
				return true;
			}
			return false;
		}

		public void DropAll(IntVec3 pos, bool forbid = true, bool dropLocked = true)
		{
			tmpApparelList.Clear();
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (dropLocked || !IsLocked(wornApparel[i]))
				{
					tmpApparelList.Add(wornApparel[i]);
				}
			}
			for (int j = 0; j < tmpApparelList.Count; j++)
			{
				TryDrop(tmpApparelList[j], out Apparel _, pos, forbid);
			}
		}

		public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
		{
			wornApparel.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing apparel)
		{
			return wornApparel.Contains(apparel);
		}

		public bool WouldReplaceLockedApparel(Apparel newApparel)
		{
			if (!AnyApparelLocked)
			{
				return false;
			}
			for (int i = 0; i < lockedApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(newApparel.def, lockedApparel[i].def, pawn.RaceProps.body))
				{
					return true;
				}
			}
			return false;
		}

		public void Notify_PawnKilled(DamageInfo? dinfo)
		{
			if (dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(pawn))
			{
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].def.useHitPoints)
					{
						int num = Mathf.RoundToInt((float)wornApparel[i].HitPoints * Rand.Range(0.15f, 0.4f));
						wornApparel[i].TakeDamage(new DamageInfo(dinfo.Value.Def, num));
					}
				}
			}
			for (int j = 0; j < wornApparel.Count; j++)
			{
				wornApparel[j].Notify_PawnKilled();
			}
		}

		public void Notify_LostBodyPart()
		{
			tmpApparel.Clear();
			for (int i = 0; i < wornApparel.Count; i++)
			{
				tmpApparel.Add(wornApparel[i]);
			}
			for (int j = 0; j < tmpApparel.Count; j++)
			{
				Apparel apparel = tmpApparel[j];
				if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
				{
					Remove(apparel);
				}
			}
		}

		private void SortWornApparelIntoDrawOrder()
		{
			wornApparel.InnerListForReading.Sort((Apparel a, Apparel b) => a.def.apparel.LastLayer.drawOrder.CompareTo(b.def.apparel.LastLayer.drawOrder));
		}

		public void HasBasicApparel(out bool hasPants, out bool hasShirt)
		{
			hasShirt = false;
			hasPants = false;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						hasShirt = true;
					}
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
					{
						hasPants = true;
					}
					if (hasShirt & hasPants)
					{
						return;
					}
				}
			}
		}

		public Apparel FirstApparelOnBodyPartGroup(BodyPartGroupDef g)
		{
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						return apparel;
					}
				}
			}
			return null;
		}

		public bool BodyPartGroupIsCovered(BodyPartGroupDef bp)
		{
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == bp)
					{
						return true;
					}
				}
			}
			return false;
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			for (int i = 0; i < wornApparel.Count; i++)
			{
				foreach (Gizmo wornGizmo in wornApparel[i].GetWornGizmos())
				{
					yield return wornGizmo;
				}
			}
		}

		private void ApparelChanged()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
				PortraitsCache.SetDirty(pawn);
			});
		}

		public void Notify_ApparelAdded(Apparel apparel)
		{
			SortWornApparelIntoDrawOrder();
			ApparelChanged();
			if (!apparel.def.equippedStatOffsets.NullOrEmpty())
			{
				pawn.health.capacities.Notify_CapacityLevelsDirty();
			}
		}

		public void Notify_ApparelRemoved(Apparel apparel)
		{
			ApparelChanged();
			if (pawn.outfits != null && pawn.outfits.forcedHandler != null)
			{
				pawn.outfits.forcedHandler.SetForced(apparel, forced: false);
			}
			if (IsLocked(apparel))
			{
				Unlock(apparel);
			}
			if (!apparel.def.equippedStatOffsets.NullOrEmpty())
			{
				pawn.health.capacities.Notify_CapacityLevelsDirty();
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return wornApparel;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
