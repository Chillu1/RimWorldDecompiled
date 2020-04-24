using System.Xml;

namespace Verse
{
	public class PatchOperation
	{
		private enum Success
		{
			Normal,
			Invert,
			Always,
			Never
		}

		public string sourceFile;

		private bool neverSucceeded = true;

		private Success success;

		public bool Apply(XmlDocument xml)
		{
			if (DeepProfiler.enabled)
			{
				DeepProfiler.Start(GetType().FullName + " Worker");
			}
			bool flag = ApplyWorker(xml);
			if (DeepProfiler.enabled)
			{
				DeepProfiler.End();
			}
			if (success == Success.Always)
			{
				flag = true;
			}
			else if (success == Success.Never)
			{
				flag = false;
			}
			else if (success == Success.Invert)
			{
				flag = !flag;
			}
			if (flag)
			{
				neverSucceeded = false;
			}
			return flag;
		}

		protected virtual bool ApplyWorker(XmlDocument xml)
		{
			Log.Error("Attempted to use PatchOperation directly; patch will always fail");
			return false;
		}

		public virtual void Complete(string modIdentifier)
		{
			if (neverSucceeded)
			{
				string text = $"[{modIdentifier}] Patch operation {this} failed";
				if (!string.IsNullOrEmpty(sourceFile))
				{
					text = text + "\nfile: " + sourceFile;
				}
				Log.Error(text);
			}
		}
	}
}
