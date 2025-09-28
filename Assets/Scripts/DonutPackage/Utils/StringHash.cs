using System.Text;

namespace DonutPackage.Utils
{
    public static class StringHash
    {
        private const uint FNV_PRIME_32 = 0x01000193; // 16777619
        private const uint FNV_OFFSET_BASIS_32 = 0x811c9dc5; // 2166136261

        public static uint Hash(this string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            uint hash = FNV_OFFSET_BASIS_32;
            foreach (char c in input)
            {
                hash ^= c;
                hash *= FNV_PRIME_32;
            }
            return hash;
        }
    }
}