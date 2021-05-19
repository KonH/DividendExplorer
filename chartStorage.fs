module DividendExplorer.chartStorage

open System
open System.IO
open FSharp.Data
open DividendExplorer
open types
open utils

let cacheDir = "Cache";

type ChartData = JsonProvider<Sample="ChartSample.json">

let chartInterval =
    let today = DateTimeOffset.UtcNow
    let nextYearStart = DateTimeOffset(today.Year + 1, 1, 1, 0, 0, 0, today.Offset)
    let prevYearStart = DateTimeOffset(today.Year - 30, 1, 1, 0, 0, 0, today.Offset)
    let periodStart = prevYearStart.ToUnixTimeSeconds()
    let periodEnd = nextYearStart.ToUnixTimeSeconds()
    (periodStart, periodEnd)

let filePath(symbol: ShareSymbol) =
    let periodStart, periodEnd = chartInterval
    $"{cacheDir}/{symbol}_{periodStart}_{periodEnd}.json"

let tryLoadChart(symbol: ShareSymbol) =
    let cacheFilePath = filePath symbol
    match File.Exists cacheFilePath with
    | true -> Some(ChartData.Load cacheFilePath)
    | _ -> None

let saveChart(symbol: ShareSymbol, contents: string) =
    let cacheFilePath = filePath symbol
    write $"Save chart data to {cacheFilePath}"
    let file = FileInfo(cacheFilePath)
    file.Directory.Create()
    File.WriteAllText(cacheFilePath, contents)