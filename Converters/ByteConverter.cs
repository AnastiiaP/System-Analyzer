namespace SystemAnalyzer.Converters
{
    public class ByteConverter
    {
        public static double BytesToMegabytes(long bytes)
        {
            return Convert.ToDouble(bytes) / (1024 * 1024);
        }

        public static long MegabytesToBytes(double megabytes)
        {
            return Convert.ToInt64(megabytes * 1024 * 1024);
        }
    }
}
