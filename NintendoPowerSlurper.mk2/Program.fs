// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.IO
open System.Net
open System.Text
open FSharp.Data
open HtmlAgilityPack.FSharp

[<EntryPoint>]
let main argv = 
    let getNPPage page = 
        "https://archive.org/details/nintendopower?&sort=-downloads&page=" + (string page)
        |> Http.AsyncRequestString

    let getMagLinks npPage =
        npPage
        |> createDoc
        |> descendants "div"
        |> Seq.filter(hasClass "results")
        |> Seq.head
        |> descendants "a"
        |> Seq.map(attr "href")
        |> Seq.toArray
        |> Array.filter(fun x -> x.Contains "Issue")
        |> Array.map(fun x -> x.Replace("details", "compress"))

    let pages = 
        [| "1"; "2"; |]
        |> Seq.map getNPPage 
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.collect getMagLinks

    let download (url:string) =
        printfn "Downloading started"
        let folder = url.Replace("/compress/", "")
        let name = folder + ".zip"
        if not <| Directory.Exists(folder) then
            Directory.CreateDirectory(folder) |> ignore
        use out = File.Create(Path.Combine(folder, name))
        Http.RequestStream(("http://archive.org" + url))
            .ResponseStream.CopyTo(out)
        printfn "Downloading finished"

    let writeZips pages =
        pages 
        |> Array.map download

            
    printfn "%A" pages
    printfn "Writing zips...."
    writeZips pages |> ignore
    Console.ReadKey() |> ignore
    0 // return an integer exit code
