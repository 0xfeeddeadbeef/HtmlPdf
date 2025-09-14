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
using System.Collections.Generic;
using System.IO;

[Cmdlet(VerbsData.Out, "Pdf", ConfirmImpact = ConfirmImpact.Low, RemotingCapability = RemotingCapability.None, SupportsShouldProcess = true)]
public sealed class OutPdfCommand : PSCmdlet
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

    [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
    [PSDefaultValue(Value = PageSize.A4)]
    public PageSize Size { get; set; } = PageSize.A4;

    [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
    [PSDefaultValue(Value = PageOrientation.Portrait)]
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    protected override void ProcessRecord()
    {
        if (this.Page is { Length: > 0 })
        {
            _pages.AddRange(this.Page);
        }
    }

    protected override void EndProcessing()
    {
        if (_pages is not { Count: > 0 })
        {
            ErrorRecord erNone = new(
                new FileNotFoundException("Pages were not found."),
                "NoPagesFound",
                ErrorCategory.ObjectNotFound,
                targetObject: null);

            this.ThrowTerminatingError(erNone);
            return;
        }

        string outputPath;
        try
        {
            outputPath = ResolveOutputPath();
        }
        catch (ItemNotFoundException ex)
        {
            var er = new ErrorRecord(
                ex,
                "UnresolvedPath",
                ErrorCategory.ObjectNotFound,
                targetObject: IsLiteralPath ? LiteralPath : Path);
            this.ThrowTerminatingError(er);
            return;
        }
        catch (Exception ex) when (ex is ArgumentException or PSArgumentException or DirectoryNotFoundException)
        {
            var er = new ErrorRecord(
                ex,
                "UnresolvedPath",
                ErrorCategory.InvalidArgument,
                targetObject: IsLiteralPath ? LiteralPath : Path);
            this.ThrowTerminatingError(er);
            return;
        }

        bool dryRun = !this.ShouldProcess(outputPath, "Create");

        var notFoundPages = new List<string>(capacity: _pages.Count);
        var resolvedPages = new List<string>(capacity: _pages.Count);

        foreach (var page in _pages)
        {
            try
            {
                foreach (var resolvedPage in this.SessionState.Path.GetResolvedPSPathFromPSPath(page))
                {
                    var rp = GetFilePathOfExistingFile(this, resolvedPage.ProviderPath);
                    if (rp is not null)
                    {
                        resolvedPages.Add(rp);
                    }
                    else
                    {
                        notFoundPages.Add(page);
                    }
                }
            }
            catch (ItemNotFoundException)
            {
                notFoundPages.Add(page);
            }
        }

        if (notFoundPages.Count > 0)
        {
            ErrorRecord er = new(
                new FileNotFoundException("Pages were not found."),
                "NoPagesFound",
                ErrorCategory.ObjectNotFound,
                targetObject: notFoundPages);

            this.ThrowTerminatingError(er);
        }

        string? html = HtmlToPdfConversion.ConvertToPdf(resolvedPages, this.Title!, outputPath, this.Size, this.Orientation, dryRun);
        this.WriteVerbose(html);
    }

    private bool IsLiteralPath => string.Equals(this.ParameterSetName, "LiteralPath", StringComparison.Ordinal);

    private string ResolveOutputPath()
    {
        string? raw = IsLiteralPath ? this.LiteralPath : this.Path;
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new PSArgumentNullException(IsLiteralPath ? nameof(this.LiteralPath) : nameof(this.Path));
        }

        // If the (non-literal) user input contains wildcards, resolve existing items
        if (!IsLiteralPath && WildcardPattern.ContainsWildcardCharacters(raw))
        {
            var matches = this.GetResolvedProviderPathFromPSPath(raw, out _);
            if (matches.Count == 0)
            {
                throw new ItemNotFoundException($"Path '{raw}' could not be resolved.");
            }

            if (matches.Count > 1)
            {
                throw new PSArgumentException($"Path '{raw}' resolved to multiple locations. Use -LiteralPath to specify an exact output file.");
            }

            // If the resolved path is a directory, you might choose to derive a filename; enforce it's not a container
            var singlePath = matches[0];
            if (Directory.Exists(singlePath))
            {
                throw new PSArgumentException($"Path '{raw}' resolves to a directory. Specify a file name.");
            }

            return singlePath;
        }

        // Treat as a (possibly new) file path. This yields the full provider path even if the file does not yet exist
        var full = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(raw);

        // Validate directory existence (optional but safer)
        var dir = System.IO.Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            throw new DirectoryNotFoundException($"Directory '{dir}' does not exist.");
        }

        return full;
    }

    private static string? GetFilePathOfExistingFile(PSCmdlet cmdlet, string path)
    {
        var resolvedProviderPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
        return File.Exists(resolvedProviderPath) ? resolvedProviderPath : null;
    }
}

// See CSS @page size keyword values:
// https://developer.mozilla.org/en-US/docs/Web/CSS/@page/size
public enum PageSize
{
    A5, A4, A3, B5, B4, JISB5, JISB4, Letter, Legal, Ledger,
}

public enum PageOrientation
{
    Portrait, Landscape,
}
