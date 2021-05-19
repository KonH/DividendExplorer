module DividendExplorer.dividendProcessor

open DividendExplorer
open FSharp.Data
open types
open chartStorage

let tryGetDividends(chart: ChartData.Root) =
    try
        let result = chart.Chart.Result.[0]
        let dividends = result.Events.Dividends
        Some dividends
    with
        _ -> None

let handleDividends(dividends: ChartData.Dividends) =
    let value = dividends.JsonValue
    match value with
    | JsonValue.Record r -> Some(r.Length.ToString())
    | _ -> None

let processChart(chart: ChartData.Root) =
    let dividends = tryGetDividends chart
    let result = Option.bind handleDividends dividends
    match result with
    | Some s -> s
    | None -> "Fail"

let processData(symbol: ShareSymbol) =
    let chart = tryLoadChart(symbol)
    match chart with
    | Some c -> processChart c
    | None -> "Chart not found"

let processDividendsFor(share: Share) =
    (share, processData share.symbol)

let processDividends(shares: Share list) =
     List.map processDividendsFor shares