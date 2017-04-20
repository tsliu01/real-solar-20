namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using UnityEngine;

    public class rdtClient
    {
        private TcpClient m_client;
        private IAsyncResult m_currentAsyncResult;
        private rdtDispatcher m_dispatcher = new rdtDispatcher();
        private Dictionary<System.Type, Action<rdtTcpMessage>> m_messageCallbacks = new Dictionary<System.Type, Action<rdtTcpMessage>>();
        private rdtReadMessageThread m_readThread;
        private State m_state;
        private Action<double>[] m_stateDelegates;
        private rdtWriteMessageThread m_writeThread;

        public rdtClient()
        {
            this.m_stateDelegates = new Action<double>[] { null, new Action<double>(this.OnConnecting), new Action<double>(this.OnConnected), new Action<double>(this.OnDisconnected) };
        }

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
            dictionary[type2] = (Action<rdtTcpMessage>) Delegate.Combine(dictionary[type2], callback);
        }

        public void Connect(IPAddress address)
        {
            this.m_client = new TcpClient();
            this.m_currentAsyncResult = this.m_client.BeginConnect(address, rdtSettings.SERVER_PORT, null, null);
            this.SetState(State.Connecting);
        }

        public void EnqueueMessage(rdtTcpMessage message)
        {
            if (this.m_writeThread != null)
            {
                this.m_writeThread.EnqueueMessage(message);
            }
        }

        private void OnConnected(double delta)
        {
            if ((!this.m_client.Connected || !this.m_writeThread.IsConnected) || !this.m_readThread.IsConnected)
            {
                this.SetState(State.Disconnected);
            }
            else
            {
                this.m_dispatcher.Update();
            }
        }

        private void OnConnecting(double delta)
        {
            if (this.m_currentAsyncResult.IsCompleted)
            {
                try
                {
                    this.m_client.EndConnect(this.m_currentAsyncResult);
                    rdtDebug.Debug(this, "Connected to server", new object[0]);
                    this.m_writeThread = new rdtWriteMessageThread(this.m_client.GetStream(), "rdtClient");
                    this.m_readThread = new rdtReadMessageThread(this.m_client.GetStream(), this.m_dispatcher, new Action<rdtTcpMessage>(this.OnReadMessage), "rdtClient");
                    this.SetState(State.Connected);
                }
                catch (SocketException exception)
                {
                    switch (exception.ErrorCode)
                    {
                        case 0x274c:
                            rdtDebug.Info("RemoteDebug: Connection timed out", new object[0]);
                            break;

                        case 0x274d:
                            rdtDebug.Info("RemoteDebug: Connection was refused", new object[0]);
                            break;

                        default:
                            rdtDebug.Info("RemoteDebug: Failed to connect to server (error code " + exception.ErrorCode + ")", new object[0]);
                            break;
                    }
                    this.Stop();
                }
                catch (ObjectDisposedException)
                {
                    rdtDebug.Error(this, "Client was disposed", new object[0]);
                    this.Stop();
                }
            }
        }

        private void OnDisconnected(double delta)
        {
            rdtDebug.Debug(this, "Server disconnected", new object[0]);
            this.Stop();
        }

        private void OnReadMessage(rdtTcpMessage message)
        {
            if (message is rdtTcpMessageLog)
            {
                rdtTcpMessageLog log = (rdtTcpMessageLog) message;
                switch (log.m_logType)
                {
                    case LogType.Error:
                        Debug.LogError(log);
                        break;

                    case LogType.Assert:
                        Debug.LogError(log);
                        break;

                    case LogType.Warning:
                        Debug.LogWarning(log);
                        break;

                    case LogType.Log:
                        Debug.Log(log);
                        break;

                    case LogType.Exception:
                        Debug.LogError(log);
                        break;
                }
            }
            if (this.m_messageCallbacks.ContainsKey(message.GetType()))
            {
                this.m_messageCallbacks[message.GetType()](message);
            }
        }

        private void SetState(State state)
        {
            rdtDebug.Debug(this, "State is {0}", new object[] { state });
            this.m_state = state;
        }

        public void Stop()
        {
            if (this.m_client != null)
            {
                rdtDebug.Debug(this, "Stopping connection", new object[0]);
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
            this.SetState(State.None);
        }

        public void Update(double delta)
        {
            if (this.m_stateDelegates[(int) this.m_state] != null)
            {
                this.m_stateDelegates[(int) this.m_state](delta);
            }
        }

        public bool IsConnected
        {
            get
            {
                return (this.m_state == State.Connected);
            }
        }

        public bool IsConnecting
        {
            get
            {
                return (this.m_state == State.Connecting);
            }
        }

        private enum State
        {
            None,
            Connecting,
            Connected,
            Disconnected,
            Max
        }
    }
}

