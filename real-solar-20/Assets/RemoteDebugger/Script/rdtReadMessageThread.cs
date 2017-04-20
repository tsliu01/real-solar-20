namespace Hdg
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    public class rdtReadMessageThread
    {
        private Action<rdtTcpMessage> m_callback;
        private rdtDispatcher m_dispatcher;
        private string m_name;
        private BinaryReader m_reader;
        private bool m_run;
        private State m_state;
        private Action[] m_stateDelegates;
        private NetworkStream m_stream;
        private Thread m_thread;

        public rdtReadMessageThread(NetworkStream stream, rdtDispatcher dispatcher, Action<rdtTcpMessage> callback, string name)
        {
            this.m_name = name;
            this.m_stateDelegates = new Action[] { new Action(this.OnReading), new Action(this.OnLostConnection) };
            this.m_stream = stream;
            this.m_reader = new BinaryReader(this.m_stream);
            this.m_dispatcher = dispatcher;
            this.m_callback = callback;
            this.m_run = true;
            this.m_thread = new Thread(new ThreadStart(this.ThreadFunc));
            this.m_thread.Name = this.m_name + " rdtReadMessageThread";
            this.m_thread.Start();
        }

        private void OnLostConnection()
        {
            this.m_run = false;
        }

        private void OnReading()
        {
            try
            {
                bool flag = true;
                int count = this.m_reader.ReadInt32();
                byte[] buffer = this.m_reader.ReadBytes(count);
                flag = false;
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    Action action = null;
                    BinaryFormatter formatter = new BinaryFormatter();
                    rdtTcpMessage message = formatter.Deserialize(stream) as rdtTcpMessage;
                    if (message != null)
                    {
                        if (action == null)
                        {
                            action = delegate {
                                this.m_callback(message);
                            };
                        }
                        this.m_dispatcher.Enqueue(action);
                    }
                    else
                    {
                        rdtDebug.Error(this, "Ignoring invalid message", new object[0]);
                    }
                }
                if (flag)
                {
                    this.m_state = State.LostConnection;
                }
            }
            catch (SocketException exception)
            {
                rdtDebug.Log(this, exception, rdtDebug.LogLevel.Debug, "{0} socket exception", new object[] { this.m_name });
                rdtDebug.Debug(this, "{3} ErrorCode={0} SocketErrorCode={1} NativeErrorCode={2}", new object[] { exception.ErrorCode, exception.SocketErrorCode, exception.NativeErrorCode, this.m_name });
                this.m_state = State.LostConnection;
            }
            catch (IOException exception2)
            {
                SocketException innerException = exception2.InnerException as SocketException;
                if (innerException != null)
                {
                    rdtDebug.Log(this, innerException, rdtDebug.LogLevel.Debug, "{0} socket exception", new object[] { this.m_name });
                    rdtDebug.Debug(this, "{3} ErrorCode={0} SocketErrorCode={1} NativeErrorCode={2}", new object[] { innerException.ErrorCode, innerException.SocketErrorCode, innerException.NativeErrorCode, this.m_name });
                }
                else
                {
                    rdtDebug.Log(this, exception2, rdtDebug.LogLevel.Debug, "{0} thread lost connection", new object[] { this.m_name });
                }
                this.m_state = State.LostConnection;
            }
            catch (ObjectDisposedException)
            {
                rdtDebug.Debug(this, "{0} thread object disposed, lost connection", new object[] { this.m_name });
                this.m_state = State.LostConnection;
            }
            catch (Exception exception4)
            {
                rdtDebug.Error(this, exception4, "{0} thread unknown exception", new object[] { this.m_name });
            }
        }

        public void Stop()
        {
            this.m_run = false;
            this.m_thread.Join();
        }

        private void ThreadFunc()
        {
            while (this.m_run)
            {
                if (this.m_stateDelegates[(int) this.m_state] != null)
                {
                    this.m_stateDelegates[(int) this.m_state]();
                }
            }
            rdtDebug.Debug(this, "Exited", new object[0]);
        }

        public bool IsConnected
        {
            get
            {
                return (this.m_state != State.LostConnection);
            }
        }

        private enum State
        {
            Reading,
            LostConnection,
            Max
        }
    }
}

