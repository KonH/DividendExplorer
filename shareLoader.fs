module DividendExplorer.shareLoader

// Source file located at https://spbexchange.ru/ru/stocks/inostrannye/Instruments.aspx

open System.IO
open System.Text
open FSharp.Data
open DividendExplorer.types

[<Literal>]
let sourceFilePath = "ListingSecurityList.csv"

type Securities = CsvProvider<sourceFilePath, Separators=";", ResolutionFolder=__SOURCE_DIRECTORY__>

let isShare (r: Securities.Row) = r.S_sec_type_name_dop = "Акции"

let toShare(row: Securities.Row) = { symbol = row.S_RTS_code; name = row.E_full_name }

let loadShares() =
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    utils.write $"Load source file from {sourceFilePath}"
    use reader = new StreamReader(sourceFilePath, Encoding.GetEncoding(1251))
    let provider = Securities.Load reader
    let result = Seq.filter isShare provider.Rows |> Seq.map toShare |> List.ofSeq
    utils.write $"Found {Seq.length result} shares"
    result
