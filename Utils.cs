using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticFramework
{
    public static class Utils
    {
        public static double Sum(double[] list)
        {
            double sum = 0;
            foreach (double i in list)
            {
                sum += i;
            }
            return sum;
        }

        public static double Sum<T>((T, double)[] arr)
        {
            double sum = 0;
            foreach ((T, double) i in arr)
            {
                sum += i.Item2;
            }

            return sum;
        }

        public static double[] CalculateRatios(double[] arr)
        {
            double sum = Sum(arr);

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i] / sum;
            }

            return arr;
        }

        public static T Choices<T>((T, double)[] arr)
        {
            if (arr.Length == 0) throw new IndexOutOfRangeException();

            double[] newVals = new double[arr.Length];

            double sum = Sum<T>(arr);

            for (int i = 0; i < newVals.Length; i++)
            {
                newVals[i] = arr[i].Item2 / (double)sum;
            }

            Random random = new Random();
            double rn = random.NextDouble();

            int index = 0;
            for (double temp = newVals[0]; temp <= rn; index++, temp += newVals[index]) ;

            return arr[index].Item1;
        }

        public static T[] Choices<T>((T, double)[] arr, int n)
        {
            if (n > arr.Length) throw new IndexOutOfRangeException();

            T[] elements = new T[n];
            for (int i = 0; i < n; i++)
            {
                T cur = Choices(arr);
                elements[i] = cur;
                arr = Filter(arr, ((T, double) i) => (!i.Item1.Equals(cur)));
            }
            return elements;
        }

        public static T[] NElements<T>(this T[] arr, int n)
        {
            if (n > arr.Length) throw new IndexOutOfRangeException();

            HashSet<T> elements = new();
            Random rnd = new();
            for (int i = 0; i < n; i++)
            {
                int pos = rnd.Next(arr.Length);
                for (; elements.Contains(arr[pos % arr.Length]); pos++) ;
                elements.Add(arr[pos % arr.Length]);
            }

            return elements.ToArray();
        }

        public static T[] NLargest<T>(T[] arr, int n, Func<T, double> func)
        {
            double[] helper = new double[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                helper[i] = func(arr[i]);
            }
            Array.Sort(helper, arr);

            T[] finalArr = new T[n];
            for (int i = 0; i < n; i++)
            {
                finalArr[i] = arr[arr.Length - i - 1];
            }

            return finalArr;
        }

        public static T[] Filter<T>(this T[] arr, Func<T, bool> func)
        {
            List<T> list = new();
            foreach (T i in arr)
            {
                if (func(i))
                {
                    list.Add(i);
                }
            }
            return list.ToArray();
        }

        public static T GetBest<T>(this T[] arr, Func<T, double> func)
        {
            T best = arr[0];
            double bestScore = func(best);

            for (int i = 1; i < arr.Length; i++)
            {
                double score = func(arr[i]);
                if (score > bestScore)
                {
                    best = arr[i];
                    bestScore = score;
                }
            }

            return best;
        }

        public static T GetBest<T>(this T[] arr, Func<T, double> func, Func<T, bool> cond)
        {
            T[] newArr = Filter(arr, cond);
            if (newArr.Length == 0)
            {
                return GetBest(arr, func);
            }
            else
            {
                return GetBest(newArr, func);
            }
        }

        public static T[] GetBest<T>(this T[] arr, Func<T, double> func, Func<T, bool> cond, int n)
        {
            T[] newArr = new T[arr.Length];
            arr.CopyTo(newArr, 0);
            newArr.Sort(func);
            newArr.Sort((T t) => cond(t) ? 1 : 0);
            return newArr.GetRange(newArr.Length - n, newArr.Length);
        }

        public static void Insert<T>(this T[] arr, T element, int index)
        {
            T toInsert = element;
            for (int i = index; i >= 0; i--)
            {
                T temp = arr[i];
                arr[i] = toInsert;
                toInsert = temp;
            }
        }

        public static void Mix<T>(this T[] arr, T[] otherArr, Func<T, T, bool> func)
        {
            foreach (T i in otherArr)
            {
                int index = -1;
                for (; index + 1 < arr.Length && func(i, arr[index + 1]); index++) ;
                if (index != -1)
                {
                    arr.Insert(i, index);
                }
            }
        }

        public static T[] GetRange<T>(this T[] arr, int start, int end)
        {
            T[] newArr = new T[end - start];
            for (int i = 0; i < end - start; i++)
            {
                newArr[i] = arr[i + start];
            }
            return newArr;
        }

        public static int IndexOf<T>(this T[] arr, T element)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(element))
                {
                    return i;
                }
            }
            return -1;
        }

        public static T[] AddToArray<T>(this T[] arr, T element)
        {
            T[] newArr = new T[arr.Length + 1];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            newArr[arr.Length] = element;
            return newArr;
        }

        public static T[] AddToArray<T>(this T[] arr, T[] ts)
        {
            T[] newArr = new T[arr.Length + ts.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            for (int i = 0; i < ts.Length; i++)
            {
                newArr[arr.Length + i] = ts[i];
            }
            return newArr;
        }

        public static T[] RemoveFromArray<T>(this T[] arr, T element)
        {
            bool wasThere = false;
            T[] newArr = new T[arr.Length - 1];
            for (int i = 0; i < arr.Length; i++)
            {
                if (wasThere)
                {
                    newArr[i - 1] = arr[i];
                }
                else
                {
                    if (arr[i].Equals(element))
                    {
                        wasThere = true;
                    }
                    else
                    {
                        newArr[i] = arr[i];
                    }
                }
            }
            return newArr;
        }

        public static T[] RemoveFromArray<T>(this T[] arr, int position)
        {
            if (0 > position || position >= arr.Length)
            {
                throw new IndexOutOfRangeException();
            }

            T[] newArr = new T[arr.Length - 1];
            for (int i = 0; i < arr.Length; i++)
            {
                if (i < position)
                {
                    newArr[i] = arr[i];
                }
                else if (i > position)
                {
                    newArr[i - 1] = arr[i];
                }
            }
            return newArr;
        }

        public static T[] Modify<T>(this T[] arr, Func<T, T> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = func(arr[i]);
            }
            return arr;
        }

        public static int Sum<T>(IEnumerable<T> arr, Func<T, int> func)
        {
            int sum = 0;
            foreach (T i in arr)
            {
                sum += func(i);
            }
            return sum;
        }

        public static double Sum<T>(IEnumerable<T> arr, Func<T, double> func)
        {
            double sum = 0.0;
            foreach (T i in arr)
            {
                sum += func(i);
            }
            return sum;
        }

        public static int Average(IEnumerable<int> arr)
        {
            int count = 0;
            int sum = 0;
            foreach (int i in arr)
            {
                sum += i;
                count++;
            }
            if (count == 0)
            {
                return 0;
            }
            return sum / count;
        }

        public static double Average(IEnumerable<double> arr)
        {
            int count = 0;
            double sum = 0.0;
            foreach (double i in arr)
            {
                sum += i;
                count++;
            }
            if (count == 0)
            {
                return 0.0;
            }
            return sum / count;
        }

        public static double Average<T>(IEnumerable<T> arr, Func<T, double> func)
        {
            int count = 0;
            double sum = 0.0;
            foreach (T i in arr)
            {
                sum += func(i);
                count++;
            }
            if (count == 0)
            {
                return 0.0;
            }
            return sum / count;
        }

        public static bool Contains<T>(this T[] arr, Func<T, bool> func)
        {
            foreach (T i in arr)
            {
                if (func(i))
                {
                    return true;
                }
            }
            return false;
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> arr, T defaultVal)
        {
            if (arr.Count() == 0)
            {
                return defaultVal;
            }
            return arr.First();
        }

        public static T2[] Transform<T1, T2>(this T1[] arr, Func<T1, T2> func)
        {
            T2[] newArr = new T2[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = func(arr[i]);
            }
            return newArr;
        }

        public static (T, double)[] Sort<T>((T, double)[] arr)
        {
            (T, double)[] newArr = arr.Transform(((T, double) tupel) => tupel);
            for (int i = 0; i < newArr.Length; i++)
            {
                for (int j = 0; j < newArr.Length - i - 1; j++)
                {
                    if (newArr[j].Item2 > newArr[j + 1].Item2)
                    {
                        (T, double) temp = newArr[j];
                        newArr[j] = newArr[j + 1];
                        newArr[j + 1] = temp;
                    }
                }
            }

            return newArr;
        }

        public static T[] Sort<T>(this T[] arr, Func<T, double> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr.Length - i - 1; j++)
                {
                    if (func(arr[j]) > func(arr[j + 1]))
                    {
                        T temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }

            return arr;
        }

        public static T[] Sort<T>(this T[] arr, Func<T, int> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                for (int j = 0; j < arr.Length - i - 1; j++)
                {
                    if (func(arr[j]) > func(arr[j + 1]))
                    {
                        T temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }

            return arr;
        }

        public static T[] Shuffle<T>(this T[] arr)
        {
            Random random = new();

            (T, double)[] newArr = (from i in arr select (i, random.NextDouble())).ToArray();

            return Sort(newArr).Transform(((T, double) tupel) => tupel.Item1);
        }

        public static (T1, T2)[] ShuffleTupels<T1, T2>((T1, T2)[] arr)
        {
            T2[] t2s = (from i in arr select i.Item2).ToArray();
            T2[] t2sShuffled = t2s.Shuffle();

            (T1, T2)[] newArr = new (T1, T2)[arr.Length];
            for (int i = 0; i < newArr.Length; i++)
            {
                newArr[i] = (arr[i].Item1, t2sShuffled[i]);
            }

            return newArr;
        }

        public static T[] PickRoulette<T>((T, double)[] wheel, int n)
        {
            return Utils.Choices(wheel, n);
        }

        public static T PickRoulette<T>((T, double)[] wheel)
        {
            return Utils.Choices(wheel);
        }

        public static T[] PickTournament<T>((T, double)[] tupels, int participants, int n)
        {
            return Utils.NLargest(Utils.NElements(tupels, participants), n, ((T, double) i) => i.Item2).Transform(((T, double) t) => t.Item1);
        }

        public static T PickTournament<T>((T, double)[] tupels, int participants)
        {
            return Utils.NLargest(Utils.NElements(tupels, participants), 1, ((T, double) i) => i.Item2)[0].Item1;
        }

        public static T PickTournament<T>((T, double)[] tupels)
        {
            return Utils.NElements(tupels, (tupels.Length + 1) / 2).Largest();
        }

        public static T Largest<T>(this (T, double)[] arr)
        {
            double biggest = arr[0].Item2;
            T largest = arr[0].Item1;
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].Item2 > biggest)
                {
                    biggest = arr[i].Item2;
                    largest = arr[i].Item1;
                }
            }
            return largest;
        }
    }
}
