using System;
using System.Linq;

namespace Smartline.Server.Runtime {
    public static class Extensions {
        private const int Empty = -1;

        public static int IndexOf(this byte[] input, byte[] pattern) {
            byte firstByte = pattern[0];
            int index = -1;

            if ((index = Array.IndexOf(input, firstByte)) >= 0) {
                for (int i = 0; i < pattern.Length; i++) {
                    if (index + i >= input.Length ||
                     pattern[i] != input[index + i])
                        return -1;
                }
            }

            return index;
        }

        public static int Locate(this byte[] self, byte[] candidate, int offset) {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            for (int i = offset; i < self.Length; i++) {
                if (IsMatch(self, i, candidate) && !IsMatch(self, i + 1, candidate))
                    return i;
            }

            return Empty;
        }

        public static int Locate(this byte[] self, byte[] candidate) {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            for (int i = 0; i < self.Length; i++) {
                if (IsMatch(self, i, candidate) && !IsMatch(self, i + 1, candidate))
                    return i;
            }

            return Empty;
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate) {
            if (candidate.Length > (array.Length - position))
                return false;

            return !candidate.Where((t, i) => array[position + i] != t).Any();
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate) {
            return array == null
                    || candidate == null
                    || array.Length == 0
                    || candidate.Length == 0
                    || candidate.Length > array.Length;
        }
    }
}
