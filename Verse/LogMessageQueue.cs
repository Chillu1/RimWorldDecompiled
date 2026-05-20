using System.Collections.Generic;
using LudeonTK;

namespace Verse;

public class LogMessageQueue
{
	public int maxMessages = 1000;

	private Queue<LogMessage> messages = new Queue<LogMessage>();

	private LogMessage lastMessage;

	public IEnumerable<LogMessage> Messages => messages;

	public void Enqueue(LogMessage msg)
	{
		Enqueue(msg, out var _);
	}

	public void Enqueue(LogMessage msg, out bool repeatsCapped)
	{
		repeatsCapped = false;
		if (lastMessage != null && msg.CanCombineWith(lastMessage))
		{
			if (lastMessage.repeats >= 99)
			{
				repeatsCapped = true;
			}
			else
			{
				lastMessage.repeats++;
			}
			return;
		}
		lastMessage = msg;
		messages.Enqueue(msg);
		if (messages.Count > maxMessages)
		{
			EditWindow_Log.Notify_MessageDequeued(messages.Dequeue());
		}
	}

	internal void Clear()
	{
		messages.Clear();
		lastMessage = null;
	}
}
