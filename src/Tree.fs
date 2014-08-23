 namespace FSharp.Collections.RangeMap.Internal

 module Tree =

    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type Tree<'K, 'V> =
        | Leaf
        | Node of 'K * 'V * byte * Tree<'K,'V> * Tree<'K,'V>
    
    /// Maps over the values.
    let map f tree =
        let rec go tree cont =
            match tree with
            | Leaf                  ->
                cont Leaf
            | Node (k,v,h,l,r)      ->
                go l <| fun l' ->
                    go r <| fun r' ->
                        cont <| Node (k, f v, h,l',r')
        go tree id

    /// Height is the maximum length of any path to a leaf node in the tree.
    let inline height tree =
        match tree with
        | Leaf              -> 1uy
        | Node (_,_,h,_,_)  -> h

    /// Balance is the difference between the left and right height of the
    /// subtrees of a node.
    let zeroBalance = 0
    let lowBalance = -2
    let highBalance = 2

    /// Builds a node
    let inline node k v l r = Node (k, v, byte 1 + max (height l) (height r), l, r)

    /// Balance of a tree is the difference of height of left and right branch.
    let inline balance tree =
        match tree with
        | Leaf              -> zeroBalance
        | Node (__,_,_,l,r) -> (int <| height l) - (int <| height r)

    /// Rotates to the left. Right child node will be the new root node.
    let inline rotateLeft tree =
        match tree with
        | Node (k,v,h,l, Node (rk,rv,rh,rl,rr))    ->
            node rk rv (node k v l rl) rr
        | _ as tree                             ->
            tree

    /// Rotates to the right. Left child node will be the new root node.
    let inline rotateRight tree =
        match tree with
        | Node (k,v,h,Node (lk, lv,lh,ll,lr), r)    ->
            node lk lv ll ( node k v lr r)
        | _ as tree                             ->
            tree

    /// Given a tree balances the top nodes in case of left or right leaning.
    let inline balanceTree tree =
        match tree with
        | Leaf                      ->
            Leaf
        | Node(k,v,_,l,r) as tree    ->
            match balance tree with
            | b when b <= lowBalance    ->
                // Right leaning case
                if balance r > zeroBalance then
                    // Right-left case.
                    node k v l (rotateRight r)
                else
                    // Right-right case.
                    tree
                |> rotateLeft
            | b when b >= highBalance   ->
                if balance l < zeroBalance then
                    // Left-right case.
                    node k v (rotateLeft l) r
                else
                    // Left-left case.
                    node k v l r
                |> rotateRight
            | _                         ->
                tree

    /// Inserts the given binding to the tree. Uses the given comparison function
    /// for determining the ordering criterion.
    let insertGeneric compare k v tree  =
        let rec go tree cont =
            match tree with
            | Leaf                      ->
                cont <| node k v Leaf Leaf
            | Node (k',v', h, l, r)   ->
                match compare k k' with
                | -1    ->
                    go l <| fun l' ->
                        node k' v' l' r |> balanceTree |> cont
                | 0     ->
                    cont <| node k v l r
                | _     ->
                    go r <| fun r'  ->
                        node k' v' l r' |> balanceTree |> cont
        go tree id

    /// Inserts a value.
    let insert<'K,'V when 'K : comparison> (k: 'K) (v: 'V) (tree: Tree<'K,'V>) : Tree<'K, 'V> =
        insertGeneric compare<'K> k v tree

    /// Builds a tree from a sequence, using the provided comparison function for ordering.
    let fromSeqGeneric compare kvs =
        Seq.fold (fun t (k,v) -> insertGeneric compare k v t) Leaf  kvs

    /// Builds a tree from a sequence. Require comparison.
    let fromSeq<'K,'V when 'K : comparison> (kvs: seq<'K * 'V>) : Tree<'K,'V> =
        fromSeqGeneric compare<'K> kvs

    /// Merges two trees.
    let inline merge l r =
        // Removes the largest element from the tree.
        let removeLargest tree =
            let rec go tree cont = 
                match tree with
                | Leaf              -> 
                    failwith ""
                | Node(k,v,_,l,Leaf) ->
                    cont ((k,v), l)
                | Node (k,v,_,l,r)    ->
                    go r <| fun (kv', r') ->
                        let tree = node k v l r'
                        cont (kv', balanceTree tree)
            go tree id

        // Removes the smallest element from the tree.
        let removeSmallest tree =
            let rec go tree cont=
                match tree with
                | Leaf                  ->
                    failwith ""
                | Node (k,v,_,Leaf, r)  ->
                    cont ((k,v),r)
                | Node (k,v,_,l,r)       ->
                    go l <| fun (kv',l') ->
                        let tree = node k v l' r
                        cont (kv', balanceTree tree)
            go tree id
        match l, r with
        | Leaf, _   ->
            r
        | _, Leaf   ->
            l
        | l, r      ->
            if height l <= height r then
                let ((k',v'), l') = removeLargest l
                node k' v' l' r
            else
                let ((k',v'),r') = removeSmallest r
                node k' v' l r'

    /// Removes a node with the given key.
    let removeGeneric compare key tree  =
        let rec go tree cont =
            match tree with
            | Leaf                      ->
                cont Leaf
            | Node (k,v , h, l, r)      ->
                match compare key k with
                | -1    ->
                    // Remove from left branch
                    go l <| fun l' ->
                        node k v l' r |> balanceTree |> cont
                | 0     ->
                    merge l r |> balanceTree |> cont
                | _     ->
                    // Remove from right branch
                    go r <| fun r' ->
                        node k v l r'  |> balanceTree |> cont
        go tree id

    /// Removes the element with the given key.
    let remove<'K,'V when 'K : comparison> (k: 'K) (t: Tree<'K,'V>) : Tree<'K,'V> = 
        removeGeneric compare<'K> k t


    /// Lookup item based on key and comparison function.
    let  inline lookupGeneric<'K,'V> compare =
        fun (key: 'K) (tree: Tree<'K, 'V>) ->
            let rec go  = function
                | Leaf                  -> 
                    None
                | Node (k,v, _, l, r)    ->
                    let rr = compare key k
                    if rr < 0 then go l elif rr = 1 then go r else Some v
            go tree

    let inline lookup<'K,'V when 'K : comparison> =
        let comparer = LanguagePrimitives.FastGenericComparer<'K> 
        let inline compare (x:'K) (y:'K) = comparer.Compare (x,y)
        fun (key: 'K) (tree: Tree<'K,'V>)  -> lookupGeneric compare key tree

    /// Checks if the tree contains the key.
    let inline containsKey k t = Option.isSome <| lookup k  t

    /// Collects all elements.
    let elements<'K,'V> (tree: Tree<'K,'V>) : list<'K * 'V> =
        let list = System.Collections.Generic.List<_>()
        let rec go = function
            | Leaf                  ->
                ()
            | Node (k,v,h, l, r)    -> 
                go l
                list.Add(k,v)
                go r
        go tree
        List.ofSeq list

    /// Lookup elements within a range.
    let inline selectRangeGeneric compare (low, high) f tree =
        let list = System.Collections.Generic.List<_>()
        let rec go = function
            | Leaf                  ->
                ()
            | Node (k,v, _, l, r)   ->
                match low, high with
                | Some l, _  when compare k l < 0 ->
                    go r
                | _, Some h when compare k h > 0  ->
                    go l
                | _                     ->
                    go l 
                    list.Add <| f (k,v)
                    go r
        go tree
        List.ofSeq list

    /// Removes a range of values using the given comparison function.
    let removeRangeGeneric<'K,'V> (compare: 'K -> 'K -> int) =
        fun (low: option<'K>) (high: option<'K>) (tree: Tree<'K,'V>) ->
            let kvs = selectRangeGeneric compare (low, high) fst tree
            Seq.fold (fun tree k -> removeGeneric compare k tree) tree kvs

    /// Removes a range of values.
    let removeRange<'K,'V when 'K : comparison>  =
        let comparer = LanguagePrimitives.FastGenericComparer<'K> 
        let inline compare (x:'K) (y:'K) = comparer.Compare (x,y)
        fun l h t -> removeRangeGeneric<'K,'V> compare l h t

    /// Lookup elements within a range.
    let inline lookupRange<'K,'V when 'K : comparison> =
        let comparer = LanguagePrimitives.FastGenericComparer<'K> 
        let inline compare (x:'K) (y:'K) = comparer.Compare (x,y)
        fun low high (tree: Tree<'K,'V>)  ->
            selectRangeGeneric compare (low, high) snd tree
