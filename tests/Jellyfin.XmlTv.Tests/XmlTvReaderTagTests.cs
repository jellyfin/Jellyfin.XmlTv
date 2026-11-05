using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Xunit;

namespace Jellyfin.XmlTv.Tests;

public class XmlTvReaderTagTests
{
    private static readonly CompositeFormat _programmeFormat = CompositeFormat.Parse("<programme start=\"20220328040000 +0000\" stop=\"20220328050000 +0000\" channel=\"{0}\">{1}</programme>");
    private static readonly CompositeFormat _episodeNumFormat = CompositeFormat.Parse("<episode-num system=\"{0}\">{1}</episode-num>");

    public static TheoryData<string, string?, string[]> GetProgramme_ProcessCategory_SelectsCorrectCategories_TestData()
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
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, categoryInput);

        var xmlTvReader = new XmlTvReader("file", language);
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Categories);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n  ")]
    public void GetProgramme_ProcessUrl_PreservesAdjacentElements(string separator)
    {
        var channel = "channel";
        var input = "<url>https://example.com/first</url>" + separator
            + "<url>https://example.com/second</url>" + separator
            + "<icon src=\"https://example.com/poster.jpg\" />" + separator
            + "<desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        Assert.Equal("https://example.com/poster.jpg", Assert.Single(result.Icons!).Source);
        Assert.Collection(
            result.Urls!,
            url => Assert.Equal("https://example.com/first", url.Uri),
            url => Assert.Equal("https://example.com/second", url.Uri));
    }

    [Theory]
    [InlineData("<icon />")]
    [InlineData("<icon></icon>")]
    [InlineData("<icon src=\"https://example.com/poster.jpg\" />")]
    [InlineData("<icon src=\"https://example.com/poster.jpg\"><ignored /></icon>")]
    [InlineData("<url />")]
    [InlineData("<url></url>")]
    public void GetProgramme_EmptyOrPopulatedElement_PreservesFollowingDescription(string input)
    {
        var channel = "channel";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input + "<desc>Test description</desc>");

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n  ")]
    public void GetChannels_ProcessUrlAndIcon_PreservesAdjacentElements(string separator)
    {
        var input = "<tv><channel id=\"channel\"><url />" + separator
            + "<url>https://example.com/first</url>" + separator
            + "<url>https://example.com/second</url>" + separator
            + "<icon />" + separator
            + "<icon src=\"https://example.com/first.jpg\" />" + separator
            + "<icon src=\"https://example.com/second.jpg\"></icon>" + separator
            + "<display-name>Test channel</display-name></channel></tv>";
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, input);
            var channel = Assert.Single(new XmlTvReader(path).GetChannels());

            Assert.Equal("Test channel", channel.DisplayName);
            Assert.Collection(
                channel.Urls!,
                url => Assert.Equal("https://example.com/first", url.Uri),
                url => Assert.Equal("https://example.com/second", url.Uri));
            Assert.Collection(
                channel.Icons!,
                icon => Assert.Equal("https://example.com/first.jpg", icon.Source),
                icon => Assert.Equal("https://example.com/second.jpg", icon.Source));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n  ")]
    public void GetProgramme_ProcessCredits_PreservesAdjacentImagesAndUrls(string separator)
    {
        var channel = "channel";
        var input = "<credits><actor>Test actor<image />" + separator
            + "<image>https://example.com/first.jpg</image>" + separator
            + "<image>https://example.com/second.jpg</image>" + separator
            + "<url />" + separator
            + "<url>https://example.com/first</url>" + separator
            + "<unknown><image>https://example.com/ignored.jpg</image></unknown>" + separator
            + "<url>https://example.com/second</url></actor>"
            + "<director>Test director</director></credits><desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        Assert.Collection(
            result.Credits!,
            actor =>
            {
                Assert.Equal("Test actor", actor.Name);
                Assert.Collection(
                    actor.Images!,
                    image => Assert.Equal("https://example.com/first.jpg", image.Path),
                    image => Assert.Equal("https://example.com/second.jpg", image.Path));
                Assert.Collection(
                    actor.Urls!,
                    url => Assert.Equal("https://example.com/first", url.Uri),
                    url => Assert.Equal("https://example.com/second", url.Uri));
            },
            director => Assert.Equal("Test director", director.Name));
    }

    [Theory]
    [InlineData("rating", "")]
    [InlineData("rating", "\n  ")]
    [InlineData("star-rating", "")]
    [InlineData("star-rating", "\n  ")]
    public void GetProgramme_ProcessRatingIcons_PreservesConsecutiveIcons(string element, string separator)
    {
        var channel = "channel";
        var input = "<" + element + "><icon />" + separator
            + "<icon src=\"https://example.com/first.jpg\" />" + separator
            + "<icon src=\"https://example.com/second.jpg\" /></" + element + "><desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        var icons = element == "rating"
            ? Assert.Single(result.Ratings!).Icons
            : Assert.Single(result.StarRatings!).Icons;
        Assert.Collection(
            icons!,
            icon => Assert.Equal("https://example.com/first.jpg", icon.Source),
            icon => Assert.Equal("https://example.com/second.jpg", icon.Source));
    }

    [Theory]
    [InlineData("rating", "", "PG")]
    [InlineData("rating", "\n  ", "PG")]
    [InlineData("star-rating", "", "4")]
    [InlineData("star-rating", "\n  ", "4")]
    public void GetProgramme_ProcessRatingValue_PreservesFollowingElements(string element, string separator, string value)
    {
        var channel = "channel";
        var input = "<" + element + "><value>" + value + "</value>" + separator
            + "<icon src=\"https://example.com/first.jpg\" />" + separator
            + "<icon src=\"https://example.com/second.jpg\" /></" + element + ">"
            + "<category>Drama</category><desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        Assert.Equal("Drama", Assert.Single(result.Categories));
        var icons = element == "rating"
            ? Assert.Single(result.Ratings!).Icons
            : Assert.Single(result.StarRatings!).Icons;
        if (element == "rating")
        {
            Assert.Equal(value, Assert.Single(result.Ratings!).Value);
        }

        Assert.Collection(
            icons!,
            icon => Assert.Equal("https://example.com/first.jpg", icon.Source),
            icon => Assert.Equal("https://example.com/second.jpg", icon.Source));
    }

    [Theory]
    [InlineData("rating", "<value />")]
    [InlineData("rating", "<value></value>")]
    [InlineData("star-rating", "<value />")]
    [InlineData("star-rating", "<value>invalid/5</value>")]
    public void GetProgramme_ProcessEmptyOrInvalidRatingValue_PreservesFollowingMetadata(string element, string value)
    {
        var channel = "channel";
        var input = "<" + element + ">" + value + "</" + element + ">"
            + "<category>Drama</category><desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        Assert.Equal("Drama", Assert.Single(result.Categories));
    }

    [Theory]
    [InlineData("rating", "PG")]
    [InlineData("star-rating", "4")]
    public void GetProgramme_ProcessRatingValueAsLastChild_PreservesFollowingRating(string element, string value)
    {
        var channel = "channel";
        var input = "<" + element + " system=\"first\"><value>" + value + "</value></" + element + ">"
            + "<" + element + " system=\"second\"><value>" + value + "</value></" + element + ">"
            + "<desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        if (element == "rating")
        {
            Assert.Collection(
                result.Ratings!,
                rating =>
                {
                    Assert.Equal("first", rating.System);
                    Assert.Equal(value, rating.Value);
                },
                rating =>
                {
                    Assert.Equal("second", rating.System);
                    Assert.Equal(value, rating.Value);
                });
        }
        else
        {
            Assert.Collection(
                result.StarRatings!,
                rating => Assert.Equal("first", rating.System),
                rating => Assert.Equal("second", rating.System));
        }
    }

    [Theory]
    [InlineData("rating", "PG")]
    [InlineData("star-rating", "4")]
    public void GetProgramme_ProcessRatingUnknownElement_PreservesFollowingIcon(string element, string value)
    {
        var channel = "channel";
        var input = "<" + element + "><value>" + value + "</value>"
            + "<unknown><icon src=\"https://example.com/ignored.jpg\" /></unknown>"
            + "<icon src=\"https://example.com/rating.jpg\" /></" + element + ">";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        var icons = element == "rating"
            ? Assert.Single(result!.Ratings!).Icons
            : Assert.Single(result!.StarRatings!).Icons;
        Assert.Equal("https://example.com/rating.jpg", Assert.Single(icons!).Source);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n  ")]
    public void GetProgramme_ProcessIcon_PreservesAttributesAndFollowingElement(string separator)
    {
        var channel = "channel";
        var input = "<icon src=\"https://example.com/poster.jpg\" width=\"320\" height=\"180\" />"
            + separator + "<desc>Test description</desc>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test description", result!.Description);
        var icon = Assert.Single(result.Icons!);
        Assert.Equal("https://example.com/poster.jpg", icon.Source);
        Assert.Equal(320, icon.Width);
        Assert.Equal(180, icon.Height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n  ")]
    public void GetChannelsAndProgrammes_ConsumingLastChild_PreservesFollowingRecords(string separator)
    {
        var firstProgramme = string.Format(
            CultureInfo.InvariantCulture,
            _programmeFormat,
            "first",
            "<title>First programme</title><image>https://example.com/image.jpg</image>");
        var secondProgramme = string.Format(
            CultureInfo.InvariantCulture,
            _programmeFormat,
            "first",
            "<title>Second programme</title><url>https://example.com/programme</url>");
        var thirdProgramme = string.Format(
            CultureInfo.InvariantCulture,
            _programmeFormat,
            "second",
            "<title>Third programme</title><icon src=\"https://example.com/programme.jpg\" />");
        var input = "<tv><channel id=\"first\"><display-name>First channel</display-name>"
            + "<url>https://example.com/channel</url></channel>" + separator
            + "<channel id=\"second\"><display-name>Second channel</display-name>"
            + "<icon src=\"https://example.com/channel.jpg\" /></channel>" + separator
            + firstProgramme + separator + secondProgramme + separator + thirdProgramme + "</tv>";
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, input);
            var xmlTvReader = new XmlTvReader(path);
            Assert.Collection(
                xmlTvReader.GetChannels(),
                channel =>
                {
                    Assert.Equal("first", channel.Id);
                    Assert.Equal("First channel", channel.DisplayName);
                    Assert.Equal("https://example.com/channel", Assert.Single(channel.Urls!).Uri);
                },
                channel =>
                {
                    Assert.Equal("second", channel.Id);
                    Assert.Equal("Second channel", channel.DisplayName);
                    Assert.Equal("https://example.com/channel.jpg", Assert.Single(channel.Icons!).Source);
                });
            Assert.Collection(
                xmlTvReader.GetProgrammes("first", DateTimeOffset.MinValue, DateTimeOffset.MaxValue),
                programme =>
                {
                    Assert.Equal("First programme", programme.Title);
                    Assert.Equal("https://example.com/image.jpg", Assert.Single(programme.Images!).Path);
                },
                programme =>
                {
                    Assert.Equal("Second programme", programme.Title);
                    Assert.Equal("https://example.com/programme", Assert.Single(programme.Urls!).Uri);
                });
            var third = Assert.Single(xmlTvReader.GetProgrammes("second", DateTimeOffset.MinValue, DateTimeOffset.MaxValue));
            Assert.Equal("Third programme", third.Title);
            Assert.Equal("https://example.com/programme.jpg", Assert.Single(third.Icons!).Source);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("<category>Thriller</category>")]
    public void GetProgramme_NoEpisodeTags_NullEpisode(string input)
    {
        var channel = "channel";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Null(result!.Episode);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("s01e02", 1, 2)]
    [InlineData("S01E02", 1, 2)]
    public void GetProgramme_ProcessEpisodeNumSxxExx_Success(string input, int? series, int? episode)
    {
        var channel = "channel";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, string.Format(CultureInfo.InvariantCulture, _episodeNumFormat, "SxxExx", input));

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.NotNull(result!.Episode);
        Assert.Equal(series, result.Episode!.Series);
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
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, string.Format(CultureInfo.InvariantCulture, _episodeNumFormat, "xmltv_ns", input));

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.NotNull(result!.Episode);
        Assert.Equal(series, result.Episode!.Series);
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
        var sxxExx = string.Format(CultureInfo.InvariantCulture, _episodeNumFormat, "SxxExx", "S01E02");
        var xmltvNs = string.Format(CultureInfo.InvariantCulture, _episodeNumFormat, "xmltv_ns", "1.3.0");
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, sxxExx + xmltvNs);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        // last one parsed overrides
        Assert.NotNull(result);
        Assert.NotNull(result!.Episode);
        Assert.Equal(2, result.Episode!.Series);
        Assert.Equal(4, result.Episode.Episode);
    }

    [Theory]
    [InlineData("<sub-title>Episode Title</sub-title>", null, "Episode Title")]
    [InlineData("<sub-title lang=\"\">Episode Title</sub-title>", null, "Episode Title")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", null, "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "de", "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "en", "english")]
    [InlineData("<sub-title lang=\"en\">english</sub-title><sub-title lang=\"es\">spanish</sub-title>", "es", "spanish")]
    public void GetProgramme_ProcessSubTitle_SelectsCorrectSubtitle(string subTitleInput, string? language, string? expected)
    {
        var channel = "channel";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, subTitleInput);

        var xmlTvReader = new XmlTvReader("file", language);
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.NotNull(result!.Episode);
        Assert.Equal(expected, result.Episode!.Title);
    }

    [Theory]
    [InlineData("<icon src=\"https://example.com/poster.jpg\" />", 0)]
    [InlineData("<image size=\"3\">https://example.com/poster.jpg</image><icon src=\"https://example.com/poster.jpg\" />", 1)]
    [InlineData("<image size=\"3\">https://example.com/poster.jpg</image>\n<icon src=\"https://example.com/poster.jpg\" />", 1)]
    [InlineData("<icon src=\"https://example.com/poster.jpg\" /><image size=\"3\">https://example.com/poster.jpg</image>", 1)]
    [InlineData("<image /><icon src=\"https://example.com/poster.jpg\" />", 0)]
    [InlineData("<image></image><icon src=\"https://example.com/poster.jpg\" />", 0)]
    public void GetProgramme_ProcessImage_PreservesAdjacentElements(string input, int expectedImageCount)
    {
        var channel = "channel";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, "<title>Test program</title>" + input + "<desc>Test description</desc>");

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Equal("Test program", result!.Title);
        Assert.Equal("Test description", result.Description);
        Assert.Equal("https://example.com/poster.jpg", Assert.Single(result.Icons!).Source);
        Assert.Equal(expectedImageCount, result.Images?.Count ?? 0);
        if (expectedImageCount > 0)
        {
            Assert.Equal("https://example.com/poster.jpg", Assert.Single(result.Images!).Path);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    public void GetProgramme_ProcessImage_PreservesConsecutiveImages(string separator)
    {
        var channel = "channel";
        var input = "<image type=\"poster\">https://example.com/poster.jpg</image>" + separator
            + "<image type=\"backdrop\">https://example.com/backdrop.jpg</image>";
        var inputString = string.Format(CultureInfo.InvariantCulture, _programmeFormat, channel, input);

        var xmlTvReader = new XmlTvReader("file");
        using var reader = XmlReader.Create(new StringReader(inputString));
        reader.ReadToNextElement();

        var result = xmlTvReader.GetProgramme(reader, channel, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        Assert.NotNull(result);
        Assert.Collection(
            result!.Images!,
            image => Assert.Equal("https://example.com/poster.jpg", image.Path),
            image => Assert.Equal("https://example.com/backdrop.jpg", image.Path));
    }

}
