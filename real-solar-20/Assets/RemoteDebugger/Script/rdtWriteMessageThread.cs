namespace Hdg
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    public class rdtWriteMessageThread
    {
        private byte[] m_currentMessage;
        private AutoResetEvent m_event = new AutoResetEvent(false);
        private Queue<byte[]> m_messageQueue = new Queue<byte[]>();
        private string m_name;
        private bool m_run;
        private State m_state;
        private Action[] m_stateDelegates;
        private NetworkStream m_stream;
        private Thread m_thread;
        private BinaryWriter m_writer;

        public rdtWriteMessageThread(NetworkStream stream, string name)
        {
            this.m_name = name;
            this.m_stateDelegates = new Action[] { new Action(this.OnIdle), new Action(this.OnWriting), new Action(this.OnLostConnection) };
            this.m_stream = stream;
            this.m_writer = new BinaryWriter(this.m_stream);
            this.m_run = true;
            this.m_thread = new Thread(new ThreadStart(this.ThreadFunc));
            this.m_thread.Name = this.m_name + " rdtWriteMessageThread";
            this.m_thread.Start();
        }

        public void EnqueueMessage(rdtTcpMessage message)
        {
            lock (this.m_messageQueue)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, message);
                    byte[] item = stream.ToArray();
                    this.m_messageQueue.Enqueue(item);
                }
            }
            this.m_event.Set();
        }

        private void OnIdle()
        {
            this.m_currentMessage = null;
            lock (this.m_messageQueue)
            {
                if (this.m_messageQueue.Count > 0)
                {
                    this.m_currentMessage = this.m_messageQueue.Dequeue();
                }
            }
            if (this.m_currentMessage != null)
            {
                this.m_state = State.Writing;
            }
            else
            {
                this.m_event.WaitOne();
            }
        }

        private void OnLostConnection()
        {
            this.m_run = false;
        }

        private void OnWriting()
        {
            try
            {
                this.m_state = State.Idle;
                this.m_writer.Write(this.m_currentMessage.Length);
                this.m_writer.Write(this.m_currentMessage);
            }
            catch (IOException exception)
            {
                rdtDebug.Log(this, exception, rdtDebug.LogLevel.Debug, "{0} lost connection", new object[] { this.m_name });
                this.m_state = State.LostConnection;
            }
            catch (ObjectDisposedException)
            {
                rdtDebug.Debug(this, "{0} object disposed, lost connection", new object[] { this.m_name });
                this.m_state = State.LostConnection;
            }
            catch (Exception exception2)
            {
                rdtDebug.Error(this, exception2, "{0} Unknown exception", new object[] { this.m_name });
            }
        }

        public void Stop()
        {
            this.m_run = false;
            this.m_event.Set();
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
            Idle,
            Writing,
            LostConnection,
            Max
        }
    }
}

