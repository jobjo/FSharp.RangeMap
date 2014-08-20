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

To lookup an element by it's key, use the function `lookup`:

```fsharp
    > let res = lookup 1024 myMap;;
    val res : string option = Some "1024"
    
    > let res2 = lookup -2000 myMap;;
    val res2 : string option = None
    
```

As seen in the example above, looking up a non-existing key yields the result `None`.



