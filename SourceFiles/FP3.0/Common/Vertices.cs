using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

//[Serializable, DebuggerDisplay("Count = {Count}")]
public class Vertices
{
    private static Vector2[] _emptyArray;
    private Vector2[] _items;

    static Vertices()
    {
        _emptyArray = new Vector2[0];
    }

    public Vertices()
    {
        _items = _emptyArray;
    }

    public Vertices(Vertices collection)
    {
        int count = collection.Count;
        _items = new Vector2[count];

        collection._items.CopyTo(_items, 0);

        Count = count;
    }

    public Vertices(int capacity)
    {
        _items = new Vector2[capacity];
    }

    public int Capacity
    {
        get
        {
            return _items.Length;
        }
        set
        {
            if (value != _items.Length)
            {
                if (value > 0)
                {
                    Vector2[] destinationArray = new Vector2[value];
                    if (Count > 0)
                    {
                        Array.Copy(_items, 0, destinationArray, 0, Count);
                    }
                    _items = destinationArray;
                }
                else
                {
                    _items = _emptyArray;
                }
            }
        }
    }

    public int Count { get; private set; }

    public Vector2 this[int index]
    {
        get
        {
            return _items[index];
        }
        set
        {
            _items[index] = value;
        }
    }

    public void Add(Vector2 item)
    {
        if (Count == _items.Length)
        {
            EnsureCapacity(Count + 1);
        }
        _items[Count++] = item;
    }

    public void InsertRange(int index, Vertices collection)
    {
        Debug.Assert(index < Count);

        int count = collection.Count;
        if (count > 0)
        {
            EnsureCapacity(Count + count);
            if (index < Count)
            {
                Array.Copy(_items, index, _items, index + count, Count - index);
            }
            if (this == collection)
            {
                Array.Copy(_items, 0, _items, index, index);
                Array.Copy(_items, (int)(index + count), _items, (int)(index * 2), (int)(Count - index));
            }
            else
            {
                Vector2[] array = new Vector2[count];
                Array.Copy(collection._items, 0, array, 0, Count);
                array.CopyTo(_items, index);
            }
            Count += count;
        }
    }

    public bool Contains(Vector2 item)
    {
        EqualityComparer<Vector2> comparer = EqualityComparer<Vector2>.Default;
        for (int i = 0; i < Count; i++)
        {
            if (comparer.Equals(_items[i], item))
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveRange(int index, int count)
    {
        if ((index < 0) || (count < 0))
        {
            //ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }
        if ((Count - index) < count)
        {
            //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
        }
        if (count > 0)
        {
            Count -= count;
            if (index < Count)
            {
                Array.Copy(_items, index + count, _items, index, Count - index);
            }
            Array.Clear(_items, Count, count);
        }
    }

    public int IndexOf(Vector2 item)
    {
        return Array.IndexOf(_items, item, 0, Count);
    }

    public void AddRange(Vertices collection)
    {
        InsertRange(Count, collection);
    }

    public void Insert(int index, Vector2 item)
    {
        Debug.Assert(index < Count);

        if (Count == _items.Length)
        {
            EnsureCapacity(Count + 1);
        }
        if (index < Count)
        {
            Array.Copy(_items, index, _items, index + 1, Count - index);
        }
        _items[index] = item;
        Count++;
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int num = (_items.Length == 0) ? 4 : (_items.Length * 2);
            if (num < min)
            {
                num = min;
            }
            Capacity = num;
        }
    }

    public void Clear()
    {
        if (Count > 0)
        {
            Array.Clear(_items, 0, Count);
            Count = 0;
        }
    }
}