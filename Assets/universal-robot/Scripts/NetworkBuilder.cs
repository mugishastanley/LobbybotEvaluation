using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System;
using System.Text;

public class NetworkBuilder {

	protected string m_ip;
	protected int m_port;
	protected int m_myPort;
	protected int m_receiveTimeOut;
	protected int m_sendTimeOut;
	protected Thread m_thread = null;
	protected Thread m_threadRead = null;
	protected volatile bool v_shouldStop = false;
	protected bool m_connected = false;
	protected bool m_connecting = false;

	protected Queue<byte[]> m_commandQueue = new Queue<byte[]>();
	public delegate void ReadCallBack (byte[] cmd, byte[] reply);
	protected ReadCallBack m_rcb = null;
	protected byte[] m_nonPersistentStartCommand = null;

	public delegate void WriteCallBack ();

	protected WriteCallBack m_wcb = null;
	protected int m_pingMillis = 500;

	public enum NETWORK_MODE
	{
		READ_WRITE = 0,
		READ_ONLY = 1,
		WRITE_ONLY = 2,
		READ_WRITE_PING_PONG = 3,
		CONNECT_CALLBACK = 4,
		NON_PERSISTENT = 5
	}

	protected NETWORK_MODE networkMode = NETWORK_MODE.READ_WRITE;

	public bool connected{
		get{
			return m_connected;
		}
	}

	public bool connecting{
		get{
			return m_connecting;
		}
	}

	public virtual void StartService()
	{

	}

	public virtual void Connect()
	{

	}

	public int commandQueueCount
	{
		get{
			return m_commandQueue.Count;
		}
	}

	public void PendCommand(byte[] cmd)
	{
		m_commandQueue.Enqueue (cmd);
	}

	public void PendCommand(string cmd)
	{
		m_commandQueue.Enqueue (Encoding.UTF8.GetBytes(cmd));
	}

	public virtual void Disconnect()
	{
		m_commandQueue.Clear();
		v_shouldStop = true;
		m_connected = false;
		m_connecting = false;
		if (m_thread != null && !m_thread.Join (50)) {
			m_thread.Abort ();
		}
		if (m_threadRead != null && !m_threadRead.Join (50)) {
			m_threadRead.Abort ();
		}
	}
}
