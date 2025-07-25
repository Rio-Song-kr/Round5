using System.Collections.Generic;
using UnityEngine;

public class WeightedRandom<T>
{
    private Dictionary<T, int> _dic;

    public WeightedRandom()
    {
        _dic = new Dictionary<T, int>();
    }

    /// <summary>
    /// Add Item to list due to weight.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="value"></param>
    public void Add(T item, int value)
    {
        if (value < 0)
        {
            Debug.LogError("Value Under 0 can't be added");
            return;
        }

        if (_dic.ContainsKey(item))
        {
            _dic[item] += value;
        }
        else
        {
            _dic.Add(item, value);
        }
    }

    /// <summary>
    /// Substract item from list due to weight.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="value"></param>
    public void Sub(T item, int value)
    {
        if (value < 0)
        {
            Debug.LogError("Value under 0 can't be substracted");
            return;
        }

        if (_dic.ContainsKey(item))
        {
            if (_dic[item] > value)
            {
                _dic[item] -= value;                
            }
            else
            {
                Remove(item);
            }
        }
    }

    /// <summary>
    /// Remove the item in the list.
    /// </summary>
    /// <param name="item"></param>
    public void Remove(T item)
    {
        if (_dic.ContainsKey(item))
        {
            _dic.Remove(item);
        }
        else
        {
            Debug.LogError($"{item} is not exist");
        }
    }

    /// <summary>
    /// Get total weight of list.
    /// </summary>
    /// <returns></returns>
    public int GetTotalWeight()
    {
        int totalWeight = 0;

        foreach (int value in _dic.Values)
        {
            totalWeight += value;
        }

        return totalWeight;
    }

    /// <summary>
    /// Get parcentage of each items.
    /// </summary>
    /// <returns></returns>
    public Dictionary<T, float> GetPercent()
    {
        Dictionary<T, float> _percentDic = new Dictionary<T, float>();
        float totalWeight = GetTotalWeight();

        foreach (var item in _dic)
        {
            _percentDic.Add(item.Key, item.Value / totalWeight);
        }

        return _percentDic;
    }

    /// <summary>
    /// Get random item and substrate.
    /// (if you picked certain item, then the item percentage of list decreases.)
    /// </summary>
    /// <returns></returns>
    public T GetRandomItemBySub()
    {
        if (_dic.Count <= 0)
        {
            Debug.LogError("There's no item in list.");
            return default;
        }

        int weight = 0;
        int totalWeight = GetTotalWeight();

        int pivot = Mathf.RoundToInt(totalWeight * Random.Range(0.0f, 1.0f));

        foreach (var item in _dic)
        {
            weight += item.Value;
            if (pivot <= weight)
            {
                _dic[item.Key] -= 1;

                if(_dic[item.Key] == 0)
                {
                    _dic.Remove(item.Key);
                }

                return item.Key;
            }
        }
        return default;
    }

    /// <summary>
    /// Get random item.
    /// No percentage change occurs.
    /// </summary>
    /// <returns></returns>
    public T GetRandomItem()
    {
        if (_dic.Count <= 0)
        {
            Debug.LogError("There's no item in list.");
            return default;
        }

        int weight = 0;
        int totalWeight = GetTotalWeight();

        int pivot = Mathf.RoundToInt(totalWeight * Random.Range(0.0f, 1.0f));

        foreach (var item in _dic)
        {
            weight += item.Value;
            if (pivot <= weight)
            {
                return item.Key;
            }
        }
        return default;
    }

    /// <summary>
    /// Get all list of dictionary.
    /// </summary>
    /// <returns></returns>
    public Dictionary<T, int> GetList()
    {
        return _dic;
    }
}