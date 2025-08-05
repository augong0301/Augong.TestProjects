using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace WPReader
{
	public static class WPFileLoader
    {
        private static readonly Regex RowCountRx = new Regex(@"^RowCount=(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex ColCountRx = new Regex(@"^ColCount=(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RowRx = new Regex(@"^Row\d+=(\d+\.?\d*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex ColRx = new Regex(@"^Col\d+=(\d+\.?\d*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex DiemeterRx = new Regex(@"^Diemeter=(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex DieLimitRx = new Regex(@"^DieLimit=(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex DieRx = new Regex(@"^Die(\d{8})=(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex VersionRx = new Regex(@"^Version=(\d+\.?\d*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex TypeRx = new Regex(@"^Type=(\w+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex EndTimeRx = new Regex(@"^Date=(\d{4}-\d{1,2}-\d{1,2}\s+\d{1,2}:\d{2}:\d{2})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static WaferPatternInfo LoadFromLocal(string originDieMap)
        {
            var tempDieEntries = new List<(int, int, int)>();

            if (string.IsNullOrEmpty(originDieMap) || ! File.Exists(originDieMap))
                return null;

            var wp = new WaferPatternInfo();

            foreach (var line in File.ReadLines(originDieMap, Encoding.UTF8))
            {
                var content = line.TrimEnd();

                var m = DiemeterRx.Match(content);
                if (m.Success)
                {
                    wp.Diameter = int.Parse(m.Groups[1].Value);
                    continue;
                }

                m = DieLimitRx.Match(content);
                if (m.Success)
                {
                    wp.DieLimit = int.Parse(m.Groups[1].Value);
                    continue;
                }

                m = VersionRx.Match(content);
                if (m.Success)
                {
                    wp.Version = m.Groups[1].Value;
                    continue;
                }

                m = TypeRx.Match(content);
                if (m.Success)
                {
                    wp.Type = m.Groups[1].Value;
                    continue;
                }

                m = EndTimeRx.Match(content);
                if (m.Success)
                {
                    wp.EndTime = DateTime.Parse(m.Groups[1].Value);
                    continue;
                }

                m = RowCountRx.Match(content);
                if (m.Success)
                {
                    wp.RowCount = int.Parse(m.Groups[1].Value);
                    continue;
                }

                m = ColCountRx.Match(content);
                if (m.Success)
                {
                    wp.ColCount = int.Parse(m.Groups[1].Value);
                    continue;
                }

                m = RowRx.Match(content);
                if (m.Success)
                {
                    wp.Rows.Add(double.Parse(m.Groups[1].Value));
                    continue;
                }

                m = ColRx.Match(content);
                if (m.Success)
                {
                    wp.Cols.Add(double.Parse(m.Groups[1].Value));
                    continue;
                }

                m = DieRx.Match(content);
                if (m.Success)
                {
                    var dn = m.Groups[1].Value;
                    int r = int.Parse(dn.AsSpan(0, 4));
                    int c = int.Parse(dn.AsSpan(4, 4));
                    int v = int.Parse(m.Groups[2].Value);
                    if (r < 0 || c < 0 || v < 0 || v > 255) 
                    {
                        throw new ArgumentOutOfRangeException($"Invalid die data [{content}]");
                    }
                    tempDieEntries.Add((r, c, v));
                }
            }
            // contains extended col/row of 2
            wp.DieInfos = new int[wp.RowCount, wp.ColCount];
            foreach (var (r, c, v) in tempDieEntries)
            {
                wp.DieInfos[r, c] = v;
            }

            ValidateWaferPatternInfo(wp);

            return wp;
        }

        private static void ValidateWaferPatternInfo(WaferPatternInfo wp)
        {
            if (wp.Diameter > 350)
            {
                throw new ArgumentOutOfRangeException("Diameter over 350mm");
            }

            if (wp.ColCount == 0 || wp.RowCount == 0 || wp.Cols.Count == 0 || wp.Rows.Count == 0 || wp.ColCount != wp.Cols.Count || wp.RowCount != wp.Rows.Count) // dismatch in col/row counts
            {
                throw new ArgumentNullException("ColCount or RowCount is zero");
            }
        }
    }
}
