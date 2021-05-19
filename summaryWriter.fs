module DividendExplorer.summaryWriter

open System.IO
open System.Text
open DividendExplorer
open utils
open types

let statusToStr(status: ChartStatus) =
    match status with
    | Cached -> "Cached"
    | New -> "New"
    | ErrorStatus(x) -> x

let divsToStr(divs: DividendResult) =
    match divs with
    | s -> s

let writeLoadItem(sb: StringBuilder, result: ShareWithChart) =
    let share, status = result
    let statusStr = statusToStr(status)
    sb.Append(share.symbol).Append(';').Append(share.name).Append(';').Append(statusStr).Append(';').AppendLine() |> ignore

let writeProcessItem(sb: StringBuilder, result: ShareWithDividendResult) =
    let share, divs = result
    let divsStr = divsToStr(divs)
    sb.Append(share.symbol).Append(';').Append(share.name).Append(';').Append(divsStr).Append(';').AppendLine() |> ignore

let writeLoadSummary(results: ShareWithChart list) =
    let path = "load_summary.csv"
    let sb = StringBuilder()
    sb.Append("SYMBOL;NAME;RESULT;").AppendLine() |> ignore
    for r in results do
        writeLoadItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Load summary saved into {path}"

let writeProcessSummary(results: ShareWithDividendResult list) =
    let path = "process_summary.csv"
    let sb = StringBuilder()
    sb.Append("SYMBOL;NAME;RESULT;").AppendLine() |> ignore
    for r in results do
        writeProcessItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Process summary saved into {path}"