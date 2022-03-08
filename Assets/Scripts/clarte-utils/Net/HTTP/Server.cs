#if !NETFX_CORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CLARTE.Net.HTTP
{
	public class Server : IDisposable
	{
		public struct Response
		{
			public string mimeType;
			public byte[] data;

			public Response(string mime_type, byte[] output_data)
			{
				mimeType = mime_type;
				data = output_data;
			}

			public Response(string mime_type, string output_str)
			{
				mimeType = mime_type;
				data = Encoding.UTF8.GetBytes(output_str);
			}
		}

		#region Delegates
		public delegate Response Endpoint(Dictionary<string, string> parameters);
		#endregion

		#region Members
		private readonly HttpListener listener;
		private readonly Threads.Thread listenerWorker;
		private readonly ManualResetEvent stopEvent;
		private readonly Dictionary<string, Endpoint> endpoints;
		private bool disposed;
		#endregion

		#region Constructors
		public Server(ushort port, Dictionary<string, Endpoint> endpoints)
		{
			if(!HttpListener.IsSupported)
			{
				throw new NotSupportedException("HTTP server is not support on this implementation.");
			}

			// Initialize unity objects in unity thread
			Threads.APC.MonoBehaviourCall.Instance.GetType();

			this.endpoints = endpoints;

			stopEvent = new ManualResetEvent(false);

			listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://*:{0}/", port));
			listener.Start();

			listenerWorker = new Threads.Thread(Listen);
			listenerWorker.Start();
		}
		#endregion

		#region IDisposable implementation
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				if(disposing)
				{
					// TODO: delete managed state (managed objects).

					listener.Stop();

					stopEvent.Set();

					listenerWorker.Join();

					stopEvent.Close();
				}

				// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
				// TODO: set fields of large size with null value.

				disposed = true;
			}
		}

		// TODO: replace finalizer only if the above Dispose(bool disposing) function as code to free unmanaged resources.
		~Server()
		{
			Dispose(/*false*/);
		}

		/// <summary>
		/// Dispose of the HTTP server.
		/// </summary>
		public void Dispose()
		{
			// Pass true in dispose method to clean managed resources too and say GC to skip finalize in next line.
			Dispose(true);

			// If dispose is called already then say GC to skip finalize on this instance.
			// TODO: uncomment next line if finalizer is replaced above.
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Control methods
		public void Stop()
		{
			Dispose();
		}
		#endregion

		#region Thread methods
		private void Listen()
		{
			while(listener.IsListening)
			{
				IAsyncResult context = listener.BeginGetContext(Receive, null);

				if(WaitHandle.WaitAny(new[] { stopEvent, context.AsyncWaitHandle }) == 0)
				{
					return;
				}
			}
		}

		private void Receive(IAsyncResult async_result)
		{
			try
			{
				HttpListenerContext context = listener.EndGetContext(async_result);

				Parallel.Invoke(() => Respond(context));
			}
			catch(Exception exception)
			{
				Debug.LogError(exception);
			}
		}
		#endregion

		#region HTTP handling
		private void Respond(HttpListenerContext context)
		{
			try
			{
				Endpoint callback;

				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;

				Debug.LogFormat("{0} {1}", request.HttpMethod, request.Url);

				// Display headers
				Debug.Log("Headers:");
				for(int i = 0; i < request.Headers.Count; i++)
				{
					Debug.LogFormat("{0}: {1}", request.Headers.GetKey(i), request.Headers.Get(i));
				}

				// Get data
				int request_size = (int)request.ContentLength64;
				byte[] data = new byte[request_size];

				Stream input = request.InputStream;
				input.Read(data, 0, request_size);
				input.Close();

				Debug.LogFormat("Data: {0}", Encoding.UTF8.GetString(data));

				if(endpoints != null && endpoints.TryGetValue(Uri.UnescapeDataString(request.Url.AbsolutePath), out callback))
				{
					Dictionary<string, string> parameters = new Dictionary<string, string>();

					// Parse query parameters
					AddParameters(parameters, request.Url.Query);

					if(request.ContentType == "application/x-www-form-urlencoded")
					{
						AddParameters(parameters, Encoding.UTF8.GetString(data));
					}

					Threads.APC.MonoBehaviourCall.Instance.Call(() =>
					{
						// Call unity callback in main unity thread
						Response res = callback(parameters);

						// Send response back to the client in another thread
						Parallel.Invoke(() =>
						{
							response.ContentType = res.mimeType;
							response.ContentEncoding = Encoding.UTF8;

							// Implement Post/Redirect/Get pattern by default
							if(parameters.Count > 0)
							{
								response.StatusCode = (int) HttpStatusCode.RedirectMethod;
								response.RedirectLocation = request.Url.AbsolutePath;
							}
							else
							{
								response.StatusCode = (int) HttpStatusCode.OK;
							}

							SendResponse(response, res.data);
						});
					});
				}
				else
				{
					const string unauthorized = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\"><html><head><title>404 Not Found</title></head><body><h1>404 Not Found</h1></body></html>";

					response.StatusCode = (int) HttpStatusCode.NotFound;
					response.ContentType = "text/html";
					response.ContentEncoding = Encoding.UTF8;

					SendResponse(response, Encoding.UTF8.GetBytes(unauthorized));
				}
			}
			catch(Exception exception)
			{
				Debug.LogError(exception);
			}
		}

		private void AddParameters(Dictionary<string, string> parameters, string query)
		{
			if(parameters != null && !string.IsNullOrEmpty(query))
			{
				string[] parameters_pair = Uri.UnescapeDataString(query).TrimStart('?').Split('&');

				foreach(string param_pair in parameters_pair)
				{
					string[] parameter = param_pair.Split('=');

					if(parameter.Length > 1)
					{
						parameters.Add(parameter[0].ToLower(), string.Join("=", parameter, 1, parameter.Length - 1).ToLower());
					}
				}
			}
		}

		private void SendResponse(HttpListenerResponse response, byte[] data)
		{
			response.ContentLength64 = data.Length;

			Stream output = response.OutputStream;
			output.Write(data, 0, data.Length);
			output.Close();
		}
		#endregion
	}
}

#endif // !NETFX_CORE
