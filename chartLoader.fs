module DividendExplorer.chartLoader

open System
open System.Net.Http
open System.Web
open DividendExplorer
open types
open utils
open chartStorage

let yahooFinanceApiUrl = "https://query1.finance.yahoo.com"

let handleChartRequestException(e: Exception) =
    match e.InnerException with
    | :? HttpRequestException as h -> h.StatusCode.ToString()
    | e -> e.GetType().Name

let loadChartForSymbolFromRemote(symbol: string, periodStart, periodEnd) =
    use client = new HttpClient()
    let safeSymbol = HttpUtility.UrlEncode(symbol)
    let url = $"{yahooFinanceApiUrl}/v8/finance/chart/{safeSymbol}?symbol=MSFT&period1={periodStart}&period2={periodEnd}&interval=1mo&includePrePost=true&events=div"
    write $"Load chart from url {url}"
    try
        Ok(client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously)
    with
        e -> Error(handleChartRequestException(e))

let handleNewChart(share: Share) =
    let periodStart, periodEnd = chartInterval
    match loadChartForSymbolFromRemote(share.symbol, periodStart, periodEnd) with
    | Ok(c) -> saveChart(share.symbol, c); Cached
    | Error(x) -> ErrorStatus(x)

let loadChartFor(share: Share) =
    let symbol = share.symbol
    write $"Load chart for {symbol}"
    match tryLoadChart(symbol) with
    | Some _ -> (share, Cached)
    | None -> (share, handleNewChart(share))

let loadCharts(shares: Share list) =
    List.map loadChartFor shares