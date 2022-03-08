using System.Collections.Generic;
using System.IO;

namespace CLARTE.Dev.Profiling
{
	static public class LogInFile
	{
		static private Dictionary<string, List<string>> logs;

		/// <summary>
		/// Add line to the log
		/// </summary>
		/// <param name="filename">Log file name</param>
		/// <param name="log">Line to be added</param>
		/// <param name="write_immediately">Should the log be immediately dumped into the file? Otherwise dump will only occur upon manual call to DumpLog() or DumpAllLogs()</param>
		static public void AddToLog(string filename, object log, bool write_immediately = false)
		{
			if(logs == null)
			{
				logs = new Dictionary<string, List<string>>();
			}

			if(!logs.ContainsKey(filename))
			{
				logs.Add(filename, new List<string>());

				File.Delete(filename);
			}

			string log_str = log.ToString();

			logs[filename].Add(log_str);

			if(write_immediately)
			{
				using(FileStream file = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None))
				{
					using(StreamWriter stream = new StreamWriter(file))
					{
						stream.WriteLine(log_str);
					}
				}
			}
		}

		/// <summary>
		/// Dump specific log ti file
		/// </summary>
		/// <param name="filename"></param>
		static public void DumpLog(string filename)
		{
			if(logs != null)
			{
				if(logs.ContainsKey(filename))
				{
					File.WriteAllLines(filename, logs[filename].ToArray());

					UnityEngine.Debug.Log("Log dumped to " + filename);
				}
				else
				{
					UnityEngine.Debug.LogWarning("No log to be dumped into " + filename);
				}
			}
		}

		/// <summary>
		/// Dump All logs to files
		/// </summary>
		static public void DumpAllLogs()
		{
			if(logs != null)
			{
				foreach(string filename in logs.Keys)
				{
					DumpLog(filename);
				}
			}
		}

		static public void ClearLog(string filename)
		{
			if(logs != null)
			{
				if(logs.ContainsKey(filename))
				{
					logs[filename].Clear();

					UnityEngine.Debug.Log("Log reset");
				}
				else
				{
					UnityEngine.Debug.LogWarning("No log to be reset");
				}
			}
		}

		static public void ClearAllLogs()
		{
			if(logs != null)
			{
				foreach(string filename in logs.Keys)
				{
					ClearLog(filename);
				}
			}
		}
	}
}