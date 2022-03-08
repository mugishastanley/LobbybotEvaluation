using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using CLARTE.Serialization;

namespace CLARTE.Replay
{

    /// <summary>
    /// Event to monitor commands
    /// </summary>
    [System.Serializable] public class CommandUnityEvent : UnityEvent<Command> { }

    /// <summary>
    /// Replay status
    /// </summary>
    public enum ReplayStatus
    {
        Recording,
        Replaying,
        Waiting
    }

    /// <summary>
    /// The replay engine.
    /// Usually, calling event in this looping order.
    /// - Record()
    /// - Stop()
    /// - Play() & Pause() is loop.
    /// - Wait()
    /// </summary>
    public class ReplayEngine: MonoBehaviour
    {
		#region Members
        [Tooltip("The replay context")]
		public ReplayContext context;
        
        [Header("Events")]
        public UnityEvent onEnterWaitMode;
        public UnityEvent onEnterRecordMode;
        public UnityEvent onEnterReplayMode;
        public UnityEvent onPause;
        public UnityEvent onPlay;
        public UnityEvent onGoto;
        
        [Header("Monitoring events")]
        public CommandUnityEvent onCommandExecuted;

        /// <summary>Serialiser used to read and write data to / from disk</summary>
        private Binary serializer;

        /// <summary>Command list</summary>
        private List<Command> commands;

        /// <summary>Mutext to avoid unordered command execution</summary>
        private Mutex commandLck;

        /// <summary>replay position in command list for replay mode</summary>
        private int replayPosition;

        /// <summary>time stamp to fast forward to for replay mode</summary>
        private double goto_timestamp;
        #endregion

        #region Properties
        /// <summary>
        /// Get last timestamp in the simulation. used for example to set a slider max value.
        /// </summary>
        public float LastCommandTimeStamp
        {
            get
            {
                if (commands.Count <= 0)
                {
                    return 0;
                }

                return (float) commands[commands.Count - 1].TimeStamp;
            }
        }

        /// <summary>
        /// Current replay mode.
        /// </summary>
        public ReplayStatus mode { get; private set; } = ReplayStatus.Waiting;
        #endregion

        #region MonoBehaviour callbacks
        private void Awake()
        {
            serializer = new Binary();
            commands = new List<Command>();
            commandLck = new Mutex();
            replayPosition = 0;
            goto_timestamp = -1f;

            Wait();
        }

        private void Update()
        {
            if (mode == ReplayStatus.Replaying)
            {
                //Go forward
                if (goto_timestamp >= 0)
                {
                    while (replayPosition < commands.Count && commands[replayPosition].TimeStamp <= goto_timestamp)
                    {
                        context.SetTimeStamp(commands[replayPosition].TimeStamp);
                        commands[replayPosition].execute(context);
                        onCommandExecuted.Invoke(commands[replayPosition]);
                        ++replayPosition;
                    }

                    //Set timestamp
                    context.SetTimeStamp(goto_timestamp);
                    goto_timestamp = -1f;

                    onGoto.Invoke();
                    Pause();
                }
                else
                {
                    var timestamp = context.GetTimeStamp();
                    while (replayPosition < commands.Count && commands[replayPosition].TimeStamp <= timestamp)
                    {
                        commands[replayPosition].execute(context);
                        onCommandExecuted.Invoke(commands[replayPosition]);
                        ++replayPosition;
                    }

                    //Stop if at end
                    if (timestamp > LastCommandTimeStamp)
                    {
                        Pause();
                    }
                }
            }
        }
		#endregion

		#region Public Methods
        /// <summary>
        /// Store and Execute a command at current simulation timestamp in record mode
        /// </summary>
        /// <param name="command">the command to execute</param>
		public void Execute(Command command)
        {
            if (mode == ReplayStatus.Recording)
            {
                using (commandLck)
                {
                    command.TimeStamp = context.GetTimeStamp();
                    command.execute(context);
                    commands.Add(command);
                }
                onCommandExecuted.Invoke(command);
            }
        }

        /// <summary>
        /// Go to Waiting Mode, does not record nor replay.
        /// </summary>
        public void Wait()
        {
            mode = ReplayStatus.Waiting;

            //Reset Simulation clear commands
            context.ResetContext();
            replayPosition = 0;
            commands.Clear();
            onEnterWaitMode.Invoke();
        }

        /// <summary>
        /// Go to record mode. Start the context and start to record.
        /// </summary>
        public void Record()
        {
            mode = ReplayStatus.Recording;
            onEnterRecordMode.Invoke();
            context.StartContext();
        }

        /// <summary>
        /// Stop the recording and go to replay mode. After context is reseted and paused.
        /// </summary>
        public void Stop()
        {
            //Reset simulation
            context.ResetContext();

            //set replay mode on
            mode = ReplayStatus.Replaying;
            replayPosition = 0;
            onEnterReplayMode.Invoke();
        }

        /// <summary>
        /// Start playing replay. start the context.
        /// </summary>
        public void Play()
        {
            if (mode == ReplayStatus.Replaying)
            {
                context.StartContext();
                onPlay.Invoke();
            }
        }

        /// <summary>
        /// Pause playing replay. Stop the context.
        /// </summary>
        public void Pause()
        {
            if (mode == ReplayStatus.Replaying)
            {
                context.StopContext();
                onPause.Invoke();
            }
        }

        /// <summary>
        /// Reset the context and tell update to fast forward the simulation at next frame. Pause the context.
        /// </summary>
        /// <param name="timestamp"></param>
        public void GoTo(float timestamp)
        {
            if (mode == ReplayStatus.Replaying)
            {
                //Reset
                context.ResetContext();
                replayPosition = 0;
                goto_timestamp = timestamp;
            }
        }

        /// <summary>
        /// Save all the recorded commands to a file.
        /// Should be called after Stop() and before next Wait().
        /// </summary>
        /// <param name="filename">file path</param>
        public void Save(string filename)
        {
            Binary.Buffer buffer = serializer.Serialize(commands);

            using(FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(buffer.Data, 0, (int) buffer.Size);
            }
        }

        /// <summary>
        /// Load the recorded commands from a file.
        /// Call Stop() just after to start replay mode.
        /// </summary>
        /// <param name="filename">file path</param>
        public void Load(string filename)
        {
            commands = serializer.Deserialize(File.ReadAllBytes(filename)) as List<Command>;
        }
		#endregion
	}
}
