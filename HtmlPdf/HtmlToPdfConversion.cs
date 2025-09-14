/*
 * Copyright (C) 2025 George Chakhidze
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace HtmlPdf;

using System;
using System.IO;
using System.Collections.Generic;
using iText.Html2pdf;
#if NET
using CompositeFormat = System.Text.CompositeFormat;
#endif

internal static class HtmlToPdfConversion
{
#if NET
    private static readonly CompositeFormat s_PageBreakTemplate = CompositeFormat.Parse(PageBreakTemplate);
    private static readonly CompositeFormat s_PageTemplate = CompositeFormat.Parse(PageTemplate);
#endif

    internal static string? ConvertToPdf(
        List<string> pages,
        string title,
        string pdfPath,
        PageSize pageSize = PageSize.A4,
        PageOrientation pageOrientation = PageOrientation.Portrait,
        bool dryRun = false)
    {
#if NET
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);
#else
        if (pages is null)
        {
            throw new ArgumentNullException(nameof(pages));
        }

        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            throw new ArgumentNullException(nameof(pdfPath));
        }
#endif

#if NET
        if (!Enum.IsDefined(pageSize))
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (!Enum.IsDefined(pageOrientation))
        {
            throw new ArgumentOutOfRangeException(nameof(pageOrientation));
        }
#else
        if (!Enum.IsDefined(typeof(PageSize), pageSize))
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (!Enum.IsDefined(typeof(PageOrientation), pageOrientation))
        {
            throw new ArgumentOutOfRangeException(nameof(pageOrientation));
        }
#endif

        title ??= string.Empty;

        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder html = StringBuilderCache.Acquire(capacity: 16 * 1024);
        html.Append(HtmlTemplate);

        html.Replace("%%TITLE%%", title);
#pragma warning disable CA1308  // STFU! Actually need lowercase here
        html.Replace("%%SIZE%%", pageSize.ToString().ToLowerInvariant());
        html.Replace("%%ORIENTATION%%", pageOrientation.ToString().ToLowerInvariant());
#pragma warning restore CA1308

        for (int i = 0; i < pages.Count; i++)
        {
            string pageUrl = new Uri(pages[i]).AbsoluteUri;

            if (i != pages.Count - 1)
            {
#if NET
                html.AppendFormat(culture, s_PageBreakTemplate, pageUrl);
#else
                html.AppendFormat(culture, PageBreakTemplate, pageUrl);
#endif
            }
            else
            {
#if NET
                html.AppendFormat(culture, s_PageTemplate, pageUrl);
#else
                html.AppendFormat(culture, PageTemplate, pageUrl);
#endif
            }
        }

        html.Append(HtmlFooter);

        string generatedHtml = StringBuilderCache.GetStringAndRelease(html);

        if (!dryRun)
        {
            using FileStream pdfFile = new(pdfPath,
                FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 8192 * 2, FileOptions.SequentialScan);

            HtmlConverter.ConvertToPdf(generatedHtml, pdfFile);
        }

        return generatedHtml;
    }

    private const string PageTemplate = "  <img class=\"apage\" src=\"{0}\"/>\r\n";
    private const string PageBreakTemplate = "  <img class=\"apagebreak\" src=\"{0}\"/>\r\n";
    private const string HtmlTemplate =
        """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8"/>
          <title>%%TITLE%%</title>
          <style type="text/css">
            @page {
              size: %%SIZE%% %%ORIENTATION%%;
              margin: 0pt;
            }
            html, body {
              margin: 0;
              padding: 0;
            }
            img.apage,
            img.apagebreak {
              object-fit: scale-down;
              margin: 0;
              padding: 0;
              width: 100%;
              height: auto;
              max-width: 100%;
            }
            img.apagebreak {
              page-break-before: always;
            }
          </style>
        </head>
        <body>

        """;
    private const string HtmlFooter = "</body>\r\n</html>\r\n";
}
