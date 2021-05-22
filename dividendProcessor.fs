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

let calculateDivIncreaseYears(divs: (DateTimeOffset * float) list) =
    let mutable last = Double.MaxValue
    let entriesWithIncrease =
        divs |> List.rev |>
        List.takeWhile (fun (_, c) ->
            let r = c <= last
            last <- c
            r)
    let dates = List.map fst entriesWithIncrease
    let minDate = List.min dates
    let maxDate = List.max dates
    let delta = maxDate - minDate
    Math.Round(delta.TotalDays / 365., 2)

let handleDividends(dividends: ChartData.Dividends) =
    let value = dividends.JsonValue
    match value with
    | JsonValue.Record r ->
        let tsEntries =
            List.ofArray r |>
            List.map (fun (ts, value) -> (Int64.Parse(ts), value)) |>
            List.map (fun (ts, value) -> (DateTimeOffset.FromUnixTimeSeconds(ts), value)) |>
            List.map (fun (ts, value) -> (ts, value.["amount"].AsFloat())) |>
            List.sortBy fst
        let divYears = calculateDivIncreaseYears tsEntries
        let tss = List.map fst tsEntries
        Ok(r.Length, List.min tss, List.max tss, divYears)
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