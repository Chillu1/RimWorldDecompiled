using System.Collections.Generic;

namespace Verse
{
	public class LogMessageQueue
	{
		public int maxMessages = 200;

		private Queue<LogMessage> messages = new Queue<LogMessage>();

		private LogMessage lastMessage;

		public IEnumerable<LogMessage> Messages => messages;

		public void Enqueue(LogMessage msg)
		{
			if (lastMessage != null && msg.CanCombineWith(lastMessage))
			{
				lastMessage.repeats++;
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
}
