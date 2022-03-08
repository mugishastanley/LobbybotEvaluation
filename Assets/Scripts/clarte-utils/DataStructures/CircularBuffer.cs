using System.Collections;
using System.Collections.Generic;

namespace CLARTE.DataStructures
{
	public class CircularBuffer<T> : IEnumerable<T>
	{
		#region Members
		protected T[] data;
		protected int start;
		protected int end;
		#endregion

		#region Constructors
		public CircularBuffer(uint size)
		{
			data = new T[size];
			start = end = 0;
		}
		#endregion

		#region Getters / Setters
		public int Count
		{
			get
			{
				int pos = end + (end < start ? data.Length : 0);

				return pos - start;
			}
		}
		#endregion

		#region IEnumerable implementation
		public IEnumerator<T> GetEnumerator()
		{
			int pos = start;

			while(pos != end)
			{
				yield return data[pos];

				Increment(ref pos);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Public methods
		public void AddLast(T item)
		{
			Increment(ref end);

			if (end == start)
			{
				Increment(ref start);
			}

			if (end != start)
			{
				data[(end == 0 ? data.Length : end) - 1] = item;
			}
		}

		public void RemoveFirst()
		{
			if(start != end)
			{
				Increment(ref start);
			}
		}
		#endregion

		#region Internal methods
		protected void Increment(ref int value)
		{
			value = (value + 1) % data.Length;
		}
		#endregion
	}
}
