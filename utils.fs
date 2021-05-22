module DividendExplorer.utils

open System

let write (message: string) = Console.WriteLine message

let tryResult(func: unit -> 'r) =
    try
        Ok(func())
    with
        e -> Error(e.ToString())