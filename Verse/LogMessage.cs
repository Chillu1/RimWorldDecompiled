using UnityEngine;

namespace Verse
{
	public class LogMessage
	{
		public string text;

		public LogMessageType type;

		public int repeats = 1;

		private string stackTrace;

		public Color Color => type switch
		{
			LogMessageType.Message => Color.white, 
			LogMessageType.Warning => Color.yellow, 
			LogMessageType.Error => Color.red, 
			_ => Color.white, 
		};

		public string StackTrace
		{
			get
			{
				if (stackTrace != null)
				{
					return stackTrace;
				}
				return "No stack trace.";
			}
		}

		public LogMessage(string text)
		{
			this.text = text;
			type = LogMessageType.Message;
			stackTrace = null;
		}

		public LogMessage(LogMessageType type, string text, string stackTrace)
		{
			this.text = text;
			this.type = type;
			this.stackTrace = stackTrace;
		}

		public override string ToString()
		{
			if (repeats > 1)
			{
				return "(" + repeats + ") " + text;
			}
			return text;
		}

		public bool CanCombineWith(LogMessage other)
		{
			if (text == other.text)
			{
				return type == other.type;
			}
			return false;
		}
	}
}
