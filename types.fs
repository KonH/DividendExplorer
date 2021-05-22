module DividendExplorer.types

open System

type ShareSymbol = string

type Share = { symbol: ShareSymbol; name: string }

type ChartStatus =
    | New
    | Cached
    | ErrorStatus of string

type ShareWithChart =
    Share * ChartStatus

type DividendResult =
    {
        marketPrice: decimal
        divCount: int;
        firstDate: DateTimeOffset;
        lastDate: DateTimeOffset;
        divIncreaseYears: float;
        divPerYear: float;
        divYield: float;
    }