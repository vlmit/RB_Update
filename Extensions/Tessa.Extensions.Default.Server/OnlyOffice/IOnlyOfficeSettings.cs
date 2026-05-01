using System;

namespace Tessa.Extensions.Default.Server.OnlyOffice
{
    public interface IOnlyOfficeSettings
    {
        string? ConverterUrl { get; }

        TimeSpan LoadTimeout { get; }
    }
}