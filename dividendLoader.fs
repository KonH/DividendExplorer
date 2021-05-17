module DividendExplorer.dividendLoader

open System
open System.IO
open System.Net.Http
open System.Web
open DividendExplorer.types
open DividendExplorer.utils

let yahooFinanceApiUrl = "https://query1.finance.yahoo.com"

let dividendInterval =
    let today = DateTimeOffset.UtcNow
    let nextYearStart = DateTimeOffset(today.Year + 1, 1, 1, 0, 0, 0, today.Offset)
    let prevYearStart = DateTimeOffset(today.Year - 30, 1, 1, 0, 0, 0, today.Offset)
    let periodStart = prevYearStart.ToUnixTimeSeconds()
    let periodEnd = nextYearStart.ToUnixTimeSeconds()
    (periodStart, periodEnd)

let cacheDir = "Cache";

let handleCachedDividends(share: Share, cacheFilePath) =
    write $"Dividends already cached at {cacheFilePath}"
    (share, Cached)

let handleDividendRequestException(e: Exception) =
    match e.InnerException with
    | :? HttpRequestException as h -> h.StatusCode.ToString()
    | e -> e.GetType().Name

let loadDividendsForSymbolFromRemote(symbol: string, periodStart, periodEnd) =
    use client = new HttpClient()
    let safeSymbol = HttpUtility.UrlEncode(symbol)
    let url = $"{yahooFinanceApiUrl}/v8/finance/chart/{safeSymbol}?symbol=MSFT&period1={periodStart}&period2={periodEnd}&interval=1mo&includePrePost=true&events=div"
    write $"Load dividends from url {url}"
    try
        Ok(client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously)
    with
        e -> Error(handleDividendRequestException(e))

let saveDividendsToCache(cacheFilePath, contents) =
    write $"Save dividends to {cacheFilePath}"
    let file = FileInfo(cacheFilePath)
    file.Directory.Create()
    File.WriteAllText(cacheFilePath, contents)

let handleNewDividends(share: Share, cacheFilePath, periodStart, periodEnd) =
    match loadDividendsForSymbolFromRemote(share.symbol, periodStart, periodEnd) with
    | Ok(c) -> saveDividendsToCache(cacheFilePath, c); Cached
    | Error(x) -> ErrorStatus(x)

let loadDividendsFor(share: Share) =
    let symbol = share.symbol
    write $"Fetch dividends for {symbol}"
    let periodStart, periodEnd = dividendInterval
    let cacheFilePath = $"{cacheDir}/{symbol}_{periodStart}_{periodEnd}.json"
    match File.Exists cacheFilePath with
    | true -> handleCachedDividends(share, cacheFilePath)
    | _ -> (share, handleNewDividends(share, cacheFilePath, periodStart, periodEnd))

let loadDividends(shares: Share list) =
    List.map loadDividendsFor shares