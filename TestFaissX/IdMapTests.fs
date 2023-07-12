module IdMapTests
open System
open System.IO
open Xunit

let addDataWithIdMap d (idx:FaissNet.CreateIndex) = 
    let sz = 100
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.randIdArray sz
    use idMap = new FaissNet.IdMap(idx) 
    idMap.AddWithIds(data,ids)
    let d1 = data.[0..1]
    let id1 = ids.[0..1]
    let nbrIds,nbrVecs = idMap.Search(d1,1)
    Assert.True((nbrIds.[0].[0]=id1.[0]))

let addRemoveIds d (idx:FaissNet.CreateIndex) =
    let sz = 100
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.randIdArray sz
    use idMap = new FaissNet.IdMap(idx) 
    idMap.AddWithIds(data,ids)
    let c1 = idMap.Count()
    let remIds = ids.[0..sz/2]
    let vecs = idMap.Reconstruct(remIds[0..1]) // this should work
    idMap.Remove(remIds)
    let c2 = idMap.Count()
    Assert.True((c2 = c1 - remIds.Length))
    //let tryRemove() = idMap.Reconstruct(remIds[0..1]) |> ignore               //this should fail as ids have been removed
    //Assert.Throws<System.Runtime.InteropServices.SEHException>(tryRemove)     //causes problems with testing host

let saveLoadIdMap d (idx:FaissNet.CreateIndex)=
    let sz = 100
    let data = TestUtils.randFloatArray d sz
    let ids = TestUtils.randIdArray sz
    use idMap = new FaissNet.IdMap(idx) 
    idMap.AddWithIds(data,ids)
    let c1 = idMap.Count()
    let fn = Path.GetTempFileName()
    FaissNet.Instance.WriteIdMap(idMap,fn)
    let idMap2 = FaissNet.Instance.ReadIdMap(fn)
    Assert.True((c1=idMap2.Count()))

[<Fact>]
let IndexFlatAddRemoveIdsTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
    addRemoveIds d idx

[<Fact>]
let IndexFlatL2AddRemoveIdsTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
    addRemoveIds d idx

[<Fact>]
let IndexFlatAddWithIdMapTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
    addDataWithIdMap d idx

[<Fact>]
let IndexFlatL2AddWithIdMapTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
    addDataWithIdMap d idx

[<Fact>]
let IndexHSNWAddWithIdMapTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2))
    addDataWithIdMap d idx

[<Fact>]
let IndexFlatAddWithIdMapSaveLoadTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlat(d,FaissNet.MetricType.METRIC_L2))
    saveLoadIdMap d idx

[<Fact>]
let IndexFlatL2AddWithIdMapSaveLoadTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexFlatL2(d,FaissNet.MetricType.METRIC_L2))
    saveLoadIdMap d idx

[<Fact>]
let IndexHSNWAddWithIdMapSaveLoadTest() =
    let d = 64
    let idx = FaissNet.CreateIndex(fun () -> new FaissNet.IndexHNSWFlat(d,32,FaissNet.MetricType.METRIC_L2))
    saveLoadIdMap d idx

