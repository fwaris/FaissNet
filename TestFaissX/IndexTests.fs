module IndexTests

open System
open System.IO
open Xunit

let DIM = 1536

[<Fact>]
let basicUsageExample() =
    let d = 64
    let sz = 10
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = [|for i in 1 .. sz -> [| for j in 1 .. d -> if j%2 = 0 then -1.f else 1.f  |]|]
    let ids = [|for i in 1 .. sz -> int64 i|]
    idx.AddWithIds(data,ids)
    let srchVec = [|for i in 1 .. sz -> 1.f |]
    let nbrDists,nbrIds = idx.Search([|srchVec|],2)
    //printfn $"nbrIds: {nbrIds}"

    Assert.True((idx.Count = int64 ids.Length))


[<Fact>]
let addWithIds() =
    let d = DIM
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d 100
    let ids = TestUtils.genIds 100
    idx.AddWithIds(data,ids)
    Assert.True((idx.Count = int64 ids.Length))

[<Fact>]
let add() =
    let d = DIM
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d 100
    Assert.Throws<Exception>(fun () -> idx.Add(data);) //need to use add with ids for IndexIdMap wrapped indexes

[<Fact>]
let searchFlat() =
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArrayFlat d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIdsFlat(sz,data,ids)
    let lookupSz = 1
    let k = 3
    let searchIds = TestUtils.randFloatArrayFlat d lookupSz
    let dists,ids = idx.SearchFlat(1,searchIds,3)
    Assert.True((dists.Length = lookupSz * k))    

[<Fact>]
let search() =
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIds(data,ids)
    let lookupSz = 1
    let k = 3
    let searchIds = TestUtils.randFloatArray d lookupSz
    let dists,ids = idx.Search(searchIds,3)
    Assert.True((dists.Length = lookupSz))    
    Assert.True((dists.[0].Length = k))

[<Fact>]
let removeIdsDefaultIndex() = 
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIds(data,ids)
    let c1 = idx.Count
    let idsToRemove = ids.[0..ids.Length/2]
    Assert.Throws<Exception>(fun () -> idx.RemoveIds(idsToRemove)) //cannot remove ids for the type of index created by default

[<Fact>]
let removeIdsBaseIndex() = 
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.Create(d,"IDMap,Flat",FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIds(data,ids)
    let c1 = idx.Count
    let idsToRemove = ids.[0..ids.Length/2]
    idx.RemoveIds(idsToRemove)
    let c2 = idx.Count
    Assert.True((c1 = c2 + int64 idsToRemove.Length))

[<Fact>]
let assign() = 
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArrayFlat d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIdsFlat(sz,data,ids)
    let k = 5
    let topVecs = data |> Seq.chunkBySize d |> Seq.take k |> Seq.collect id |> Seq.toArray
    let topVecIds = ids.[0..k-1]
    let assignedIds = idx.Assign(k,topVecs)
    Assert.True((topVecIds=assignedIds))
            

[<Fact>]
let searchAndReconstruct() =
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArrayFlat d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIdsFlat(sz,data,ids)
    let k = 5
    let topVecs = data |> Seq.chunkBySize d |> Seq.take k |> Seq.collect id |> Seq.toArray
    let topVecIds = ids.[0..k-1]
    let nDists,nIds,nVecs = idx.SearchAndReconstruct(k,topVecs,1)
    Assert.True((topVecIds=nIds))
    Assert.True((nVecs = topVecs)) //this may not hold for all index types

[<Fact>]
let indexSaveLoad()=
    let sz = 100
    let d = DIM
    let sz = 100
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArrayFlat d sz
    let ids = TestUtils.genIds sz
    idx.AddWithIdsFlat(sz,data,ids)
    let fn = Path.GetTempFileName()
    idx.Save(fn)
    let idx2 = FaissNet.Index.Load(fn)
    Assert.True((idx.Count = idx2.Count))

[<Fact>]
let indexMerge()=
    let sz = 100
    let d = DIM
    let sz = 100
    use idx1 = FaissNet.Index.Create(d,"IDMap,Flat",FaissNet.MetricType.METRIC_L2)
    use idx2 = FaissNet.Index.Create(d,"IDMap,Flat",FaissNet.MetricType.METRIC_L2)

    let data1 = TestUtils.randFloatArrayFlat d sz
    let ids1 = TestUtils.genIds sz
    idx1.AddWithIdsFlat(sz,data1,ids1)

    let data2 = TestUtils.randFloatArrayFlat d sz
    let ids2 = TestUtils.genIds sz
    idx2.AddWithIdsFlat(sz,data2,ids2)

    let c1 = idx1.Count
    let c2 = idx2.Count
    idx1.MergeFrom(idx2)
    let c3 = idx1.Count
    Assert.True((c3 = c1 + c2))
    Assert.True((idx2.Count = 0L))
