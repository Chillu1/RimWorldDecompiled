using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class SignalManager
{
	private int signalsThisFrame;

	private const int MaxSignalsPerFrame = 3000;

	public List<ISignalReceiver> receivers = new List<ISignalReceiver>();

	private static List<ISignalReceiver> tmpReceivers = new List<ISignalReceiver>();

	public void RegisterReceiver(ISignalReceiver receiver)
	{
		if (receiver == null)
		{
			Log.Error("Tried to register a null receiver.");
		}
		else if (receivers.Contains(receiver))
		{
			Log.Error("Tried to register the same receiver twice: " + receiver.ToStringSafe());
		}
		else
		{
			receivers.Add(receiver);
		}
	}

	public void DeregisterReceiver(ISignalReceiver receiver)
	{
		receivers.Remove(receiver);
	}

	public void SendSignal(Signal signal)
	{
		if (signalsThisFrame >= 3000)
		{
			if (signalsThisFrame == 3000)
			{
				Log.Error("Reached max signals per frame (" + 3000 + "). Ignoring further signals.");
			}
			signalsThisFrame++;
			return;
		}
		signalsThisFrame++;
		if (DebugViewSettings.logSignals)
		{
			Log.Message("Signal: tag=" + signal.tag.ToStringSafe() + " args=" + signal.args.Args.ToStringSafeEnumerable());
		}
		tmpReceivers.Clear();
		tmpReceivers.AddRange(receivers);
		for (int i = 0; i < tmpReceivers.Count; i++)
		{
			try
			{
				tmpReceivers[i].Notify_SignalReceived(signal);
			}
			catch (Exception ex)
			{
				Log.Error("Error while sending signal to " + tmpReceivers[i].ToStringSafe() + ": " + ex);
			}
		}
	}

	public void SignalManagerUpdate()
	{
		signalsThisFrame = 0;
	}
}
