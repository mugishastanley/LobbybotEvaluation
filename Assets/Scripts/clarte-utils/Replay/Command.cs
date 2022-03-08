
using System;
using CLARTE.Serialization;

namespace CLARTE.Replay
{
	/// <summary>
	/// Serializable command, inhérit to create a Replayable command sent to a ReplayContext.
	/// </summary>
	[Serializable]
	public abstract class Command : IBinaryTypeMapped
	{
		
		#region Members
		/// <summary>
		/// command TimeStamp, Setted by Replay Engine
		/// </summary>
		public double TimeStamp;
		#endregion

		#region Public Methods
		/// <summary>
		/// Implement the action on the replay context. Usually, cast the context and call a method.
		/// This method is called by the Replay Engine.
		/// </summary>
		/// <param name="context">The replay context in witch this command is executed.</param>
		public abstract void execute(ReplayContext context);

		#region IBinaryTypeMapped implementation
		/// <summary>
		/// Used by replay engine to load the command list from a file
		/// </summary>
		public abstract uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start);

		/// <summary>
		/// Used by replay engine to save the command list to a file
		/// </summary>
		public abstract uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start);
		#endregion

		#endregion

	}
}
