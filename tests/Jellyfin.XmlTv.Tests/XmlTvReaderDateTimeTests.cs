using System;
using System.IO;
using Xunit;

namespace Jellyfin.XmlTv.Tests
{
    public class XmlTvReaderDateTimeTests
    {
        [Fact]
        public void HandlePartDatesTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");

            Assert.Equal(Parse("01 Jan 2016 00:00:00"), reader.ParseDate("2016"));
            Assert.Equal(Parse("01 Jan 2016 00:00:00"), reader.ParseDate("201601"));
            Assert.Equal(Parse("01 Jan 2016 00:00:00"), reader.ParseDate("20160101"));
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("2016010112"));
            Assert.Equal(Parse("01 Jan 2016 12:34:00"), reader.ParseDate("201601011234"));
            Assert.Equal(Parse("01 Jan 2016 12:34:56"), reader.ParseDate("20160101123456"));
        }

        [Fact]
        public void HandleDateWithOffsetTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");

            // parse variations on 1:00AM
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("20160101120000 +0000"));
            Assert.Equal(Parse("01 Jan 2016 02:00:00"), reader.ParseDate("20160101120000 +1000"));
            Assert.Equal(Parse("01 Jan 2016 11:00:00"), reader.ParseDate("20160101120000 +0100"));
            Assert.Equal(Parse("01 Jan 2016 11:50:00"), reader.ParseDate("20160101120000 +0010"));
            Assert.Equal(Parse("01 Jan 2016 11:59:00"), reader.ParseDate("20160101120000 +0001"));

            Assert.Equal(Parse("01 Jan 2016 22:00:00"), reader.ParseDate("20160101120000 -1000"));
            Assert.Equal(Parse("01 Jan 2016 13:00:00"), reader.ParseDate("20160101120000 -0100"));
            Assert.Equal(Parse("01 Jan 2016 12:10:00"), reader.ParseDate("20160101120000 -0010"));
            Assert.Equal(Parse("01 Jan 2016 12:01:00"), reader.ParseDate("20160101120000 -0001"));
        }

        [Fact]
        public void HandlePartDatesWithOffsetTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");

            Assert.Equal(Parse("01 Jan 2016 01:00:00"), reader.ParseDate("2016 -0100"));
            Assert.Equal(Parse("01 Jan 2016 01:00:00"), reader.ParseDate("201601 -0100"));
            Assert.Equal(Parse("01 Jan 2016 01:00:00"), reader.ParseDate("20160101 -0100"));
            Assert.Equal(Parse("01 Jan 2016 13:00:00"), reader.ParseDate("2016010112 -0100"));
            Assert.Equal(Parse("01 Jan 2016 13:00:00"), reader.ParseDate("201601011200 -0100"));
            Assert.Equal(Parse("01 Jan 2016 13:00:00"), reader.ParseDate("20160101120000 -0100"));
        }

        [Fact]
        public void HandleSpacesTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");

            // parse variations on 1:00AM
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("20160101120000 +000"));
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("20160101120000 +00"));
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("20160101120000 +0"));
        }

        [Fact]
        public void HandleSpacesTest2()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");

            // parse variations on 1:00AM
            Assert.Equal(Parse("01 Jan 2016 12:00:00"), reader.ParseDate("20160101120000 0"));
        }

        private DateTimeOffset Parse(string value)
            => new DateTimeOffset(DateTimeOffset.Parse(value).Ticks, TimeSpan.Zero);
    }
}
