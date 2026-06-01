using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kaleidoscope2.Menu.About
{
    internal sealed class KaelisAboutContent
    {
        private const string AboutPackageRelativePath = "_Project/Kaleidoscope2/About";

        public string ProjectTitle { get; private set; }
        public string ShortDescription { get; private set; }
        public string Author { get; private set; }
        public string Music { get; private set; }
        public string PseudonymNote { get; private set; }
        public string YouTube { get; private set; }
        public string PrimaryEmail { get; private set; }
        public string MusicEmail { get; private set; }
        public string Copyright { get; private set; }
        public string MusicRightsNote { get; private set; }
        public string CreationStory { get; private set; }
        public string CreditsAndLicense { get; private set; }
        public string License { get; private set; }

        public string GetDisplayText()
        {
            return string.Join("\n", new[]
            {
                ProjectTitle,
                ShortDescription,
                Author,
                Music,
                PseudonymNote,
                YouTube,
                PrimaryEmail,
                MusicEmail,
                Copyright,
                License,
                MusicRightsNote,
                CreationStory,
                CreditsAndLicense,
                "ABOUT",
                "О ПРОГРАММЕ",
                "ÜBER DAS PROGRAMM",
                "ПРО ПРОГРАМУ",
                "© “ ” ‘ ’ — – /"
            });
        }

        public static bool TryLoad(out KaelisAboutContent content, out string error)
        {
            content = null;
            error = null;

            string packagePath = Path.Combine(Application.dataPath, AboutPackageRelativePath);
            if (!Directory.Exists(packagePath))
            {
                error = "About package not found at Assets/" + AboutPackageRelativePath + ".";
                return false;
            }

            try
            {
                string metadataText = ReadRequired(packagePath, "kaelis_about_metadata.json");
                KaelisAboutMetadata metadata = JsonUtility.FromJson<KaelisAboutMetadata>(metadataText);
                if (metadata == null)
                {
                    error = "About metadata could not be parsed.";
                    return false;
                }

                string creationStory = ReadRequired(packagePath, "CREATION_STORY.md");
                ReadRequired(packagePath, "CREDITS.md");
                string license = ReadRequired(packagePath, "LICENSE_ALL_RIGHTS_RESERVED.txt");
                ReadRequired(packagePath, "ABOUT_CONTENT.md");
                ReadRequired(packagePath, "AUTHOR_AND_ARTIST_IDENTITY.md");
                ReadRequired(packagePath, "MUSIC_RIGHTS_AND_CREDITS.md");

                content = new KaelisAboutContent
                {
                    ProjectTitle = metadata.project,
                    ShortDescription = !string.IsNullOrWhiteSpace(metadata.aboutShort)
                        ? metadata.aboutShort
                        : "Interactive audiovisual kaleidoscope / visual instrument.",
                    Author = metadata.author,
                    Music = metadata.artistPseudonym,
                    PseudonymNote = metadata.artistPseudonymNote,
                    YouTube = metadata.youtube,
                    PrimaryEmail = metadata.emails != null && metadata.emails.Length > 0 ? metadata.emails[0] : string.Empty,
                    MusicEmail = metadata.emails != null && metadata.emails.Length > 1 ? metadata.emails[1] : string.Empty,
                    Copyright = metadata.copyright,
                    MusicRightsNote = metadata.musicCredit,
                    License = !string.IsNullOrWhiteSpace(metadata.license) ? metadata.license : "All Rights Reserved",
                    CreationStory = NormalizeMarkdown(creationStory),
                    CreditsAndLicense = BuildCreditsAndLicenseSummary(metadata, license)
                };

                return content.HasRequiredContent(out error);
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private bool HasRequiredContent(out string error)
        {
            if (string.IsNullOrWhiteSpace(ProjectTitle)
                || string.IsNullOrWhiteSpace(Author)
                || string.IsNullOrWhiteSpace(Music)
                || string.IsNullOrWhiteSpace(PseudonymNote)
                || string.IsNullOrWhiteSpace(YouTube)
                || string.IsNullOrWhiteSpace(PrimaryEmail)
                || string.IsNullOrWhiteSpace(MusicEmail)
                || string.IsNullOrWhiteSpace(Copyright)
                || string.IsNullOrWhiteSpace(MusicRightsNote)
                || string.IsNullOrWhiteSpace(License)
                || string.IsNullOrWhiteSpace(CreationStory)
                || string.IsNullOrWhiteSpace(CreditsAndLicense))
            {
                error = "About package is present but required content is incomplete.";
                return false;
            }

            error = null;
            return true;
        }

        private static string ReadRequired(string packagePath, string fileName)
        {
            string path = Path.Combine(packagePath, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Required About package file is missing: " + fileName, path);
            }

            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static string BuildCreditsAndLicenseSummary(KaelisAboutMetadata metadata, string licenseText)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Application");
            builder.AppendLine(metadata.project);
            builder.AppendLine();
            builder.AppendLine("Created by");
            builder.AppendLine(metadata.author);
            builder.AppendLine();
            builder.AppendLine("Music and Sound");
            builder.AppendLine(metadata.musicCredit);
            builder.AppendLine(metadata.artistPseudonymNote);
            builder.AppendLine();
            builder.AppendLine("YouTube");
            builder.AppendLine(metadata.youtube);
            builder.AppendLine();
            builder.AppendLine("Contact");
            if (metadata.emails != null)
            {
                for (int index = 0; index < metadata.emails.Length; index++)
                {
                    if (!string.IsNullOrWhiteSpace(metadata.emails[index]))
                    {
                        builder.AppendLine(metadata.emails[index]);
                    }
                }
            }

            builder.AppendLine();
            builder.AppendLine("License");
            builder.AppendLine(!string.IsNullOrWhiteSpace(metadata.license) ? metadata.license : "All Rights Reserved");
            builder.AppendLine(NormalizeMarkdown(licenseText));
            return builder.ToString().Trim();
        }

        private static string NormalizeMarkdown(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string[] lines = value.Replace("\r\n", "\n").Split('\n');
            StringBuilder builder = new StringBuilder();
            string[] tableHeader = null;

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].TrimEnd();
                string trimmed = line.Trim();

                if (IsMarkdownTableSeparator(trimmed))
                {
                    continue;
                }

                if (IsMarkdownTableRow(trimmed))
                {
                    string[] cells = ParseMarkdownTableRow(trimmed);
                    if (tableHeader == null)
                    {
                        tableHeader = cells;
                        continue;
                    }

                    AppendConvertedTableRow(builder, tableHeader, cells);
                    continue;
                }

                tableHeader = null;

                if (trimmed.StartsWith("# ", StringComparison.Ordinal))
                {
                    builder.AppendLine(trimmed.Substring(2).Trim());
                    continue;
                }

                if (trimmed.StartsWith("## ", StringComparison.Ordinal))
                {
                    builder.AppendLine(trimmed.Substring(3).Trim());
                    continue;
                }

                if (trimmed.StartsWith("- ", StringComparison.Ordinal))
                {
                    builder.AppendLine("• " + CleanupInlineMarkdown(trimmed.Substring(2).Trim()));
                    continue;
                }

                builder.AppendLine(CleanupInlineMarkdown(line));
            }

            return Regex.Replace(builder.ToString().Trim(), @"\n{3,}", "\n\n");
        }

        private static bool IsMarkdownTableRow(string line)
        {
            return line.Length > 2 && line[0] == '|' && line[line.Length - 1] == '|';
        }

        private static bool IsMarkdownTableSeparator(string line)
        {
            if (!IsMarkdownTableRow(line))
            {
                return false;
            }

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];
                if (character != '|' && character != '-' && character != ':' && !char.IsWhiteSpace(character))
                {
                    return false;
                }
            }

            return true;
        }

        private static string[] ParseMarkdownTableRow(string line)
        {
            string trimmed = line.Trim('|');
            string[] cells = trimmed.Split('|');
            for (int index = 0; index < cells.Length; index++)
            {
                cells[index] = CleanupInlineMarkdown(cells[index].Trim());
            }

            return cells;
        }

        private static void AppendConvertedTableRow(StringBuilder builder, string[] headers, string[] cells)
        {
            if (builder.Length > 0 && builder[builder.Length - 1] != '\n')
            {
                builder.AppendLine();
            }

            for (int index = 0; index < cells.Length && index < headers.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(cells[index]))
                {
                    continue;
                }

                builder.AppendLine(CleanupInlineMarkdown(headers[index]));
                builder.AppendLine(cells[index]);
            }

            builder.AppendLine();
        }

        private static string CleanupInlineMarkdown(string value)
        {
            return value
                .Replace("**", string.Empty)
                .Replace("`", string.Empty)
                .TrimEnd();
        }

        [Serializable]
        private sealed class KaelisAboutMetadata
        {
            public string project;
            public string author;
            public string artistPseudonym;
            public string artistPseudonymNote;
            public string copyright;
            public string youtube;
            public string[] emails;
            public string aboutShort;
            public string musicCredit;
            public string license;
        }
    }
}
