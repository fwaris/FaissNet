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

