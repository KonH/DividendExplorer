module DividendExplorer.types

type ShareSymbol = string

type Share = { symbol: ShareSymbol; name: string }

type ChartStatus =
    | New
    | Cached
    | ErrorStatus of string

type ShareWithChart =
    Share * ChartStatus

type DividendResult =
    int