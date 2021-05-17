module DividendExplorer.program

open DividendExplorer
open shareLoader
open dividendLoader
open summaryWriter

[<EntryPoint>]
let main _ =
    loadShares() |> loadDividends |> writeSummary
    0