module DividendExplorer.dividendProcessor

open System
open DividendExplorer
open FSharp.Data
open types
open chartStorage

let tryGetResult(chart: ChartData.Root) =
    try
        Ok chart.Chart.Result.[0]
    with
        e -> Error(e.ToString())

let tryGetPrice(result: ChartData.Result) =
    try
        Ok result.Meta.RegularMarketPrice
    with
        e -> Error(e.ToString())

let tryGetDividends(result: ChartData.Result) =
    try
        Ok result.Events.Dividends
    with
        e -> Error(e.ToString())

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

let trySkip count list =
    if List.length list > count then List.skip count list else list

let calculateDivPerYear(divs: (DateTimeOffset * float) list) =
    let lastDivTs = divs |> List.last |> fst
    let lastYearDivs = List.where (fun (ts: DateTimeOffset, _) -> (lastDivTs - ts).TotalDays < 365.) divs
    List.sumBy snd lastYearDivs

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
        let divPerYear = calculateDivPerYear tsEntries
        Ok(r.Length, List.min tss, List.max tss, divYears, divPerYear)
    | _ -> Error "Parsing failed"

let bind2(result1: Result<'r1, 'e>, result2: Result<'r2, 'e>) =
    match result1 with
    | Ok r1 ->
        match result2 with
        | Ok r2 -> Ok(r1, r2)
        | Error e2 -> Error e2
    | Error e1 -> Error e1

let combine(marketPrice: decimal, dividends: int * DateTimeOffset * DateTimeOffset * float * float) =
    let divCount, firstDate, lastDate, divYears, divPerYear = dividends
    Ok {
        marketPrice = marketPrice
        divCount = divCount; firstDate = firstDate; lastDate = lastDate;
        divIncreaseYears = divYears; divPerYear = divPerYear; divYield = divPerYear / float marketPrice
    }

let processChart(chart: ChartData.Root) =
    let result = tryGetResult chart
    let marketPrice = Result.bind tryGetPrice result
    let dividends = Result.bind tryGetDividends result |> Result.bind handleDividends
    let r = bind2(marketPrice, dividends)
    Result.bind combine r

let processData(symbol: ShareSymbol) =
    let chart = tryLoadChart(symbol)
    match chart with
    | Some c -> processChart c
    | None -> Error "Chart not found"

let processDividendsFor(share: Share) =
    (share, processData share.symbol)

let processDividends(shares: Share list) =
     List.map processDividendsFor shares