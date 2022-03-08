using UnityEngine;
using CLARTE.Serialization;

namespace CLARTE.Net.Utils
{
	public class InterpolatedTransform : MonoBehaviour
	{
		public class State : IBinaryTypeMapped
		{
			#region Members
			public float timestamp;
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 scale;
			#endregion

			#region Constructors
			public State()
			{
				// Required by deserialization
			}

			public State(Transform transform, bool relative)
			{
				timestamp = Time.realtimeSinceStartup;

				if (relative)
				{
					position = transform.localPosition;
					rotation = transform.localRotation;
					scale = transform.localScale;
				}
				else
				{
					position = transform.position;
					rotation = transform.rotation;
					scale = transform.lossyScale;
				}
			}
			#endregion

			#region IBinaryTypeMapped implementation
			public uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
			{
				uint read = 0;

				read += serializer.FromBytes(buffer, start + read, out timestamp);
				read += serializer.FromBytes(buffer, start + read, out position);
				read += serializer.FromBytes(buffer, start + read, out rotation);
				read += serializer.FromBytes(buffer, start + read, out scale);

				return read;
			}

			public uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
			{
				uint written = 0;

				written += serializer.ToBytes(ref buffer, start + written, timestamp);
				written += serializer.ToBytes(ref buffer, start + written, position);
				written += serializer.ToBytes(ref buffer, start + written, rotation);
				written += serializer.ToBytes(ref buffer, start + written, scale);

				return written;
			}
			#endregion
		}

		#region Members
		protected const uint bufferedStateSize = 20;
		protected const uint timeOffsetsSize = 20;
		protected const uint timeOffsetsMinSize = 5;
		protected const float timeOffsetsMaxDeviationPercentage = 0.25f;

		[Range(1, 100)]
		public float networkSendRate = 10f; // In Hz
		[Range(0, 10)]
		public uint interpolatedFrames = 2;
		[Range(0, 10)]
		public float extrapolatedFrames = 2;
		public bool relative = true;

		// We store twenty states with "playback" information
		protected State[] bufferedState;
		protected DataStructures.CircularBuffer<float> timeOffsets;
		// Keep track of what slots are used
		protected int timestamp;
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			bufferedState = new State[bufferedStateSize];
			timeOffsets = new DataStructures.CircularBuffer<float>(timeOffsetsSize);
		}

		protected void OnDisable()
		{
			// We have finished interpolation & extrapolation is desactivated: we have nothing left to do
			if (bufferedState[0] != null)
			{
				SetTransform(bufferedState[0].position, bufferedState[0].rotation, bufferedState[0].scale, relative);
			}

			for (int i = 0; i < bufferedState.Length; i++)
			{
				bufferedState[i] = null;
			}

			timestamp = 0;
		}

		// This only runs where the component is enabled, which is only on remote peers (server/clients)
		protected void Update()
		{
			float current_time = Time.realtimeSinceStartup - AverageTimeOffset();
			float interpolation_time = current_time - interpolatedFrames / networkSendRate;
			// We have a window of interpolationBackTime where we basically play 
			// By having interpolationBackTime the average ping, you will usually use interpolation.
			// And only if no more data arrives we will use extrapolation

			// Use interpolation
			// Check if latest state exceeds interpolation time, if this is the case then
			// it is too old and extrapolation should be used
			if(bufferedState[0] != null && bufferedState[0].timestamp > interpolation_time)
			{
				for(int i = 0; i < timestamp; i++)
				{
					// Find the state which matches the interpolation time (time+0.1) or use last state
					if(bufferedState[i] != null && (bufferedState[i].timestamp <= interpolation_time || i == timestamp - 1))
					{
						// The state one slot newer (<100ms) than the best playback state
						State rhs = bufferedState[Mathf.Max(i - 1, 0)];

						// The best playback state (closest to 100 ms old (default time))
						State lhs = bufferedState[i];

						// Use the time between the two slots to determine if interpolation is necessary
						float length = rhs.timestamp - lhs.timestamp;
						float t = 0.0F;

						// As the time difference gets closer to 100 ms t gets closer to 1 in 
						// which case rhs is only used
						if(length > 0.0001)
						{
							t = (float) ((interpolation_time - lhs.timestamp) / length);
						}

						// if t=0 => lhs is used directly
						SetTransform(
							Vector3.Lerp(lhs.position, rhs.position, t),
							Quaternion.Slerp(lhs.rotation, rhs.rotation, t),
							Vector3.Lerp(lhs.scale, rhs.scale, t),
							relative
						);

						return;
					}
				}
			}
			else if(bufferedState[0] != null && bufferedState[1] != null)
			{
				// Latest received state
				State latest = bufferedState[0];

				float extrapolation_length = (float) (interpolation_time - latest.timestamp);

				// Don't extrapolation for too long, you would need to do that carefully
				if(extrapolation_length < extrapolatedFrames / networkSendRate)
				{
					// State just before the latest state
					State before = bufferedState[1];

					float delta_time = (float) (latest.timestamp - before.timestamp);

					Vector3 velocity = new Vector3(
						(latest.position.x - before.position.x) / delta_time,
						(latest.position.y - before.position.y) / delta_time,
						(latest.position.z - before.position.z) / delta_time
					);

					Vector3 scale = new Vector3(
						(latest.scale.x - before.scale.x) / delta_time,
						(latest.scale.y - before.scale.y) / delta_time,
						(latest.scale.z - before.scale.z) / delta_time
						);

					Quaternion drot = latest.rotation * Quaternion.Inverse(before.rotation);

					float angle;
					Vector3 axis;
					drot.ToAngleAxis(out angle, out axis);

					if(angle > 180.0f)
					{
						angle -= 360.0f;
					}

					float angular_velocity = angle / delta_time;

					SetTransform(
						latest.position + velocity * extrapolation_length,
						Quaternion.AngleAxis(angular_velocity * extrapolation_length, axis) * latest.rotation,
						latest.scale + scale * extrapolation_length,
						relative
					);
				}
			}
		}
		#endregion

		#region Public methods
		public State Send(bool relative)
		{
			return new State(transform, relative);
		}

		public void Receive(State state)
		{
			if (enabled)
			{
				float time_offset = Time.realtimeSinceStartup - state.timestamp;

				float average = AverageTimeOffset();

				// Add time offset to pool with outlier filtering
				if (timeOffsets.Count < timeOffsetsMinSize || (Mathf.Abs(time_offset) > Vector3.kEpsilon ? 1f - Mathf.Abs(average / time_offset) : Mathf.Abs(average)) <= timeOffsetsMaxDeviationPercentage)
				{
					timeOffsets.AddLast(time_offset);
				}

				// Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
				for (int i = bufferedState.Length - 1; i >= 1; i--)
				{
					bufferedState[i] = bufferedState[i - 1];
				}

				// Save correct received state as 0 in the buffer, safe to overwrite after shifting
				bufferedState[0] = state;

				// Increment state count but never exceed buffer size
				timestamp = Mathf.Min(timestamp + 1, bufferedState.Length);

				// Check integrity, lowest numbered state in the buffer is newest and so on
				for (int i = 0; i < timestamp - 1; i++)
				{
					if (bufferedState[i] == null || bufferedState[i + 1] == null || bufferedState[i].timestamp < bufferedState[i + 1].timestamp)
					{
						Debug.LogWarning("Interpolation state inconsistent.");
					}
				}
			}
		}
		#endregion

		#region Internal methods
		protected void SetTransform(Vector3 position, Quaternion rotation, Vector3 scale, bool relative)
		{
			if (relative)
			{
				transform.localPosition = position;
				transform.localRotation = rotation;
				transform.localScale = scale;
			}
			else
			{
				transform.position = position;
				transform.rotation = rotation;

				if (transform.parent != null)
				{
					transform.localScale = new Vector3(
						scale.x / transform.parent.lossyScale.x,
						scale.y / transform.parent.lossyScale.y,
						scale.z / transform.parent.lossyScale.z
					);
				}
				else
				{
					transform.localScale = scale;
				}
			}
		}

		protected float AverageTimeOffset()
		{
			float average = 0f;
			uint count = 0;

			foreach (float offset in timeOffsets)
			{
				average += offset;
				count++;
			}

			return count != 0 ? average / count : 0f;
		}
		#endregion
	}
}
