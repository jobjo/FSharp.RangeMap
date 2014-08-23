namespace FSharp.Collections.RangeMap

[<AutoOpen>]
module RangeMap =
    open FSharp.Collections.RangeMap.Internal.Tree

    /// Map interface
    type IRangeMap<'K,'V> =
        abstract member Elements : unit -> list<'K * 'V>
        abstract member Lookup : 'K -> option<'V>
        abstract member Remove : 'K -> IRangeMap<'K, 'V>
        abstract member RemoveRange : option<'K> -> option<'K> -> IRangeMap<'K,'V>
        abstract member Insert : 'K -> 'V -> IRangeMap<'K,'V>
        abstract member LookupRange : option<'K> -> option<'K> -> list<'V>
        abstract member Map<'U> : ('V -> 'U) -> IRangeMap<'K,'U>


    let rec private fromTree<'K,'V when 'K : comparison> (tree: Tree<'K,'V>) : IRangeMap<'K,'V> =
        { new IRangeMap<'K,'V> with
            member this.Elements () = elements tree
            member this.Lookup (k: 'K) = lookup k tree
            member this.Remove (k: 'K) = fromTree <| remove k tree
            member this.RemoveRange l h = fromTree <| removeRange l h tree
            member this.Insert (k: 'K) (v: 'V) = fromTree <| insert k v tree
            member this.LookupRange low high = lookupRange low high tree
            member this.Map<'U> (f: 'V -> 'U) = fromTree <| map f tree
        }

    /// Builds a range map from a sequence of key-value pairs.
    let fromSeq<'K,'V when 'K : comparison> (xs : seq<'K * 'V>) =
        xs |> fromSeq |> fromTree

    /// Empty range map.
    let empty<'K,'V when 'K : comparison> : IRangeMap<'K,'V> = fromSeq []
    
    /// Maps over all elements.
    let inline map f (rm: IRangeMap<'K, 'V>) = rm.Map f

    /// Extracts all key-value pairs from a range map.
    let inline elements (rm: IRangeMap<'K,'V>) = rm.Elements()

    /// Tries to find the value of an element with the given key.
    let inline lookup k (rm: IRangeMap<'K, 'V>) = rm.Lookup k

    /// Removes an element with the given key if exists.
    let inline remove k (rm: IRangeMap<'K, 'V>) = rm.Remove k

    /// Tries to find the value of an element with the given key.
    let inline removeRange l h (rm: IRangeMap<'K, 'V>) = rm.RemoveRange l h

    /// Returns true if there exists an element with the key.
    let inline containsKey k (rm: IRangeMap<'K, 'V>) = Option.isSome <| rm.Lookup k 

    /// Returns all elements who's keys are in the given range.
    let inline lookupRange low high (rm: IRangeMap<'K, 'V>) = rm.LookupRange low high

    /// Inserts a key-value pair into a range map.
    let inline insert<'K,'V> k v (rm: IRangeMap<'K,'V>) = rm.Insert k v
