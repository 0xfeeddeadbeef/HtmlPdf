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
using System.Collections.Generic;

[Cmdlet(VerbsData.Out, "Pdf",
    ConfirmImpact = ConfirmImpact.Low,
    RemotingCapability = RemotingCapability.None,
    SupportsShouldProcess = true)]
public class OutPdfCommand : PSCmdlet
{
    private readonly List<string> _pages = new(capacity: 16);

    [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = false)]
    [ValidateNotNullOrEmpty]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "PowerShell")]
    public string[]? Page
    {
        get; set;
    }

    [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "Path")]
    [ValidateNotNullOrEmpty]
    public string? Path
    {
        get; set;
    }

    [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "LiteralPath")]
    [ValidateNotNullOrEmpty]
    public string? LiteralPath
    {
        get; set;
    }

    [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNull]
    public string? Title
    {
        get; set;
    }

    protected override void ProcessRecord()
    {
        if (this.Page is { Length: > 0 })
        {
            _pages.AddRange(this.Page);
        }
    }

    protected override void EndProcessing()
    {
        if (_pages is { Count: > 0 })
        {
            string path = string.Equals(this.ParameterSetName, "Path", StringComparison.Ordinal)
                ? this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.Path!)
                : System.IO.Path.GetFullPath(this.LiteralPath!);

            bool dryRun = !this.ShouldProcess("Pdf", "Create");

            string? html = HtmlToPdfConversion.ConvertToPdf(_pages, this.Title!, path, dryRun);
            this.WriteVerbose(html);
        }
    }
}
