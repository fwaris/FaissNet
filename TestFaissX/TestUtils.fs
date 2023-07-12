module TestUtils

open System
open System.IO
open Xunit

let rng = System.Random()
let randFloatArray d sz = [|0..sz-1|] |> Array.map(fun i -> Array.init d (fun i -> rng.NextSingle() ))
let randIdArray sz = [|0..sz-1|] |> Array.map(fun i -> rng.NextInt64())

let addData sz d (idx:FaissNet.Index) = 
    let data = randFloatArray d sz
    let d' = idx.Dimension()
    Assert.True((d=d'))
    let m = idx.MetricType()
    Assert.True((m=FaissNet.MetricType.METRIC_L2))
    idx.Add(data)
    Assert.True((sz=idx.Count()))


///should throw for base indexes - need to use IdMap
let addDataWithIds d (idx:FaissNet.Index) = 
    let sz = 100
    let data = randFloatArray d sz
    let ids = randIdArray sz
    //none of the indexes support id'ed insertions
    Assert.Throws<System.Runtime.InteropServices.SEHException>(fun () -> idx.AddWithIds(data,ids)) 
      
let baseIndexTest d (idx:FaissNet.Index) =
    let sz = 100
    addData sz d idx
    let srchData = randFloatArray d 2
    let k = 3
    let (neighborIds,neighborDists) = idx.Search(srchData,k)
    Assert.True( (neighborDists.[0].Length = k))
    let topNeighbors = neighborIds |> Array.map (fun xs -> xs.[0])
    let neighborVecs = idx.Reconstruct(topNeighbors)
    Assert.True((topNeighbors.Length = neighborVecs.Length))
    Assert.True((neighborVecs.[0].Length = d))

let indexSaveLoad<'t when 't :> FaissNet.Index> d (idx:FaissNet.Index) =
    let sz = 100
    addData sz d idx
    let fn = Path.GetTempFileName()
    FaissNet.Instance.WriteIndex(idx,fn)
    use idx2:'t = FaissNet.Instance.ReadIndex<'t>(fn)
    Assert.True((idx2.Count() = idx.Count()))

