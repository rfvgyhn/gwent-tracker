namespace GwentTracker

open Fabulous.Core
open Xamarin.Forms

module App =
    open System.IO
    open System.Globalization
    open W3SavegameEditor.Core.Savegame
    open W3SavegameEditor.Core.Savegame.Variables
    open System
    open Model

    let shouldFilterCard (filters: string list) (card:Card)  =
        let compareInfo = CultureInfo.CurrentUICulture.CompareInfo
        
        filters
        |> List.forall (fun f -> compareInfo.IndexOf(card.Name, f, CompareOptions.IgnoreCase) >= 0 ||
                                 compareInfo.IndexOf(card.Deck, f, CompareOptions.IgnoreCase) >= 0 ||
                                 compareInfo.IndexOf(card.Type, f, CompareOptions.IgnoreCase) >= 0 ||
                                 compareInfo.IndexOf(card.Location, f, CompareOptions.IgnoreCase) >= 0 ||
                                 compareInfo.IndexOf(card.Region, f, CompareOptions.IgnoreCase) >= 0)
        
    let parseSaveFile path =
        let parseInfo (items : Variable[])=
            let index = ((items.[0] :?> VlVariable).Value :?> VariableValue<int>).Value
            let copies = ((items.[1] :?> VlVariable).Value :?> VariableValue<int>).Value
            (index, copies)
            
        async {
            let! saveGame = SavegameFile.ReadAsync(path) |> Async.AwaitTask
            let gwentData = (saveGame.Variables.[11]) :?> BsVariable
            return gwentData.Variables
                            |> Seq.skip 2
                            |> Seq.takeWhile (fun v -> v.Name <> "SBSelectedDeckIndex")
                            |> Seq.filter (fun v -> v.Name = "cardIndex" || v.Name = "numCopies")
                            |> Seq.chunkBySize 2
                            |> Seq.map parseInfo
                            |> Map.ofSeq
        }
        
    let loadCardData =
        let decks = [ "monsters"; "nilfgaard"; "neutral"; "northernrealms"; "scoiatael" ]

        async {
            let! decks = decks
                         |> List.map (fun deck -> async {
                             let! deck = Deck.AsyncLoad(Path.Combine("data", deck + ".json"))
                             return deck.Deck.Cards
                         })
                         |> Async.Parallel
                         
            return decks
                   |> Array.concat
                   |> List.ofArray
                   |> CardDataLoaded
        }
        |> Cmd.ofAsyncMsg
       
    let loadCardState path =
        async {
            let! cardData = parseSaveFile path
            return LoadCards (CardState.Success cardData)
        }
        |> Cmd.ofAsyncMsg

    let timed methodName f = 
            let timer = new System.Diagnostics.Stopwatch()
            timer.Start()
            let returnValue = f()
            printfn "[%s] - Elapsed Time: %i" methodName timer.ElapsedMilliseconds
            returnValue
    
    let filterCards cards cardsProgress filters =
        Cmd.ofMsg (
            let toRow cardProgress (card:Card) =
                let copies = match cardProgress with
                             | Loading | NotAsked | Error _ -> 0
                             | Success c -> c.TryFind(card.Index) |> function
                                                                     | Some copies -> copies
                                                                     | None -> 0
                { Index = card.Index; Name = card.Name; Copies = copies; Deck = card.Deck; Type = card.Type;
                  Location = card.Location; Region = card.Region; Flavor = card.Flavor;
                  Visible = (shouldFilterCard filters card) }
                
            FilteredCards (cards
                           |> List.map (toRow cardsProgress)
                           |> List.sortBy (fun c -> c.Obtained, c.Name))
        )

    let removeFilter list item =
        let compareInfo = CultureInfo.CurrentUICulture.CompareInfo
        
        list |> List.filter (fun x -> compareInfo.Compare(x, item, CompareOptions.IgnoreCase) < 0)
    
    let getSaveDir path =
        match Directory.Exists path with
        | true -> Some path
        | false ->
            printfn "Directory %s doesn't exist" path
            let fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "The Witcher 3", "gamesaves")
            match Directory.Exists fallback with
            | true -> Some fallback
            | false ->
                printfn "Fallback directory %s doesn't exist" fallback
                None
    
    let getLatestSave (dir: DirectoryInfo) =
        dir.GetFiles("*.sav")
           |> Seq.sortByDescending (fun f -> f.LastWriteTime)
           |> Seq.map (fun f -> f.FullName)
           |> Seq.tryHead
    
    let watchFiles path dispatch =
        let changed = (fun (e: FileSystemEventArgs) -> dispatch (SaveGameUpdated e.FullPath))
        let finalPath = getSaveDir path
            
        match finalPath with
        | Some path ->
            try
                let watcher = new FileSystemWatcher(path, "*.sav")
                watcher.EnableRaisingEvents <- true            
                watcher.Renamed.Subscribe changed |> ignore
                watcher.Created.Subscribe changed |> ignore
                
                let save = getLatestSave (DirectoryInfo(path))
                match save with
                | Some savePath -> dispatch (SaveGameUpdated savePath)
                | None -> printfn "No save games found in '%s'" path             
            with err ->
                printf "Unable to watch save game directory %s for changes" path
        | None -> printfn "Couldn't find directory '%s'" path
    
    let initModel = {
        IsBusy = false
        CardsProgress=CardState.Loading
        Filters = List.empty
        FilterText = ""
        SelectedCard=None
        Cards = List.empty
        VisibleCards = List.empty
    }

    let init () = initModel, loadCardData
    
    let update msg model =
        match msg with
        | CardDataLoaded cards   -> { model with Cards = cards }, Cmd.none
        | SaveGameUpdated path   -> { model with IsBusy = true }, loadCardState path
        | SelectCard card        -> { model with SelectedCard = card }, Cmd.none
        | FilterCards            -> { model with IsBusy = true }, filterCards model.Cards model.CardsProgress model.Filters
        | FilteredCards cards    -> { model with VisibleCards = cards; IsBusy = false }, Cmd.none
        | AddFilter text         -> { model with Filters = (model.Filters @ [text]) }, Cmd.batch [Cmd.ofMsg ClearFilter; Cmd.ofMsg FilterCards]
        | RemoveFilter filter    -> { model with Filters = (removeFilter model.Filters filter) }, Cmd.batch [Cmd.ofMsg ClearFilter; Cmd.ofMsg FilterCards]
        | ClearFilter            -> { model with FilterText = "" }, Cmd.none
        | LoadCards cardProgress -> { model with CardsProgress = cardProgress; }, Cmd.ofMsg FilterCards
    
    let program watchDir texturePathFmt =
        let view = Views.view texturePathFmt
        Program.mkProgram init update view
        |> Program.withSubscription (fun _ -> Cmd.ofSub (watchFiles watchDir))

type App () as app = 
    inherit Application ()
    
    let watchDir = "/mnt/games/Steam/Linux/steamapps/compatdata/292030/pfx/drive_c/users/steamuser/My Documents/The Witcher 3/gamesaves/"
    let texturePathFmt = Printf.StringFormat<int->string>("https://rfvgyhn.blob.core.windows.net/gwent/%i.png")
    
    let runner = 
        App.program watchDir texturePathFmt
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app



