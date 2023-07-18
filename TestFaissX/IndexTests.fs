module IndexTests

open System
open System.IO
open Xunit

let DIM = 1536

[<Fact>]
let addWithIds() =
    let d = DIM
    use idx = FaissNet.Index.CreateDefault(d,FaissNet.MetricType.METRIC_L2)
    let data = TestUtils.randFloatArray d 100
    let ids = TestUtils.randIdArray 100
    idx.AddWithIds(data,ids)
    Assert.True(true)

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
    let ids = TestUtils.randIdArray sz
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
    let ids = TestUtils.randIdArray sz
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
    let ids = TestUtils.randIdArray sz
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
    let ids = TestUtils.randIdArray sz
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
    let ids = TestUtils.randIdArray sz
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
    let ids = TestUtils.randIdArray sz
    idx.AddWithIdsFlat(sz,data,ids)
    let k = 5
    let topVecs = data |> Seq.chunkBySize d |> Seq.take k |> Seq.collect id |> Seq.toArray
    let topVecIds = ids.[0..k-1]
    let nDists,nIds,nVecs = idx.SearchAndReconstruct(k,topVecs,1)
    Assert.True((topVecIds=nIds))
    Assert.True((nVecs = topVecs))


//[<Fact>]
//let IndexFlatL2Test() =
//    let d = 64
//    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.baseIndexTest d idx        

//[<Fact>]
//let IndexHSNWTest() =
//    let d = 64
//    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
//    TestUtils.baseIndexTest d idx        


//[<Fact>]
//let IndexFlatWriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexFlat> d idx

//[<Fact>]
//let IndexFlatL2WriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexFlatL2> d idx

//[<Fact>]
//let IndexHSNWWriteTest() =
//    let d = 64
//    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
//    TestUtils.indexSaveLoad<FaissNet.IndexHNSWFlat> d idx

(*
//these throw c++ exceptions which are not handled well by the testing infrastructure
[<Fact>]
let IndexFlatAddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx

[<Fact>]
let IndexFlatL2AddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx

[<Fact>]
let IndexHSNWAddWithIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIds d idx
*)


(*
//removal not supported for HSNW
[<Fact>]
let IndexHSNWAddRemoveIdsTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.addRemoveIds d idx
*)
