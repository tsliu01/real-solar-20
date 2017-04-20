namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class RemoteDebugServer : MonoBehaviour
    {
        [CompilerGenerated]
        private static RemoteDebugServer k__BackingField;
        private rdtServerBroadcaster m_broadcaster;
        private TcpClient m_client;
        private IAsyncResult m_currentAsyncResult;
        private rdtDispatcher m_dispatcher = new rdtDispatcher();
        private rdtMessageGameObjectsHandler m_handler;
        private TcpListener m_listener;
        private Dictionary<System.Type, Action<rdtTcpMessage>> m_messageCallbacks = new Dictionary<System.Type, Action<rdtTcpMessage>>();
        private List<rdtTcpMessage> m_messagesToProcess;
        private rdtReadMessageThread m_readThread;
        private rdtSerializerRegistry m_serializerRegistry = new rdtSerializerRegistry();
        private State m_state;
        private Action[] m_stateDelegates;
        private rdtWriteMessageThread m_writeThread;

        public void AddCallback(System.Type type, Action<rdtTcpMessage> callback)
        {
            Dictionary<System.Type, Action<rdtTcpMessage>> dictionary;
            System.Type type2;
            if (!this.m_messageCallbacks.ContainsKey(type))
            {
                this.m_messageCallbacks[type] = null;
            }
            dictionary = this.m_messageCallbacks;
            type2 = type;
            dictionary[type2] = (Action<rdtTcpMessage>)Delegate.Combine(dictionary[type2], callback);
        }

        public void EnqueueMessage(rdtTcpMessage message)
        {
            if (this.m_writeThread != null)
            {
                this.m_writeThread.EnqueueMessage(message);
            }
        }

        private void Init()
        {
            this.RegisterCallbacks();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            }
            this.m_messagesToProcess = new List<rdtTcpMessage>(0x10);
            this.m_stateDelegates = new Action[] { null, new Action(this.OnWaiting), new Action(this.OnConnecting), new Action(this.OnConnected), new Action(this.OnDisconnected) };
            this.m_broadcaster = new rdtServerBroadcaster();
        }

        private void OnApplicationPause(bool pause)
        {
            if (Application.isMobilePlatform)
            {
                if (pause)
                {
                    rdtDebug.Debug(this, "OnApplicationPause: Is pausing", new object[0]);
                    if (this.m_listener != null)
                    {
                        this.m_listener.Stop();
                        this.m_listener = null;
                    }
                    this.Stop();
                }
                else if (this.m_listener == null)
                {
                    rdtDebug.Debug(this, "OnApplicationPause: Is resuming", new object[0]);
                    this.StartListening();
                }
            }
        }

        private void OnConnected()
        {
            if (this.m_listener.Pending())
            {
                this.m_listener.AcceptTcpClient().Close();
            }
            if ((!this.m_client.Connected || !this.m_readThread.IsConnected) || !this.m_writeThread.IsConnected)
            {
                this.SetState(State.Disconnected);
            }
            else
            {
                this.m_dispatcher.Update();
                bool flag = false;
                bool flag2 = false;
                for (int i = 0; i < this.m_messagesToProcess.Count; i++)
                {
                    System.Type type = this.m_messagesToProcess[i].GetType();
                    if (type == typeof(rdtTcpMessageGetGameObjects))
                    {
                        if (flag)
                        {
                            this.m_messagesToProcess.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else if (type == typeof(rdtTcpMessageGetComponents))
                    {
                        if (flag2)
                        {
                            this.m_messagesToProcess.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                }
                while (this.m_messagesToProcess.Count > 0)
                {
                    rdtTcpMessage message2 = this.m_messagesToProcess[0];
                    this.m_messagesToProcess.RemoveAt(0);
                    this.m_messageCallbacks[message2.GetType()](message2);
                }
            }
        }

        private void OnConnecting()
        {
            if (this.m_currentAsyncResult.IsCompleted)
            {
                try
                {
                    this.m_client = this.m_listener.EndAcceptTcpClient(this.m_currentAsyncResult);
                    rdtDebug.Debug(this, "Client connected from " + this.m_client.Client.RemoteEndPoint, new object[0]);
                    this.m_readThread = new rdtReadMessageThread(this.m_client.GetStream(), this.m_dispatcher, new Action<rdtTcpMessage>(this.OnReadMessage), "rdtServer");
                    this.m_writeThread = new rdtWriteMessageThread(this.m_client.GetStream(), "rdtServer");
                    this.SetState(State.Connected);
                }
                catch (SocketException exception)
                {
                    rdtDebug.Error(this, "Socket exception while client was connecting (error code " + exception.ErrorCode + ")", new object[0]);
                    this.StartListening();
                }
                catch (ObjectDisposedException)
                {
                    rdtDebug.Error(this, "Tcp listener was disposed", new object[0]);
                    this.StartListening();
                }
            }
        }

        private void OnDisable()
        {
            if (this.m_broadcaster != null)
            {
                this.m_broadcaster.Stop();
                this.m_listener.Stop();
                this.Stop();
                this.SetState(State.None);
            }
        }

        private void OnDisconnected()
        {
            rdtDebug.Debug(this, "Client disconnected", new object[0]);
            this.Stop();
            this.SetState(State.Waiting);
        }

        private void OnEnable()
        {
            if (Instance != null)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
            else
            {
                Instance = this;
                UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
                this.Init();
                this.StartListening();
            }
        }

        private void OnLogMessageReceivedThreaded(string message, string stackTrace, LogType type)
        {
            rdtTcpMessageLog log = new rdtTcpMessageLog(message, stackTrace, type);
            this.EnqueueMessage(log);
        }

        private void OnReadMessage(rdtTcpMessage message)
        {
            if (this.m_messageCallbacks.ContainsKey(message.GetType()))
            {
                this.m_messagesToProcess.Add(message);
            }
        }

        private void OnWaiting()
        {
            if (this.m_listener.Pending())
            {
                this.SetState(State.Connecting);
                this.m_currentAsyncResult = this.m_listener.BeginAcceptTcpClient(null, null);
            }
        }

        private void RegisterCallbacks()
        {
            if (!Application.isEditor)
            {
#if UNITY_5
                Application.logMessageReceivedThreaded += new Application.LogCallback(this.OnLogMessageReceivedThreaded);
#else
                Application.RegisterLogCallbackThreaded(new Application.LogCallback(this.OnLogMessageReceivedThreaded));
#endif
            }
            this.m_messageCallbacks.Clear();
            this.m_handler = new rdtMessageGameObjectsHandler(this);
        }

        private void SetState(State state)
        {
            rdtDebug.Debug(this, "State is {0}", new object[] { state });
            this.m_state = state;
        }

        private void StartListening()
        {
            try
            {
                rdtDebug.Debug(this, "StartListening()", new object[0]);
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, rdtSettings.SERVER_PORT);
                this.m_listener = new TcpListener(localEP);
                this.m_listener.Start(1);
                this.SetState(State.Waiting);
            }
            catch (Exception exception)
            {
                rdtDebug.Error(this, "Failed to listen to server port ({0})", new object[] { exception.Message });
            }
        }

        private void Stop()
        {
            if (this.m_client != null)
            {
                this.m_client.Close();
                this.m_client = null;
            }
            if (this.m_readThread != null)
            {
                this.m_readThread.Stop();
                this.m_readThread = null;
            }
            if (this.m_writeThread != null)
            {
                this.m_writeThread.Stop();
                this.m_writeThread = null;
            }
            this.m_dispatcher.Clear();
        }

        [Button]
        public void ToggleWorldPaused()
        {
            Time.timeScale = (Time.timeScale == 0f) ? 1f : 0f;
        }

        private void Update()
        {
            if (this.m_stateDelegates[(int) this.m_state] != null)
            {
                this.m_stateDelegates[(int) this.m_state]();
            }
        }

        public string ClientIP
        {
            get
            {
                if (this.m_client == null)
                {
                    return "";
                }
                return this.m_client.Client.RemoteEndPoint.ToString();
            }
        }

        public static RemoteDebugServer Instance
        {
            [CompilerGenerated]
            get
            {
                return k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                k__BackingField = value;
            }
        }

        public rdtSerializerRegistry SerializerRegistry
        {
            get
            {
                if (this.m_serializerRegistry == null)
                {
                    this.m_serializerRegistry = new rdtSerializerRegistry();
                }
                return this.m_serializerRegistry;
            }
        }

        private enum State
        {
            None,
            Waiting,
            Connecting,
            Connected,
            Disconnected,
            Max
        }
    }
}

