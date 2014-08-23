namespace FSharp.Collections.RangeMap.Internal

module Tree =

    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type Tree<'K, 'V> =
        | Leaf
        | Node of 'K * 'V * byte * Tree<'K,'V> * Tree<'K,'V>

    /// Maps over the values of a tree.
    val map : ('V -> 'U) -> Tree<'K,'V> -> Tree<'K, 'U>

    /// Extends a tree with a key-value pair.
    val insert<'K,'V when 'K : comparison> : 'K -> 'V -> (Tree<'K, 'V> -> Tree<'K, 'V>)

    /// Creates a tree from a sequence of key-value pairs.
    val fromSeq<'K,'V when 'K : comparison> : seq<'K * 'V> -> Tree<'K,'V>

    /// Removes an element with the given key, if exists.
    val remove<'K,'V when 'K : comparison> : 'K -> Tree<'K,'V> -> Tree<'K,'V>

    /// Removes all elements with keys within the given range.
    val removeRange<'K,'V when 'K : comparison> : (option<'K> -> option<'K> -> Tree<'K,'V> -> Tree<'K,'V>)

    /// Tries to lookup an element with the given key. Returns 'None' if the key is present.
    val inline lookup<'K,'V when 'K : comparison> : ('K -> Tree<'K,'V> -> option<'V>)

    /// Returns true if an element with the given key exists.
    val inline containsKey<'K,'V when 'K : comparison> : 'K -> Tree<'K,'V> -> bool

    /// Returns all key-value pairs from the given tree.
    val elements : Tree<'K,'V>  -> list<'K * 'V>

    /// Looks up a range of values
    val inline lookupRange<'K,'V when 'K : comparison> : (option<'K> -> option<'K> -> Tree<'K,'V> -> list<'V>)