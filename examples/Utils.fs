namespace FSharp.RangeMap.Examples

module Utils =

    // Measure time
    let time f =
        let sw = new System.Diagnostics.Stopwatch()
        sw.Start();
        let x = f ()
        sw.Stop();
        let ts = sw.Elapsed;
        x,  System.Math.Round(ts.TotalMilliseconds / 1000., 2)

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

    let randList n =
        let r = System.Random ()
        [for _ in [1..n] do yield r.Next()]