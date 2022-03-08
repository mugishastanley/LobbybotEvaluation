#if !NETFX_CORE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using CLARTE.Memory;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation
{
	[RequireComponent(typeof(Base))]
	public class BinarySerializerAdapter : MonoBehaviour
	{
		protected abstract class Context
		{
			#region Members
			public IEnumerator task;
			#endregion

			#region Abstract methods
			public abstract void Execute();
			#endregion
		}

		protected class Context<T> : Context
		{
			#region Members
			public Action<T> callback;
			public T data;
			#endregion

			#region Constructors
			public Context(Action<T> callback)
			{
				this.callback = callback;

				task = null;
				data = default(T);
			}
			#endregion

			#region Public methods
			public override void Execute()
			{
				callback(data);
			}
			#endregion
		}

		protected class SerializationContext : Context<Binary.Buffer>
		{
			#region Members
			public IBinarySerializable input;
			#endregion

			#region Constructors
			public SerializationContext(IBinarySerializable input, Action<Binary.Buffer> callback) : base(callback)
			{
				this.input = input;
			}
			#endregion

			#region Public methods
			public uint SerializationCallback(Binary serializer, ref Binary.Buffer buffer)
			{
				// We must serialize as object to properly serialize IBinaryTypeMapped objects without overhead
				return serializer != null && buffer != null ? serializer.ToBytes(ref buffer, 0, (object) input) : 0;
			}

			public void SaveBuffer(Binary.Buffer buffer)
            {
				data = buffer;
			}
			#endregion
		}

		protected class DeserializationContext : Context<IBinarySerializable>
		{
			#region Constructors
			public DeserializationContext(Action<IBinarySerializable> callback) : base(callback)
			{

			}
			#endregion

			#region Public methods
			public uint DeserializationCallback(Binary serializer, Binary.Buffer buffer)
			{
				uint read = serializer.FromBytes(buffer, 0, out object d);

				data = d as IBinarySerializable;

				return read;
			}
			#endregion
		}

		#region Members
		public bool blockingUpdate = false;
		public Events.ReceiveDeserializedCallback onReceive;

		protected Queue<SerializationContext> serializationTasks;
		protected Queue<DeserializationContext> deserializationTasks;
		protected SerializationContext currentSerialization;
		protected DeserializationContext currentDeserialization;
		protected Binary serializer;
		protected Base network;
		#endregion

		#region Members
		public Base Network
		{
			get
			{
				return network;
			}
		}

		public Binary Serializer
		{
			get
			{
				return serializer;
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			serializationTasks = new Queue<SerializationContext>();
			deserializationTasks = new Queue<DeserializationContext>();

			serializer = new Binary();

			network = GetComponent<Base>();

			currentSerialization = null;
			currentDeserialization = null;

			Action<IPAddress, Guid, ushort, BufferPool.Buffer> method = Receive;

			// Add this component as a receiver for receive events if necessary
			foreach(Channel channel in network.Channels)
			{
				bool found = false;

				int count = channel.onReceive.GetPersistentEventCount();

				for(int i = 0; i < count; i++)
				{
					if(channel.onReceive.GetPersistentTarget(i) == this && channel.onReceive.GetPersistentMethodName(i) == method.Method.Name)
					{
						found = true;

						break;
					}
				}

				if(!found)
				{
					channel.onReceive.AddListener(Receive);
				}
			}
		}

		protected void Update()
		{
			Update(serializationTasks, ref currentSerialization);
			Update(deserializationTasks, ref currentDeserialization);
		}
		#endregion

		#region Public methods
		public void Receive(IPAddress remote, Guid id, ushort channel, BufferPool.Buffer data)
		{
			if (onReceive.GetPersistentEventCount() > 0)
			{
				lock (deserializationTasks)
				{
					DeserializationContext context = new DeserializationContext(r => onReceive.Invoke(remote, id, channel, r));

					context.task = serializer.Deserialize(new Binary.Buffer(data, serializer), context.DeserializationCallback, null);

					deserializationTasks.Enqueue(context);
				}
			}
		}

		public void Send(Guid remote, ushort channel, IBinarySerializable data)
		{
			lock(serializationTasks)
			{
				SerializationContext context = new SerializationContext(data, d => network.Send(remote, channel, d));

				context.task = serializer.Serialize(context.SerializationCallback, context.SaveBuffer);

				serializationTasks.Enqueue(context);
			}
		}

		public void SendOthers(Guid remote, ushort channel, IBinarySerializable data)
		{
			lock(serializationTasks)
			{
				SerializationContext context = new SerializationContext(data, d => network.SendOthers(remote, channel, d));

				context.task = serializer.Serialize(context.SerializationCallback, context.SaveBuffer);

				serializationTasks.Enqueue(context);
			}
		}

		public void SendAll(ushort channel, IBinarySerializable data)
		{
			lock(serializationTasks)
			{
				SerializationContext context = new SerializationContext(data, d => network.SendAll(channel, d));

				context.task = serializer.Serialize(context.SerializationCallback, context.SaveBuffer);

				serializationTasks.Enqueue(context);
			}
		}
		#endregion

		#region Internal methods
		protected void Update<T>(Queue<T> queue, ref T context) where T : Context
		{
			int count = 0;
			lock (queue)
			{
				count = queue.Count;
			}

			do
			{
				if(context == null)
				{
					lock(queue)
					{

						if(count > 0)
						{
							context = queue.Dequeue();

							count--;
						}
					}
				}

				if(context != null && !context.task.MoveNext())
				{
					try
					{
						context.Execute();
					}
					catch(Exception e)
					{
						Debug.LogErrorFormat("{0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
					}
					finally
					{
						context = null;
					}
				}
			}
			while((blockingUpdate || context == null) && count > 0);
		}
		#endregion
	}
}

#endif // !NETFX_CORE
