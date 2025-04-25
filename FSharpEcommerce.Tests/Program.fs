module Program

open Expecto
open FSharpEcommerce.Tests

[<EntryPoint>]
let main args =
    try
        runTestsInAssemblyWithCLIArgs [] args |> ignore
    finally
        TestServer.dispose ()

    0
