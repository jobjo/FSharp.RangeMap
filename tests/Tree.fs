namespace FSharp.Data.RangeMap.Tests

module Tree =

   
    open FSharp.Data.RangeMap.Internal.Tree
    module T = FSharp.Data.RangeMap.Internal.Tree

    open FsCheck.Xunit

    
    let (==) t1 t2 =
        T.elements t1 = T.elements t2

    let rec realHeight = function
        | Leaf              -> 1
        | Node (_,_,l,r)    -> 1 + max (realHeight l) (realHeight r)

    let rec isBalanced = function
        | Leaf              -> 
            true
        | Node (_,_,l,r)    ->
            let diff = abs <| realHeight l - realHeight r
            diff <= 2 && isBalanced l && isBalanced r

    let rec hasCorrectHeights = function
        | Leaf              ->
            true
        | Node (_,h,l,r) as t   ->
            int h = realHeight t 
            && hasCorrectHeights l 
            && hasCorrectHeights r

    [<Property>]
    let ``Tree - All elements are inserted`` (xs: list<int * string>) =
        let ys = T.fromSeq xs |> T.elements |> Set.ofList
        let zs = Set.ofSeq (Seq.distinctBy fst (List.rev xs))
        ys = zs

    [<Property>]
    let ``Tree - All elements are inserted in range map`` (xs: list<int * string>) =
        let ys = T.fromSeq xs |> T.elements |> Set.ofList
        let zs = Set.ofSeq (Seq.distinctBy fst (List.rev xs))
        ys = zs

    [<Property>]
    let ``Tree - Real heights same as assinged heights`` (xs: list<int * string>) =
        let t = T.fromSeq xs
        hasCorrectHeights t

    [<Property>]
    let ``Tree - Is balanced`` (xs: list<int * string>) =
        let t = T.fromSeq xs
        isBalanced t

    [<Property>]
    let ``Tree - Remove all elements results in empty tree`` (xs: list<int * string>) =
        let t = T.fromSeq xs
        let t2 = Seq.fold (fun t k -> T.remove k t) t (List.map fst xs)
        t2 == Leaf

    [<Property>]
    let ``Tree - Remove an element preserves balance`` (xs: list<int * string>) =
        let t = T.fromSeq xs
        [
            for (k,_) in xs do
                yield isBalanced <| remove k t
        ]
        |> List.forall id

    [<Property>]
    let ``Tree - Element accessible after insertion`` (xs: list<int * string>) (k: int) (v: string) =
        let t = T.fromSeq xs
        let t'= T.insert k v t
        T.lookup k t' = Some v

    [<Property>]
    let ``Tree - Element not accessible after insertion`` (xs: list<int * string>) (k: int) (v: string) =
        let t = T.fromSeq xs
        [
            for (k,_) in xs do
                yield T.lookup k (remove k t) = None
        ]
        |> List.forall id

    [<Property>]
    let ``Tree - Create tree from it's element recreates the tree`` (xs: list<int * string>) (k: int) (v: string) =
        let t = T.fromSeq xs
        let es = T.elements t
        let t' = T.fromSeq es
        t == t'

    [<Property>]
    let ``Tree - Map preserves structure`` (xs: list<int * string>) (k: int) (v: string) =
        let xs = List.filter (fun (k,v) -> System.String.IsNullOrEmpty v |> not) xs
        let f (s: string) = s.ToLower()
        let t = T.fromSeq xs
        let t' = T.map f t
        let es = T.elements t' |> List.map snd
        let es' = T.elements t |> List.map (snd >> f)
        es = es'

    [<Property>]
    let ``Tree - Alll elements within a given range are found`` (xs: list<int * int>) low high =
        let t = T.fromSeq xs
        let low, high = min low high, max low high
        let manElems f =
            elements t
            |> List.filter (fun (k,_) -> f k)
            |> List.map snd
        [
            lookupRange None None t, manElems (fun _ -> true)
                
            lookupRange None (Some high) t, manElems (fun k -> k <= high)

            lookupRange (Some low) None t, manElems (fun k -> k >= low)

            lookupRange (Some low) (Some high) t, manElems (fun k -> k >= low && k <= high)
        ]
        |> List.forall (fun (x,y) -> Set.ofList x = Set.ofList y)



