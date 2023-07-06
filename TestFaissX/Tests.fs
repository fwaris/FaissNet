module Tests

open System
open System.IO
open Xunit

module TestUtils = 
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

    let addDataWithIdMap d (idx:FaissNet.Index) = 
        let sz = 100
        let data = randFloatArray d sz
        let ids = randIdArray sz
        use idMap = new FaissNet.IdMap(idx) 
        idMap.AddWithIds(data,ids)
        let d1 = data.[0..1]
        let id1 = ids.[0..1]
        let nbrIds,nbrVecs = idMap.Search(d1,1)
        Assert.True((nbrIds.[0].[0]=id1.[0]))

    ///should throw for base indexes - need ot use IdMap
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

[<Fact>]
let IndexFlatTest() =
    let d = 64
    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.baseIndexTest d idx        

[<Fact>]
let IndexFlatL2Test() =
    let d = 64
    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.baseIndexTest d idx        

[<Fact>]
let IndexHSNWTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.baseIndexTest d idx        


[<Fact>]
let IndexFlatWriteTest() =
    let d = 64
    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.indexSaveLoad<FaissNet.IndexFlat> d idx

[<Fact>]
let IndexFlatL2WriteTest() =
    let d = 64
    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.indexSaveLoad<FaissNet.IndexFlatL2> d idx

[<Fact>]
let IndexHSNWWriteTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.indexSaveLoad<FaissNet.IndexHNSWFlat> d idx

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

[<Fact>]
let IndexFlatAddWithIdMapTest() =
    let d = 64
    use idx = new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIdMap d idx

[<Fact>]
let IndexFlatL2AddWithIdMapTest() =
    let d = 64
    use idx = new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIdMap d idx

[<Fact>]
let IndexHSNWAddWithIdMapTest() =
    let d = 64
    use idx = new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2)
    TestUtils.addDataWithIdMap d idx

