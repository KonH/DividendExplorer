module DividendExplorer.program

open DividendExplorer
open shareLoader
open chartLoader
open dividendProcessor
open summaryWriter

[<EntryPoint>]
let main argv =
    let shares = loadShares()
    if Array.contains "load" argv then shares |> loadCharts |> writeLoadSummary
    if Array.contains "process" argv then shares |> processDividends |> writeProcessSummary
    0