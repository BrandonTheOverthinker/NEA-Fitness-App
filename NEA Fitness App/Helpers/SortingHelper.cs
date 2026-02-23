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
    }
}