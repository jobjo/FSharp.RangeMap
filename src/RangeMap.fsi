namespace FSharp.Collections.RangeMap

[<AutoOpen>]
module RangeMap =

    /// Map interface
    type IRangeMap<'K,'V> =
        abstract member Elements : unit -> list<'K * 'V>
        abstract member Lookup : 'K -> option<'V>
        abstract member Remove : 'K -> IRangeMap<'K, 'V>
        abstract member RemoveRange : option<'K> -> option<'K> -> IRangeMap<'K,'V>
        abstract member Insert : 'K -> 'V -> IRangeMap<'K,'V>
        abstract member LookupRange : option<'K> -> option<'K> -> list<'V>
        abstract member Map<'U> : ('V -> 'U) -> IRangeMap<'K,'U>

    /// Builds a range map from a sequence of key and value pairs.
    val fromSeq<'K,'V when 'K : comparison> : (seq<'K * 'V>) -> IRangeMap<'K, 'V>

    /// Maps over all elements.
    val inline map : ('V -> 'U) -> IRangeMap<'K, 'V> -> IRangeMap<'K, 'U>

    /// Extracts all key value pairs from a range map.
    val inline elements : IRangeMap<'K,'V> -> list<'K * 'V>

    /// Tries to find the value of an element with the given key.
    val inline lookup : 'K -> IRangeMap<'K, 'V> -> option<'V>

    /// Removes the element with the given key from a range map.
    val inline remove : 'K -> IRangeMap<'K,'V> -> IRangeMap<'K,'V>

    /// Removes all elements within the given range of keys.
    val inline removeRange : option<'K> -> option<'K> -> IRangeMap<'K,'V> -> IRangeMap<'K,'V>

    /// Returns true if there exists an element with the key.
    val inline containsKey : 'K -> IRangeMap<'K, 'V> -> bool

    /// Inserts a key-value pair into a range map.
    val inline insert : 'K -> 'V -> IRangeMap<'K,'V> -> IRangeMap<'K,'V>

    /// Returns all elements who's keys are in the given range.
    val inline lookupRange : option<'K> -> option<'K> -> IRangeMap<'K, 'V> -> list<'V>

    /// Empty range map.
    val empty<'K,'V when 'K : comparison> : IRangeMap<'K,'V>


