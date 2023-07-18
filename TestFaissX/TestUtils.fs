module TestUtils

open System
open System.IO
open Xunit

let rng = System.Random()
let mutable _nextId = 0L
let nextId() = Threading.Interlocked.Increment(&_nextId)
let randFloatArray d sz = [|0..sz-1|] |> Array.map(fun i -> Array.init d (fun i -> rng.NextSingle() ))
let randFloatArrayFlat d sz = [|for _ in 0 .. (d*sz)-1 -> rng.NextSingle() |]
let genIds sz = [|0..sz-1|] |> Array.map(fun i -> nextId())

