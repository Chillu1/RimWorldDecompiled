using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class QuestPartActivable : QuestPart
	{
		public string inSignalEnable;

		public string inSignalDisable;

		public bool reactivatable;

		public List<string> outSignalsCompleted = new List<string>();

		public QuestEndOutcome outcomeCompletedSignalArg;

		private QuestPartState state;

		protected int enableTick = -1;

		private Alert cachedAlert;

		public QuestPartState State => state;

		public int EnableTick
		{
			get
			{
				if (State != QuestPartState.Enabled)
				{
					return -1;
				}
				return enableTick;
			}
		}

		public string OutSignalEnabled => "Quest" + quest.id + ".Part" + base.Index + ".Enabled";

		public string OutSignalCompleted => "Quest" + quest.id + ".Part" + base.Index + ".Completed";

		public virtual string ExpiryInfoPart
		{
			get;
		}

		public virtual string ExpiryInfoPartTip
		{
			get;
		}

		public virtual AlertReport AlertReport => AlertReport.Inactive;

		public virtual string AlertLabel => null;

		public virtual string AlertExplanation => null;

		public virtual bool AlertCritical => false;

		public bool AlertDirty
		{
			get
			{
				if (cachedAlert == null)
				{
					return false;
				}
				if (!AlertCritical || cachedAlert is Alert_CustomCritical)
				{
					if (!AlertCritical)
					{
						return !(cachedAlert is Alert_Custom);
					}
					return false;
				}
				return true;
			}
		}

		public Alert CachedAlert
		{
			get
			{
				AlertReport alertReport = AlertReport;
				if (cachedAlert == null)
				{
					if (!alertReport.active)
					{
						return null;
					}
					if (AlertCritical)
					{
						cachedAlert = new Alert_CustomCritical();
					}
					else
					{
						cachedAlert = new Alert_Custom();
					}
				}
				Alert_Custom alert_Custom = cachedAlert as Alert_Custom;
				if (alert_Custom != null)
				{
					if (alertReport.active)
					{
						alert_Custom.label = AlertLabel;
						alert_Custom.explanation = AlertExplanation;
					}
					alert_Custom.report = alertReport;
				}
				Alert_CustomCritical alert_CustomCritical = cachedAlert as Alert_CustomCritical;
				if (alert_CustomCritical != null)
				{
					if (alertReport.active)
					{
						alert_CustomCritical.label = AlertLabel;
						alert_CustomCritical.explanation = AlertExplanation;
					}
					alert_CustomCritical.report = alertReport;
				}
				return cachedAlert;
			}
		}

		public virtual void QuestPartTick()
		{
		}

		public virtual string ExtraInspectString(ISelectable target)
		{
			return null;
		}

		public void ClearCachedAlert()
		{
			cachedAlert = null;
		}

		protected virtual void Enable(SignalArgs receivedArgs)
		{
			if (state == QuestPartState.Enabled)
			{
				Log.Error("Tried to enable QuestPart while already enabled. part=" + this);
				return;
			}
			state = QuestPartState.Enabled;
			enableTick = Find.TickManager.TicksGame;
			Find.SignalManager.SendSignal(new Signal(OutSignalEnabled));
		}

		protected void Complete()
		{
			Complete(default(SignalArgs));
		}

		protected void Complete(NamedArgument signalArg1)
		{
			Complete(new SignalArgs(signalArg1));
		}

		protected void Complete(NamedArgument signalArg1, NamedArgument signalArg2)
		{
			Complete(new SignalArgs(signalArg1, signalArg2));
		}

		protected void Complete(NamedArgument signalArg1, NamedArgument signalArg2, NamedArgument signalArg3)
		{
			Complete(new SignalArgs(signalArg1, signalArg2, signalArg3));
		}

		protected void Complete(NamedArgument signalArg1, NamedArgument signalArg2, NamedArgument signalArg3, NamedArgument signalArg4)
		{
			Complete(new SignalArgs(signalArg1, signalArg2, signalArg3, signalArg4));
		}

		protected void Complete(params NamedArgument[] signalArgs)
		{
			Complete(new SignalArgs(signalArgs));
		}

		protected virtual void Complete(SignalArgs signalArgs)
		{
			if (state != QuestPartState.Enabled)
			{
				Log.Error("Tried to end QuestPart but its state is not Active. part=" + this);
				return;
			}
			state = QuestPartState.Disabled;
			if (outcomeCompletedSignalArg != 0)
			{
				signalArgs.Add(outcomeCompletedSignalArg.Named("OUTCOME"));
			}
			Find.SignalManager.SendSignal(new Signal(OutSignalCompleted, signalArgs));
			if (outSignalsCompleted.NullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < outSignalsCompleted.Count; i++)
			{
				if (!outSignalsCompleted[i].NullOrEmpty())
				{
					Find.SignalManager.SendSignal(new Signal(outSignalsCompleted[i], signalArgs));
				}
			}
		}

		protected virtual void Disable()
		{
			if (state != QuestPartState.Enabled)
			{
				Log.Error("Tried to disable QuestPart but its state is not enabled. part=" + this);
			}
			else
			{
				state = QuestPartState.Disabled;
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (signal.tag == inSignalEnable && (state == QuestPartState.NeverEnabled || (state == QuestPartState.Disabled && reactivatable)))
			{
				Enable(signal.args);
			}
			else if (state == QuestPartState.Enabled)
			{
				if (signal.tag == inSignalDisable)
				{
					Disable();
				}
				else
				{
					ProcessQuestSignal(signal);
				}
			}
		}

		protected virtual void ProcessQuestSignal(Signal signal)
		{
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignalEnable = quest.InitiateSignal;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignalEnable, "inSignalEnable");
			Scribe_Values.Look(ref inSignalDisable, "inSignalDisable");
			Scribe_Values.Look(ref reactivatable, "reactivatable", defaultValue: false);
			if (Scribe.mode != LoadSaveMode.Saving || !outSignalsCompleted.NullOrEmpty())
			{
				Scribe_Collections.Look(ref outSignalsCompleted, "outSignalsCompleted", LookMode.Value);
			}
			Scribe_Values.Look(ref outcomeCompletedSignalArg, "outcomeCompletedSignalArg", QuestEndOutcome.Unknown);
			Scribe_Values.Look(ref state, "state", QuestPartState.NeverEnabled);
			Scribe_Values.Look(ref enableTick, "enableTick", -1);
		}
	}
}
