using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtils {
    public static List<T> Combine<T>(params List<T>[] lists) {
        var combined = new List<T>();
        foreach (var list in lists) {
            combined.AddRange(list);
        }

        return combined;
    }
    
    public static int CountAllowNull<T>(this IList<T> list) {
            if (list != null)
                return list.Count;
            return 0;
        }

        public static bool NullOrEmpty<T>(this IList<T> list) {
            if (list != null)
                return list.Count == 0;
            return true;
        }

        public static List<T> ListFullCopy<T>(this List<T> source) {
            List<T> objList = new List<T>(source.Count);
            for (int index = 0; index < source.Count; ++index)
                objList.Add(source[index]);
            return objList;
        }

        public static List<T> ListFullCopyOrNull<T>(this List<T> source) {
            if (source == null)
                return null;
            return source.ListFullCopy();
        }

        public static void RemoveDuplicates<T>(this List<T> list) where T : class {
            if (list.Count <= 1)
                return;
            for (int index1 = list.Count - 1; index1 >= 0; --index1) {
                for (int index2 = 0; index2 < index1; ++index2) {
                    if (list[index1] == list[index2]) {
                        list.RemoveAt(index1);
                        break;
                    }
                }
            }
        }

        public static void Shuffle<T>(this IList<T> list) {
            int count = list.Count;
            while (count > 1) {
                --count;
                int index = Rand.RangeInclusive(0, count);
                T obj = list[index];
                list[index] = list[count];
                list[count] = obj;
            }
        }

        public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison) {
            int count = list.Count;
            for (int index1 = 1; index1 < count; ++index1) {
                T y = list[index1];
                int index2;
                for (index2 = index1 - 1; index2 >= 0 && comparison(list[index2], y) > 0; --index2)
                    list[index2 + 1] = list[index2];
                list[index2 + 1] = y;
            }
        }
}