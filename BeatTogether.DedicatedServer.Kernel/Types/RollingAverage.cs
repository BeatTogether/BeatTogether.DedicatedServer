using System;

namespace BeatTogether.DedicatedServer.Kernel.Types
{
    public class RollingAverage
	{
		private long _currentTotal;
		private float _currentAverage;
		private readonly long[] _buffer;
		private int _index;
		private int _length;

		private const long _granularity = 1000L;

		public float CurrentAverage => _currentAverage;
		public bool HasValue => _length > 0;

		public RollingAverage(int window)
		{
			_buffer = new long[window];
		}

		public void Update(float value)
		{
			var currentTotal = _currentTotal;
			if (_length == _buffer.Length)
			{
				currentTotal -= _buffer[_index];
			}
			var i = (long)(value * 1000f);
			_buffer[_index] = i;
			_index = (_index + 1) % _buffer.Length;
			_length = Math.Min(_length + 1, _buffer.Length);
			currentTotal += i;
			_currentTotal = currentTotal;
			_currentAverage = (float)(currentTotal / (double)(_granularity * _length));
		}

		public void Reset()
		{
			_currentAverage = 0f;
			_index = 0;
			_length = 0;
			_currentTotal = 0L;
		}
	}
}
