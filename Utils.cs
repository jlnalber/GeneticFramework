using System;
using System.Collections.Generic;

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

            double sum = Sum<T> (arr);

            for (int i = 0; i < newVals.Length; i++)
            {
                newVals[i] = arr[i].Item2 / sum;
            }

            Random random = new Random();
            double rn = random.NextDouble();

            int index = 0;
            for (double temp = newVals[0]; temp <= rn; index++, temp += newVals[index]);

            return arr[index] switch { (T t, _) => t }; 
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

        public static T[] NElements<T>(T[] arr, int n)
        {
            if (n > arr.Length) throw new IndexOutOfRangeException();

            List<T> elements = new();
            Random rnd = new();
            for (int i = 0; i < n; i++)
            {
                T cur = arr[rnd.Next(arr.Length)];
                elements.Add(cur);
                arr = Filter(arr, (T t) => !t.Equals(cur));
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

        public static T[] Filter<T>(T[] arr, Func<T, bool> func)
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

        public static T GetBest<T>(T[] arr, Func<T, double> func)
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

        public static int IndexOf<T>(T[] arr, T element)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(element)) return i;
            }
            return -1;
        }

        public static T[] AddToArray<T>(T[] arr, T element)
        {
            T[] newArr = new T[arr.Length + 1];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            newArr[arr.Length] = element;
            return newArr;
        }

        public static T[] RemoveFromArray<T>(T[] arr, T element)
        {
            int index = IndexOf(arr, element);
            if (index == -1)
            {
                throw new IndexOutOfRangeException();
            }
            return RemoveFromArray(arr, index);
        }

        public static T[] RemoveFromArray<T>(T[] arr, int position)
        {
            if (0 > position || position >= arr.Length)
            {
                throw new IndexOutOfRangeException();
            }

            T[] newArr = new T[arr.Length - 1];
            for (int i = 0; i < newArr.Length; i++)
            {
                if (i < position)
                {
                    newArr[i] = arr[i];
                }
                else if (i > position)
                {
                    newArr[i] = arr[i + 1];
                }
            }
            return newArr;
        }
    }
}
