using System;
using System.Collections.Generic;

namespace Verse.AI.Group;

public class Transition
{
	public List<LordToil> sources;

	public LordToil target;

	public List<Trigger> triggers = new List<Trigger>();

	public List<TransitionAction> preActions = new List<TransitionAction>();

	public List<TransitionAction> postActions = new List<TransitionAction>();

	public bool canMoveToSameState;

	public bool updateDutiesIfMovedToSameState = true;

	public Map Map => target.Map;

	public Transition(LordToil firstSource, LordToil target, bool canMoveToSameState = false, bool updateDutiesIfMovedToSameState = true)
	{
		this.canMoveToSameState = canMoveToSameState;
		this.updateDutiesIfMovedToSameState = updateDutiesIfMovedToSameState;
		this.target = target;
		sources = new List<LordToil>();
		AddSource(firstSource);
	}

	public void AddSource(LordToil source)
	{
		if (sources.Contains(source))
		{
			Log.Error("Double-added source to Transition: " + source);
			return;
		}
		if (!canMoveToSameState && target == source)
		{
			Log.Error("Transition !canMoveToSameState and target is source: " + source);
		}
		sources.Add(source);
	}

	public void AddSources(IEnumerable<LordToil> sources)
	{
		foreach (LordToil source in sources)
		{
			AddSource(source);
		}
	}

	public void AddSources(params LordToil[] sources)
	{
		for (int i = 0; i < sources.Length; i++)
		{
			AddSource(sources[i]);
		}
	}

	public void AddTrigger(Trigger trigger)
	{
		triggers.Add(trigger);
	}

	public void AddPreAction(TransitionAction action)
	{
		preActions.Add(action);
	}

	public void AddPostAction(TransitionAction action)
	{
		postActions.Add(action);
	}

	public void SourceToilBecameActive(Transition transition, LordToil previousToil)
	{
		for (int i = 0; i < triggers.Count; i++)
		{
			triggers[i].SourceToilBecameActive(transition, previousToil);
		}
	}

	public bool CheckSignal(Lord lord, TriggerSignal signal)
	{
		for (int i = 0; i < triggers.Count; i++)
		{
			if (!triggers[i].ActivateOn(lord, signal))
			{
				continue;
			}
			if (triggers[i].filters != null)
			{
				bool flag = true;
				for (int j = 0; j < triggers[i].filters.Count; j++)
				{
					if (!triggers[i].filters[j].AllowActivation(lord, signal))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if (DebugViewSettings.logLordToilTransitions)
			{
				string[] obj = new string[8]
				{
					"Transitioning ",
					sources?.ToString(),
					" to ",
					target?.ToString(),
					" by trigger ",
					triggers[i]?.ToString(),
					" on signal ",
					null
				};
				TriggerSignal triggerSignal = signal;
				obj[7] = triggerSignal.ToString();
				Log.Message(string.Concat(obj));
			}
			Execute(lord);
			return true;
		}
		return false;
	}

	public void Execute(Lord lord)
	{
		if (!canMoveToSameState && target == lord.CurLordToil)
		{
			return;
		}
		for (int i = 0; i < preActions.Count; i++)
		{
			try
			{
				preActions[i].DoAction(this);
			}
			catch (Exception ex)
			{
				Log.Error("Error in lord's preAction: " + ex);
			}
		}
		if (target != lord.CurLordToil || updateDutiesIfMovedToSameState)
		{
			lord.GotoToil(target);
		}
		for (int j = 0; j < postActions.Count; j++)
		{
			try
			{
				postActions[j].DoAction(this);
			}
			catch (Exception ex2)
			{
				Log.Error("Error in lord's postAction: " + ex2);
			}
		}
	}

	public override string ToString()
	{
		string text = (sources.NullOrEmpty() ? "null" : sources[0].ToString());
		int num = ((sources != null) ? sources.Count : 0);
		string text2 = ((target == null) ? "null" : target.ToString());
		return text + "(" + num + ")->" + text2;
	}
}
