module MangaDownloader

open System
open System.Threading.Tasks
open System.IO
open AngleSharp

type Site = MangaLife

let config = Configuration.Default.WithDefaultLoader()
let browsingContext = BrowsingContext.New(config)

let download(site: Site)(title: string)(chapter: int) = 
    let titleUri = 
        match site with
        | Site.MangaLife -> sprintf "http://mangalife.org/read-online/%s-chapter-%d.html" title chapter

    Directory.CreateDirectory(sprintf "%s_%03d" title chapter) |> ignore
    async {
        let! document = Async.AwaitTask(browsingContext.OpenAsync(titleUri))
        let images = document.QuerySelectorAll("div.fullchapimage img") |> Seq.toArray
        let downloads = seq {
            for i = 0 to images.Length-1 do 
                yield
                    async {
                        let image = images.[i]
                        let imageUri = image.GetAttribute("src")
                        printfn "%s" imageUri
                        let req = Network.DocumentRequest.Get(new Url(imageUri))
                        let download = browsingContext.Loader.DownloadAsync(req)
                        let! response = Async.AwaitTask(download.Task)
                        response.Content.CopyTo(File.Create(sprintf "%s_%03d/%03d.png" title chapter i)) |> ignore
                        }
                    }
        let! _ = Async.Parallel(downloads)
        return ()
    } |> Async.RunSynchronously

[<EntryPoint>]
let main argv = 
    for i = 201 to 300 do
        download Site.MangaLife "Naruto" i
    0 
