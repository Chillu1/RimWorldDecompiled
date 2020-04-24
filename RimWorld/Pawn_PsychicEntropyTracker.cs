using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Pawn_PsychicEntropyTracker : IExposable
	{
		private Pawn pawn;

		private float currentEntropy;

		private int ticksSinceLastMote;

		public bool limitEntropyAmount = true;

		private Gizmo gizmo;

		public const float RecoveryRateSeconds = 30f;

		private static readonly int[] TicksBetweenMotes = new int[5]
		{
			300,
			200,
			100,
			75,
			50
		};

		public static readonly Dictionary<PsychicEntropySeverity, float> EntropyThresholds = new Dictionary<PsychicEntropySeverity, float>
		{
			{
				PsychicEntropySeverity.Safe,
				0f
			},
			{
				PsychicEntropySeverity.Overloaded,
				1f
			},
			{
				PsychicEntropySeverity.Hyperloaded,
				1.33f
			},
			{
				PsychicEntropySeverity.BrainCharring,
				1.66f
			},
			{
				PsychicEntropySeverity.BrainRoasting,
				2f
			}
		};

		public static Dictionary<PsychicEntropySeverity, SoundDef> EntropyThresholdSounds;

		public Pawn Pawn => pawn;

		public float MaxEntropy => pawn.GetStatValue(StatDefOf.PsychicEntropyMax);

		public float PainMultiplier => 1f + pawn.health.hediffSet.PainTotal * 1.5f;

		public float RecoveryRate => pawn.GetStatValue(StatDefOf.PsychicEntropyRecoveryRate) * PainMultiplier;

		public float RecoveryRatePerSecond => RecoveryRate / 30f;

		public float EntropyValue => currentEntropy;

		public float EntropyRelativeValue
		{
			get
			{
				if (!(MaxEntropy > float.Epsilon))
				{
					return 0f;
				}
				return currentEntropy / MaxEntropy;
			}
		}

		public PsychicEntropySeverity Severity
		{
			get
			{
				PsychicEntropySeverity result = PsychicEntropySeverity.Safe;
				foreach (PsychicEntropySeverity key in EntropyThresholds.Keys)
				{
					if (!(EntropyThresholds[key] < EntropyRelativeValue))
					{
						return result;
					}
					result = key;
				}
				return result;
			}
		}

		public Pawn_PsychicEntropyTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public static void ResetStaticData()
		{
			EntropyThresholdSounds = new Dictionary<PsychicEntropySeverity, SoundDef>
			{
				{
					PsychicEntropySeverity.Overloaded,
					SoundDefOf.PsychicEntropyOverloaded
				},
				{
					PsychicEntropySeverity.Hyperloaded,
					SoundDefOf.PsychicEntropyHyperloaded
				},
				{
					PsychicEntropySeverity.BrainCharring,
					SoundDefOf.PsychicEntropyBrainCharring
				},
				{
					PsychicEntropySeverity.BrainRoasting,
					SoundDefOf.PsychicEntropyBrainRoasting
				}
			};
		}

		public void PsychicEntropyTrackerTick()
		{
			if (currentEntropy > float.Epsilon)
			{
				float num = currentEntropy - 1.TicksToSeconds() * RecoveryRatePerSecond;
				currentEntropy = Mathf.Max(num, 0f);
				GiveHangoverIfNeeded_NewTemp(num);
			}
			if (currentEntropy > float.Epsilon && !pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicEntropy))
			{
				pawn.health.AddHediff(HediffDefOf.PsychicEntropy);
			}
			if (currentEntropy > float.Epsilon)
			{
				if (ticksSinceLastMote >= TicksBetweenMotes[(int)Severity])
				{
					if (pawn.Spawned)
					{
						MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_EntropyPulse, Vector3.zero);
					}
					ticksSinceLastMote = 0;
				}
				else
				{
					ticksSinceLastMote++;
				}
			}
			else
			{
				ticksSinceLastMote = 0;
			}
		}

		public bool WouldOverflowEntropy(float value)
		{
			if (!limitEntropyAmount)
			{
				return false;
			}
			return currentEntropy + value * pawn.GetStatValue(StatDefOf.PsychicEntropyGain) > MaxEntropy;
		}

		public bool TryAddEntropy(float value, Thing source = null, bool scale = true, bool overLimit = false)
		{
			PsychicEntropySeverity severity = Severity;
			float num = scale ? (value * pawn.GetStatValue(StatDefOf.PsychicEntropyGain)) : value;
			if (!WouldOverflowEntropy(num) | overLimit)
			{
				currentEntropy = Mathf.Max(currentEntropy + num, 0f);
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					hediff.Notify_EntropyGained(value, num, source);
				}
				if (severity != Severity && num > 0f && Severity != 0)
				{
					EntropyThresholdSounds[Severity].PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
					if (Severity == PsychicEntropySeverity.Overloaded)
					{
						Messages.Message("MessageWentOverPsychicEntropyLimit".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
					}
				}
				return true;
			}
			return false;
		}

		[Obsolete]
		private void GiveHangoverIfNeeded()
		{
		}

		private void GiveHangoverIfNeeded_NewTemp(float entropyChange)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicHangover);
			if (firstHediffOfDef == null)
			{
				if (entropyChange <= 0f)
				{
					pawn.health.AddHediff(HediffDefOf.PsychicHangover);
				}
			}
			else
			{
				firstHediffOfDef.Severity = 0.01f;
			}
		}

		public bool NeedToShowGizmo()
		{
			if (pawn.GetStatValue(StatDefOf.PsychicSensitivity) > float.Epsilon)
			{
				if (!(EntropyValue > float.Epsilon))
				{
					return pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicAmplifier);
				}
				return true;
			}
			return false;
		}

		public Gizmo GetGizmo()
		{
			if (gizmo == null)
			{
				gizmo = new PsychicEntropyGizmo(this);
			}
			return gizmo;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref currentEntropy, "currentEntropy", 0f);
			Scribe_Values.Look(ref limitEntropyAmount, "limitEntropyAmount", defaultValue: false);
		}
	}
}
