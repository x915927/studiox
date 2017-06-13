﻿using System;
using System.Collections.Generic;
using System.Linq;
using StudioX.Collections.Extensions;
using StudioX.Timing.Timezone;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace StudioX.Tests.Timing
{
    public class TimezoneHelperTests
    {
        [Theory]
        [InlineData("Pacific Standard Time", "America/Los_Angeles")]
        [InlineData("Eastern Standard Time", "America/New_York")]
        [InlineData("UTC", "Etc/UTC")]
        [InlineData("Greenland Standard Time", "America/Godthab")]
        [InlineData("GMT Standard Time", "Europe/London")]
        [InlineData("W. Europe Standard Time", "Europe/Berlin")]
        [InlineData("Romance Standard Time", "Europe/Paris")]
        [InlineData("Jordan Standard Time", "Asia/Amman")]
        [InlineData("South Africa Standard Time", "Africa/Johannesburg")]
        [InlineData("Mauritius Standard Time", "Indian/Mauritius")]
        [InlineData("Malay Peninsula Standard Time", "Asia/Kuala_Lumpur")]
        public void WindowsTimezoneIdToIanaTests(string windowsTimezoneId, string ianaTimezoneId)
        {
            TimezoneHelper.WindowsToIana(windowsTimezoneId).ShouldBe(ianaTimezoneId);
        }

        [Theory]
        [InlineData("America/Los_Angeles", "Pacific Standard Time")]
        [InlineData("America/Argentina/San_Luis", "Argentina Standard Time")]
        [InlineData("Etc/UTC", "UTC")]
        [InlineData("America/Godthab", "Greenland Standard Time")]
        [InlineData("Europe/London", "GMT Standard Time")]
        [InlineData("Europe/Berlin", "W. Europe Standard Time")]
        [InlineData("Europe/Paris", "Romance Standard Time")]
        [InlineData("Asia/Amman", "Jordan Standard Time")]
        [InlineData("Europe/Zaporozhye", "FLE Standard Time")]
        [InlineData("Asia/Choibalsan", "Ulaanbaatar Standard Time")]
        public void IanaTimezoneIdToWindowsTests(string ianaTimezoneId, string windowsTimezoneId)
        {
            TimezoneHelper.IanaToWindows(ianaTimezoneId).ShouldBe(windowsTimezoneId);
        }

        [Fact]
        public void AllWindowsTimezonesShouldBeConvertableToIana()
        {
            var allTimezones = TimeZoneInfo.GetSystemTimeZones();
            Should.NotThrow(() =>
            {
                var exceptions = new List<string>();

                foreach (var timezone in allTimezones)
                {
                    try
                    {
                        TimezoneHelper.WindowsToIana(timezone.Id);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex.Message);
                    }
                }

                if (exceptions.Any())
                {
                    throw new Exception(exceptions.JoinAsString(Environment.NewLine));
                }
            });
        }

        [Fact]
        public void ShouldThrowExceptionForUnknownWindowsTimezoneId()
        {
            Should.Throw<Exception>(() =>
            {
                TimezoneHelper.WindowsToIana("abc");
            });
        }

        [Fact]
        public void ShouldThrowExceptionForUnknownIanaTimezoneId()
        {
            Should.Throw<Exception>(() =>
            {
                TimezoneHelper.IanaToWindows("cba");
            });
        }

        [Fact]
        public void ConvertByIanaTimezoneShouldBeConvertByWindowsTimezone()
        {
            var now = DateTime.UtcNow;
            TimezoneHelper.ConvertTimeFromUtcByIanaTimeZoneId(now, "Asia/Shanghai")
                .ShouldBe(TimezoneHelper.ConvertFromUtc(now, "China Standard Time"));
        }
    }
}
