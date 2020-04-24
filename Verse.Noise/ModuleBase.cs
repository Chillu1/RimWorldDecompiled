using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Verse.Noise
{
	public abstract class ModuleBase : IDisposable
	{
		protected ModuleBase[] modules;

		[NonSerialized]
		[XmlIgnore]
		private bool m_disposed;

		public int SourceModuleCount
		{
			get
			{
				if (modules != null)
				{
					return modules.Length;
				}
				return 0;
			}
		}

		public virtual ModuleBase this[int index]
		{
			get
			{
				if (index < 0 || index >= modules.Length)
				{
					throw new ArgumentOutOfRangeException("Index out of valid module range");
				}
				if (modules[index] == null)
				{
					throw new ArgumentNullException("Desired element is null");
				}
				return modules[index];
			}
			set
			{
				if (index < 0 || index >= modules.Length)
				{
					throw new ArgumentOutOfRangeException("Index out of valid module range");
				}
				if (value == null)
				{
					throw new ArgumentNullException("Value should not be null");
				}
				modules[index] = value;
			}
		}

		public bool IsDisposed => m_disposed;

		protected ModuleBase(int count)
		{
			if (count > 0)
			{
				modules = new ModuleBase[count];
			}
		}

		public abstract double GetValue(double x, double y, double z);

		public float GetValue(IntVec2 coordinate)
		{
			return (float)GetValue(coordinate.x, 0.0, coordinate.z);
		}

		public float GetValue(IntVec3 coordinate)
		{
			return (float)GetValue(coordinate.x, coordinate.y, coordinate.z);
		}

		public float GetValue(Vector3 coordinate)
		{
			return (float)GetValue(coordinate.x, coordinate.y, coordinate.z);
		}

		public void Dispose()
		{
			if (!m_disposed)
			{
				m_disposed = Disposing();
			}
			GC.SuppressFinalize(this);
		}

		protected virtual bool Disposing()
		{
			if (modules != null)
			{
				for (int i = 0; i < modules.Length; i++)
				{
					modules[i].Dispose();
					modules[i] = null;
				}
				modules = null;
			}
			return true;
		}
	}
}
