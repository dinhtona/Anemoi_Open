using System;
using System.Collections.Generic;

namespace Anemoi.BuildingBlock.Application.Errors;

public sealed class ErrorDetail : Exception
{
    public IEnumerable<string> Messages { get; set; }
    public string Code { get; set; }

    public static ErrorDetail FromCode(string code) => new()
    {
        Code = code,
        Messages = [code]
    };
}
