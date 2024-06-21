namespace Poen.Services
{
    public static class DecimalHelpers
    {

        public static string ToStringSI(this decimal value, int decimals = 2)
        {
            string[] siSuffix = {"p", "n", "µ", "m", " ", "k", "M", "G", "T"};

            int shift = Array.IndexOf(siSuffix, " ");
            while (Math.Abs(value) >= 1000m && shift < siSuffix.Length - 1)
            {
                value /= 1000;
                shift++;
            }

            while (Math.Abs(value) < 1m && shift > 0)
            {
                value *= 1000;
                shift--;
            }


            string format = $"N{decimals}";
            string suffix = siSuffix[shift];
            return $"{value.ToString(format)}{suffix}".Trim();
        }
    }
}





