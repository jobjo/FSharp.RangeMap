namespace FSharp.Collections.RangeMap.Examples

module ListRangeMap=

    open Utils
    open FSharp.Collections.RangeMap

    /// Here is an example of creating a custom range map. In this case using
    /// a naive list based implementation.
    let rec fromList<'K,'V when 'K : comparison> (list: List<'K * 'V>) : IRangeMap<'K,'V> =
        let inRange l h k =
            match l, h with
            | Some l, _  when k < l -> false
            | _, Some h when k > h  -> false
            | _                     -> true
        { new IRangeMap<'K,'V> with
            member this.Elements () = list
            member this.Lookup k = 
                List.tryPick (fun (k',v) -> if k' = k then Some v else None) list
            member this.Remove (k: 'K) = 
                fromList <| List.filter (fun (k',v) -> k <> k' ) list
            member this.RemoveRange l h = 
                fromList <| List.filter (fun (k,v) -> inRange l h k ) list
            member this.Insert k v = 
                let list' = List.filter (fun (k',v) -> k <> k' ) list
                fromList ((k,v) :: list')
            member this.LookupRange l h = 
                List.filter (fst >> inRange l h) list  |> List.map snd
            member this.Map<'U> (f: 'V -> 'U) =
                fromList <| List.map (fun (k,v) -> (k, f v)) list
        }

    let myListMap = fromList <| List.map (fun x -> (x,x)) [1 .. 100]
    let range = lookupRange (Some 10) (Some 15) myListMap
    