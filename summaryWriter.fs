module DividendExplorer.summaryWriter

open System.IO
open System.Text
open DividendExplorer
open utils
open types

let join(sb: StringBuilder, lines: string list) =
    sb.AppendJoin(";", lines)

let statusToStr(status: ChartStatus) =
    match status with
    | Cached -> "Cached"
    | New -> "New"
    | ErrorStatus(x) -> x

let writeLoadItem(sb: StringBuilder, result: ShareWithChart) =
    let share, status = result
    let statusStr = statusToStr(status)
    join(sb, [ share.symbol; share.name; statusStr ]).AppendLine() |> ignore

let writeLoadSummary(results: ShareWithChart list) =
    let path = "load_summary.csv"
    let sb = StringBuilder()
    join(sb, ["SYMBOL"; "NAME"; "RESULT"]).AppendLine() |> ignore
    for r in results do
        writeLoadItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Load summary saved into {path}"

let divsToLines(divs: DividendResult) =
    [divs.marketPrice.ToString()
     divs.divPerYear.ToString()
     divs.divYield.ToString("p")
     divs.divCount.ToString()
     divs.firstDate.Date.ToShortDateString()
     divs.lastDate.Date.ToShortDateString()
     divs.divIncreaseYears.ToString()]

let writeProcessItem(sb: StringBuilder, result: Share * DividendResult) =
    let share, divs = result
    let divsLines = divsToLines(divs)
    join(sb, [ share.symbol; share.name ] @ divsLines).AppendLine() |> ignore

let isValidResult(result: Share * Result<DividendResult, string>) =
    let _, divResult = result
    match divResult with
    | Ok _ -> true
    | Error _ -> false

let tryGetValidResult(result: Share * Result<DividendResult, string>) =
    let share, divResult = result
    match divResult with
    | Ok r -> Some(share, r)
    | Error _ -> None

let order(results: (Share * DividendResult) list) =
    results |>
    List.sortBy (fun (_, r) -> -r.divYield, -r.divIncreaseYears, -r.marketPrice, r.firstDate.ToUnixTimeSeconds(), -r.lastDate.ToUnixTimeSeconds())

let writeProcessSummary(results: (Share * Result<DividendResult, string>) list) =
    let path = "process_summary.csv"
    let sb = StringBuilder()
    join(sb, ["SYMBOL"; "NAME"; "MARKET_PRICE"; "DIV_PER_YEAR"; "DIV_YIELD"; "DIV_COUNT"; "FIRST_DIV_DATE"; "LAST_DIV_DATE"; "DIV_INCREASE_YEARS"]).AppendLine() |> ignore
    let validResults = List.map tryGetValidResult results |> List.filter Option.isSome |> List.map Option.get |> order
    write $"Found {List.length validResults} valid results of {List.length results} total results"
    for r in validResults do
        writeProcessItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Process summary saved into {path}"