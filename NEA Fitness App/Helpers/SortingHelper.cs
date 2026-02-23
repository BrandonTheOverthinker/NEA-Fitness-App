using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NEAFitnessApp.Models;

namespace NEAFitnessApp.Helpers
{
    public static class SortingHelper
    {
        // ALPHABETICAL SORT (For the Food Database)
        public static List<FoodItem> MergeSort(List<FoodItem> list)
        {
            // Check if list is sorted:
            if (list.Count <= 1)
                return list;

            // Divide list into two halves:
            int midpoint = list.Count / 2;
            List<FoodItem> left = new List<FoodItem>();
            List<FoodItem> right = new List<FoodItem>();

            for (int i = 0; i < midpoint; i++)
                left.Add(list[i]);
            
            for (int i = midpoint; i < list.Count; i++)
                right.Add(list[i]);

            // Recursively sort both halves:
            left = MergeSort(left);
            right = MergeSort(right);

            // Merge the sorted halves:
            return Merge(left, right);
        }

        private static List<FoodItem> Merge(List<FoodItem> left, List<FoodItem> right)
        {
            List<FoodItem> result = new List<FoodItem>();

            while (left.Count > 0 && right.Count > 0)
            {
                // Alphabetical comparison using FoodName:
                if (string.Compare(left[0].FoodName, right[0].FoodName, StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    result.Add(left[0]);
                    left.RemoveAt(0);
                }
                else
                {
                    result.Add(right[0]);
                    right.RemoveAt(0);
                }
            }

            while (left.Count > 0)
            {
                result.Add(left[0]);
                left.RemoveAt(0);
            }

            while (right.Count > 0)
            {
                result.Add(right[0]);
                right.RemoveAt(0);
            }

            return result;
        }

        // CHRONOLOGICAL SORT (For the Daily Food Logs)
        public static List<FoodLogEntry> MergeSortLogs(List<FoodLogEntry> list)
        {
            if (list.Count <= 1) return list;

            int midpoint = list.Count / 2;
            var left = list.GetRange(0, midpoint);
            var right = list.GetRange(midpoint, list.Count - midpoint);

            return MergeLogs(MergeSortLogs(left), MergeSortLogs(right));
        }

        private static List<FoodLogEntry> MergeLogs(List<FoodLogEntry> left, List<FoodLogEntry> right)
        {
            List<FoodLogEntry> result = new List<FoodLogEntry>();
            while (left.Count > 0 && right.Count > 0)
            {
                // Compare LogTime instead of FoodName
                if (left[0].LogTime <= right[0].LogTime)
                {
                    result.Add(left[0]); left.RemoveAt(0);
                }
                else
                {
                    result.Add(right[0]); right.RemoveAt(0);
                }
            }
            result.AddRange(left); result.AddRange(right);
            return result;
        }
    }
}