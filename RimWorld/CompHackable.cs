using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompHackable : ThingComp, IThingGlower
	{
		public string hackingStartedSignal;

		public string hackingCompletedSignal;

		private float progress;

		public float defence;

		private float lastUserSpeed = 1f;

		private int lastHackTick = -1;

		private Pawn lastUser;

		private float progressLastLockout;

		private int lockedOutUntilTick = -1;

		private bool lockedOutPermanently;

		private bool sentLetter;

		private bool autohack;

		private bool hacked;

		public const string HackedSignal = "Hacked";

		private const float MinHackProgressBeforeLockout = 3000f;

		private static readonly Texture2D HackTexture = ContentFinder<Texture2D>.Get("UI/Commands/Hack");

		private static readonly Texture2D AutohackTexture = ContentFinder<Texture2D>.Get("UI/Commands/Autohack");

		private static readonly List<Thing> tmpSpawnedThings = new List<Thing>();

		private static readonly List<Pawn> tmpAllowedPawns = new List<Pawn>();

		public CompProperties_Hackable Props => (CompProperties_Hackable)props;

		public float ProgressPercent => progress / defence;

		public bool Autohack => autohack;

		public bool IsHacked => hacked;

		public bool LockedOut
		{
			get
			{
				if (!lockedOutPermanently)
				{
					return lockedOutUntilTick > Find.TickManager.TicksGame;
				}
				return true;
			}
		}

		public bool ShouldBeLitNow()
		{
			if (IsHacked)
			{
				return Props.glowIfHacked;
			}
			return true;
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
			{
				defence = Props.defence;
			}
			base.PostSpawnSetup(respawningAfterLoad);
		}

		public void HackNow()
		{
			progress = defence + 1f;
			ProcessHacked(null, suppressMessages: true);
		}

		public void Hack(float amount, Pawn hacker = null, bool suppressMessages = false)
		{
			bool isHacked = IsHacked;
			progress += amount;
			progress = Mathf.Clamp(progress, 0f, defence);
			if (!isHacked && progress >= defence)
			{
				ProcessHacked(hacker, suppressMessages);
			}
			if (lastHackTick < 0)
			{
				if (!hackingStartedSignal.NullOrEmpty())
				{
					Find.SignalManager.SendSignal(new Signal(hackingStartedSignal, parent.Named("SUBJECT")));
				}
				QuestUtility.SendQuestTargetSignals(parent.questTags, "HackingStarted", parent.Named("SUBJECT"));
			}
			lastUserSpeed = amount;
			lastHackTick = Find.TickManager.TicksGame;
			lastUser = hacker;
			if (hacker != null && (Props.lockoutDurationHoursRange.TrueMax > 0 || Props.lockoutPermanently) && !IsHacked && progress - progressLastLockout > 3000f && Rand.MTBEventOccurs(hacker.GetStatValue(StatDefOf.HackingStealth), amount * 60f, 1f))
			{
				LockOut(hacker);
			}
		}

		private void ProcessHacked(Pawn hacker, bool suppressMessages)
		{
			if (hacked)
			{
				return;
			}
			hacked = true;
			if (!hackingCompletedSignal.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(hackingCompletedSignal, parent.Named("SUBJECT")));
			}
			Pawn arg = hacker ?? ((Current.ProgramState == ProgramState.Playing && parent.Map.mapPawns.ColonistCount > 0) ? parent.Map.mapPawns.FreeColonists.RandomElement() : null);
			QuestUtility.SendQuestTargetSignals(parent.questTags, "Hacked", parent.Named("SUBJECT"), arg.Named("ACTIVATOR"));
			parent.BroadcastCompSignal("Hacked");
			if (Props.completedQuest != null)
			{
				Slate slate = new Slate();
				slate.Set("map", parent.Map);
				if (Props.completedQuest.CanRun(slate, parent.Map))
				{
					QuestUtility.GenerateQuestAndMakeAvailable(Props.completedQuest, slate);
				}
			}
			OnHacked(hacker, suppressMessages);
		}

		public override IEnumerable<ThingDefCountClass> GetAdditionalLeavings(Map map, DestroyMode mode)
		{
			if (IsHacked)
			{
				yield break;
			}
			foreach (ThingDefCountClass item in Props.dropOnDestroyed)
			{
				yield return item;
			}
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			if (!sentLetter && !Props.destroyedLetterLabel.NullOrEmpty() && !Props.destroyedLetterText.NullOrEmpty())
			{
				sentLetter = true;
				Find.LetterStack.ReceiveLetter(Props.destroyedLetterLabel, Props.destroyedLetterText, Props.destroyedLetterDef);
			}
		}

		public override bool CompPreventClaimingBy(Faction faction)
		{
			if (faction.IsPlayer)
			{
				return !IsHacked;
			}
			return false;
		}

		protected virtual void OnHacked(Pawn hacker = null, bool suppressMessages = false)
		{
			if (parent is IHackable hackable)
			{
				hackable.OnHacked(hacker);
			}
			foreach (ThingComp allComp in parent.AllComps)
			{
				allComp.Notify_Hacked(hacker);
			}
			GenLeaving.DropThingsNear(parent, Props.dropOnHacked, tmpSpawnedThings);
			if (!Props.hackedMessage.NullOrEmpty() && hacker != null && !suppressMessages)
			{
				Messages.Message(Props.hackedMessage.Formatted(hacker.Named("HACKER"), parent.Named("SUBJECT")), parent, MessageTypeDefOf.PositiveEvent, historical: false);
			}
			if (!sentLetter && !Props.hackedLetterLabel.NullOrEmpty() && !Props.hackedLetterText.NullOrEmpty() && !suppressMessages)
			{
				sentLetter = true;
				Find.LetterStack.ReceiveLetter(Props.hackedLetterLabel, Props.hackedLetterText, Props.hackedLetterDef, tmpSpawnedThings);
			}
			tmpSpawnedThings.Clear();
			if (parent.Spawned)
			{
				parent.DirtyMapMesh(parent.Map);
			}
		}

		public override void PostDraw()
		{
			if (Props.graphicDrawerType != DrawerType.MapMeshOnly)
			{
				if (IsHacked && Props.hackedGraphicData != null)
				{
					Props.hackedGraphicData.Graphic.Draw(parent.DrawPos + Props.hackedGraphicOffset, parent.Rotation, parent);
				}
				else if (!IsHacked && Props.unhackedGraphicData != null)
				{
					Props.unhackedGraphicData.Graphic.Draw(parent.DrawPos + Props.unhackedGraphicOffset, parent.Rotation, parent);
				}
			}
		}

		public override void PostPrintOnto(SectionLayer layer)
		{
			if (Props.graphicDrawerType != DrawerType.RealtimeOnly)
			{
				if (IsHacked && Props.hackedGraphicData != null)
				{
					Props.hackedGraphicData.Graphic.Print(layer, parent, 0f);
				}
				else if (!IsHacked && Props.unhackedGraphicData != null)
				{
					Props.unhackedGraphicData.Graphic.Print(layer, parent, 0f);
				}
			}
		}

		public override bool DontDrawParent()
		{
			if (IsHacked && Props.hackedGraphicData != null)
			{
				return true;
			}
			if (!IsHacked && Props.unhackedGraphicData != null)
			{
				return true;
			}
			return false;
		}

		public override void CompTick()
		{
			if (lockedOutUntilTick >= 0 && lockedOutUntilTick <= Find.TickManager.TicksGame && !Props.lockoutPermanently)
			{
				EndLockout();
			}
		}

		public void LockOut(Pawn hacker)
		{
			if (Props.lockoutPermanently)
			{
				lockedOutPermanently = true;
				Messages.Message("MessageHackerLockedOutPermanently".Translate(hacker.Named("HACKER")), parent, MessageTypeDefOf.NegativeEvent);
			}
			else
			{
				int num = Props.lockoutDurationHoursRange.RandomInRange * 2500;
				progressLastLockout = progress;
				lockedOutUntilTick = Find.TickManager.TicksGame + num;
				Messages.Message("MessageHackerLockedOut".Translate(hacker.Named("HACKER"), num.ToStringTicksToPeriod().Named("DURATION")), parent, MessageTypeDefOf.NegativeEvent);
			}
			OnLockedOut(hacker);
		}

		protected virtual void OnLockedOut(Pawn hacker = null)
		{
			Pawn arg = hacker ?? ((parent.Map.mapPawns.ColonistCount > 0) ? parent.Map.mapPawns.FreeColonists.RandomElement() : null);
			QuestUtility.SendQuestTargetSignals(parent.questTags, "LockedOut", parent.Named("SUBJECT"), arg.Named("ACTIVATOR"));
			if (parent is IHackable hackable)
			{
				hackable.OnLockedOut(hacker);
			}
		}

		private void EndLockout()
		{
			lockedOutUntilTick = -1;
			lockedOutPermanently = false;
			Messages.Message("MessageHackableUnlocked".Translate(parent), parent, MessageTypeDefOf.PositiveEvent);
		}

		private AcceptanceReport CanHackNow(bool reportAlreadyHacked)
		{
			if (IsHacked && reportAlreadyHacked)
			{
				return "AlreadyHacked".Translate();
			}
			if (IsHacked)
			{
				return false;
			}
			if (LockedOut)
			{
				return "LockedOut".Translate();
			}
			return true;
		}

		public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
		{
			if (Props.onlyRemotelyHackable)
			{
				yield break;
			}
			AcceptanceReport acceptanceReport = CanHackNow(reportAlreadyHacked: false);
			if (!acceptanceReport.Accepted)
			{
				if (!IsHacked)
				{
					yield return new FloatMenuOption("CannotHack".Translate(parent.Label) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
				}
				yield break;
			}
			tmpAllowedPawns.Clear();
			foreach (Pawn selPawn in selPawns)
			{
				if (selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawn);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield return new FloatMenuOption("CannotHack".Translate(parent.Label) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
				yield break;
			}
			tmpAllowedPawns.Clear();
			foreach (Pawn selPawn2 in selPawns)
			{
				if (HackUtility.IsCapableOfHacking(selPawn2))
				{
					tmpAllowedPawns.Add(selPawn2);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield return new FloatMenuOption("CannotHack".Translate(parent.Label) + ": " + "IncapableOfHacking".Translate(), null);
				yield break;
			}
			tmpAllowedPawns.Clear();
			if (Props.intellectualSkillPrerequisite > 0)
			{
				foreach (Pawn selPawn3 in selPawns)
				{
					if (selPawn3.skills?.GetSkill(SkillDefOf.Intellectual).Level >= Props.intellectualSkillPrerequisite)
					{
						tmpAllowedPawns.Add(selPawn3);
					}
				}
				if (tmpAllowedPawns.Count <= 0)
				{
					yield return new FloatMenuOption("CannotHack".Translate(parent.Label) + ": " + "SkillTooLow".Translate(SkillDefOf.Intellectual.label, selPawns.First().skills.GetSkill(SkillDefOf.Intellectual).Level, Props.intellectualSkillPrerequisite), null);
					yield break;
				}
				tmpAllowedPawns.Clear();
			}
			foreach (Pawn selPawn4 in selPawns)
			{
				if (HackUtility.IsCapableOfHacking(selPawn4) && selPawn4.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					tmpAllowedPawns.Add(selPawn4);
				}
			}
			if (tmpAllowedPawns.Count <= 0)
			{
				yield break;
			}
			yield return new FloatMenuOption("Hack".Translate(parent.Label), delegate
			{
				tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Hack, parent), JobTag.Misc);
				for (int i = 1; i < tmpAllowedPawns.Count; i++)
				{
					FloatMenuOptionProvider_DraftedMove.PawnGotoAction(parent.Position, tmpAllowedPawns[i], RCellFinder.BestOrderedGotoDestNear(parent.Position, tmpAllowedPawns[i]));
				}
			});
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref progress, "progress", 0f);
			Scribe_Values.Look(ref lastUserSpeed, "lastUserSpeed", 0f);
			Scribe_Values.Look(ref lastHackTick, "lastHackTick", 0);
			Scribe_Values.Look(ref defence, "defence", 0f);
			Scribe_References.Look(ref lastUser, "lasterUser");
			Scribe_Values.Look(ref hackingStartedSignal, "hackingStartedSignal");
			Scribe_Values.Look(ref hackingCompletedSignal, "hackingCompletedSignal");
			Scribe_Values.Look(ref lockedOutUntilTick, "lockedOutUntilTick", -1);
			Scribe_Values.Look(ref progressLastLockout, "progressLastLockout", 0f);
			Scribe_Values.Look(ref lockedOutPermanently, "lockedOutPermanently", defaultValue: false);
			Scribe_Values.Look(ref sentLetter, "sentLetter", defaultValue: false);
			Scribe_Values.Look(ref autohack, "autohack", defaultValue: false);
			Scribe_Values.Look(ref hacked, "hacked", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && parent.Spawned)
			{
				hacked = hacked || progress >= defence;
			}
		}

		public override string CompInspectStringExtra()
		{
			TaggedString empty = TaggedString.Empty;
			bool isHacked = IsHacked;
			if (!isHacked && !Props.notHackedInspectString.NullOrEmpty())
			{
				empty += Props.notHackedInspectString;
			}
			if ((!isHacked || Props.showProgressAfterHackCompletion) && !Props.onlyRemotelyHackable)
			{
				if (empty.Length > 0)
				{
					empty += "\n";
				}
				empty += "HackProgress".Translate() + ": " + progress.ToStringWorkAmount() + " / " + defence.ToStringWorkAmount();
				if (isHacked)
				{
					empty += " (" + "Hacked".Translate() + ")";
				}
			}
			if (isHacked)
			{
				if (!Props.hackedInspectString.NullOrEmpty())
				{
					if (empty.Length > 0)
					{
						empty += "\n";
					}
					empty += Props.hackedInspectString;
				}
			}
			else
			{
				if (LockedOut)
				{
					if (empty.Length > 0)
					{
						empty += "\n";
					}
					empty += ("LockedOut".Translate() + ": " + (lockedOutUntilTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()).Colorize(ColorLibrary.RedReadable);
				}
				if (lastHackTick > Find.TickManager.TicksGame - 30)
				{
					string text = ((lastUser == null) ? ((string)StatDefOf.HackingSpeed.LabelCap) : ((string)("HackingLastUser".Translate(lastUser) + " " + StatDefOf.HackingSpeed.label)));
					if (empty.Length > 0)
					{
						empty += "\n";
					}
					empty += text + ": " + StatDefOf.HackingSpeed.ValueToString(lastUserSpeed);
				}
			}
			if (!IsHacked && Props.intellectualSkillPrerequisite > 0)
			{
				if (empty.Length > 0)
				{
					empty += "\n";
				}
				empty += string.Concat("IntellectualSkillPrerequisite".Translate() + ": ", Props.intellectualSkillPrerequisite.ToString());
			}
			return empty.Resolve();
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (!Props.onlyRemotelyHackable)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "HackProgress".Translate(), progress.ToStringWorkAmount() + " / " + defence.ToStringWorkAmount(), "Stat_Thing_HackingProgress".Translate(), 3100);
			}
		}

		private AcceptanceReport ValidateHacker(LocalTargetInfo target)
		{
			if (!(target.Thing is Pawn pawn))
			{
				return false;
			}
			if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
			{
				return "NoPath".Translate();
			}
			if (!HackUtility.IsCapableOfHacking(pawn))
			{
				return "IncapableOfHacking".Translate();
			}
			if (Props.intellectualSkillPrerequisite > 0 && pawn.skills?.GetSkill(SkillDefOf.Intellectual).Level < Props.intellectualSkillPrerequisite)
			{
				return "SkillTooLow".Translate(SkillDefOf.Intellectual.label, pawn.skills.GetSkill(SkillDefOf.Intellectual).Level, Props.intellectualSkillPrerequisite);
			}
			return true;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!Props.onlyRemotelyHackable)
			{
				if (!IsHacked)
				{
					yield return new Command_Toggle
					{
						defaultLabel = "AutoHack".Translate(),
						defaultDesc = "AutoHackDesc".Translate(),
						icon = AutohackTexture,
						hotKey = KeyBindingDefOf.Misc3,
						isActive = () => autohack,
						toggleAction = delegate
						{
							autohack = !autohack;
							if (autohack && !Props.autohackWarningString.NullOrEmpty())
							{
								Messages.Message(Props.autohackWarningString.Formatted(parent), parent, MessageTypeDefOf.NegativeEvent, historical: false);
							}
						}
					};
				}
				Command_Action command_Action = new Command_Action
				{
					defaultLabel = "HackGizmo".Translate(),
					defaultDesc = "HackDesc".Translate(),
					icon = HackTexture,
					action = delegate
					{
						Find.Targeter.BeginTargeting(TargetingParameters.ForColonist(), delegate(LocalTargetInfo target)
						{
							target.Pawn?.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Hack, parent), JobTag.Misc);
						}, delegate(LocalTargetInfo target)
						{
							Pawn pawn = target.Pawn;
							if (pawn != null && pawn.IsColonistPlayerControlled)
							{
								GenDraw.DrawTargetHighlight(target);
							}
						}, (LocalTargetInfo target) => ValidateHacker(target).Accepted, null, null, HackTexture, playSoundOnAction: true, delegate(LocalTargetInfo target)
						{
							AcceptanceReport acceptanceReport2 = ValidateHacker(target);
							Pawn pawn = target.Pawn;
							if (pawn != null && pawn.IsColonistPlayerControlled && !acceptanceReport2.Accepted)
							{
								Widgets.MouseAttachedLabel(("CannotChooseHacker".Translate() + ": " + acceptanceReport2.Reason.CapitalizeFirst()).Colorize(ColorLibrary.RedReadable));
							}
							else
							{
								Widgets.MouseAttachedLabel("CommandChooseHacker".Translate());
							}
						});
					}
				};
				AcceptanceReport acceptanceReport = CanHackNow(reportAlreadyHacked: true);
				if (!acceptanceReport.Accepted)
				{
					command_Action.Disable(acceptanceReport.Reason.CapitalizeFirst());
				}
				yield return command_Action;
			}
			if (DebugSettings.ShowDevGizmos && !IsHacked)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Hack +10%",
					action = delegate
					{
						Hack(defence * 0.1f);
					}
				};
				yield return new Command_Action
				{
					defaultLabel = "DEV: Complete hack",
					action = delegate
					{
						HackNow();
					}
				};
				if (LockedOut)
				{
					yield return new Command_Action
					{
						defaultLabel = "DEV: Unlock",
						action = EndLockout
					};
				}
			}
		}

		public AcceptanceReport CanHackNow(Pawn pawn)
		{
			if (Props.onlyRemotelyHackable || IsHacked)
			{
				return false;
			}
			if (LockedOut)
			{
				return "LockedOut".Translate();
			}
			if (!HackUtility.IsCapableOfHacking(pawn))
			{
				return "IncapableOfHacking".Translate();
			}
			if (!pawn.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				return "NoPath".Translate().CapitalizeFirst();
			}
			if (Props.intellectualSkillPrerequisite > 0 && pawn.skills.GetSkill(SkillDefOf.Intellectual).Level < Props.intellectualSkillPrerequisite)
			{
				return "SkillTooLow".Translate(SkillDefOf.Intellectual.label, pawn.skills.GetSkill(SkillDefOf.Intellectual).Level, Props.intellectualSkillPrerequisite);
			}
			return true;
		}
	}
}
