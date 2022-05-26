using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TensionFields.Utils
{
    public static class TextFileParser
    {
        public static Field[] Parse(String path)
        {
            int KT, N, M;
            double T;
            double[] R, Z, SI;

            StreamReader stream = new StreamReader(path);
            KT = FindProperty<int>(ref stream, "KT");
            M = FindProperty<int>(ref stream, "M");
            N = FindProperty<int>(ref stream, "N");

            Field[] fields = new Field[KT];

            for (int kt = 0; kt < KT; kt++)
            {
                T = TryFindValue<double>(ref stream);
                stream.ReadLine();
                R = ReadArray(ref stream, N * M);
                stream.ReadLine();
                Z = ReadArray(ref stream, N * M);
                stream.ReadLine();
                SI = ReadArray(ref stream, (N - 1) * (M - 1));
                fields[kt] = new Field(N, M, R, Z, SI);
            }
            return fields;
        }

        private static T FindProperty<T>(ref StreamReader stream, string propertyName)
        {
            string line = stream.ReadLine() ?? "";
            string[] subs = line.Split($"{propertyName}=");
            subs = subs[1].Split(" ");
            return (T)Convert.ChangeType(subs[0], typeof(T), CultureInfo.InvariantCulture);
        }

        private static T TryFindValue<T>(ref StreamReader stream)
        {
            string line = stream.ReadLine() ?? "";
            for (int start = 0; start < line.Length; start++)
            {
                if (Char.IsDigit(line[start]) == false)
                    continue;
                for (int length = line.Length - start; length > 0; length--)
                {
                    try
                    {
                        return (T)Convert.ChangeType(line.Substring(start, length), typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException e)
                    {
                        continue;
                    }
                }
            }
            return (T)Convert.ChangeType(0, typeof(T));
        }

        private static double[] ReadArray(ref StreamReader stream, int count)
        {
            double[] values = new double[count];
            int c = 0;
            while (c < count)
            {
                string line = stream?.ReadLine() ?? "";
                string[] subs = line.Split(" ");
                for (int v = 0; v < subs.Length; v++)
                {
                    if (subs[v] == "")
                        continue;
                    values[c] = double.Parse(subs[v], CultureInfo.InvariantCulture);
                    c++;
                }
            }
            return values;
        }
    }
}
