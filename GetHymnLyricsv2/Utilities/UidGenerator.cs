using System;

namespace GetHymnLyricsv2.Utilities
{
    public static class UidGenerator
    {
        private static readonly int Size = 256;
        private static readonly string[] Hex = new string[Size];
        private static string? Buffer;
        private static int Idx;

        static UidGenerator()
        {
            for (int i = 0; i < Size; i++)
            {
                Hex[i] = (i + 256).ToString("x2");
            }
        }

        public static string Generate(int? length = 11)
        {
            int len = length ?? 11;
            
            if (Buffer == null || ((Idx + len) > Size * 2))
            {
                var random = new Random();
                Buffer = string.Empty;
                Idx = 0;
                
                for (int i = 0; i < Size; i++)
                {
                    Buffer += Hex[random.Next(256)];
                }
            }

            string result = Buffer.Substring(Idx, len);
            Idx += len;
            return result;
        }
    }
}
