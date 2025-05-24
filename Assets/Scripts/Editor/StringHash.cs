public static class StringHash
{
    public static uint FNV1aHash(this string input)
    {
        const uint fnvOffset = 2166136261;
        const uint fnvPrime = 16777619;

        uint hash = fnvOffset;
        foreach (char c in input)
        {
            hash ^= c;
            hash *= fnvPrime;
        }
        return hash;
    }
}