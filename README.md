FSharp.RangeMap
===============
RangeMap is a key-value data store also supporting fast retrievel of element within a range of keys. 

Usage
---------
`RangeMap`s can be created by using the `fromSeq` function. Here's and example defining a range-map holding 10000
values with integer keys and string values:
```fsharp
> open FSharp.Data.RangeMap
> let myMap = fromSeq <| List.init 10000 (fun ix -> (ix, string ix))
val myMap : IRangeMap<int,string>
    
```

To lookup a single element by it's key, the function `lookup` is provided:
```fsharp
> let res = lookup 1024 myMap;;
val res : string option = Some "1024"
    
> let res2 = lookup -2000 myMap;;
val res2 : string option = None
    
```
As seen in the example above, looking up a non-existing key yields the result `None`.

The existense of a key can also be tested using `containsKey`:

```fsharp
> let containsFive = containsKey 5 myMap;;
val containsFive : bool = true

> let containsMinusFive = containsKey -5 myMap;;
val containsMinusFive : bool = false

```

It's also possible to lookup elements by a range of keys. Here is an example of fiding all elements with keys within the range 5 to 10:

```fsharp
> let res = lookupRange (Some 5) (Some 10) myMap;;
val res : string list = ["5"; "6"; "7"; "8"; "9"; "10"]

```

The first two parameters to `lookupRange` are optional values indicating the lower and higher bounds.


`RangeMap`s can be extend by inserting elements using the function `insert`:

```fsharp
> let myMap = insert -5 "Minus five" myMap;;
val myMap : IRangeMap<int,string>

> let res = lookup -5 myMap;;
val res : string option = Some "Minus five"
```

Elementes can also be removed with `remove`:
```fsharp
> let myMap = remove 5 myMap;;
val myMap : IRangeMap<int,string>

> let containsFive = containsKey 5 myMap;;
val containsFive : bool = false

```

There is also the possibility to remove elements for given a range of key values:

```fsharp
> let myMap = removeRange (Some 1) (Some 20) myMap;;
val myMap : IRangeMap<int,string>

> let contains20 = containsKey 20 myMap;;
val contains20 : bool = false
```

The function `elements` retrives all key-value pairs of `RangeMap` and is equivalent to `lookupRange None None`.

To map over the values of the elements in a `RangeMap`, `map` is used:

```fsharp
> let myMap2 = map Seq.length myMap;;
val myMap2 : IRangeMap<int,int>
```fsharp

The following invariant holds: `map f >> elements == elements >> List.map (fun (k,v) -> (k, f v))`.













