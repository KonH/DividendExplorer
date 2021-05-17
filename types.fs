module DividendExplorer.types

type ShareSymbol = string

type Share = { symbol: ShareSymbol; name: string }

type DividendStatus =
    | New
    | Cached
    | ErrorStatus of string

type ShareWithDividends =
    Share * DividendStatus