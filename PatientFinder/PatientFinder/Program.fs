namespace PatientFinder

open System
open Elmish.WPF
open System.Windows
open PatientFinder.View
open PatientFinder.Network
open MedicalService

(* The WCF service defines ViewName as:
       type ViewName =
          new: unit -> ViewName
          member birthDate: DateTime with get,set
          member chart_number: Nullable<int> with get,set
          member donotsee: Nullable<bool> with get,set
          member firstname: string with get,set
          member lastname: string with get, set
          member missed_appointments: Nullable<int> with get,settype ViewName =
          new: unit -> ViewName
          member birthDate: DateTime with get,set
          member chart_number: Nullable<int> with get,set
          member donotsee: Nullable<bool> with get,set
          member firstname: string with get,set
          member lastname: string with get, set
          member missed_appointments: Nullable<int> with get,set
*)


module Name =
    (*I am trying to give an Id to each of the downloaded ViewName objects. Also, I want to read the database only once, despite having three 
      separate views of the same list of objects.*)

    type name =
       { NameId: Guid      
         ViewName: ViewName}
  
    type names =
       { Names: name[] 
         LastName: string
         FirstName: string
         BirthDate: DateTime option}

    // read database
    let getAllAppointmentsForDate : ViewName[] = 
        Async.RunSynchronously (FsNetwork.GetAllPatientNamesAsync())

    let SetLastName (n:names, t:string) =
        {n with LastName = t.ToLower() }

    let SetFirstName (n:names, t:string) =
       {n with FirstName = t.ToLower() }

    let SetBirthDate (n:names, d:DateTime option) =
       match d with
       | Some dd -> {n with BirthDate = Some dd }
       | None -> { n with BirthDate = None }

    let newFilter (ns:names)  =
       ns.Names |> Array.filter ( fun n -> 
                                          n.ViewName.lastname.ToLower().StartsWith(ns.LastName)   &&
                                          n.ViewName.firstname.ToLower().StartsWith(ns.FirstName)                       
                                          )
            
    let init =
        let MakeNames =
            getAllAppointmentsForDate |> Array.map (fun q -> {NameId = Guid.NewGuid(); ViewName=q})
        {Names = MakeNames; LastName  = ""; FirstName = ""; BirthDate =None}

    let filteredSuggestions s =
        {s with Names = newFilter s}
       

(*Why do I need an "open" statement here? *)
open Name
    

module FinderLastName =
    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: string
          SelectedItem: name option
        }

    let init (n) = {
       Visibility = Visibility.Collapsed  
       IsOpen = true
       Text = "Hi"
       SelectedItem = None
       }

    type Msg =
          | SetOpen of bool
          | SetText of string
          | SetSelectedItem of name option

    let CloseAutoSuggestionBox m =
        { m with IsOpen = false; Visibility = Visibility.Collapsed}  

    let OpenAutoSuggestionBox m  =
         { m with  IsOpen = true; Visibility = Visibility.Visible } 

    let update msg m  =
        match msg with
        | SetOpen o -> { m with IsOpen = o }
        | SetText s -> { m with Text = s; IsOpen = true; Visibility = Visibility.Visible }
        | SetSelectedItem i -> { m with SelectedItem = i; IsOpen = i.IsNone; Visibility = Visibility.Collapsed; Text = if i.IsSome then i.Value.ViewName.lastname else "" }  

    
    let bindings () = [
        "Suggestions" |> Binding.oneWay filteredSuggestions
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), SetOpen)
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module FinderFirstName =
    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: string
          SelectedItem: ViewName option
        }

    let init (n) = {
       Visibility = Visibility.Collapsed  
       IsOpen = true
       Text = "Hi"
       SelectedItem = None
       }

    type Msg =
          | SetOpen of bool
          | SetText of string
          | SetSelectedItem of ViewName option

    let CloseAutoSuggestionBox m =
        { m with IsOpen = false; Visibility = Visibility.Collapsed}  

    let OpenAutoSuggestionBox m  =
         { m with  IsOpen = true; Visibility = Visibility.Visible } 

    
    let update msg m  =
        match msg with
        | SetOpen o -> { m with IsOpen = o }
        | SetText s -> { m with Text = s; IsOpen = true; Visibility = Visibility.Visible }
        | SetSelectedItem i -> { m with SelectedItem = i; IsOpen = i.IsNone; Visibility = Visibility.Collapsed; Text = if i.IsSome then i.Value.firstname else "" }  

    
    let bindings () = [
        "Suggestions" |> Binding.oneWay filteredSuggestions
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), SetOpen)
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module FinderBirthDate =
    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: DateTime option
          SelectedItem: ViewName option
        }

    let init (n) = {
       Visibility = Visibility.Collapsed  
       IsOpen = true
       Text = None
       SelectedItem = None
       }

    type Msg =
          | SetOpen of bool
          | SetText of DateTime option
          | SetSelectedItem of ViewName option

    let CloseAutoSuggestionBox m =
        { m with IsOpen = false; Visibility = Visibility.Collapsed}  

    let OpenAutoSuggestionBox m  =
         { m with  IsOpen = true; Visibility = Visibility.Visible } 

(*No filter as yet on BirthDate as I do not know how to test for a valid birthdate from the user nor how to filter on it!*)
    
    let update (ns,msg) m  =
        match msg with
        | SetOpen o -> { m with IsOpen = o }
        | SetText s -> { m with Text = s; IsOpen = true; Visibility = Visibility.Visible }
        | SetSelectedItem i -> { m with SelectedItem = i; IsOpen = i.IsNone; Visibility = Visibility.Collapsed; Text = if i.IsSome then Some DateTime.Now else None }  

    
    let bindings () = [
        "Suggestions" |> Binding.oneWay filteredSuggestions
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), SetOpen)
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module App =

    type Model =
        { FinderBirthDate: FinderBirthDate.Model
          FinderLastName:  FinderLastName.Model 
          FinderFirstName: FinderFirstName.Model 
          Suggestions: names }
    
    let init () =
        Name.init |> (fun q -> 
            { FinderBirthDate = FinderBirthDate.init (q)
              FinderLastName =  FinderLastName.init (q) 
              FinderFirstName = FinderFirstName.init(q)
              Suggestions = q}
         )

    let filteredSuggestions m =
        Name.filteredSuggestions m.Suggestions
    
    type Msg =
        | FinderBirthDateMsg of names * FinderBirthDate.Msg
        | FinderLastNameMsg  of names * FinderLastName.Msg
        | FinderFirstNameMsg of names * FinderFirstName.Msg

    let update msg m =
        match msg with
        | FinderBirthDateMsg (ns,msg) ->
            { m with FinderBirthDate = FinderBirthDate.update (ns,msg) m.FinderBirthDate; Suggestions = filteredSuggestions m }
        | FinderLastNameMsg (ns,msg) ->
            { m with FinderLastName  = FinderLastName.update msg m.FinderLastName; Suggestions = filteredSuggestions m }
        | FinderFirstNameMsg (ns,msg) ->
            { m with FinderFirstName = FinderFirstName.update msg m.FinderFirstName; Suggestions = filteredSuggestions m } 


    let bindings () : Binding<Model, Msg> list = [
        "FinderBirthDate" |> Binding.subModel(
          (fun m -> m.FinderBirthDate),
          snd,
          FinderBirthDateMsg,
          FinderBirthDate.bindings)
    
        "FinderLastName" |> Binding.subModel(
          (fun m -> m.FinderLastName),
          snd,
          FinderLastNameMsg,
          FinderLastName.bindings)

        "FinderFirstName" |> Binding.subModel(
          (fun m -> m.FinderFirstName),
          snd,
          FinderFirstNameMsg,
          FinderFirstName.bindings)
      ]

    [<EntryPoint; STAThread>]
    let main argv =
      Program.mkSimpleWpf init update bindings
      |> Program.runWindowWithConfig
         { ElmConfig.Default with LogConsole = true }      
         (MainWindow())

   