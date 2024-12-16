using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Jellyfin.XmlTv.Tests
{
    public class XmlTvReaderTests
    {
        [Fact]
        public void UKDataChannelsTest()
        {
            var testFile = Path.Join("Test Data", "UK_Data.xml");
            var reader = new XmlTvReader(testFile);

            var channels = reader.GetChannels().ToList();
            Assert.Equal(5, channels.Count);

            // Check each channel
            var channel = channels.SingleOrDefault(c => c.Id == "UK_RT_2667");
            Assert.NotNull(channel);
            Assert.Equal("BBC1 HD", channel.DisplayName);
            Assert.Equal("7.1", channel.Number);

            var icon = channel.Icons?.FirstOrDefault();
            Assert.NotNull(icon);
            Assert.Equal("Logo_UK_RT_2667", icon.Source);
            Assert.Equal(100, icon.Width);
            Assert.Equal(200, icon.Height);

            channel = channels.SingleOrDefault(c => c.Id == "UK_RT_105");
            Assert.NotNull(channel);
            Assert.Equal("BBC2", channel.DisplayName);

            icon = channel.Icons?.FirstOrDefault();
            Assert.NotNull(icon);
            Assert.Equal("Logo_UK_RT_105", icon.Source);
            Assert.False(icon.Width.HasValue);
            Assert.False(icon.Height.HasValue);

            channel = channels.SingleOrDefault(c => c.Id == "UK_RT_2118");
            Assert.NotNull(channel);
            Assert.Equal("ITV1 HD", channel.DisplayName);

            icon = channel.Icons?.FirstOrDefault();
            Assert.NotNull(icon);
            Assert.Equal("Logo_UK_RT_2118", icon.Source);
            Assert.Equal(100, icon.Width);
            Assert.False(icon.Height.HasValue);

            channel = channels.SingleOrDefault(c => c.Id == "UK_RT_2056");
            Assert.NotNull(channel);
            Assert.Equal("Channel 4 HD", channel.DisplayName);

            icon = channel.Icons?.First();
            Assert.NotNull(icon);
            Assert.Equal("Logo_UK_RT_2056", icon.Source);
            Assert.False(icon.Width.HasValue);
            Assert.Equal(200, icon.Height);

            channel = channels.SingleOrDefault(c => c.Id == "UK_RT_134");
            Assert.NotNull(channel);
            Assert.Equal("Channel 5", channel.DisplayName);
            Assert.Null(channel.Icons);
        }

        [Fact]
        public void UKDataGeneralTest()
        {
            var testFile = Path.Join("Test Data", "UK_Data.xml");
            var reader = new XmlTvReader(testFile, null);

            var channels = reader.GetChannels().ToList();
            Assert.Equal(5, channels.Count);

            // Pick a channel to check the data for
            var channel = channels.SingleOrDefault(c => c.Id == "UK_RT_2056");
            Assert.NotNull(channel);

            var startDate = new DateTimeOffset(2015, 11, 26, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).ToList();

            Assert.Equal(27, programmes.Count);
            var programme = programmes.SingleOrDefault(p => p.Title == "The Secret Life of");

            Assert.NotNull(programme);
            Assert.Equal(new DateTimeOffset(2015, 11, 26, 20, 0, 0, 0, new TimeSpan()), programme!.StartDate);
            Assert.Equal(new DateTimeOffset(2015, 11, 26, 21, 0, 0, 0, new TimeSpan()), programme.EndDate);
            Assert.Equal("Cameras follow the youngsters' development after two weeks apart and time has made the heart grow fonder for Alfie and Emily, who are clearly happy to be back together. And although Alfie struggled to empathise with the rest of his peers before, a painting competition proves to be a turning point for him. George takes the children's rejection of his family recipe to heart, but goes on to triumph elsewhere, and romance is in the air when newcomer Sienna captures Arthur's heart.", programme.Description);
            Assert.Equal("Documentary", programme.Categories.Single());
            Assert.NotNull(programme.Episode);
            Assert.Equal("The Secret Life of 5 Year Olds", programme.Episode!.Title);
            Assert.Equal(1, programme.Episode.Series);
            Assert.Null(programme.Episode.SeriesCount);
            Assert.Equal(4, programme.Episode.Episode);
            Assert.Equal(6, programme.Episode.EpisodeCount);
            Assert.NotNull(programme.Premiere);
            Assert.Equal("First showing on national terrestrial TV", programme!.Premiere!.Details);
            Assert.True(programme.IsNew);
        }

        [Fact]
        public void UKDataMultipleTitlesSameLanguageFirstValueTest()
        {
            var testFile = Path.Join("Test Data", "UK_Data.xml");
            var reader = new XmlTvReader(testFile, null);

            /*
                <title lang="en">Homes Under the Hammer - Title 1</title>
                <title lang="en">Homes Under the Hammer - Title 2</title>
                <title lang="en">Homes Under the Hammer - Title 3</title>
            */

            var startDate = new DateTimeOffset(2015, 11, 26, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes("UK_RT_2667", startDate, startDate.AddDays(1)).ToList();
            var programme = programmes.SingleOrDefault(p => p.Title == "Homes Under the Hammer - Title 1");

            Assert.NotNull(programme);
        }

        [Fact]
        public void UKDataMultipleTitlesNoLanguageFirstValueTest()
        {
            var testFile = Path.Join("Test Data", "UK_Data.xml");
            var reader = new XmlTvReader(testFile, null);

            /*
                <title>Oxford Street Revealed - Title 1</title>
                <title>Oxford Street Revealed - Title 2</title>
                <title>Oxford Street Revealed - Title 3</title>
            */

            var startDate = new DateTimeOffset(2015, 11, 26, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes("UK_RT_2667", startDate, startDate.AddDays(1)).ToList();
            var programme = programmes.SingleOrDefault(p => p.Title == "Oxford Street Revealed - Title 1");

            Assert.NotNull(programme);
        }

        [Fact]
        public void ESMultiLanguageDataTest()
        {
            var testFile = Path.Join("Test Data", "ES_MultiLanguageData.xml");
            var reader = new XmlTvReader(testFile, "es"); // Specify the spanish language explicitly

            var channels = reader.GetChannels().ToList();
            Assert.Equal(141, channels.Count);

            // Pick a channel to check the data for
            var channel = channels.SingleOrDefault(c => c.Id == "Canal + HD" && c.DisplayName == "Canal + HD");
            Assert.NotNull(channel);

            var startDate = new DateTimeOffset(2016, 02, 18, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).ToList();

            Assert.Equal(22, programmes.Count);
            var programme = programmes.SingleOrDefault(p => p.Title == "This is Comedy. Judd Apatow & Co.");

            /*
            <programme start="20160218055100 +0100" stop="20160218065400 +0100" channel="Canal + HD">
                <title lang="es">This is Comedy. Judd Apatow &amp; Co.</title>
                <title lang="en">This is Comedy</title>
                <desc lang="es">El resurgir creativo de la comedia estadounidense en los últimos 15 años ha tenido un nombre indiscutible, Judd Apatow, y unos colaboradores indispensables, sus amigos (actores, cómicos, escritores) Jonah Hill, Steve Carrell, Paul Rudd, Seth Rogen, Lena Dunham... A través de extractos de sus filmes y de entrevistas a algunos los miembros de su 'banda' (Adam Sandler, Lena Dunham o Jason Segel), este documental muestra la carrera de un productor y director excepcional que ha sido capaz de llevar la risa a su máxima expresión</desc>
                <credits>
                  <director>Jacky Goldberg</director>
                </credits>
                <date>2014</date>
                <category lang="es">Documentales</category>
                <category lang="es">Sociedad</category>
                <icon src="http://www.plus.es/recorte/n/caratula4/F3027798" />
                <country>Francia</country>
                <rating system="MPAA">
                  <value>TV-G</value>
                </rating>
                <star-rating>
                  <value>3/5</value>
                </star-rating>
            </programme>
            */

            Assert.NotNull(programme);
            Assert.Equal(new DateTimeOffset(2016, 02, 18, 4, 51, 0, new TimeSpan()), programme!.StartDate);
            Assert.Equal(new DateTimeOffset(2016, 02, 18, 5, 54, 0, new TimeSpan()), programme.EndDate);
            Assert.Equal("El resurgir creativo de la comedia estadounidense en los últimos 15 años ha tenido un nombre indiscutible, Judd Apatow, y unos colaboradores indispensables, sus amigos (actores, cómicos, escritores) Jonah Hill, Steve Carrell, Paul Rudd, Seth Rogen, Lena Dunham... A través de extractos de sus filmes y de entrevistas a algunos los miembros de su 'banda' (Adam Sandler, Lena Dunham o Jason Segel), este documental muestra la carrera de un productor y director excepcional que ha sido capaz de llevar la risa a su máxima expresión", programme.Description);
            Assert.Equal(2, programme.Categories.Count);
            Assert.Equal("Documentales", programme.Categories[0]);
            Assert.Equal("Sociedad", programme.Categories[1]);
            Assert.Null(programme.Episode);
        }

        [Fact]
        public void HoneybeeTest()
        {
            var testFile = Path.Join("Test Data", "honeybee.xml");
            var reader = new XmlTvReader(testFile, null);

            var channels = reader.GetChannels().ToList();
            Assert.Equal(16, channels.Count);

            var startDate = new DateTimeOffset(2017, 2, 1, 0, 0, 0, new TimeSpan());
            var programs = reader.GetProgrammes(
                "2013.honeybee.it",
                startDate,
                startDate.AddMonths(1)).ToList();
            Assert.Equal(297, programs.Count);
        }

        [Fact]
        public void SchedulesDirectTest()
        {
            var testFile = Path.Join("Test Data", "schedulesdirect.xml");
            var reader = new XmlTvReader(testFile, null);

            var channels = reader.GetChannels().ToList();

            // Pick a channel to check the data for
            var channel = channels.SingleOrDefault(c => c.Id == "EPG123.10108.schedulesdirect.org"); // CTV Toronto
            Assert.NotNull(channel);

            var startDate = new DateTimeOffset(2020, 07, 29, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).ToList();

            var programme = programmes.SingleOrDefault(p => p.Title == "Match Game");
            Assert.NotNull(programme!.Episode);
            Assert.True(programme.Credits?.Count == 9);
            Assert.True(programme.Icons?.Count == 4);

            var icon = programme.Icons?.FirstOrDefault(s => s.Width > s.Height);
            Assert.NotNull(icon);
            Assert.True(icon!.Width > icon.Height);
        }

        [Fact]
        public void BasicTest()
        {
            var testFile = Path.Join("Test Data", "custom.xml");
            var reader = new XmlTvReader(testFile, null);

            var channels = reader.GetChannels().ToList();

            // Pick a channel to check the data for
            var channel = channels.SingleOrDefault(c => c.Id == "EPG123.10108.schedulesdirect.org"); // CTV Toronto
            Assert.NotNull(channel);

            var startDate = new DateTimeOffset(2020, 07, 29, 0, 0, 0, new TimeSpan());
            var programmes = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).ToList();

            var programme = programmes.SingleOrDefault(p => p.Title == "Unforgettable");
            Assert.NotNull(programme);
            Assert.True(programme.Images?.Count == 4);
            Assert.True(programme.Icons?.Count == 5);

            var credits = programme.Credits;
            Assert.NotNull(credits);
            Assert.True(credits?.Count == 30);
            Assert.True(credits[5]?.Images?.Count == 1);
            Assert.True(credits[5]?.Urls?.Count == 1);

            var icon = programme.Icons?.FirstOrDefault(s => s.Width > s.Height);
            Assert.NotNull(icon);
            Assert.Equal("p8680420_b_h12_ag.jpg", icon.Source);
        }
    }
}
