FSharp.RangeMap
===============
*RangeMap* is an immutable key-value data store similar to the standard FSharp *Map* implementation. The most important difference, compared to the `Map` interface, is the ability to efficiently look up values from a range of keys. While *RangeMap* seems to perform slightly better than `Map` for looking up elements, it is considerably slower when it comes to *inserting* and *removing* elements.


Usage
---------
`IRangeMap` values can be created by using the `fromSeq` function. Here's and example defining a range-map holding 10000 values with integer keys and string values:

```fsharp
> open FSharp.Data.RangeMap
> let myMap = fromSeq <| List.init 10000 (fun ix -> (ix, string ix))
val myMap : IRangeMap<int,string>
```

To lookup a single element by its key, the function `lookup` is provided:

```fsharp
> let res = lookup 1024 myMap;;
val res : string option = Some "1024"
    
> let res2 = lookup -2000 myMap;;
val res2 : string option = None
```

As seen in the example above, looking up a non-existing key yields the result `None`.

The existence of a key can be be tested using `containsKey`:

```fsharp
> let containsFive = containsKey 5 myMap;;
val containsFive : bool = true

> let containsMinusFive = containsKey -5 myMap;;
val containsMinusFive : bool = false
```

Elements may also be retrieved by providing a range of keys. Here is an example of fiding all elements with keys between 5 and 10 (including 5 and 10):

```fsharp
> let res = lookupRange (Some 5) (Some 10) myMap;;
val res : string list = ["5"; "6"; "7"; "8"; "9"; "10"]
```

The first two parameters to `lookupRange` are optional values specifying the lower and upper bounds.


`IRangeMap`s can be extend by inserting elements using the function `insert`:

```fsharp
> let myMap = insert -5 "Minus five" myMap;;
val myMap : IRangeMap<int,string>

> let res = lookup -5 myMap;;
val res : string option = Some "Minus five"
```

Elementes may be be removed with `remove`:

```fsharp
> let myMap = remove 5 myMap;;
val myMap : IRangeMap<int,string>

> let containsFive = containsKey 5 myMap;;
val containsFive : bool = false
```

It is also possible to remove elements for given a range of key values:

```fsharp
> let myMap = removeRange (Some 1) (Some 20) myMap;;
val myMap : IRangeMap<int,string>

> let contains20 = containsKey 20 myMap;;
val contains20 : bool = false
```

The function `elements` retrives all key-value pairs of `RangeMap` and is equivalent to `lookupRange None None`.

To map over the values of the elements in a `IRangeMap`, `map` is used:

```fsharp
> let myMap2 = map Seq.length myMap;;
val myMap2 : IRangeMap<int,int>
```

The following invariant holds for any `RangeMap` `rm` and feasible function `f`: 

```fhsarp
(map f >> elements) = (elements >> List.map (fun (k,v) -> (k, f v)))`
```


Perforamce
--------------------
Inital benchmarking indicates that `FSharp.RangeMap` is on par with, or faster than the standard FSharp `Map` implementation in terms of looking up elements using the `lookup` function. 

Below are some result of comparing lookup for Fharp `Map`, `RangeMap` and standard .NET dictionaries. Have a look at the *examples project* for details.

The following table shows the average total time for looking up 10000 existing keys from collections holding 100000 elements, generated using random integer as keys:


| Operation                                        | Time (s)  |
|:-------------------------------------------------|----------:|
| Lookup 10K existing keys from map                | 0.0027    |
| Lookup 10K existing keys from range-map          | 0.0022    |
| Lookup 10K existing keys from dictionary         | 0.0036    |


What is interesting here are the relative times. As can be seen *RangeMap* is fast than both *Dictionary* and *Map*.


The next table displays the total time of looking up non-existing keys for the same collections:


| Operation                                            | Time (s)  |
|:-----------------------------------------------------|----------:|
| Lookup 10K non-existing keys from map                | 0.0029    |
| Lookup 10K non-existing keys from range-map          | 0.0022    |
| Lookup 10K non-existing keys from dictionary         | 0.0018    |

This time `Dictionary` is faster. The values for `RangeMap` and `Map` are not significantly affected.


The next table reveals that building and removing elements from `RangeMap`s are considerably slower than the equivalent functions on *Maps*.

| Operation                                            | Time (s)  | 
|:-----------------------------------------------------|----------:|
| Remove 10K existing keysfrom map                     | 0.0132    |
| Remove 10K existing key from range map               | 0.0601    |




Implementation
--------------------
The provided implementation for `IRangeMap` is a simple [AVL] tree. Each destructive operation on the tree, preserves a strict balance, where the difference between the maximum height of a the left and right sub-trees of any node is at most one.


[AVL]:http://en.wikipedia.org/wiki/AVL_tree










