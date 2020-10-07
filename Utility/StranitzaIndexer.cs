using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace stranitza.Utility
{
    public class StranitzaIndexer
    {
        public class IndexingResult
        {
            public string PdfFilePath { get; set; }

            [JsonIgnore]
            public bool PrecedingOrigins { get; set; }

            public List<IndexEntry> Entries { get; set; } = new List<IndexEntry>();

            public List<string> Origins { get; set; } = new List<string>();

            public List<string> Categories { get; set; } = new List<string>();

            public List<string> Unclassified { get; set; } = new List<string>();

            public int NumberOfPages { get; set; }

            public string Compiler { get; set; }

            public IndexingResult(string pdfFilePath)
            {
                PdfFilePath = pdfFilePath;
            }
        }

        public class IndexEntry
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Origin { get; set; }

            public string Title { get; set; }

            public string Pages { get; set; }

            public int StartingPage { get; set; }

            public int EndPage { get; set; }

            public int? SuggestedCategoryId { get; set; }

            public int? ConflictingSourceId { get; set; }

            public string OriginalLine { get; set; }
        }

        private static readonly Regex PageNumberRegex = new Regex("\\s\\.\\s?(?<PageNumber>\\d+)\\s?$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public int IndexPageNumber { get; set; } = 3;

        protected string IndexLocator { get; set; } = "Съдържание";
        
        protected string CompilerLocator { get; set; } = "Съставител";

        public List<string> CategoriesLocator { get; set; }

        public Dictionary<int, Regex> CategoriesClassifier { get; set; }

        public IndexingResult IndexIssue(string pdfFilePath)
        {
            WriteLog(""); // empty line
            WriteLog("Започва.");

            var r = new IndexingResult(pdfFilePath);
            
            var indexAsText = GetIndexAsText(r);

            WriteLog("Разбор на съдържанието...");
            ParseIndex(r, indexAsText);

            WriteLog("Разбор на произведенията...");
            DeduceFromEntryTitles(r);

            MapIndexEntriesToOrigin(r);
            SplitOriginNames(r);
            WriteLog("Завърши.");
            WriteLog(""); // empty line

            return r;
        }

        private void ParseIndex(IndexingResult r, IReadOnlyList<string> textLines)
        {
            WriteLog("Събиране на информация за произведенията...");

            var accumulator = string.Empty;
            var originCandidatesList = new List<string>();
            r.PrecedingOrigins = true;

            int? firstMatchedEntry = null;
            for (var i = 0; i < textLines.Count; i++)
            {
                var line = textLines[i];
                var match = PageNumberRegex.Match(line);
                if (!match.Success)
                {
                    // accumulate entry with a new line for further inspection
                    // - it could be a double lined index entry
                    // - or we could be accumulating origins
                    accumulator += line + Environment.NewLine;
                }
                else
                {
                    // regex successfully identified an index entry

                    if (!firstMatchedEntry.HasValue)
                    {
                        // mark the first matched entry number
                        firstMatchedEntry = i;
                    }

                    if (i == 0)
                    {
                        // if we have matched the first line against the regex
                        // then the origin information is not preceding the entries
                        r.PrecedingOrigins = false;
                    }

                    // on rare occasions this could be resolved
                    // from the index alone during parsing,
                    // otherwise we need further info
                    int? categoryId = null;

                    // extract pageNumber from regex capture group
                    var pageNumber = match.Groups["PageNumber"].Value;

                    // save reference for enhancement
                    var originalLine = line;

                    // remove it from the line to form title
                    var title = line.Replace(pageNumber, string.Empty);

                    // trim all trailing dots and spaces
                    // this works for entries without titles also
                    // it will preserve the empty string
                    title = TrimTitle(title);

                    if (!string.IsNullOrEmpty(accumulator))
                    {
                        if (r.PrecedingOrigins && firstMatchedEntry == i)
                        {
                            // if origins are preceding index entries content
                            // populate origin information and flush accumulator
                            originCandidatesList.AddRange(accumulator.Split(Environment.NewLine,
                                StringSplitOptions.RemoveEmptyEntries).ToList());
                        }
                        else
                        {
                            var accumulatedLine = accumulator.Replace(Environment.NewLine, " ");
                            // some entries title information can be spanned on to several lines
                            // concatenate the information with the current title and flush the accumulator
                            title = accumulatedLine + title;

                            // append those lines to the original line also
                            originalLine = accumulatedLine + originalLine;
                        }

                        accumulator = string.Empty;
                    }

                    var pages = pageNumber;
                    var startingPage = int.Parse(pageNumber);
                    if (r.Entries.Any())
                    {
                        // if there was a previous index entry recorded try to
                        // deduce the range of pages for that piece

                        var previousEntry = r.Entries.Last();

                        // if these are equal - the two pieces are on consecutive pages
                        // if the previous is greater than the current number -
                        // they most probably are on the same page
                        // otherwise - if the previous number is lower than the current -
                        // most probably this is the ending page for the previous piece
                        if (previousEntry.StartingPage < startingPage - 1)
                        {
                            previousEntry.EndPage = startingPage - 1;
                            previousEntry.Pages += "-" + previousEntry.EndPage;
                        }
                    }

                    // NOTE: deduce categories here
                    // and maybe origin

                    // construct entry
                    var entry = new IndexEntry()
                    {
                        StartingPage = startingPage,
                        EndPage = startingPage,
                        SuggestedCategoryId = categoryId,
                        OriginalLine = originalLine,
                        Pages = pages,
                        Title = title
                    };

                    r.Entries.Add(entry);
                }
            }

            // resolve the end page of the last entry

            var lastEntry = r.Entries.Last();
            var lastEntryStartingPage = lastEntry.StartingPage;
            if (r.NumberOfPages != lastEntryStartingPage)
            {
                lastEntry.Pages += "-" + r.NumberOfPages;
            }

            // in any cases - gather all lines left in the
            // accumulator and flush accumulator for further use
            originCandidatesList.AddRange(accumulator.Split(Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries).ToList());
            accumulator = string.Empty;

            WriteLog("Намерени: " + r.Entries.Count + " произведения");

            WriteLog("Събиране на информация за произход...");

            r.Categories = new List<string>();
            r.Unclassified = new List<string>();

            for (int i = 0; i < originCandidatesList.Count; i++)
            {
                var origin = originCandidatesList[i];

                if (origin.Equals(IndexLocator))
                {
                    continue;
                }

                if (origin.Contains(CategoriesLocator))
                {
                    // flush accumulator
                    // originsList.Add(accumulator);
                    // accumulator = string.Empty;
                    r.Categories.Add(origin);
                    continue;
                }

                if (origin.Contains(CompilerLocator))
                {
                    r.Compiler = origin;
                    continue;
                }

                if (!string.IsNullOrEmpty(accumulator))
                {
                    origin = accumulator + " " + origin;
                    accumulator = string.Empty;
                }

                if (origin.StartsWith("и ", StringComparison.CurrentCulture))
                {
                    var lastOrigin = r.Origins.Last();
                    var updatedOrigin = lastOrigin + " " + origin;

                    r.Origins.RemoveAt(r.Origins.Count - 1);
                    r.Origins.Add(updatedOrigin);

                    continue;
                }

                if (origin.EndsWith(" и", StringComparison.CurrentCulture))
                {
                    accumulator += origin;
                    continue;
                }

                if (origin.EndsWith("."))   // most common use case
                {
                    var last = origin.Split(" ").Last().TrimEnd('.');
                    if (last.All(char.IsUpper))
                    {
                        r.Origins.Add(origin.Remove(origin.Length - 1));
                        continue;
                    }
                }

                // further inspection required
                r.Unclassified.Add(origin);
            }

            WriteLog("Намерени: " + r.Origins.Count + " автора");
        }

        private void DeduceFromEntryTitles(IndexingResult r)
        {
            foreach (var classifier in CategoriesClassifier)
            {
                var id = classifier.Key;
                var regex = classifier.Value;

                foreach (var entry in r.Entries)
                {
                    var match = regex.Match(entry.Title);

                    if (match.Success)
                    {
                        entry.SuggestedCategoryId = id;
                        WriteLog($"За произведение '{entry.Title}' беше установена категория #{entry.SuggestedCategoryId}");
                        var origin = match.Groups.GetValueOrDefault("Origin", null);
                        if (origin != null)
                        {
                            entry.Origin = origin.Value;
                            WriteLog($"За произведение '{entry.Title}' беше установен произход: {entry.Origin}");
                        }
                    }
                }
            }
        }

        private void MapIndexEntriesToOrigin(IndexingResult r)
        {
            WriteLog($"Свързване на произведение с произход...");

            var originCandidatesList = new List<string>(r.Origins);
            var unresolvedEntriesList = r.Entries.Where(x => string.IsNullOrEmpty(x.Origin)).ToList();
            using (var pdf = PdfDocument.Open(r.PdfFilePath))
            {
                foreach (var indexEntry in unresolvedEntriesList)
                {
                    WriteLog($"Търсене на произход в страница '{indexEntry.StartingPage}'...");

                    var page = pdf.GetPage(indexEntry.StartingPage);
                    var pageWords = page.GetWords().Select(x => x.Text);
                    var pageText = string.Join(" ", pageWords);

                    foreach (var origin in originCandidatesList)
                    {
                        // First check looks up if the two names from the origin are to be found on the starting page

                        if (pageText.Contains(origin, StringComparison.CurrentCultureIgnoreCase))
                        {
                            indexEntry.Origin = origin;
                            originCandidatesList.Remove(origin);

                            WriteLog($"Произходът за произведение '{indexEntry.Title}' е установен: {origin}");

                            break;
                        }
                    }
                }

                unresolvedEntriesList = r.Entries.Where(x => string.IsNullOrEmpty(x.Origin)).ToList();
                if (unresolvedEntriesList.Any())
                {
                    WriteLog($"Произходът не бе установен за {unresolvedEntriesList.Count} произведения.");
                    WriteLog($"Започва детайлен разбор...");

                    foreach (var indexEntry in unresolvedEntriesList)
                    {
                        WriteLog($"Търсене на произход в страници '{indexEntry.Pages}'...");

                        for (int i = indexEntry.StartingPage; i <= indexEntry.EndPage; i++)
                        {
                            var page = pdf.GetPage(i);

                            var pageWords = page.GetWords().Select(x => x.Text);
                            var pageText = string.Join(" ", pageWords);

                            foreach (var origin in originCandidatesList)
                            {
                                // Go through all the index entry pages to map the origin information

                                if (pageText.Contains(origin, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    indexEntry.Origin = origin;
                                    originCandidatesList.Remove(origin);

                                    WriteLog($"Произходът за произведение '{indexEntry.Title}' е установен: {origin}");

                                    break;
                                }
                            }
                        }
                    }

                    if (unresolvedEntriesList.Count == originCandidatesList.Count)
                    {
                        WriteLog($"Установяване на произход по поредност...");

                        for (int i = 0; i < unresolvedEntriesList.Count; i++)
                        {
                            unresolvedEntriesList[i].Origin = originCandidatesList[i];
                        }
                    }
                    else
                    {
                        WriteLog($"Произходът не успя да бъде установен за {unresolvedEntriesList.Count} записа, с " +
                                 $"{originCandidatesList.Count} записа останала информация за произход.");
                    }

                    if (originCandidatesList.Any())
                    {
                        r.Unclassified.AddRange(originCandidatesList);
                    }
                }
            }
        }

        private void SplitOriginNames(IndexingResult r)
        {
            Console.WriteLine($"Установявавне на две имена на автор от произход...");

            foreach (var indexEntry in r.Entries)
            {
                if (string.IsNullOrEmpty(indexEntry.Origin))
                {
                    continue;
                }

                var names = indexEntry.Origin.Split(" ");

                if (names.Length < 2)
                {
                    // with only one name we cannot proceed
                }
                else if (names.Length == 2)
                {
                    indexEntry.FirstName = StranitzaExtensions.Capitalize(names.First());
                    indexEntry.LastName = StranitzaExtensions.Capitalize(names.Last());
                }
                else if (names.Length > 2)
                {
                    indexEntry.FirstName = StranitzaExtensions.Capitalize(names[0]);
                    indexEntry.LastName = StranitzaExtensions.Capitalize(names[1]);
                }
            }
        }

        private string[] GetIndexAsText(IndexingResult r)
        {
            WriteLog($"Извличане на съдържанието (стр. №{IndexPageNumber}) от файл '{r.PdfFilePath}'.");

            using (var pdf = PdfDocument.Open(r.PdfFilePath))
            {
                r.NumberOfPages = pdf.NumberOfPages;
                WriteLog($"Общ брой на страниците: {r.NumberOfPages}");
                var page = pdf.GetPage(IndexPageNumber);

                var pdfText = ContentOrderTextExtractor.GetText(page);
                WriteLog($"Съдържанието е извлечено успешно: {pdfText.Length} символа");
                return pdfText.Split(Environment.NewLine);
            }
        }

        private static void WriteLog(string logMessage)
        {
            Log.Logger.Information(logMessage);
        }

        private static string TrimTitle(string title)
        {
            return title.TrimEnd(' ', '.');
        }
    }
}
