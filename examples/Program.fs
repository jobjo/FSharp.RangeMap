namespace FSharp.RangeMap.Examples

module Main =

    open Utils
    open FSharp.Data.RangeMap
    module RM = FSharp.Data.RangeMap.RangeMap


    let benchmarks numElems =
        // Generate a list of elements.
        let elements =
            let size = int numElems
            List.map (fun x -> (x,x)) (randomList size)

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

        // Iterate over 10K randomly selected random keys (not likely to exist)
        let withNonExistingKeys =
            let keys = randomList 10000
            fun (f: int -> unit) -> Seq.iter f keys

        // Compare map, range map and dictionary performance.
        let res1 =
            [
                // Lookup existing keys
                "Lookup existing keys from map", fun _ -> 
                    withExistingKeys (fun key -> Map.containsKey key map |> ignore)
                "Lookup existing keys from range map", fun _ -> 
                    withExistingKeys (fun key -> RM.containsKey key rm |> ignore)

                "Lookup existing keys from dictionary", fun _ -> 
                    withExistingKeys (fun key -> dict.ContainsKey key |> ignore)
            ]
            |> benchmark 100

        let res2 =
            [
                // Lookup non-existing keys
                "Lookup non-existing keys from map", fun _ -> 
                    withNonExistingKeys (fun key -> Map.containsKey key map |> ignore)
                "Lookup non-existing keys from range map", fun _ -> 
                    withNonExistingKeys (fun key -> RM.containsKey key rm |> ignore)
                "Lookup non-existing keys from dictionary", fun _ -> 
                    withNonExistingKeys (fun key -> dict.ContainsKey key |> ignore)
            ]
            |> benchmark 100

        let res3 =
            [

                // Remove existing key
                "Remove existing key from map", fun _ ->
                    withExistingKeys (fun key -> Map.remove key map |> ignore)
                "Remove existing key from range map", fun _ ->
                    withExistingKeys (fun key -> RM.remove key rm |> ignore)
            ]
            |> benchmark 100

        // Example of how to use range lookup.
        let kvs = List.map (fun k -> (k,k)) [1 .. int numElems]
        let rm = RM.fromSeq kvs
        let map = Map.ofSeq kvs

        let res4 =
            [
                "Lookup 1000 elements in a range from map", fun _ ->
                    for k in [1000 .. 2000] do
                        ignore <| Map.find k map

                "Lookup 1000 elements in a range from range map", fun _ ->
                    RM.lookupRange (Some 1000) (Some 1100) rm
                    |> ignore

                "Lookup 10000 elements in a range from map", fun _ ->
                    for k in [1000 .. 11000] do
                        ignore <| Map.tryFind k map

                "Lookup 10000 elements in a range from range map", fun _ ->
                    RM.lookupRange (Some 1000) (Some 11000) rm
                    |> ignore
            ]
            |> benchmark 100
        [res1; res2; res3; res4]


    [<EntryPoint>]
    let main argv = 
        let numElems = 1e5

        printfn "\nResults with 10K elements\n"
        printLabelTimeResults <| benchmarks 1e4

        printfn "\nResults with 100K elements\n"
        printLabelTimeResults <| benchmarks 1e5

        printfn "\nResults with 1m elements\n"
        printLabelTimeResults <| benchmarks 1e6

        0