namespace FSharp.Data.RangeMap.Tests

module RangeMap =

    open FSharp.Data.RangeMap
    open FSharp.Data.RangeMap.RangeMap
    open FsCheck.Xunit

    
    let private (==) t1 t2 = elements t1 = elements t2
    



    [<Property>]
    let ``RangeMap - All elements are inserted`` (xs: list<int * string>) =
        let ys = fromSeq xs |> elements |> Set.ofList
        let zs = Set.ofSeq (Seq.distinctBy fst (List.rev xs))
        ys = zs

    [<Property>]
    let ``RangeMap - All elements are inserted in range map`` (xs: list<int * string>) =
        let ys = fromSeq xs |> elements |> Set.ofList
        let zs = Set.ofSeq (Seq.distinctBy fst (List.rev xs))
        ys = zs

    [<Property>]
    let ``RangeMap - Remove all elements results in empty range map`` (xs: list<int * string>) =
        let t = fromSeq xs
        let t2 = Seq.fold (fun t k -> remove k t) t (List.map fst xs)
        t2 == empty

    [<Property>]
    let ``RangeMap - Element accessible after insertion`` (xs: list<int * string>) (k: int) (v: string) =
        let t = fromSeq xs
        let t'= insert k v t
        lookup k t' = Some v

    [<Property>]
    let ``RangeMap - Element not accessible after insertion`` (xs: list<int * string>) (k: int) (v: string) =
        let t = fromSeq xs
        [
            for (k,_) in xs do
                yield lookup k (remove k t) = None
        ]
        |> List.forall id

    [<Property>]
    let ``RangeMap - Create range map from it's element recreates the map`` (xs: list<int * string>) (k: int) (v: string) =
        let t = fromSeq xs
        let es = elements t
        let t' = fromSeq es
        t == t'

    [<Property>]
    let ``RangeMap - Map preserves structure`` (xs: list<int * string>) (k: int) (v: string) =
        let xs = List.filter (fun (k,v) -> System.String.IsNullOrEmpty v |> not) xs
        let f (s: string) = s.ToLower()
        let t = fromSeq xs
        let t' = map f t
        let es = elements t' |> List.map snd
        let es' = elements t |> List.map (snd >> f)
        es = es'

    [<Property>]
    let ``RangeMap - Alll elements within a given range are found`` (xs: list<int * int>) low high =
        let t = fromSeq xs
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



