(*
The goal of the architecture is to provide a solid UI-independent core to build the rest of the functionality around. Elm architecture operates 
using the following concepts, as they translate to Elmish:

Model
This is a snapshot of your application's state, defined as an immutable data structure.

Message
This an event representing a change (delta) in the state of your application, defined as a discriminated union.

Command
This is a carrier of instructions, that when evaluated may produce one or more messages.

Init
This is a pure function that produces the inital state of your application and, optionally, commands to process.

Update
This is a pure function that produces a new state of your application given the previous state and, optionally, new commands to process.

View
This is a pure function that produces a new UI layout/content given the current state, defined as an F# function that uses a renderer (such as React) to declaratively build a UI.

Program
This is an opaque data structure that combines all of the above plus a setState function to produce a view from the model. See the Program module for more details.
*)


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
          member missed_appointments: Nullable<int> with get,set
*)


open Elmish
open System.Text.RegularExpressions
open System.Globalization
    

module FinderLastName =
    type ExternalMsg =
       | NoOp
       | SelectionMade of ViewName option
       | GetSuggestions of string


    /// Msg. This an event representing a change (delta) in the state of the application, defined as a discriminated union.
    type Msg =
        /// Open the PopUp and make it visible.
        | OpenPopup
        /// Close the Popup and collapse it.
        | ClosePopup
        /// Text string typed in by user.
        | SetText of string
        /// User selected an item from the PopUp.
        | SetSelectedItem of ViewName option

    /// Model. This is a snapshot of the application's state, defined as an immutable data structure.
    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: string
          SelectedItem: ViewName option
          Suggestions: ViewName[]
        }

    /// Init. This is a pure function that produces the inital state of the application and, optionally, commands to process.
    let init () = 
        {
           Visibility = Visibility.Collapsed
           IsOpen = false
           Text = ""
           SelectedItem = None
           Suggestions = [||]
        },  Cmd.none           // no initial command

    let SetSuggestion (m:Model, vn:ViewName option) =
        { m with SelectedItem = vn; Text = match vn with |Some s -> s.lastname |None -> m.Text}
       
    let SetSuggestions m s =
       {m with Suggestions = s}

    let FilterSuggestions m v : ViewName[] =
        v |> Array.filter (fun q -> q.lastname.ToLower().StartsWith(m.Text.ToLower() ))


    /// Update. This is a pure function that produces a new state of the application given the previous state and, optionally, new commands to process.
    /// It is called by the parent update function.
    let update  msg m  =
        match msg with
        | OpenPopup ->  { m with IsOpen = true; Visibility = Visibility.Visible }, Cmd.none, NoOp
        | ClosePopup -> { m with IsOpen = false; Visibility = Visibility.Collapsed}  , Cmd.none, NoOp
        | SetText s ->  { m with Text = s }, Cmd.ofMsg OpenPopup, GetSuggestions s
        | SetSelectedItem i -> { m with SelectedItem = i; Text = if i.IsSome then i.Value.lastname else "" }, Cmd.ofMsg ClosePopup, SelectionMade i 

    
    let bindings () = [
        "Suggestions" |> Binding.oneWay (fun m -> m.Suggestions)
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), (fun b -> if b = true then OpenPopup else ClosePopup)) 
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module FinderFirstName =
    type ExternalMsg =
       | NoOp
       | SelectionMade of ViewName option
       | GetSuggestions of string

    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: string
          SelectedItem: ViewName option
          Suggestions: ViewName[]
        }

    let init () = 
       {
           Visibility = Visibility.Collapsed  
           IsOpen = true
           Text = ""
           SelectedItem = None
           Suggestions = [||]
       }, Cmd.none               // no initial command

    /// Msg. This an event representing a change (delta) in the state of the application, defined as a discriminated union.
    type Msg =
          | OpenPopup
          | ClosePopup 
          | SetText of string
          | SetSelectedItem of ViewName option

    let SetSuggestion (m:Model, vn:ViewName option) =
        {m with SelectedItem = vn; Text = match vn with |Some s -> s.firstname  | None -> m.Text}

    let SetSuggestions m s =
        {m with Suggestions = s}
    
    let FilterSuggestions m v : ViewName[] =
        v |> Array.filter (fun q -> q.firstname.ToLower().StartsWith(m.Text.ToLower() ))

    let update msg m  =
        match msg with
        | OpenPopup ->  { m with IsOpen = true; Visibility = Visibility.Visible }, Cmd.none, NoOp
        | ClosePopup -> { m with IsOpen = false; Visibility = Visibility.Collapsed}  , Cmd.none, NoOp
        | SetText s -> { m with Text = s }, Cmd.ofMsg OpenPopup, GetSuggestions s
        | SetSelectedItem i -> { m with SelectedItem = i; Text = if i.IsSome then i.Value.firstname else "" }, Cmd.ofMsg ClosePopup, SelectionMade i  
   
    let bindings () = [
        "Suggestions" |> Binding.oneWay (fun m -> m.Suggestions)
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), (fun b -> if b = true then OpenPopup else ClosePopup))
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module FinderBirthDate =
    type ExternalMsg =
        | NoOp
        | SelectionMade of ViewName option
        | GetSuggestions of string

    type Model =
        { Visibility: System.Windows.Visibility
          IsOpen: bool
          Text: string
          SelectedItem: ViewName option
          Suggestions: ViewName[]
        }

    let init () = 
       {
           Visibility = Visibility.Collapsed  
           IsOpen = true
           Text = ""
           SelectedItem = None
           Suggestions = [||]
       },  Cmd.none           // no initial command

    let SetSuggestion (m:Model, vn:ViewName option) =
       {m with SelectedItem = vn; Text = match vn with |Some s -> s.birthdate.ToShortDateString()  |None -> m.Text }

    let SetSuggestions m s =
        {m with Suggestions = s}

    let IsDate m :bool =
        let _regex = new Regex(@"[0-9]{2}/[0-9]{2}/[0-9]{4}") // This is ALLOWED text for full DATE pattern. But maynot be a real date.
        _regex.IsMatch(m.Text); 

    let FilterSuggestions m v : ViewName[] =
        v |> Array.filter (fun q -> if IsDate m then match System.DateTime.TryParseExact(m.Text,"MM/dd/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None) with
                                                     | true, d -> q.birthdate.ToShortDateString() =  d.ToShortDateString()
                                                     | _ -> true      // match everything
                                    else true                     
                          )

    /// Msg. This an event representing a change (delta) in the state of the application, defined as a discriminated union.
    type Msg =
          | OpenPopup
          | ClosePopup 
          | SetText of string
          | SetSelectedItem of ViewName option

    let CloseAutoSuggestionBox m =
        { m with IsOpen = false; Visibility = Visibility.Collapsed}  

    let OpenAutoSuggestionBox m  =
         { m with  IsOpen = true; Visibility = Visibility.Visible } 
   
    let update msg m  =
        match msg with
        | OpenPopup ->  { m with IsOpen = true; Visibility = Visibility.Visible }, Cmd.none, NoOp
        | ClosePopup -> { m with IsOpen = false; Visibility = Visibility.Collapsed}  , Cmd.none, NoOp
        | SetText s -> { m with Text = s }, Cmd.ofMsg OpenPopup, GetSuggestions s
        | SetSelectedItem i -> { m with SelectedItem = i; Text = if i.IsSome then i.Value.birthdate.ToShortDateString() else "" }, Cmd.ofMsg ClosePopup, SelectionMade i 

    
    let bindings () = [
        "Suggestions" |> Binding.oneWay (fun m -> m.Suggestions)
        "Visibility" |> Binding.oneWay ( fun m -> m.Visibility)
        "IsOpen" |> Binding.twoWay ( (fun m -> m.IsOpen), (fun b -> if b = true then OpenPopup else ClosePopup))
        "Text" |> Binding.twoWay((fun m -> m.Text), SetText)
        "SelectedSuggestion" |> Binding.twoWayOpt((fun m -> m.SelectedItem), SetSelectedItem)
     ]

module App =

    /// The parent module holds the message cases for each child instance
    /// Msg. This an event representing a change (delta) in the state of the application, defined as a discriminated union.
    type Msg =
       | FinderBirthDateMsg of FinderBirthDate.Msg
       | FinderLastNameMsg  of FinderLastName.Msg
       | FinderFirstNameMsg of FinderFirstName.Msg

    /// The parent module holds the types of its children. This is an example of nesting logic. 
    type Model =
        { FinderBirthDate: FinderBirthDate.Model
          FinderLastName:  FinderLastName.Model 
          FinderFirstName: FinderFirstName.Model
          Suggestions: ViewName[]
          SelectedSuggestion: ViewName option
        }

    /// read database
    let getAllPatientNames : ViewName[] = 
        Async.RunSynchronously (FsNetwork.GetAllPatientNamesAsync())

    /// init. The initialization logic, where we ask for the children to be initialized
    let init () =
        let finderBirthDate, finderBirthDateCmd = FinderBirthDate.init()
        let finderLastName, finderLastNameCmd   = FinderLastName.init()
        let finderFirstName, finderFirstNameCmd = FinderFirstName.init()

        { FinderBirthDate = finderBirthDate
          FinderLastName  = finderLastName
          FinderFirstName = finderFirstName
          Suggestions = getAllPatientNames
          SelectedSuggestion = None        
        }, 
        Cmd.batch [ Cmd.map FinderBirthDateMsg finderBirthDateCmd
                    Cmd.map FinderLastNameMsg  finderLastNameCmd
                    Cmd.map FinderFirstNameMsg finderFirstNameCmd  ]
        // Cmd.map is used to "elevate" the child message into the parent container type, using corresponding parent message types (i.e., FinderBirthDateMsg, FinderLastNameMsg, FinderFirstNameMsg),
        // case constructors as the mapping function. We batch the commands together to produce a single command for our entire container.
    
    /// This Main update will be called BEFORE the child updates are called.
    let update msg m =            
        match msg with
        | FinderBirthDateMsg finderBirthDateMsg ->
            // Text input starts here!

            // Have the FinderBirthDate child update itself. Decompose the results of the FinderBirthDate.update function.
            let finderBirthDate, cmd, extraMsg = FinderBirthDate.update finderBirthDateMsg m.FinderBirthDate

            let newSuggestions (m:Model) =
                getAllPatientNames 
                |> FinderLastName.FilterSuggestions m.FinderLastName
                |> FinderFirstName.FilterSuggestions m.FinderFirstName
                |> FinderBirthDate.FilterSuggestions finderBirthDate
                |> Array.sortBy (fun s ->s.birthdate.ToString("yyyy-MM-dd")+" "+ s.lastname+" "+ s.firstname)           
               

            // Match over finderBirthDateExtraMsg to do something
            let newModel =
                match extraMsg with
                | FinderBirthDate.ExternalMsg.NoOp -> { m with FinderBirthDate = finderBirthDate }
                | FinderBirthDate.ExternalMsg.GetSuggestions text ->
                    let newlist = newSuggestions (m) |> FinderBirthDate.SetSuggestions finderBirthDate
                    {m with FinderBirthDate = newlist; SelectedSuggestion = None }
                | FinderBirthDate.ExternalMsg.SelectionMade viewname -> { m with    SelectedSuggestion = viewname;
                                                                                    FinderLastName  = FinderLastName.SetSuggestion (m.FinderLastName, viewname) ;
                                                                                    FinderFirstName = FinderFirstName.SetSuggestion (m.FinderFirstName, viewname); 
                                                                                    FinderBirthDate = finderBirthDate 
                                                                        }
            newModel, Cmd.map FinderBirthDateMsg cmd

        | FinderLastNameMsg finderLastNameMsg ->
            // Text input starts here!

            // Tell FinderLastName to update its Text property. The finderLastNameMsg is SetText from the FinderLastName module.
            // Decompose the results of the FinderLastName.update function. This update will update the FinderLastName model with new text. 
            let finderLastName, cmd, extraMsg = FinderLastName.update finderLastNameMsg m.FinderLastName

            let newSuggestions (t:string) =
                getAllPatientNames 
                   |> FinderLastName.FilterSuggestions  finderLastName
                   |> FinderFirstName.FilterSuggestions m.FinderFirstName
                   |> FinderBirthDate.FilterSuggestions m.FinderBirthDate
                   |> Array.sortBy (fun s -> s.lastname+" "+s.firstname+" "+s.birthdate.ToString("yyyy-MM-dd") ) 
                 
            // Match over extraMsg to do something.
            let newModel =
                match extraMsg with
                | FinderLastName.ExternalMsg.NoOp -> { m with FinderLastName = finderLastName }
                | FinderLastName.ExternalMsg.SelectionMade viewname -> { m with SelectedSuggestion = viewname;  
                                                                                FinderLastName  = finderLastName;
                                                                                FinderFirstName = FinderFirstName.SetSuggestion (m.FinderFirstName, viewname); 
                                                                                FinderBirthDate = FinderBirthDate.SetSuggestion (m.FinderBirthDate, viewname) 
                                                                       }
                | FinderLastName.ExternalMsg.GetSuggestions text -> 
                        let newlist = newSuggestions text |> FinderLastName.SetSuggestions finderLastName
                        {m with FinderLastName = newlist; SelectedSuggestion = None }

            // Route (i.e. Map) commands specific to the FinderLastName back to it for execution.
            // Must update FinderLastName for Popup to be shown.
            newModel, Cmd.map FinderLastNameMsg cmd  
            
        | FinderFirstNameMsg finderFirstNameMsg ->
             // Text input starts here!
           
            // Tell FinderFirstName to update its Text property. The finderFirstNameMsg is SetText from the FinderFirstName module.
            // Decompose the results of the FinderFirstname.update function.
            let finderFirstName, cmd, extraMsg = FinderFirstName.update finderFirstNameMsg m.FinderFirstName

            let newSuggestions (t:string) =
                getAllPatientNames 
                |> FinderLastName.FilterSuggestions  m.FinderLastName
                |> FinderFirstName.FilterSuggestions finderFirstName
                |> FinderBirthDate.FilterSuggestions m.FinderBirthDate
                |> Array.sortBy (fun s -> s.firstname+" "+s.lastname+" "+s.birthdate.ToString("yyyy-MM-dd") ) 
               
            // Match over extraMsg to do something
            let newModel =
                match extraMsg with
                | FinderFirstName.ExternalMsg.NoOp -> { m with FinderFirstName = finderFirstName }
                | FinderFirstName.ExternalMsg.SelectionMade viewname -> { m with    SelectedSuggestion = viewname;    
                                                                                    FinderLastName  = FinderLastName.SetSuggestion (m.FinderLastName, viewname)
                                                                                    FinderFirstName = finderFirstName ; 
                                                                                    FinderBirthDate = FinderBirthDate.SetSuggestion (m.FinderBirthDate, viewname) 
                                                                        }
                | FinderFirstName.ExternalMsg.GetSuggestions text -> 
                        let newlist = newSuggestions text |> FinderFirstName.SetSuggestions finderFirstName
                        {m with FinderFirstName = newlist;  SelectedSuggestion = None }

            newModel, Cmd.map FinderFirstNameMsg cmd


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
      Program.mkProgramWpf init update bindings
      |> Program.runWindowWithConfig
         { ElmConfig.Default with LogConsole = true }      
         (MainWindow())
   