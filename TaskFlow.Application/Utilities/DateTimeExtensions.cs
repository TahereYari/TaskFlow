using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Utilities
{
    using System.Globalization;

    namespace TaskFlow.Application.Utilities.Extensions
    {
        public static class DateTimeExtensions
        {
            private static readonly PersianCalendar PersianCalendar = new();

            /// <summary>
            /// تبدیل تاریخ شمسی به DateTime میلادی
            /// مثال: ۱۴۰۵/۰۶/۲۷
            /// </summary>
            public static DateTime? ToGregorianDate(this string? persianDate)
            {
                if (string.IsNullOrWhiteSpace(persianDate))
                    return null;

                try
                {
                    persianDate = persianDate.Trim()
                        .Replace("۰", "0")
                        .Replace("۱", "1")
                        .Replace("۲", "2")
                        .Replace("۳", "3")
                        .Replace("۴", "4")
                        .Replace("۵", "5")
                        .Replace("۶", "6")
                        .Replace("۷", "7")
                        .Replace("۸", "8")
                        .Replace("۹", "9");

                    var parts = persianDate.Split(
                        '/',
                        '-',
                        '.');

                    if (parts.Length != 3)
                        return null;

                    var year = int.Parse(parts[0]);
                    var month = int.Parse(parts[1]);
                    var day = int.Parse(parts[2]);

                    return PersianCalendar.ToDateTime(
                        year,
                        month,
                        day,
                        0,
                        0,
                        0,
                        0);
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// تبدیل DateTime میلادی به تاریخ شمسی
            /// </summary>
            public static string? ToPersianDate(
                this DateTime? date)
            {
                if (!date.HasValue)
                    return null;

                return $"{PersianCalendar.GetYear(date.Value):0000}/" +
                       $"{PersianCalendar.GetMonth(date.Value):00}/" +
                       $"{PersianCalendar.GetDayOfMonth(date.Value):00}";
            }

            /// <summary>
            /// تبدیل DateTime میلادی به تاریخ شمسی
            /// </summary>
            public static string ToPersianDate(
                this DateTime date)
            {
                return $"{PersianCalendar.GetYear(date):0000}/" +
                       $"{PersianCalendar.GetMonth(date):00}/" +
                       $"{PersianCalendar.GetDayOfMonth(date):00}";
            }
        }
    }
}
