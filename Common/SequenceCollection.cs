using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
    public class SequenceCollection<T> : IEnumerable<T>
    {
        public class SeqItem
        {
            public SeqItem Prev { get; set; }
            public SeqItem Next { get; set; }
            public T Item { get; set; }

            public override string ToString()
            {
                return Item.ToString();
            }
        }

        private SeqItem _first;
        private SeqItem _last;


        public T GetFirst()
        {
            if (_first== null)
                throw new Exception("Collection is empty");

            return _first.Item;
        }

        public T GetLast()
        {
            if (_last == null)
                throw new Exception("Collection is empty");

            return _last.Item;
        }

        private IEnumerable<T> EnumerateAll()
        {

            if (_first == null)
                yield break;

            var current = _first;

            while (current != null)
            {
                yield return current.Item;
                current = current.Next;
            }

        }

        public int Count { get; private set; }

        public void Add(T item)
        {
            var newItem = new SeqItem
            {
                Prev = _last,
                Item = item
            };

            if (_first == null)
                _first = newItem;

            if (_last != null)
                _last.Next = newItem;

            _last = newItem;

            Count++;
        }

        private SeqItem FindSeqItem(T item)
        {
            var current = _first;

            while (current != null)
            {
                if (typeof (T) == typeof (string))
                {
                    if (item.ToString() == current.Item.ToString())
                        return current;
                }
                else
                {
                    if (current.Item.GetHashCode() == item.GetHashCode())
                        if (current.Item.Equals(item))
                            return current;
                }

                current = current.Next;
            }

            return null;
        }




        private void Remove(SeqItem seqItem)
        {
            Count--;

            if (Count == 0)
            {
                _first = null;
                _last = null;
                return;
            }

            if (seqItem == _first)
            {
                _first = _first.Next;
                _first.Prev = null;
                return;
            }

            if (seqItem == _last)
            {
                _last = _last.Prev;
                _last.Next = null;
                return;
            }

            seqItem.Prev.Next = seqItem.Next;
            seqItem.Next.Prev = seqItem.Prev;

        }

        public void Remove(T item)
        {
            var seqItem = FindSeqItem(item);
            Remove(seqItem);
        }


        public IEnumerable<T> EnumerateAndDelete(Func<T, bool> predicate = null)
        {

            var current = _first;

            while (current != null)
            {
                var toYeld = current.Item;
                var next = current.Next;

                if (predicate == null || predicate(toYeld))
                {
                    Remove(current);
                    yield return toYeld;
                }

                current = next;
            }
        }



        public IEnumerator<T> GetEnumerator()
        {
            return EnumerateAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
