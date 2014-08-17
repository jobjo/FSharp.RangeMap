 namespace FSharp.Data.RangeMap.Internal

 module Tree =

    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    [<NoEquality; NoComparison>]
    type Tree<'K, 'V> =
        | Leaf
        | Node of ('K * 'V) * byte * Tree<'K,'V> * Tree<'K,'V>
    
    /// Maps over the values.
    let map f tree =
        let rec go tree cont =
            match tree with
            | Leaf                  ->
                cont Leaf
            | Node ((k,v),h,l,r)    ->
                go l <| fun l' ->
                    go r <| fun r' ->
                        cont <| Node((k,f v), h,l',r')
        go tree id

    /// Height is the maximum length of any path to a leaf node in the tree.
    let inline height tree =
        match tree with
        | Leaf              -> 1uy
        | Node (_,h,_,_)    -> h

    /// Balance is the difference between the left and right height of the
    /// subtrees of a node.
    let zeroBalance = 0
    let lowBalance = -2
    let highBalance = 2

    /// Builds a node
    let inline node kv l r = Node (kv, byte 1 + max (height l) (height r), l, r)

    /// Balance of a tree is the difference of height of left and right branch.
    let inline balance tree =
        match tree with
        | Leaf              -> zeroBalance
        | Node (_,_,l,r)    -> (int <| height l) - (int <| height r)

    /// Rotates to the left. Right child node will be the new root node.
    let inline rotateLeft tree =
        match tree with
        | Node (kv,h,l, Node (rkv,rh,rl,rr))    ->
            node rkv (node kv l rl) rr
        | _ as tree                             ->
            tree

    /// Rotates to the right. Left child node will be the new root node.
    let inline rotateRight tree =
        match tree with
        | Node (kv,h,Node (lkv,lh,ll,lr), r)    ->
            node lkv ll ( node kv lr r)
        | _ as tree                             ->
            tree

    /// Given a tree balances the top nodes in case of left or right leaning.
    let inline balanceTree tree =
        match tree with
        | Leaf                      ->
            Leaf
        | Node(kv,_,l,r) as tree    ->
            match balance tree with
            | b when b <= lowBalance    ->
                // Right leaning case
                if balance r > zeroBalance then
                    // Right-left case.
                    node kv l (rotateRight r)
                else
                    // Right-right case.
                    tree
                |> rotateLeft
            | b when b >= highBalance   ->
                if balance l < zeroBalance then
                    // Left-right case.
                    node kv (rotateLeft l) r
                else
                    // Left-left case.
                    node kv l r
                |> rotateRight
            | _                         ->
                tree

    /// Inserts the given binding to the tree. Uses the given comparison function
    /// for determining the ordering criterion.
    let insertGeneric compare k v tree  =
        let rec go tree cont =
            match tree with
            | Leaf                      ->
                cont <| node (k,v) Leaf Leaf
            | Node ((k',v'), h, l, r)   ->
                match compare k k' with
                | -1    ->
                    go l <| fun l' ->
                        node (k',v') l' r |> balanceTree |> cont
                | 0     ->
                    cont <| Node ((k,v),h,l,r)
                | _     ->
                    go r <| fun r'  ->
                        node (k',v') l r' |> balanceTree |> cont
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
    let merge l r =
        // Removes the largest element from the tree.
        let removeLargest tree =
            let rec go tree cont = 
                match tree with
                | Leaf              -> 
                    failwith ""
                | Node(kv,_,l,Leaf) ->
                    cont (kv, l)
                | Node (kv,_,l,r)    ->
                    go r <| fun (kv', r') ->
                        let tree = node kv l r'
                        cont (kv', balanceTree tree)
            go tree id

        // Removes the smallest element from the tree.
        let removeSmallest tree =
            let rec go tree cont=
                match tree with
                | Leaf                  ->
                    failwith ""
                | Node (kv,_,Leaf, r)   ->
                    cont (kv,r)
                | Node (kv,_,l,r)       ->
                    go l <| fun (kv',l') ->
                        let tree = node kv l' r
                        cont (kv', balanceTree tree)
            go tree id
        match l, r with
        | Leaf, _   ->
            r
        | _, Leaf   ->
            l
        | l, r      ->
            if height l <= height r then
                let (kv', l') = removeLargest l
                node kv' l' r
            else
                let (kv',r') = removeSmallest r
                node kv' l r'

    /// Removes a node with the given key.
    let removeGeneric compare k tree  =
        let rec go k tree c =
            match tree with
            | Leaf                      ->
                c Leaf
            | Node ((k',_) as kv, h, l, r)    ->
                match compare k k' with
                | -1    ->
                    // Remove from left branch
                    go k l <| fun l'    ->
                        let tree = node kv l' r
                        c <| if balance tree <= lowBalance then rotateLeft tree else tree
                | 0     ->
                    c <| merge l r
                | _     ->
                    // Remove from right branch
                    go k r <| fun r' ->
                        let tree = node kv l r'
                        c <| if balance tree >= highBalance then rotateRight tree else tree
        go k tree id

    /// Removes the element with the given key.
    let remove<'K,'V when 'K : comparison> (k: 'K) (t: Tree<'K,'V>) : Tree<'K,'V> = 
        removeGeneric compare<'K> k t

    let  inline lookupGeneric<'K,'V> compare =
        fun (key: 'K) (tree: Tree<'K, 'V>) ->
            let rec go  = function
                | Leaf                  -> 
                    None
                | Node ((k,v), _, l, r)    ->
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
            | Node ((k,v),h, l, r)  -> 
                go l
                list.Add(k,v)
                go r
        go tree
        List.ofSeq list

    /// Lookup elements within a range.
    let inline lookupRangeGeneric compare (low, high) tree =
        let list = System.Collections.Generic.List<_>()
        let rec go = function
            | Leaf                  ->
                ()
            | Node ((k,v), _, l, r)    ->
                match low, high with
                | Some l, _  when compare k l < 0 ->
                    go r
                | _, Some h when compare k h > 0  ->
                    go l
                | _                     ->
                    go l 
                    list.Add(v)
                    go r
        go tree
        List.ofSeq list

    /// Lookup elements within a range.
    let inline lookupRange<'K,'V when 'K : comparison> =
        let comparer = LanguagePrimitives.FastGenericComparer<'K> 
        let inline compare (x:'K) (y:'K) = comparer.Compare (x,y)
        fun low high (tree: Tree<'K,'V>)  ->
            lookupRangeGeneric compare (low, high) tree
