using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Memory.Timers
{
    public class TimerBase : IDisposable
    {
        protected readonly ICollection<TimerBase> _child = new List<TimerBase>();
        protected readonly int _level;
        protected readonly string _name;
        protected readonly Stopwatch _sw = new Stopwatch();

        public TimerBase(string name, int level)
        {
            _name = name;
            _level = level;

            _sw.Start();
        }

        public virtual void Dispose()
        {
            _sw.Stop();
        }

        public TimerBase StartChildTimer(string name)
        {
            var child = new TimerBase(name, _level + 1);

            _child.Add(child);

            return child;
        }

        protected void WriteReport(StringWriter writer)
        {
            var reportLine = TimerUtil.FormatReportLine(_name, _level, _sw.ElapsedMilliseconds);
            writer.Write(reportLine);

            foreach (var child in _child) child.WriteReport(writer);

            WriteRestTime(writer);
        }

        private void WriteRestTime(StringWriter writer)
        {
            if (!_child.Any()) return;

            var restTime = GetRestTime();
            var reportLine = TimerUtil.FormatReportLine("Rest", _level + 1, restTime);

            writer.Write(reportLine);
        }

        private long GetRestTime()
        {
            var childTotalTime = _child.Sum(c => c._sw.ElapsedMilliseconds);

            return _sw.ElapsedMilliseconds - childTotalTime;
        }
    }

    public class Timer : TimerBase
    {
        private readonly StringWriter _writer;

        private Timer(StringWriter writer, string name) : base(name, 0)
        {
            _writer = writer;
        }

        public override void Dispose()
        {
            base.Dispose();

            WriteReport(_writer);

            _writer.Dispose();
        }


        public static Timer Start(StringWriter writer, string name = "*")
        {
            return new Timer(writer, name);
        }
    }

    internal class TimerUtil
    {
        // Use this method in your solution to fit report formatting requirements from the tests
        internal static string FormatReportLine(string timerName, int level, long value)
        {
            var intro = new string(' ', level * 4) + timerName;
            return $"{intro,-20}: {value}\n";
        }
    }
}