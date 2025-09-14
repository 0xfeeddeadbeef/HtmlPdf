#
# Module manifest for module 'HtmlPdf'
#
@{
    # Script module or binary module file associated with this manifest.
    RootModule = 'HtmlPdf.dll'

    # Version number of this module.
    ModuleVersion = '3.8.0'

    # ID used to uniquely identify this module
    GUID = '7405F222-3FEE-45AE-8E2E-1C2B41DDB827'

    # Supported PSEditions
    CompatiblePSEditions = @('Core','Desktop')

    # Author of this module
    Author = 'Giorgi Chakhidze'

    # Company or vendor of this module
    CompanyName = 'Giorgi Chakhidze'

    # Copyright statement for this module
    Copyright = '(c) 2025 Giorgi Chakhidze. All rights reserved.'

    # Description of the functionality provided by this module
    Description = 'Module to bind a bunch of images into a PDF file'

    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '5.1'

    # Functions to export from this module (either specific ones as array @('name1','name2') or all '*')
    FunctionsToExport = @('Out-Pdf')

    # Private data to pass to the module specified in RootModule/ModuleToProcess.
    # This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{
        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('HTML','PDF')

            # A URL to the license for this module.
            LicenseUri = 'https://www.gnu.org/licenses/agpl-3.0.txt'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/0xfeeddeadbeef/HtmlPdf'

            # A URL to an icon representing this module.
            IconUri = 'https://upload.wikimedia.org/wikipedia/commons/thumb/8/87/PDF_file_icon.svg/195px-PDF_file_icon.svg.png'
        }
    }
}
