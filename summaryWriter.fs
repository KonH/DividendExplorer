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
    let count, firstDate, lastDate = divs
    [count.ToString(); firstDate.Date.ToShortDateString(); lastDate.Date.ToShortDateString()]

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

let writeProcessSummary(results: (Share * Result<DividendResult, string>) list) =
    let path = "process_summary.csv"
    let sb = StringBuilder()
    join(sb, ["SYMBOL"; "NAME"; "DIV_COUNT"; "FIRST_DIV_DATE"; "LAST_DIV_DATE"]).AppendLine() |> ignore
    let validResults = List.map tryGetValidResult results |> List.filter Option.isSome |> List.map Option.get
    write $"Found {List.length validResults} valid results of {List.length results} total results"
    for r in validResults do
        writeProcessItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Process summary saved into {path}"