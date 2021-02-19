using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System;
using System.Net.Sockets;

public class TCPClientBuilder : NetworkBuilder
{

	TcpClient m_client = null;
	public delegate void PendStartCommandsCallBack( );
	PendStartCommandsCallBack m_psccb;

	/// <summary>
	/// Create tcp client
	/// </summary>
	/// <param name="ip"></param>
	/// <param name="port"></param>
	/// <param name="receiveTimeOut"></param>
	/// <param name="sendTimeOut"></param>
	/// <param name="mode"></param>
	/// <param name="rcb"></param>
	/// <param name="nonPersistentStartCommand"></param>
	/// <param name="psccb"></param>
	/// <param name="wcb"></param>
	/// <param name="pingMillis"></param>
	public TCPClientBuilder( string ip, int port, int receiveTimeOut, int sendTimeOut, NETWORK_MODE mode, ReadCallBack rcb = null,
		byte[] nonPersistentStartCommand = null, PendStartCommandsCallBack psccb = null, WriteCallBack wcb = null, int pingMillis = 500 )
	{
		m_ip = ip;
		m_port = port;
		m_receiveTimeOut = receiveTimeOut;
		m_sendTimeOut = sendTimeOut;
		networkMode = mode;
		m_rcb = rcb;
		m_nonPersistentStartCommand = nonPersistentStartCommand;
		m_psccb = psccb;
		m_wcb = wcb;
		m_pingMillis = pingMillis;

		IPAddress a;
		if (!IPAddress.TryParse( ip, out a ) || port < 1)
		{
			Debug.LogWarning( "Wrong format ip/port : " + ip + ":" + port.ToString() );
			return;
		}
		v_shouldStop = false;
		StartService();
	}

	/// <summary>
	/// Starts threads
	/// </summary>
	public override void StartService( )
	{
		if (networkMode != NETWORK_MODE.NON_PERSISTENT)
		{
			m_connected = false;
			m_thread = new Thread( new ThreadStart( Service ) );
			m_thread.Start();

			if (networkMode == NETWORK_MODE.READ_WRITE)
			{
				m_threadRead = new Thread( new ThreadStart( ServiceRead ) );
				m_threadRead.Start();
			}
		}
		else
		{
			m_connected = false;
			m_thread = new Thread( new ThreadStart( NonPersistentService ) );
			m_thread.Start();
		}
	}

	/// <summary>
	/// Non persistent service thread
	/// </summary>
	void NonPersistentService( )
	{
		System.Diagnostics.Stopwatch swPing = new System.Diagnostics.Stopwatch();
		swPing.Reset();
		swPing.Start();
		while (!v_shouldStop)
		{
			if (!m_connected && m_commandQueue.Count == 0)
			{
				m_connecting = true;
				if (m_nonPersistentStartCommand != null)
				{
					m_commandQueue.Enqueue( m_nonPersistentStartCommand );
					NonPersistentWriteRead( m_commandQueue.Dequeue() );
					if (m_connected)
					{
						m_psccb();
					}
				}
			}

			if (m_wcb != null && commandQueueCount == 0 && swPing.ElapsedMilliseconds > m_pingMillis)
			{
				m_wcb();
			}


			if (m_commandQueue.Count > 0)
			{
				NonPersistentWriteRead( m_commandQueue.Dequeue() );
				swPing.Reset();
				swPing.Start();
			}
			Thread.Sleep( 1 );
		}
	}

	/// <summary>
	/// Non persistent write and read function
	/// </summary>
	/// <param name="cmd"></param>
	void NonPersistentWriteRead( byte[] cmd )
	{
		//Debug.Log(System.Text.Encoding.UTF8.GetString(cmd));

		try
		{
			m_client = new TcpClient();
			IAsyncResult ar = m_client.BeginConnect( m_ip, m_port, null, null );
			WaitHandle wh = ar.AsyncWaitHandle;
			try
			{
				if (!ar.AsyncWaitHandle.WaitOne( TimeSpan.FromSeconds( m_sendTimeOut ), false ))
				{
					m_client.Close();
					throw new TimeoutException();
				}
				m_client.EndConnect( ar );
			}
			finally
			{
				wh.Close();
			}
			m_client.GetStream().Write( cmd, 0, cmd.Length );
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Reset();
			sw.Start();

			while (!v_shouldStop && m_client.Available == 0 && sw.ElapsedMilliseconds < m_receiveTimeOut)
			{
				Thread.Sleep( 1 );
			}
			if (m_client.Available > 0)
			{
				if (!m_connected)
				{
					m_connected = true;
					m_connecting = false;
				}
				byte[] readBytes = new byte[m_client.Available];
				m_client.GetStream().Read( readBytes, 0, readBytes.Length );
				m_rcb( cmd, readBytes );
			}
		}
		catch
		{
			m_connected = false;
		}
	}


	/// <summary>
	/// Service thread
	/// </summary>
	void Service( )
	{
		Connect();
		System.Diagnostics.Stopwatch swPing = new System.Diagnostics.Stopwatch();
		swPing.Reset();
		swPing.Start();
		while (!v_shouldStop)
		{
			if (m_client != null && !m_client.Connected)
			{
				m_connected = false;
			}
			if (m_connected)
			{
				if (m_wcb != null && commandQueueCount == 0 && swPing.ElapsedMilliseconds > m_pingMillis)
				{
					m_wcb();
				}

				if (commandQueueCount > 0)
				{
					swPing.Reset();
					swPing.Start();
				}

				switch (networkMode)
				{
					case NETWORK_MODE.READ_ONLY:
						Read();
						break;
					case NETWORK_MODE.WRITE_ONLY:
						Write();
						break;
					case NETWORK_MODE.READ_WRITE:
						Write();
						break;
					case NETWORK_MODE.READ_WRITE_PING_PONG:
						if (Write())
						{
							Read();
						}
						break;
				}
				Thread.Sleep( 1 );
			}
			else
			{
				Connect();
				Thread.Sleep( 1 );
			}
		}
	}
	
	/// <summary>
	/// Service read thread
	/// </summary>
	void ServiceRead( )
	{
		while (!v_shouldStop)
		{

			if (m_connected)
			{
				Read();
			}
			Thread.Sleep( 1 );
		}
	}

	/// <summary>
	/// Read function
	/// </summary>
	void Read( )
	{
		try
		{
			if (networkMode == NETWORK_MODE.READ_WRITE_PING_PONG)
			{
				System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
				sw.Reset();
				sw.Start();
				while (!v_shouldStop && sw.ElapsedMilliseconds < m_receiveTimeOut && m_client.Available == 0)
				{
					Thread.Sleep( 1 );
				}
			}
			if (m_client != null && m_client.Available > 0)
			{
				Thread.Sleep( 1 );
				if (m_client != null)
				{
                    byte[] buffer = new byte[m_client.Available];
					m_client.GetStream().Read( buffer, 0, buffer.Length );
					if (m_rcb != null)
						m_rcb( new byte[] { }, buffer );
				}
			}
			else
			{
				Thread.Sleep( 1 );
			}
		}
		catch (Exception e)
		{
			Debug.LogException( e );
		}
	}

	/// <summary>
	/// Write function
	/// </summary>
	/// <returns>send ok</returns>
	bool Write( )
	{
		bool sending = false;
		try
		{
			if (m_commandQueue.Count > 0)
			{
				byte[] send = m_commandQueue.Dequeue();
				m_client.GetStream().Write( send, 0, send.Length );
				sending = true;
            }
		}
		catch (Exception e)
		{
			Debug.LogException( e );
			m_connected = false;
			return sending;
		}
		return sending;
	}

    public void cuicui()
    {
        m_client.GetStream().Position = 0;
    }


	/// <summary>
	/// Connect the socket
	/// </summary>
	public override void Connect( )
	{
		try
		{
			if (m_client != null)
				m_client.Close();
			m_commandQueue.Clear();
			m_client = new TcpClient();
			m_client.Client.ReceiveTimeout = m_receiveTimeOut;
			m_client.Client.SendTimeout = m_sendTimeOut;
			m_connected = false;
			m_connecting = true;
			Debug.Log( "Connecting to " + m_ip + ":" + m_port );
			IAsyncResult res = m_client.BeginConnect( m_ip, m_port, new AsyncCallback( ConnexionConnectCallback ), null );
			bool success = res.AsyncWaitHandle.WaitOne( TimeSpan.FromSeconds( 2 ) );
			if (!success)
			{
				Debug.Log( "Failed to connect at " + m_ip + ":" + m_port );
			}
			else
			{
				Debug.Log( "Connected at " + m_ip + ":" + m_port );
				m_connected = true;
				Thread.Sleep( 1 );
			}
			m_connecting = false;
		}
		catch (Exception e)
		{
			Debug.LogException( e );
		}
	}

	/// <summary>
	/// connexion callback
	/// </summary>
	/// <param name="asyncResult"></param>
	private void ConnexionConnectCallback( System.IAsyncResult asyncResult )
	{
		m_client.EndConnect( asyncResult );
	}

	/// <summary>
	/// Dicsconnect socket
	/// </summary>
	public override void Disconnect( )
	{
		if (m_client != null)
			m_client.Close();
		base.Disconnect();
	}
}
