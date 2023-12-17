using System;

namespace BeatTogether.DedicatedServer.Kernel.Types
{
    public class RollingAverage
	{
		private long _currentTotal;
		private long _currentAverage;
		private readonly long[] _buffer;
		private int _index;
		private int _length;

		public long CurrentAverage => _currentAverage;
		public bool HasValue => _length > 0;

		public RollingAverage(int window)
		{
			_buffer = new long[window];
		}

		public void Update(long value)
		{
			var currentTotal = _currentTotal;
			if (_length == _buffer.Length)
			{
				currentTotal -= _buffer[_index];
			}
			_buffer[_index] = value;
			_index = (_index + 1) % _buffer.Length;
			_length = Math.Min(_length + 1, _buffer.Length);
			currentTotal += value;
			_currentTotal = currentTotal;
			_currentAverage = (currentTotal / (long)_length);
		}

		public void Reset()
		{
			_currentAverage = 0L;
			_index = 0;
			_length = 0;
			_currentTotal = 0L;
		}
	}
}
