using RimWorld.Planet;
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

		private float currentPsyfocus = -1f;

		private float targetPsyfocus = 0.5f;

		private int lastMeditationTick = -1;

		private Gizmo gizmo;

		private Hediff_Psylink psylinkCached;

		private int psylinkCachedForTick = -1;

		private static readonly int[] TicksBetweenMotes = new int[5]
		{
			300,
			200,
			100,
			75,
			50
		};

		public const float PercentageAfterGainingPsylink = 0.75f;

		public const int PsyfocusUpdateInterval = 150;

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

		public static readonly List<float> PsyfocusBandPercentages = new List<float>
		{
			0f,
			0.25f,
			0.5f,
			1f
		};

		public static readonly List<float> FallRatePerPsyfocusBand = new List<float>
		{
			0.035f,
			0.055f,
			0.075f
		};

		public static readonly List<int> MaxAbilityLevelPerPsyfocusBand = new List<int>
		{
			2,
			4,
			6
		};

		public static Dictionary<PsychicEntropySeverity, SoundDef> EntropyThresholdSounds;

		public static string psyfocusLevelInfoCached = null;

		public Pawn Pawn => pawn;

		public float MaxEntropy => pawn.GetStatValue(StatDefOf.PsychicEntropyMax);

		public float MaxPotentialEntropy => Mathf.Max(pawn.GetStatValue(StatDefOf.PsychicEntropyMax), MaxEntropy);

		public float PainMultiplier => 1f + pawn.health.hediffSet.PainTotal * 3f;

		public float RecoveryRate => pawn.GetStatValue(StatDefOf.PsychicEntropyRecoveryRate) * PainMultiplier;

		public float EntropyValue => currentEntropy;

		public float CurrentPsyfocus => currentPsyfocus;

		public float TargetPsyfocus => targetPsyfocus;

		public int MaxAbilityLevel => MaxAbilityLevelPerPsyfocusBand[PsyfocusBand];

		public bool IsCurrentlyMeditating => Find.TickManager.TicksGame < lastMeditationTick + 10;

		public float EntropyRelativeValue
		{
			get
			{
				if (currentEntropy < float.Epsilon)
				{
					return 0f;
				}
				if (currentEntropy < MaxEntropy)
				{
					if (!(MaxEntropy > float.Epsilon))
					{
						return 0f;
					}
					return currentEntropy / MaxEntropy;
				}
				if (!(MaxPotentialEntropy > float.Epsilon))
				{
					return 0f;
				}
				return 1f + (currentEntropy - MaxEntropy) / MaxPotentialEntropy;
			}
		}

		public PsychicEntropySeverity Severity
		{
			get
			{
				PsychicEntropySeverity result = PsychicEntropySeverity.Safe;
				foreach (PsychicEntropySeverity key in EntropyThresholds.Keys)
				{
					if (EntropyThresholds[key] < EntropyRelativeValue)
					{
						result = key;
						continue;
					}
					return result;
				}
				return result;
			}
		}

		public int PsyfocusBand
		{
			get
			{
				if (currentPsyfocus < PsyfocusBandPercentages[1])
				{
					return 0;
				}
				if (currentPsyfocus < PsyfocusBandPercentages[2])
				{
					return 1;
				}
				return 2;
			}
		}

		public Hediff_Psylink Psylink
		{
			get
			{
				if (psylinkCachedForTick != Find.TickManager.TicksGame)
				{
					psylinkCached = pawn.GetMainPsylinkSource();
					psylinkCachedForTick = Find.TickManager.TicksGame;
				}
				return psylinkCached;
			}
		}

		public bool NeedsPsyfocus
		{
			get
			{
				if (Psylink == null)
				{
					return false;
				}
				if (pawn.Suspended)
				{
					return false;
				}
				if (!pawn.Spawned && !pawn.IsCaravanMember())
				{
					return false;
				}
				return true;
			}
		}

		private float PsyfocusFallPerDay
		{
			get
			{
				if (pawn.GetPsylinkLevel() == 0)
				{
					return 0f;
				}
				return FallRatePerPsyfocusBand[PsyfocusBand];
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
				currentEntropy = Mathf.Max(currentEntropy - 1.TicksToSeconds() * RecoveryRate, 0f);
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
			if (NeedsPsyfocus && pawn.IsHashIntervalTick(150))
			{
				float num = 400f;
				if (!IsCurrentlyMeditating)
				{
					currentPsyfocus = Mathf.Clamp(currentPsyfocus - PsyfocusFallPerDay / num, 0f, 1f);
				}
				MeditationUtility.CheckMeditationScheduleTeachOpportunity(pawn);
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
			if (!WouldOverflowEntropy(num) || overLimit)
			{
				currentEntropy = Mathf.Max(currentEntropy + num, 0f);
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					hediff.Notify_EntropyGained(value, num, source);
				}
				if (severity != Severity && num > 0f && Severity != 0)
				{
					EntropyThresholdSounds[Severity].PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
					if (severity < PsychicEntropySeverity.Overloaded && Severity >= PsychicEntropySeverity.Overloaded)
					{
						Messages.Message("MessageWentOverPsychicEntropyLimit".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.NegativeEvent);
					}
				}
				return true;
			}
			return false;
		}

		public void RemoveAllEntropy()
		{
			currentEntropy = 0f;
		}

		[Obsolete("Only used for mod compatibility")]
		private void GiveHangoverIfNeeded()
		{
		}

		[Obsolete("Only used for mod compatibility")]
		private void GiveHangoverIfNeeded_NewTemp(float entropyChange)
		{
		}

		public void GainPsyfocus(Thing focus = null)
		{
			currentPsyfocus = Mathf.Clamp(currentPsyfocus + MeditationUtility.PsyfocusGainPerTick(pawn, focus), 0f, 1f);
			if (focus != null && !focus.Destroyed)
			{
				focus.TryGetComp<CompMeditationFocus>()?.Used(pawn);
			}
		}

		public void Notify_Meditated()
		{
			lastMeditationTick = Find.TickManager.TicksGame;
		}

		public void OffsetPsyfocusDirectly(float offset)
		{
			currentPsyfocus = Mathf.Clamp(currentPsyfocus + offset, 0f, 1f);
		}

		public void SetInitialPsyfocusLevel()
		{
			if (pawn.IsColonist && !pawn.IsQuestLodger())
			{
				currentPsyfocus = 0.75f;
			}
			else
			{
				currentPsyfocus = Rand.Range(0.5f, 0.7f);
			}
		}

		public void SetPsyfocusTarget(float val)
		{
			targetPsyfocus = Mathf.Clamp(val, 0f, 1f);
		}

		public void Notify_GainedPsylink()
		{
			currentPsyfocus = Mathf.Max(currentPsyfocus, 0.75f);
		}

		public void Notify_PawnDied()
		{
			currentEntropy = 0f;
			currentPsyfocus = 0f;
		}

		public bool NeedToShowGizmo()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
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

		public string PsyfocusTipString()
		{
			if (psyfocusLevelInfoCached == null)
			{
				for (int i = 0; i < PsyfocusBandPercentages.Count - 1; i++)
				{
					psyfocusLevelInfoCached += "PsyfocusLevelInfoRange".Translate((PsyfocusBandPercentages[i] * 100f).ToStringDecimalIfSmall(), (PsyfocusBandPercentages[i + 1] * 100f).ToStringDecimalIfSmall()) + ": " + "PsyfocusLevelInfoPsycasts".Translate(MaxAbilityLevelPerPsyfocusBand[i]) + "\n";
				}
				psyfocusLevelInfoCached += "\n";
				for (int j = 0; j < PsyfocusBandPercentages.Count - 1; j++)
				{
					psyfocusLevelInfoCached += "PsyfocusLevelInfoRange".Translate((PsyfocusBandPercentages[j] * 100f).ToStringDecimalIfSmall(), (PsyfocusBandPercentages[j + 1] * 100f).ToStringDecimalIfSmall()) + ": " + "PsyfocusLevelInfoFallRate".Translate(FallRatePerPsyfocusBand[j].ToStringPercent()) + "\n";
				}
			}
			return "Psyfocus".Translate() + ": " + currentPsyfocus.ToStringPercent("0.#") + "\n" + "DesiredPsyfocus".Translate() + ": " + targetPsyfocus.ToStringPercent("0.#") + "\n\n" + "DesiredPsyfocusDesc".Translate(pawn.Named("PAWN")) + "\n\n" + "PsyfocusDesc".Translate() + ":\n\n" + psyfocusLevelInfoCached;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref currentEntropy, "currentEntropy", 0f);
			Scribe_Values.Look(ref currentPsyfocus, "currentPsyfocus", -1f);
			Scribe_Values.Look(ref targetPsyfocus, "targetPsyfocus", 0.5f);
			Scribe_Values.Look(ref lastMeditationTick, "lastMeditationTick", -1);
			Scribe_Values.Look(ref limitEntropyAmount, "limitEntropyAmount", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && currentPsyfocus < 0f)
			{
				SetInitialPsyfocusLevel();
			}
		}
	}
}
