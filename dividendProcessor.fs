module DividendExplorer.dividendProcessor

open System
open DividendExplorer
open FSharp.Data
open utils
open types
open chartStorage

let tryGetResult(chart: ChartData.Root) =
    tryResult(fun _ -> chart.Chart.Result.[0])

let tryGetPrice(result: ChartData.Result) =
    tryResult(fun _ -> result.Meta.RegularMarketPrice)

let tryGetDividends(result: ChartData.Result) =
    tryResult(fun _ -> result.Events.Dividends)

let calculateDivIncreaseYears(divs: (DateTimeOffset * float) list) =
    let mutable last = Double.MaxValue
    let entriesWithIncrease =
        divs |> List.rev |>
        List.takeWhile (fun (_, c) ->
            let r = c <= last
            last <- c
            r)
    let dates = entriesWithIncrease |> List.map fst
    let minDate = dates |> List.min
    let maxDate = dates |> List.max
    let delta = maxDate - minDate
    Math.Round(delta.TotalDays / 365., 2)

let trySkip count list =
    if List.length list > count then List.skip count list else list

let calculateDivPerYear(divs: (DateTimeOffset * float) list) =
    let lastDivTs = divs |> List.last |> fst
    let lastYearDivs = List.where (fun (ts: DateTimeOffset, _) -> (lastDivTs - ts).TotalDays < 365.) divs
    lastYearDivs |> List.sumBy snd

let handleDividends(dividends: ChartData.Dividends) =
    let value = dividends.JsonValue
    match value with
    | JsonValue.Record r ->
        let tsEntries =
            r |> List.ofArray |>
            List.map (fun (ts, value) -> (Int64.Parse(ts), value)) |>
            List.map (fun (ts, value) -> (DateTimeOffset.FromUnixTimeSeconds(ts), value)) |>
            List.map (fun (ts, value) -> (ts, value.["amount"].AsFloat())) |>
            List.sortBy fst
        let divYears = tsEntries |> calculateDivIncreaseYears
        let tss = tsEntries |> List.map fst
        let divPerYear = tsEntries |> calculateDivPerYear
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
    let marketPrice = result |> Result.bind tryGetPrice
    let dividends = result |> Result.bind tryGetDividends |> Result.bind handleDividends
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