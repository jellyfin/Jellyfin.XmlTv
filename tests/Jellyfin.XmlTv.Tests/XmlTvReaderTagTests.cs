using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Xunit;

namespace Jellyfin.XmlTv.Tests;

public class XmlTvReaderTagTests
{
    private const string ProgrammeFormat = "<programme start=\"20220328040000 +0000\" stop=\"20220328050000 +0000\" channel=\"{0}\">{1}</programme>";
    private const string EpisodeNumFormat = "<episode-num system=\"{0}\">{1}</episode-num>";

    private static TheoryData<string, string?, string[]> GetProgramme_ProcessCategory_SelectsCorrectCategories_TestData()
        => new()
        {
            {
                "",
                null,
                Array.Empty<string>()
            },
            {
                "<category>series</category>",
                null,
                new [] { "series" }
            },
            {
                "<category>series</category><category>Animation</category>",
                null,
                new [] { "series", "Animation" }
            },
            {
                "<category>series</category><category>Crime</category><category>Drama</category><category>Mystery</category><category>Thriller</category>",
                null,
                new [] { "series", "Crime", "Drama", "Mystery", "Thriller" }
            },
            {
                "<category lang=\"\">Crime</category><category lang=\"\">Crimen</category>",
                null,
                new [] { "Crime", "Crimen" }
            },
            {
                "<category lang=\"en\">Crime</category><category lang=\"es\">Crimen</category>",
                null,
                new [] { "Crime", "Crimen" }
            },
            {
                "<category lang=\"en\">Crime</category><category lang=\"es\">Crimen</category>",
                "de",
                new [] { "Crime", "Crimen" }
            },
            {
                "<category>series</category>",
                "en",
                new [] { "series" }
            },
            {
                "<category lang=\"en\">Crime</category><category lang=\"es\">Crimen</category>",
                "en",
                new [] { "Crime" }
            },
            {
                "<category lang=\"en\">Crime</category><category lang=\"es\">Crimen</category>",
                "es",
                new [] { "Crimen" }
            },
        };

        [Theory]
        [MemberData(nameof(GetProgramme_ProcessCategory_SelectsCorrectCategories_TestData))]
        public void GetProgramme_ProcessCategory_SelectsCorrectCategories(string categoryInput, string? language, string[] expected)
        {
            var channel = "channel";
            var inputString = String.Format(CultureInfo.InvariantCulture, ProgrammeFormat, channel, categoryInput);

            var xmlTvReader = new XmlTvReader("file", language);
            using var reader = XmlReader.Create(new StringReader(inputString));
            reader.ReadToNextElement();

            var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

            Assert.NotNull(result);
            Assert.Equal(expected, result!.Categories);
        }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("s01e02", 1, 2)]
    [InlineData("S01E02", 1, 2)]
    public void GetProgramme_ProcessEpisodeNumSxxExx_Success(string input, int? series, int? episode)
    {
        var channel = "channel";
        var inputString = String.Format(CultureInfo.InvariantCulture, ProgrammeFormat, channel, String.Format(CultureInfo.InvariantCulture, EpisodeNumFormat, "SxxExx", input));

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(series, result!.Episode.Series);
        Assert.Equal(episode, result.Episode.Episode);
    }

    [Theory]
    [InlineData("", null, null, null)]
    [InlineData("..", null, null, null)]
    [InlineData(" . . ", null, null, null)]
    [InlineData("1.2.", 2, 3, null)]
    [InlineData("1.2.3", 2, 3, 4)]
    [InlineData("1 . 2 . 3", 2, 3, 4)]
    [InlineData("1.13.0/1", 2, 14, 1, null, null, 1)]
    [InlineData("2/10.3/22.0/2", 3, 4, 1, 10, 22, 2)]
    [InlineData("2 / 10 . 3 / 22 . 0 / 2", 3, 4, 1, 10, 22, 2)]
    public void GetProgramme_ProcessEpisodeNumXmlTvNs_Success(string input, int? series, int? episode, int? part, int? seriesCount = null, int? episodeCount = null, int? partCount = null)
    {
        var channel = "channel";
        var inputString = String.Format(CultureInfo.InvariantCulture, ProgrammeFormat, channel, String.Format(CultureInfo.InvariantCulture, EpisodeNumFormat, "xmltv_ns", input));

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(series, result!.Episode.Series);
        Assert.Equal(seriesCount, result.Episode.SeriesCount);
        Assert.Equal(episode, result.Episode.Episode);
        Assert.Equal(episodeCount, result.Episode.EpisodeCount);
        Assert.Equal(part, result.Episode.Part);
        Assert.Equal(partCount, result.Episode.PartCount);
    }

    [Fact]
    public void GetProgramme_ProcessEpisodeNumConflicting_LastWins()
    {
        var channel = "channel";
        // add multiple conflicting episode numbers
        var sxxExx = String.Format(CultureInfo.InvariantCulture, EpisodeNumFormat, "SxxExx", "S01E02");
        var xmltvNs = String.Format(CultureInfo.InvariantCulture, EpisodeNumFormat, "xmltv_ns", "1.3.0");
        var inputString = String.Format(CultureInfo.InvariantCulture, ProgrammeFormat, channel, sxxExx + xmltvNs);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        // last one parsed overrides
        Assert.NotNull(result);
        Assert.Equal(2, result!.Episode.Series);
        Assert.Equal(4, result.Episode.Episode);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("<sub-title>Episode Title</sub-title>", null, "Episode Title")]
    [InlineData("<sub-title lang=\"\">Episode Title</sub-title>", null, "Episode Title")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", null, "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "de", "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "en", "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "es", "spanish")]
    public void GetProgramme_ProcessSubTitle_SelectsCorrectSubtitle(string subTitleInput, string? language, string? expected)
    {
        var channel = "channel";
        var inputString = String.Format(CultureInfo.InvariantCulture, ProgrammeFormat, channel, subTitleInput);

        var xmlTvReader = new XmlTvReader("file", language);
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Episode.Title);
    }
}
