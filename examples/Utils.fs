namespace FSharp.Collections.RangeMap.Examples

module internal Utils =


    // Return current used memory in MBs.
    let getCurrentMemory () =
        let mem = System.GC.GetTotalMemory(true)
        System.Math.Round((float mem) / (System.Math.Pow(2.,20.)), 1)

    let time f =
        let sw = new System.Diagnostics.Stopwatch()
        sw.Start();
        let x = f ()
        sw.Stop();
        let ts = sw.Elapsed;
        x,  System.Math.Round(ts.TotalMilliseconds / 1000., 4)
    
    let timeAndMemory f =
        let m1 = getCurrentMemory()
        let res, t = time f
        let m2 = getCurrentMemory()
        res, t, (m2 - m1)

    let rec repeate n f = if n <= 0 then () else f () ; repeate (n-1) f

    let benchmark n fs =
        [
            for name, f in fs do
                let t = snd <| time (fun _ -> repeate n f)
                yield name, t / float n
        ]

    let shuffle xs =
        let r = System.Random()
        List.sortBy (fun _ -> r.Next()) xs

    let randomList n =
        let r = System.Random ()
        [for _ in [1..n] do yield r.Next()]

    let printTable = function
        | row :: _ as rows   ->
            let replicate s n = System.String.Join (s, List.replicate n "")
            let numCols = List.length row
            let maxColLengths =
                [
                    for cIx in [0 .. numCols - 1] do
                        yield Seq.max <| [for row in rows do yield Seq.length <| row.[cIx]]
                ]
            let colSpace = 3
            let rowLine = replicate "-" <| List.sum maxColLengths + (numCols * (1 + colSpace))
            for row in rows do
                for (col,maxLength) in List.zip row maxColLengths do
                    let colLenght = Seq.length col
                    let space = replicate " " <| colSpace + maxLength - colLenght
                    printf "%s%s| " col space
                printfn ""
                printfn "%s" rowLine
        | _                     ->
            ()

    let printLabelTimeResults (results: seq<list<string * float>>) =
        let res =
            ([], results)
            ||> Seq.fold (fun rows res -> 
                let ys = res |> List.map (fun (name, time) -> [name; string time]) 
                rows @ [["";""]] @ ys
            )
        printTable (["Label"; "Time (s)"] :: res)
