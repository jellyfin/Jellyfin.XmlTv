using System;
using System.Globalization;
using Xunit;

namespace Jellyfin.XmlTv.Tests
{
    public class XmlTvReaderDateTimeTests
    {
        [Theory]
        [InlineData("01 Jan 2016 00:00:00", "2016")]
        [InlineData("01 Jan 2016 00:00:00", "201601")]
        [InlineData("01 Jan 2016 00:00:00", "20160101")]
        [InlineData("01 Jan 2016 12:00:00", "2016010112")]
        [InlineData("01 Jan 2016 12:34:00", "201601011234")]
        [InlineData("01 Jan 2016 12:34:56", "20160101123456")]
        [InlineData("01 Jan 2016 12:00:00", "20160101120000 +0000")]
        [InlineData("01 Jan 2016 02:00:00", "20160101120000 +1000")]
        [InlineData("01 Jan 2016 11:00:00", "20160101120000 +0100")]
        [InlineData("01 Jan 2016 11:50:00", "20160101120000 +0010")]
        [InlineData("01 Jan 2016 11:59:00", "20160101120000 +0001")]
        [InlineData("01 Jan 2016 22:00:00", "20160101120000 -1000")]
        [InlineData("01 Jan 2016 13:00:00", "20160101120000 -0100")]
        [InlineData("01 Jan 2016 12:10:00", "20160101120000 -0010")]
        [InlineData("01 Jan 2016 12:01:00", "20160101120000 -0001")]
        [InlineData("01 Jan 2016 01:00:00", "2016 -0100")]
        [InlineData("01 Jan 2016 01:00:00", "201601 -0100")]
        [InlineData("01 Jan 2016 01:00:00", "20160101 -0100")]
        [InlineData("01 Jan 2016 13:00:00", "2016010112 -0100")]
        [InlineData("01 Jan 2016 13:00:00", "201601011200 -0100")]
        [InlineData("01 Jan 2016 12:00:00", "20160101120000 +000")]
        [InlineData("01 Jan 2016 12:00:00", "20160101120000 +00")]
        [InlineData("01 Jan 2016 12:00:00", "20160101120000 +0")]
        [InlineData("01 Jan 2016 12:00:00", "20160101120000 0")]
        public void HandlePartDatesTest(string expected, string value)
        {
            Assert.Equal(
                DateTimeOffset.Parse(expected, styles: DateTimeStyles.AssumeUniversal),
                XmlTvReader.ParseDate(value));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("1")]
        public void ParseDate_Invalid_Null(string value)
        {
            Assert.Null(XmlTvReader.ParseDate(value));
        }
    }
}
