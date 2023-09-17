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

#nullable disable

#if NET462_OR_GREATER

namespace HtmlPdf;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
///    Load dependencies from module directory when running on .NET Framework.
/// </summary>
/// <remarks>
///    Assembly binding redirect in DLLs does not work, and we cant modify powershell.exe.config,
///    this is the only way.
/// </remarks>
public sealed class ModuleInitializer : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    private static readonly string s_modulePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    void IModuleAssemblyInitializer.OnImport()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    void IModuleAssemblyCleanup.OnRemove(PSModuleInfo psModuleInfo)
    {
        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
    }

    [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "Sorry")]
    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        // Load assemblies from the module directory regardless of the requested versions:
        var assemblyName = new AssemblyName(args.Name);
        return s_localAssemblies.Contains(assemblyName.Name)
            ? Assembly.LoadFrom(Path.Combine(s_modulePath, assemblyName.Name) + ".dll")
            : null;
    }

    private static readonly HashSet<string> s_localAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BOUNCYCASTLE.CRYPTO",
        "BOUNCYCASTLE.CRYPTOGRAPHY",
        "ITEXT.BARCODES",
        "ITEXT.BOUNCY-CASTLE-ADAPTER",
        "ITEXT.BOUNCY-CASTLE-CONNECTOR",
        "ITEXT.COMMONS",
        "ITEXT.FORMS",
        "ITEXT.HTML2PDF",
        "ITEXT.IO",
        "ITEXT.KERNEL",
        "ITEXT.LAYOUT",
        "ITEXT.PDFA",
        "ITEXT.SIGN",
        "ITEXT.STYLEDXMLPARSER",
        "ITEXT.SVG",
        "MICROSOFT.BCL.ASYNCINTERFACES",
        "MICROSOFT.EXTENSIONS.DEPENDENCYINJECTION.ABSTRACTIONS",
        "MICROSOFT.EXTENSIONS.DEPENDENCYINJECTION",
        "MICROSOFT.EXTENSIONS.LOGGING.ABSTRACTIONS",
        "MICROSOFT.EXTENSIONS.LOGGING",
        "MICROSOFT.EXTENSIONS.OPTIONS",
        "MICROSOFT.EXTENSIONS.PRIMITIVES",
        "NEWTONSOFT.JSON",
        "SYSTEM.BUFFERS",
        "SYSTEM.DIAGNOSTICS.DIAGNOSTICSOURCE",
        "SYSTEM.MEMORY",
        "SYSTEM.NUMERICS.VECTORS",
        "SYSTEM.RUNTIME.COMPILERSERVICES.UNSAFE",
        "SYSTEM.THREADING.TASKS.EXTENSIONS",
        "SYSTEM.VALUETUPLE",
    };
}

#endif
