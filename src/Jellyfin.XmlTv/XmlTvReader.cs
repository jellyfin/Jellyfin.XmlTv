using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Jellyfin.XmlTv.Entities;
using Jellyfin.XmlTv.Enums;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv;

/// <summary>
/// Reads XMLTV files.
/// </summary>
public partial class XmlTvReader
{
    private readonly string _fileName;
    private readonly string? _language;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvReader" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="language">The specific language to return.</param>
    public XmlTvReader(string fileName, string? language = null)
    {
        _fileName = fileName;
        _language = language;
    }

    [GeneratedRegex(@"^(?<dateDigits>[0-9]{4,14})(\s(?<dateOffset>[+-]*[0-9]{1,4}))?$")]
    private static partial Regex DateWithOffsetRegex();

    private static XmlReader CreateXmlTextReader(string path)
    {
        var settings = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Ignore,
            CheckCharacters = false,
            IgnoreProcessingInstructions = true,
            IgnoreComments = true
        };

        return XmlReader.Create(path, settings);
    }

    /// <summary>
    /// Gets the list of channels present in the XML.
    /// </summary>
    /// <returns>The list of channels present in the XML.</returns>
    public IEnumerable<XmlTvChannel> GetChannels()
    {
        var list = new List<XmlTvChannel>();

        using (var reader = CreateXmlTextReader(_fileName))
        {
            if (reader.ReadToDescendant("tv")
                && reader.ReadToDescendant("channel"))
            {
                do
                {
                    var channel = GetChannel(reader);
                    if (channel is not null)
                    {
                        list.Add(channel);
                    }
                }
                while (reader.ReadToFollowing("channel"));
            }
        }

        return list;
    }

    private XmlTvChannel? GetChannel(XmlReader reader)
    {
        var id = reader.GetAttribute("id");

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var result = new XmlTvChannel(id);

        using (var xmlChannel = reader.ReadSubtree())
        {
            xmlChannel.MoveToContent();
            xmlChannel.Read();

            // Read out the data for each node and process individually
            while (!xmlChannel.EOF && xmlChannel.ReadState == ReadState.Interactive)
            {
                if (xmlChannel.NodeType == XmlNodeType.Element)
                {
                    switch (xmlChannel.Name)
                    {
                        case "display-name":
                            ProcessNode(xmlChannel, s => result.DisplayName = s, _language, s => SetChannelNumber(result, s));
                            break;
                        case "url":
                            ProcessUrlNode(xmlChannel, result);
                            xmlChannel.Skip();

                            break;
                        case "icon":
                            ProcessIconNode(xmlChannel, result);
                            xmlChannel.Skip();

                            break;
                        default:
                            xmlChannel.Skip(); // unknown, skip entire node
                            break;
                    }
                }
                else
                {
                    xmlChannel.Read();
                }
            }
        }

        if (string.IsNullOrEmpty(result.DisplayName))
        {
            return null;
        }

        return result;
    }

    private void SetChannelNumber(XmlTvChannel channel, string value)
    {
        value = value.Replace('-', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
        {
            channel.Number = value;
        }
    }

    /// <summary>
    /// Gets the programmes for a specified channel.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="startDateUtc">The UTC start date.</param>
    /// <param name="endDateUtc">The UTC end date.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The programmes for a specified channel.</returns>
    public IEnumerable<XmlTvProgram> GetProgrammes(
        string channelId,
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc,
        CancellationToken cancellationToken = default)
    {
        var list = new List<XmlTvProgram>();

        using (var reader = CreateXmlTextReader(_fileName))
        {
            if (reader.ReadToDescendant("tv")
                && reader.ReadToDescendant("programme"))
            {
                do
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        continue; // Break out
                    }

                    var programme = GetProgramme(reader, channelId, startDateUtc, endDateUtc);
                    if (programme is not null)
                    {
                        list.Add(programme);
                    }
                }
                while (reader.ReadToFollowing("programme"));
            }
        }

        return list;
    }

    /// <summary>
    /// Gets the programme from a <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/>.</param>
    /// <param name="channelId">The channel id.</param>
    /// <param name="startDateUtc">The UTC start date.</param>
    /// <param name="endDateUtc">The UTC end date.</param>
    /// <returns>The programmes for a specified channel.</returns>
    public XmlTvProgram? GetProgramme(XmlReader reader, string channelId, DateTimeOffset startDateUtc, DateTimeOffset endDateUtc)
    {
        var id = reader.GetAttribute("channel");

        // First up, validate that this is the correct channel, and programme is within the time we are expecting
        if (id is null || !string.Equals(id, channelId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var result = new XmlTvProgram(id);

        PopulateHeader(reader, result);
        if (result.EndDate < startDateUtc || result.StartDate >= endDateUtc)
        {
            return null;
        }

        using (var xmlProg = reader.ReadSubtree())
        {
            xmlProg.MoveToContent();
            xmlProg.Read();

            // Loop through each element
            while (!xmlProg.EOF && xmlProg.ReadState == ReadState.Interactive)
            {
                if (xmlProg.NodeType == XmlNodeType.Element)
                {
                    switch (xmlProg.Name)
                    {
                        case "title":
                            ProcessTitleNode(xmlProg, result);
                            break;
                        case "category":
                            ProcessCategory(xmlProg, result);
                            break;
                        case "country":
                            ProcessCountry(xmlProg, result);
                            break;
                        case "desc":
                            ProcessDescription(xmlProg, result);
                            break;
                        case "keyword":
                            ProcessKeyword(xmlProg, result);
                            break;
                        case "language":
                            ProcessLanguage(xmlProg, result);
                            break;
                        case "orig-language":
                            ProcessOriginalLanguage(xmlProg, result);
                            break;
                        case "sub-title":
                            ProcessSubTitle(xmlProg, result);
                            break;
                        case "new":
                            ProcessNew(xmlProg, result);
                            break;
                        case "live":
                            ProcessLive(xmlProg, result);
                            break;
                        case "previously-shown":
                            ProcessPreviouslyShown(xmlProg, result);
                            break;
                        case "quality":
                            ProcessQuality(xmlProg, result);
                            break;
                        case "episode-num":
                            ProcessEpisodeNum(xmlProg, result);
                            break;
                        case "date": // Copyright date
                            ProcessCopyrightDate(xmlProg, result);
                            break;
                        case "star-rating": // Community Rating
                            ProcessStarRating(xmlProg, result);
                            break;
                        case "rating": // Certification Rating
                            ProcessRating(xmlProg, result);
                            break;
                        case "credits":
                            if (xmlProg.IsEmptyElement)
                            {
                                xmlProg.Skip();
                            }
                            else
                            {
                                using var subtree = xmlProg.ReadSubtree();
                                ProcessCredits(subtree, result);
                            }

                            break;
                        case "icon":
                            ProcessIconNode(xmlProg, result);
                            xmlProg.Skip();

                            break;
                        case "url":
                            ProcessUrlNode(xmlProg, result);
                            xmlProg.Skip();

                            break;
                        case "image":
                            ProcessImageNode(xmlProg, result);
                            xmlProg.Skip();

                            break;
                        case "premiere":
                            ProcessPremiereNode(xmlProg, result);
                            break;
                        default:
                            // unknown, skip entire node
                            xmlProg.Skip();
                            break;
                    }
                }
                else
                {
                    xmlProg.Read();
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the list of supported languages in the XML.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that may be used to cancel the operation.</param>
    /// <returns>The list of supported languages in the XML.</returns>
    public IReadOnlyCollection<XmlTvLanguage> GetLanguages(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, int>();

        // Loop through and parse out all elements and then lang= attributes
        using (var reader = CreateXmlTextReader(_fileName))
        {
            while (reader.Read())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    continue; // Break out
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var language = reader.GetAttribute("lang");
                    if (!string.IsNullOrEmpty(language))
                    {
                        results.TryAdd(language, 0);
                        results[language]++;
                    }
                }
            }
        }

        return results.Keys.Select(k => new XmlTvLanguage(k, results[k]))
                .OrderByDescending(l => l.Relevance)
                .ToList();
    }

    private void ProcessCopyrightDate(XmlReader xmlProg, XmlTvProgram result)
    {
        var startValue = xmlProg.ReadElementContentAsString();
        if (string.IsNullOrEmpty(startValue))
        {
            result.CopyrightDate = null;
        }
        else
        {
            var copyrightDate = ParseDate(startValue);
            if (copyrightDate is not null)
            {
                result.CopyrightDate = copyrightDate;
            }
        }
    }

    private void ProcessCredits(XmlReader creditsXml, XmlTvProgram result)
    {
        creditsXml.MoveToContent();
        creditsXml.Read();

        // Loop through each element
        while (!creditsXml.EOF && creditsXml.ReadState == ReadState.Interactive)
        {
            if (creditsXml.NodeType == XmlNodeType.Element)
            {
                var type = creditsXml.Name switch
                {
                    "director" => CreditType.Director,
                    "actor" => CreditType.Actor,
                    "writer" => CreditType.Writer,
                    "adapter" => CreditType.Adapter,
                    "producer" => CreditType.Producer,
                    "composer" => CreditType.Composer,
                    "editor" => CreditType.Editor,
                    "presenter" => CreditType.Presenter,
                    "commentator" => CreditType.Commentator,
                    "guest" => CreditType.Guest,
                    _ => CreditType.NotSpecified
                };

                if (type is not CreditType.NotSpecified)
                {
                    var credit = new XmlTvCredit(type)
                    {
                        // Role
                        Role = creditsXml.GetAttribute("role")
                    };

                    // Guest status
                    var guestAttributeString = creditsXml.GetAttribute("guest");
                    credit.Guest = string.Equals(guestAttributeString, "yes", StringComparison.OrdinalIgnoreCase);

                    // Name
                    // credit.Name = creditsXml.ReadElementContentAsString();

                    // Loop through each element
                    using var creditXml = creditsXml.ReadSubtree();
                    creditXml.MoveToContent();
                    creditXml.Read();
                    while (!creditXml.EOF && creditXml.ReadState == ReadState.Interactive)
                    {
                        if (creditXml.NodeType == XmlNodeType.Text)
                        {
                            // Name
                            credit.Name = creditXml.ReadContentAsString().Trim();
                        }
                        else if (creditXml.NodeType == XmlNodeType.Element)
                        {
                            var nodeName = creditXml.Name;
                            if (string.Equals(nodeName, "image", StringComparison.OrdinalIgnoreCase))
                            {
                                // Image
                                ProcessImageNode(creditXml, credit);
                            }
                            else if (string.Equals(nodeName, "url", StringComparison.OrdinalIgnoreCase))
                            {
                                // URL
                                ProcessUrlNode(creditXml, credit);
                            }

                            creditXml.Skip();
                        }
                        else
                        {
                            creditXml.Skip();
                        }
                    }

                    if (credit is not null)
                    {
                        if (result.Credits is null)
                        {
                            result.Credits = [credit];
                        }
                        else
                        {
                            result.Credits.Add(credit);
                        }
                    }
                }
                else
                {
                    creditsXml.Skip();
                }
            }
            else
            {
                creditsXml.Read();
            }
        }
    }

    private void ProcessStarRating(XmlReader reader, XmlTvProgram result)
    {
        /*
        <star-rating system="TV Guide">
            <value>4/5</value>
            <icon src="stars.png" />
        </star-rating>
        */

        var rating = new XmlTvStarRating();
        var system = reader.GetAttribute("system");
        if (!string.IsNullOrEmpty(system))
        {
            rating.System = system;
        }

        // Loop through each element
        using var starRatingXml = reader.ReadSubtree();
        starRatingXml.MoveToContent();
        starRatingXml.Read();
        while (!starRatingXml.EOF && starRatingXml.ReadState == ReadState.Interactive)
        {
            if (starRatingXml.NodeType == XmlNodeType.Element)
            {
                var nodeName = starRatingXml.Name;
                if (string.Equals(nodeName, "value", StringComparison.OrdinalIgnoreCase))
                {
                    // Value
                    var textValue = reader.ReadElementContentAsString();
                    int index = textValue.IndexOf('/', StringComparison.Ordinal);
                    if (index != -1)
                    {
                        var substring = textValue.Substring(index);
                        if (decimal.TryParse(substring, out var value))
                        {
                            rating.StarRating = value;
                        }
                        else
                        {
                            starRatingXml.Skip();
                            return;
                        }
                    }
                }
                else if (string.Equals(nodeName, "icon", StringComparison.OrdinalIgnoreCase))
                {
                    // Icon
                    ProcessIconNode(starRatingXml, rating);
                }

                starRatingXml.Skip();
            }
            else
            {
                starRatingXml.Skip();
            }
        }

        if (result.StarRatings is null)
        {
            result.StarRatings = [rating];
        }
        else
        {
            result.StarRatings.Add(rating);
        }
    }

    private void ProcessRating(XmlReader reader, XmlTvProgram result)
    {
        /*
        <rating system="MPAA">
            <value>NC-17</value>
            <icon src="NC-17_symbol.png" />
        </rating>
        */

        var rating = new XmlTvRating();
        var system = reader.GetAttribute("system");
        if (!string.IsNullOrEmpty(system))
        {
            rating.System = system;
        }

        // Loop through each element
        using var starRatingXml = reader.ReadSubtree();
        starRatingXml.MoveToContent();
        starRatingXml.Read();
        while (!starRatingXml.EOF && starRatingXml.ReadState == ReadState.Interactive)
        {
            if (starRatingXml.NodeType == XmlNodeType.Element)
            {
                var nodeName = starRatingXml.Name;
                if (string.Equals(nodeName, "value", StringComparison.OrdinalIgnoreCase))
                {
                    // Value
                    var value = reader.ReadElementContentAsString();
                    if (string.IsNullOrEmpty(value))
                    {
                        starRatingXml.Skip();
                        return;
                    }

                    rating.Value = value;
                }
                else if (string.Equals(nodeName, "icon", StringComparison.OrdinalIgnoreCase))
                {
                    // Icon
                    ProcessIconNode(starRatingXml, rating);
                }

                starRatingXml.Skip();
            }
            else
            {
                starRatingXml.Skip();
            }
        }

        if (result.Ratings is null)
        {
            result.Ratings = [rating];
        }
        else
        {
            result.Ratings.Add(rating);
        }
    }

    private void ProcessEpisodeNum(XmlReader reader, XmlTvProgram result)
    {
        result.Episode ??= new XmlTvEpisode();

        /*
        <episode-num system="dd_progid">EP00003026.0666</episode-num>
        <episode-num system="onscreen">2706</episode-num>
        <episode-num system="xmltv_ns">.26/0.</episode-num>
        */

        var episodeSystem = reader.GetAttribute("system");
        switch (episodeSystem)
        {
            case "dd_progid":
                ParseEpisodeDataForProgramId(reader, result);
                break;
            case "icetv":
                result.ProviderIds["icetv"] = reader.ReadElementContentAsString();
                break;
            case "xmltv_ns":
                ParseEpisodeDataForXmlTvNs(reader, result);
                break;
            case "onscreen":
                ParseEpisodeDataForOnScreen(reader);
                break;
            case "thetvdb.com":
                ParseTvdbSystem(reader, result);
                break;
            case "imdb.com":
                ParseImdbSystem(reader, result);
                break;
            case "themoviedb.org":
                ParseMovieDbSystem(reader, result);
                break;
            case "SxxExx":
                ParseSxxExxSystem(reader, result);
                break;
            default: // Handles empty string and nulls
                reader.Skip();
                break;
        }
    }

    private void ParseSxxExxSystem(XmlReader reader, XmlTvProgram result)
    {
        // <episode-num system="SxxExx">S012E32</episode-num>

        var value = reader.ReadElementContentAsString();
        var res = Regex.Match(value, "s([0-9]+)e([0-9]+)", RegexOptions.IgnoreCase);

        if (res.Success)
        {
            int parsedInt;

            if (int.TryParse(res.Groups[1].Value, out parsedInt))
            {
                result.Episode!.Series = parsedInt;
            }

            if (int.TryParse(res.Groups[2].Value, out parsedInt))
            {
                result.Episode!.Episode = parsedInt;
            }
        }
    }

    private void ParseMovieDbSystem(XmlReader reader, XmlTvProgram result)
    {
        // <episode-num system="thetvdb.com">series/248841</episode-num>
        // <episode-num system="thetvdb.com">episode/4749206</episode-num>

        var value = reader.ReadElementContentAsString();
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (string.Equals(parts[0], "series", StringComparison.OrdinalIgnoreCase))
        {
            result.SeriesProviderIds["tmdb"] = parts[1];
        }
        else if (parts.Length == 1
            || string.Equals(parts[0], "episode", StringComparison.OrdinalIgnoreCase))
        {
            result.ProviderIds["tmdb"] = parts.Last();
        }
    }

    private void ParseImdbSystem(XmlReader reader, XmlTvProgram result)
    {
        // <episode-num system="imdb.com">series/tt1837576</episode-num>
        // <episode-num system="imdb.com">episode/tt3288596</episode-num>

        var value = reader.ReadElementContentAsString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return;
        }

        if (string.Equals(parts[0], "series", StringComparison.OrdinalIgnoreCase))
        {
            result.SeriesProviderIds["imdb"] = parts[1];
        }
        else if (string.Equals(parts[0], "episode", StringComparison.OrdinalIgnoreCase))
        {
            result.ProviderIds["imdb"] = parts[1];
        }
    }

    private void ParseTvdbSystem(XmlReader reader, XmlTvProgram result)
    {
        // <episode-num system="thetvdb.com">series/248841</episode-num>
        // <episode-num system="thetvdb.com">episode/4749206</episode-num>

        var value = reader.ReadElementContentAsString();
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return;
        }

        if (string.Equals(parts[0], "series", StringComparison.OrdinalIgnoreCase))
        {
            result.SeriesProviderIds["tvdb"] = parts[1];
        }
        else if (string.Equals(parts[0], "episode", StringComparison.OrdinalIgnoreCase))
        {
            result.ProviderIds["tvdb"] = parts[1];
        }
    }

    private void ParseEpisodeDataForOnScreen(XmlReader reader)
    {
        reader.Skip();
        // _ = reader;
        // _ = result;

        // example: 'Episode #FFEE'
        // TODO: This could be textual - how do we populate an Int32

        // var value = reader.ReadElementContentAsString();
        // value = HttpUtility.HtmlDecode(value);
        // value = value.Replace(" ", "");

        // Take everything from the hash to the end.
        // var hashIndex = value.IndexOf('#');
        // if (hashIndex > -1)
        // {
        //     result.EpisodeNumber
        // }
    }

    private void ParseEpisodeDataForProgramId(XmlReader reader, XmlTvProgram result)
    {
        var value = reader.ReadElementContentAsString();
        if (!string.IsNullOrWhiteSpace(value))
        {
            result.ProgramId = value;
        }
    }

    private void ParseEpisodeDataForXmlTvNs(XmlReader reader, XmlTvProgram result)
    {
        var value = reader.ReadElementContentAsString();

        value = value.Replace(" ", string.Empty, StringComparison.Ordinal);

        // Episode details
        var components = value.Split('.');

        int parsedInt;
        if (!string.IsNullOrEmpty(components[0]))
        {
            // Handle either "5/12" or "5"
            var seriesComponents = components[0].Split('/');

            // handle the zero basing!
            if (int.TryParse(seriesComponents[0], out parsedInt))
            {
                result.Episode!.Series = parsedInt + 1;
                if (seriesComponents.Length == 2
                    && int.TryParse(seriesComponents[1], out parsedInt))
                {
                    result.Episode.SeriesCount = parsedInt;
                }
            }
        }

        if (components.Length >= 2)
        {
            if (!string.IsNullOrEmpty(components[1]))
            {
                // Handle either "5/12" or "5"
                var episodeComponents = components[1].Split('/');

                // handle the zero basing!
                if (int.TryParse(episodeComponents[0], out parsedInt))
                {
                    result.Episode!.Episode = parsedInt + 1;
                    if (episodeComponents.Length == 2
                        && int.TryParse(episodeComponents[1], out parsedInt))
                    {
                        result.Episode.EpisodeCount = parsedInt;
                    }
                }
            }
        }

        if (components.Length >= 3)
        {
            if (!string.IsNullOrEmpty(components[2]))
            {
                // Handle either "5/12" or "5"
                var partComponents = components[2].Split('/');

                // handle the zero basing!
                if (int.TryParse(partComponents[0], out parsedInt))
                {
                    result.Episode!.Part = parsedInt + 1;
                    if (partComponents.Length == 2
                        && int.TryParse(partComponents[1], out parsedInt))
                    {
                        result.Episode.PartCount = parsedInt;
                    }
                }
            }
        }
    }

    private void ProcessQuality(XmlReader reader, XmlTvProgram result)
    {
        result.Quality = reader.ReadElementContentAsString();
    }

    private void ProcessPreviouslyShown(XmlReader reader, XmlTvProgram result)
    {
        // <previously-shown start="20070708000000" />
        var value = reader.GetAttribute("start");
        if (!string.IsNullOrEmpty(value))
        {
            // TODO: this may not be correct = validate it
            result.PreviouslyShown = ParseDate(value);
            if (result.PreviouslyShown != result.StartDate)
            {
                result.IsPreviouslyShown = true;
            }
        }
        else
        {
            result.IsPreviouslyShown = true;
        }

        reader.Skip(); // Move on
    }

    private void ProcessNew(XmlReader reader, XmlTvProgram result)
    {
        result.IsNew = true;
        reader.Skip(); // Move on
    }

    private void ProcessLive(XmlReader reader, XmlTvProgram result)
    {
        result.IsLive = true;
        reader.Skip(); // Move on
    }

    private void ProcessLanguage(XmlReader reader, XmlTvProgram result)
    {
        result.Language = reader.ReadElementContentAsString();
        reader.Skip(); // Move on
    }

    private void ProcessOriginalLanguage(XmlReader reader, XmlTvProgram result)
    {
        result.OriginalLanguage = reader.ReadElementContentAsString();
        reader.Skip(); // Move on
    }

    private void ProcessCategory(XmlReader reader, XmlTvProgram result)
    {
        /*
        <category lang="en">News</category>
        */

        ProcessMultipleNodes(reader, result.Categories.Add, _language);
    }

    private void ProcessKeyword(XmlReader reader, XmlTvProgram result)
    {
        /*
        <keyword lang="en">News</category>
        */

        ProcessMultipleNodes(reader, result.Keywords.Add, _language);
    }

    private void ProcessCountry(XmlReader reader, XmlTvProgram result)
    {
        /*
        <country>Canadá</country>
        <country>EE.UU</country>
        */

        ProcessNode(reader, result.Countries.Add, _language);
    }

    private void ProcessSubTitle(XmlReader reader, XmlTvProgram result)
    {
        result.Episode ??= new XmlTvEpisode();

        /*
        <sub-title lang="en">Gino&apos;s Italian Escape - Islands in the Sun: Southern Sardinia Celebrate the Sea</sub-title>
        <sub-title lang="en">8782</sub-title>
        */
        ProcessNode(reader, s => result.Episode.Title = s, _language);
    }

    private void ProcessDescription(XmlReader reader, XmlTvProgram result)
    {
        ProcessNode(reader, s => result.Description = s, _language);
    }

    private void ProcessTitleNode(XmlReader reader, XmlTvProgram result)
    {
        // <title lang="en">Gino&apos;s Italian Escape</title>
        ProcessNode(reader, s => result.Title = s, _language);
    }

    private void ProcessPremiereNode(XmlReader reader, XmlTvProgram result)
    {
        // <title lang="en">Gino&apos;s Italian Escape</title>
        ProcessNode(
            reader,
            s =>
            {
                result.Premiere = new XmlTvPremiere(s);
            },
            _language);
    }

    private void ProcessIconNode(XmlReader reader, IHasIcons output)
    {
        var result = new XmlTvIcon();
        var isPopulated = false;

        var source = reader.GetAttribute("src");
        if (!string.IsNullOrEmpty(source))
        {
            result.Source = source;
            isPopulated = true;
        }

        var widthString = reader.GetAttribute("width");
        if (!string.IsNullOrEmpty(widthString) && int.TryParse(widthString, out int width))
        {
            result.Width = width;
            isPopulated = true;
        }

        var heightString = reader.GetAttribute("height");
        if (!string.IsNullOrEmpty(heightString) && int.TryParse(heightString, out int height))
        {
            result.Height = height;
            isPopulated = true;
        }

        if (!isPopulated)
        {
            return;
        }

        if (output.Icons == null)
        {
            output.Icons = [result];

            return;
        }

        output.Icons.Add(result);
    }

    private void ProcessImageNode(XmlReader reader, IHasImages output)
    {
        var result = new XmlTvImage();
        var isPopulated = false;

        var typeString = reader.GetAttribute("type");
        if (!string.IsNullOrEmpty(typeString) && Enum.TryParse<ImageType>(typeString, true, out var parsedType))
        {
            result.Type = parsedType;
            isPopulated = true;
        }

        var sizeString = reader.GetAttribute("size");
        if (!string.IsNullOrEmpty(sizeString) && Enum.TryParse<ImageSize>(sizeString, true, out var parsedSize))
        {
            result.Size = parsedSize;
            isPopulated = true;
        }

        var orientationString = reader.GetAttribute("orient");
        if (!string.IsNullOrEmpty(orientationString) && Enum.TryParse<ImageOrientation>(orientationString, true, out var parsedOrientation))
        {
            result.Orientation = parsedOrientation;
            isPopulated = true;
        }

        var system = GetSystem(reader.GetAttribute("system"));
        if (system is not null)
        {
            result.System = system;
            isPopulated = true;
        }

        var path = reader.ReadElementContentAsString();
        if (!string.IsNullOrEmpty(path))
        {
            result.Path = path;
            isPopulated = true;
        }

        if (!isPopulated)
        {
            return;
        }

        if (output.Images == null)
        {
            output.Images = [result];

            return;
        }

        output.Images.Add(result);
    }

    private void ProcessUrlNode(XmlReader reader, IHasUrls output)
    {
        var result = new XmlTvUrl();
        var isPopulated = false;

        var system = GetSystem(reader.GetAttribute("system"));
        if (!string.IsNullOrEmpty(system))
        {
            result.System = system;
            isPopulated = true;
        }

        var uri = reader.ReadElementContentAsString();
        if (!string.IsNullOrEmpty(uri))
        {
            result.Uri = uri;
            isPopulated = true;
        }

        if (!isPopulated)
        {
            return;
        }

        if (output.Urls == null)
        {
            output.Urls = [result];

            return;
        }

        output.Urls.Add(result);
    }

    private string? GetSystem(string? systemString)
    {
        return systemString switch
        {
            "tmdb" => "tmdb",
            "themoviedb" => "tmdb",
            "themoviedb.org" => "tmdb",
            "moviedb" => "tmdb",
            "tvdb" => "tvdb",
            "thetvdb" => "tvdb",
            "thetvdb.com" => "tvdb",
            "imdb" => "imdb",
            "imdb.com" => "imdb",
            _ => null
        };
    }

    private void ProcessNode(
        XmlReader reader,
        Action<string> setter,
        string? languageRequired = null,
        Action<string>? allOccurrencesSetter = null)
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

        var results = new List<(string, string?)>();

        // We will always use the first value - so that if there are no matches we can return something
        var currentElementName = reader.Name;

        var lang = reader.HasAttributes ? reader.GetAttribute("lang") : null;
        var currentValue = reader.ReadElementContentAsString();
        results.Add((currentValue, lang));

        allOccurrencesSetter?.Invoke(currentValue);

        while (!reader.EOF && reader.ReadState == ReadState.Interactive)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == currentElementName)
                {
                    lang = reader.HasAttributes ? reader.GetAttribute("lang") : null;
                    currentValue = reader.ReadElementContentAsString();

                    allOccurrencesSetter?.Invoke(currentValue);

                    results.Add((currentValue, lang));
                }
                else
                {
                    break;
                }
            }
            else
            {
                reader.Read();
            }
        }

        foreach (var result in results)
        {
            if (string.Equals(languageRequired, result.Item2, StringComparison.OrdinalIgnoreCase))
            {
                setter(result.Item1);
                return;
            }
        }

        foreach (var result in results)
        {
            if (string.IsNullOrWhiteSpace(result.Item2))
            {
                setter(result.Item1);
                return;
            }
        }

        foreach (var result in results)
        {
            setter(result.Item1);
            return;
        }
    }

    private void ProcessMultipleNodes(XmlReader reader, Action<string> setter, string? languageRequired = null)
    {
        /* <category lang="en">Property - English</category>
            * <category lang="en">Property - English 2</category>
            * <category lang="es">Property - Spanish</category>
            * <category lang="es">Property - Spanish 2</category>
            * <category lang="">Property - Empty Language</category>
            * <category lang="">Property - Empty Language 2</category>
            * <category>Property - No Language</category>
            * <category>Property - No Language 2</category>
            */

        /* Expected Behaviour:
            *  - Language = Null   Property - No Language / Property - No Language 2
            *  - Language = ""     Property - Empty Language / Property - Empty Language 2
            *  - Language = es     Property - Spanish / Property - Spanish 2
            *  - Language = en     Property - English / Property - English 2
            */

        var currentElementName = reader.Name;
        var values = new List<(string? Language, string Value)>();

        while (!reader.EOF && reader.ReadState == ReadState.Interactive)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == currentElementName)
                {
                    values.Add((reader.GetAttribute("lang"), reader.ReadElementContentAsString()));
                }
                else
                {
                    break;
                }
            }
            else
            {
                reader.Read();
            }
        }

        if (values.Any(v => v.Language == languageRequired))
        {
            values.RemoveAll(v => v.Language != languageRequired);
        }

        // Enumerate and return all the matches
        foreach (var (language, value) in values)
        {
            setter(value);
        }
    }

    private void PopulateHeader(XmlReader reader, XmlTvProgram result)
    {
        var startValue = reader.GetAttribute("start");
        if (string.IsNullOrEmpty(startValue))
        {
            result.StartDate = DateTimeOffset.MinValue;
        }
        else
        {
            result.StartDate = ParseDate(startValue).GetValueOrDefault();
        }

        var endValue = reader.GetAttribute("stop");
        if (string.IsNullOrEmpty(endValue))
        {
            result.EndDate = DateTimeOffset.MinValue;
        }
        else
        {
            result.EndDate = ParseDate(endValue).GetValueOrDefault();
        }
    }

    /// <summary>
    /// Parses a string into <see cref="DateTimeOffset"/> if possible.
    /// </summary>
    /// <param name="dateValue">The input string.</param>
    /// <returns>DateTimeOffset.</returns>
    public static DateTimeOffset? ParseDate(string dateValue)
    {
        /*
        All dates and times in this DTD follow the same format, loosely based
        on ISO 8601.  They can be 'YYYYMMDDhhmmss' or some initial
        substring, for example if you only know the year and month you can
        have 'YYYYMM'.  You can also append a timezone to the end; if no
        explicit timezone is given, UTC is assumed.  Examples:
        '200007281733 BST', '200209', '19880523083000 +0300'.  (BST == +0100.)
        */

        if (string.IsNullOrEmpty(dateValue))
        {
            return null;
        }

        const string CompleteDate = "20000101000000";
        string? dateOffset = null;
        var match = DateWithOffsetRegex().Match(dateValue);
        string? dateComponent;
        if (match.Success)
        {
            dateComponent = match.Groups["dateDigits"].Value;
            if (!string.IsNullOrEmpty(match.Groups["dateOffset"].Value))
            {
                var tmpDateOffset = match.Groups["dateOffset"].Value; // Add in the colon to ease parsing later
                if (tmpDateOffset.Length == 5)
                {
                    dateOffset = tmpDateOffset.Insert(3, ":"); // Add in the colon to ease parsing later
                }
            }
        }
        else
        {
            return null;
        }

        // Pad out the date component part to 14 characaters so 2016061509 becomes 20160615090000
        if (dateComponent.Length < 14)
        {
            dateComponent += CompleteDate[dateComponent.Length..];
        }

        if (dateOffset is null)
        {
            if (DateTimeOffset.TryParseExact(dateComponent, "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedDateTime))
            {
                return parsedDateTime;
            }
        }
        else
        {
            var standardDate = string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}",
                dateComponent,
                dateOffset);
            if (DateTimeOffset.TryParseExact(standardDate, "yyyyMMddHHmmss zzz", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedDateTime))
            {
                return parsedDateTime;
            }
        }

        return null;
    }

    /// <summary>
    /// Standardized the date.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The standardized date string.</returns>
    public string StandardiseDate(string value)
    {
        const string CompleteDate = "20000101000000";
        var dateComponent = string.Empty;
        var dateOffset = "+0000";

        var match = DateWithOffsetRegex().Match(value);
        if (match.Success)
        {
            dateComponent = match.Groups["dateDigits"].Value;
            dateOffset = match.Groups["dateOffset"].Value;
        }

        // Pad out the date component part to 14 characaters so 2016061509 becomes 20160615090000
        if (dateComponent.Length < 14)
        {
            dateComponent += CompleteDate[dateComponent.Length..];
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1}",
            dateComponent,
            dateOffset);
    }
}
