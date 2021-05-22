module DividendExplorer.dividendProcessor

open System
open DividendExplorer
open FSharp.Data
open types
open chartStorage

let tryGetDividends(chart: ChartData.Root) =
    try
        let result = chart.Chart.Result.[0]
        let dividends = result.Events.Dividends
        Ok dividends
    with
        e -> Error(e.GetType().FullName)

let handleDividends(dividends: ChartData.Dividends) =
    let value = dividends.JsonValue
    match value with
    | JsonValue.Record r ->
        let tsEntries =
            List.ofArray r |>
            List.map (fun (ts, value) -> (Int64.Parse(ts), value)) |>
            List.map (fun (ts, value) -> (DateTimeOffset.FromUnixTimeSeconds(ts), value))
        let tss = List.map fst tsEntries
        Ok(r.Length, List.min tss, List.max tss)
    | _ -> Error "Parsing failed"

let processChart(chart: ChartData.Root) =
    let dividends = tryGetDividends chart
    Result.bind handleDividends dividends

let processData(symbol: ShareSymbol) =
    let chart = tryLoadChart(symbol)
    match chart with
    | Some c -> processChart c
    | None -> Error "Chart not found"

let processDividendsFor(share: Share) =
    (share, processData share.symbol)

let processDividends(shares: Share list) =
     List.map processDividendsFor shares