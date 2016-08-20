using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pegelalarm.Core.Utils
{
    public static class DateExtensions
    {

        public static DateTimeOffset DateFromString(this string date)
        {
            //01.09.2014T20:39:13%2B0200
            var reg = new Regex("([0-9]{2}).([0-9]{2}).([0-9]{4})T([0-9]{2}):([0-9]{2}):([0-9]{2})([+-][0-9]{2})([0-9]{2})");
            var match = reg.Match(date);
            if (!match.Success) return DateTime.MinValue;

            var day = int.Parse(match.Groups[1].Value);
            var month = int.Parse(match.Groups[2].Value);
            var year = int.Parse(match.Groups[3].Value);
            var hour = int.Parse(match.Groups[4].Value);
            var minute = int.Parse(match.Groups[5].Value);
            var second = int.Parse(match.Groups[6].Value);
            var tzHours = int.Parse(match.Groups[7].Value);
            var tzMinutes = int.Parse(match.Groups[8].Value);

            var offset = TimeSpan.FromHours(tzHours) + TimeSpan.FromMinutes(tzMinutes);

            var dto = new DateTimeOffset(year, month, day, hour, minute, second, offset);
            return dto;
        }

    }
}
