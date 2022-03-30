using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Jellyfin.XmlTv.Test
{
    public class XmlTvReaderLanguageTests
    {
        /* <title lang="es">Homes Under the Hammer - Spanish</title>
         * <title lang="es">Homes Under the Hammer - Spanish 2</title>
         * <title lang="en">Homes Under the Hammer - English</title>
         * <title lang="en">Homes Under the Hammer - English 2</title>
         * <title lang="">Homes Under the Hammer - Empty Language</title>
         * <title lang="">Homes Under the Hammer - Empty Language 2</title>
         * <title>Homes Under the Hammer - No Language</title>
         * <title>Homes Under the Hammer - No Language 2</title>
         */

        /* Expected Behaviour:
         *  - Language = Null   Homes Under the Hammer - No Language
         *  - Language = ""     Homes Under the Hammer - No Language
         *  - Language = es     Homes Under the Hammer - Spanish
         *  - Language = en     Homes Under the Hammer - English
         */

        [Fact]
        public void FirstMatchingLanguageESTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es");
            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - Spanish", programme!.Title);
            Assert.Single(programme.Categories);
            Assert.Equal("Property - Spanish", programme.Categories[0]);
        }

        [Fact]
        public void FirstMatchingLanguageENTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "en");

            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - English", programme!.Title);
            Assert.Single(programme.Categories);
            Assert.Equal("Property - English", programme.Categories[0]);
        }

        [Fact]
        public void FirstMatchingNoLanguageTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, null);

            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - No Language", programme!.Title);
            Assert.Single(programme.Categories);
            Assert.Equal("Property - No Language", programme.Categories[0]);
        }

        [Fact]
        public void FirstMatchingEmptyLanguageTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, string.Empty);

            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - Empty Language", programme!.Title);
            Assert.Single(programme.Categories);
            Assert.Equal("Property - Empty Language", programme.Categories[0]);
        }

        [Fact]
        public void FirstNoMatchFoundTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, "es"); // There are no titles or categories for spanish

            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).Skip(1).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - No Language", programme!.Title);

            // Should return all categories
            Assert.Equal(2, programme.Categories.Count);
            Assert.Contains("Property - English", programme.Categories);
            Assert.Contains("Property - Empty Language", programme.Categories);
        }

        [Fact]
        public void FirstNoLanguageTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile, null);

            var channel = reader.GetChannels().FirstOrDefault();
            Assert.NotNull(channel);

            var startDate = new DateTime(2015, 11, 26);
            var programme = reader.GetProgrammes(channel!.Id, startDate, startDate.AddDays(1)).Skip(1).FirstOrDefault();

            Assert.NotNull(programme);
            Assert.Equal("Homes Under the Hammer - No Language", programme!.Title); // Should return the first in the list

            // Should return all categories
            Assert.Equal(2, programme.Categories.Count);
            Assert.Contains("Property - English", programme.Categories);
            Assert.Contains("Property - Empty Language", programme.Categories);
        }

        [Fact]
        public void AllLanguagesTest()
        {
            var testFile = Path.Join("Test Data", "MultilanguageData.xml");
            var reader = new XmlTvReader(testFile);
            var results = reader.GetLanguages();
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("en", results[0].Name);
            Assert.Equal(11, results[0].Relevance);
            Assert.Equal("es", results[1].Name);
            Assert.Equal(3, results[1].Relevance);
        }
    }
}
