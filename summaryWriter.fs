module DividendExplorer.summaryWriter

open System.IO
open System.Text
open DividendExplorer
open utils
open types

let statusToStr(status: DividendStatus) =
    match status with
    | Cached -> "Cached"
    | New -> "New"
    | ErrorStatus(x) -> x

let writeSummaryItem(sb: StringBuilder, result: ShareWithDividends) =
    let share, status = result
    let statusStr = statusToStr(status)
    sb.Append(share.symbol).Append(';').Append(share.name).Append(';').Append(statusStr).Append(';').AppendLine() |> ignore

let writeSummary(results: ShareWithDividends list) =
    let path = "summary.csv"
    let sb = StringBuilder()
    sb.Append("SYMBOL;NAME;RESULT;").AppendLine() |> ignore
    for r in results do
        writeSummaryItem(sb, r)
    File.WriteAllText(path, sb.ToString())
    write $"Results saved into {path}"