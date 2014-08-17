namespace FSharp.RangeMap.Examples

module Main =

    open Utils
    open FSharp.Data.RangeMap

    module RM = FSharp.Data.RangeMap.RangeMap

    [<EntryPoint>]
    let main argv = 

        // Generate a list of elements.
        let elements =
            let size = int 1e5
            List.map (fun x -> (x,x)) (randList size)

        // Create a standard .NET dictionary containing all elements.
        let dict, tm = time <| fun _ -> dict<_,_>(elements)
        printfn "Dictionary created in: %As" tm

        // Create a FSharp map with all elements.
        let map, tm = time <| fun _ -> Map.ofSeq elements
        printfn "Map created in: %As" tm

        // Create a Range Map holding the elements.
        let rm, tm = time <| fun _ -> RM.fromSeq elements
        printfn "Range map created in %As" tm

        // Iterate over 10K randomly selected existing keys.
        let withExistingKeys =
            let keys = 
                elements
                |> shuffle
                |> Seq.truncate 10000
                |> Seq.map fst
                |> List.ofSeq
            fun (f: int -> unit) -> Seq.iter f keys

        // let withRange =

        // Iterate over 10K randomly selected random keys (not likely to exist)
        let withNonExistingKeys =
            let keys = randList 10000
            fun (f: int -> unit) -> Seq.iter f keys

        let res =
            [
                // Lookup existing keys
                "Lookup existing keys from map", fun _ -> 
                    withExistingKeys (fun key -> Map.containsKey key map |> ignore)
                "Lookup existing keys from range map", fun _ -> 
                    withExistingKeys (fun key -> RM.containsKey key rm |> ignore)
                "Lookup existing keys from dictionary", fun _ -> 
                    withExistingKeys (fun key -> dict.ContainsKey key |> ignore)
                
                // Lookup non-existing keys
                "Lookup non-existing keys from map", fun _ -> 
                    withNonExistingKeys (fun key -> Map.containsKey key map |> ignore)
                "Lookup non-existing keys from range map", fun _ -> 
                    withNonExistingKeys (fun key -> RM.containsKey key rm |> ignore)
                "Lookup non-existing keys from dictionary", fun _ -> 
                    withNonExistingKeys (fun key -> dict.ContainsKey key |> ignore)

                // Remove existing key
                "Remove existing key from map", fun _ ->
                    withExistingKeys (fun key -> Map.remove key map |> ignore)

                "Remove existing key from range map", fun _ ->
                    withExistingKeys (fun key -> RM.remove key rm |> ignore)

                // Lookup range


            ]
            |> benchmark 100

        for (name, time) in res do
            printfn "%s: %A" name time


        0 // return an integer exit code
