/*
 * Copyright (C) 2023 George Chakhidze
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

internal static class HtmlToPdfConversion
{
    internal static string? ConvertToPdf(List<string> pages, string title, string pdfPath, bool dryRun = false)
    {
        if (pages is null)
        {
            throw new ArgumentNullException(nameof(pages));
        }

        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            throw new ArgumentNullException(nameof(pdfPath));
        }

        title ??= string.Empty;

        StringBuilder html = StringBuilderCache.Acquire(capacity: 24 * 1024);
        html.Append(HtmlTemplate);

        html.Replace("%%TITLE%%", title);

        for (var i = 0; i < pages.Count; i++)
        {
            string pageUrl = new Uri(pages[i]).AbsoluteUri;

            if (i != pages.Count - 1)
            {
                html.AppendFormat(CultureInfo.InvariantCulture, PageBreakTemplate, pageUrl);
            }
            else
            {
                html.AppendFormat(CultureInfo.InvariantCulture, PageTemplate, pageUrl);
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

    // TODO: Implement portrait/landscape orientation switch
    private const string PageTemplate = "  <img class=\"a4page\" src=\"{0}\" />\r\n";
    private const string PageBreakTemplate = "  <img class=\"a4pagebreak\" src=\"{0}\" />\r\n";
    private const string HtmlTemplate =
        """
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
          <meta charset=""utf-8"" />
          <title>%%TITLE%%</title>
          <style type=""text/css"">
            @page { margin: 0pt; }
            .a4page {
              object-fit: scale-down;
              margin: 0; padding: 0;
              width: 794px; height: 1122px;
            }
            .a4pagebreak {
              object-fit: scale-down;
              margin: 0; padding: 0;
              width: 794px; height: 1122px;
              page-break-before: always;
            }
          </style>
        </head>
        <body style=""margin: 0; padding: 0;"">

        """;
    private const string HtmlFooter = "</body>\r\n</html>\r\n";
}
