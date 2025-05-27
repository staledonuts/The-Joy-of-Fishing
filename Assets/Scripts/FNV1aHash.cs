using System.Text;

public static class FNV1aHash
{
    private const uint FNV_PRIME_32 = 0x01000193; // 16777619
    private const uint FNV_OFFSET_BASIS_32 = 0x811c9dc5; // 2166136261

    /// <summary>
    /// Calculates a 32-bit FNV1a hash for the given string.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <returns>A 32-bit unsigned integer hash.</returns>
    public static uint Calculate(string text)
    {
        if (text == null)
        {
            return 0; // Or throw ArgumentNullException, depending on desired behavior
        }

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        uint hash = FNV_OFFSET_BASIS_32;

        for (int i = 0; i < bytes.Length; i++)
        {
            hash ^= bytes[i];
            hash *= FNV_PRIME_32;
        }
        return hash;
    }

    /// <summary>
    /// Calculates a 32-bit FNV1a hash for the given string, case-insensitively.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <returns>A 32-bit unsigned integer hash.</returns>
    public static uint CalculateCaseInsensitive(string text)
    {
        if (text == null)
        {
            return 0;
        }
        // Consider using ToLowerInvariant() for consistent casing across cultures
        return Calculate(text.ToLower()); 
    }
}
