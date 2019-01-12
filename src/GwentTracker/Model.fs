module Model
    open System
    open System.Collections.Generic
    open System.Diagnostics
    open System.Linq
    open Fabulous.Core
    open Fabulous.DynamicViews
    open Xamarin.Forms
    open FSharp.Configuration
    open FSharp.Data
    open System.Globalization
    open System.IO

    type Deck = JsonProvider<"data/monsters.json">
    let emptyIfDefault = Option.defaultValue ""
    type Deck.Card with
        member this.Location = String.Join(", ", this.Locations.Select(fun l -> l.Type).Distinct())
        member this.Region = String.Join(", ", this.Locations.Select(fun l -> if l.Type = "Base Deck" then l.Type else l.Region |> emptyIfDefault).Distinct())
    type Card = Deck.Card     
    
    type CardRow = {
        Index : int
        Name : string
        Flavor : string
        Copies : int
        Deck : string
        Type : string
        Location : string
        Region : string
        Visible : bool
    } with
        member this.Obtained = this.Copies > 0
    
    type CardState =
    | NotAsked
    | Loading
    | Success of Map<int, int>
    | Error of string
    
    type Model = {
        IsBusy : bool
        Cards : Card list
        VisibleCards : CardRow list
        CardsProgress : CardState
        Filters : string list
        FilterText : string
        SelectedCard : CardRow option
    }
    
    type Msg =
    | CardDataLoaded of Card list
    | SaveGameUpdated of string
    | SelectCard of CardRow option
    | FilterCards
    | FilteredCards of CardRow list
    | AddFilter of string
    | RemoveFilter of string
    | ClearFilter
    | LoadCards of CardState

