namespace Hdg
{
    using System;
    using UnityEngine;

    internal class rdtProfiler : IDisposable
    {
        private string m_description;
        private DateTime m_start = DateTime.Now;

        public rdtProfiler(string desc)
        {
            this.m_description = desc;
        }

        public void Dispose()
        {
            TimeSpan span = (TimeSpan) (DateTime.Now - this.m_start);
            Debug.Log(string.Format("{0} took {1}s", this.m_description, span.TotalSeconds));
        }
    }
}

