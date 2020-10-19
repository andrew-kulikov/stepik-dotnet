using System;
using System.Collections.Generic;
using System.Text;

namespace Delegates.Observers
{
    public class StackOperationsLogger
    {
        private readonly StringBuilder _log = new StringBuilder();

        public void SubscribeOn<T>(ObservableStack<T> stack)
        {
            stack.ItemChanged += data => _log.Append(data);
        }

        public string GetLog()
        {
            return _log.ToString();
        }
    }

    public class ObservableStack<TItem>
    {
        private readonly IList<TItem> data = new List<TItem>();

        public event Action<StackEventData<TItem>> ItemChanged;

        public void Push(TItem obj)
        {
            data.Add(obj);
            ItemChanged?.Invoke(new StackEventData<TItem> {IsPushed = true, Value = obj});
        }

        public TItem Pop()
        {
            if (data.Count == 0) throw new InvalidOperationException();

            var result = data[data.Count - 1];
            ItemChanged?.Invoke(new StackEventData<TItem> {IsPushed = false, Value = result});

            return result;
        }
    }
}