module Views
    open Fabulous.DynamicViews
    open Xamarin.Forms
    open Model

    let texturePath texturePathFmt card =
        match card with
        | Some c -> sprintf texturePathFmt c.Index
        | None -> ""
    
    let hasCard copies =
        match copies with
        | 0 -> "☐"
        | _ -> "☑"
    
    let displayName (card:CardRow) =
        let obtained = card.Copies |> hasCard
        
        sprintf "%s %s %i %s %s %s %s" obtained card.Name card.Copies card.Deck card.Type card.Location card.Region
    
    let cardsDisplay cards =
        cards |> List.map (fun c -> View.Label (displayName c))
        
    let cardName = function
        | Some card -> card.Name
        | None -> ""
        
    let cardFlavor = function
        | Some card -> card.Flavor
        | None -> ""
    
    let itemSelected dispatch (cards:CardRow list) id =
        match id with
         | Some i -> dispatch (SelectCard (Some(cards.Item(i))))
         | None -> dispatch (SelectCard None)
    
    let selectedCard texturePathFmt card =
        View.Grid(rowdefs = ["auto"; "auto"; "auto"], children = [
            View.Image(source = texturePath texturePathFmt card, aspect = Aspect.AspectFill).GridRow(0)
            View.StackLayout(children = [
                View.Label(text = cardName card)
                View.Label(text = cardFlavor card)
            ]).GridRow(1)    
//            View.StackLayout(children = [
//                View.Button(text = "map")
//            ]).GridRow(2)
        ])
    
    let messages =
        View.Label()
        
    let filterBar dispatch =
        View.SearchBar(placeholder = "Filter", searchCommand = (fun text -> dispatch (AddFilter text)))
        
    let activeFilters model dispatch =
        let filters = model.Filters
                      |> List.map (fun filter -> View.Button(text = filter, command = (fun _ -> dispatch (RemoveFilter filter))))  
        View.StackLayout(children = filters)
              
    let progressBar progress =
        let visible = match progress with
                      | Success _ | Error _ | NotAsked -> false
                      | Loading -> true
        View.ProgressBar(progress = 0.5, isVisible = visible)
                
    let cardRows cards =
        match cards with
        | [] -> [View.ActivityIndicator(isRunning = true, horizontalOptions = LayoutOptions.Center)]
        | c -> c |> List.map (fun card -> View.Label(text = displayName card, isVisible = card.Visible))
             
    let cardTable model dispatch =
        match model.CardsProgress with
        | Success _ | Loading ->
            View.ListView(items = cardRows model.VisibleCards, itemSelected = itemSelected dispatch model.VisibleCards)
        | Error msg -> View.Label(text = msg)
        | NotAsked -> View.Label(text = "Load a save file")
    
    let view texturePathFmt (model: Model) dispatch =
        View.ContentPage(content =
            View.Grid(coldefs = [204.; "*"], children = [
                View.StackLayout(margin = Thickness(4., 0., 4., 0.), children = [
                    selectedCard texturePathFmt model.SelectedCard
                    messages
                ]).GridColumn(0)
                
                View.Grid(rowdefs = ["auto"; "auto"; "auto"; "*"], children = [
                    View.Grid(coldefs = ["auto"; "auto"], children = [
                        (filterBar dispatch).GridColumn(0)
                        (activeFilters model dispatch).GridColumn(1)
                    ]).GridRow(0)
                    (progressBar model.CardsProgress).GridRow(2)
                    (cardTable model dispatch).GridRow(3)
                ]).GridColumn(1)
            ]))

